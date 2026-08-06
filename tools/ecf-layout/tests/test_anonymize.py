from __future__ import annotations

import hashlib
import subprocess
from pathlib import Path

import pytest

import ecf_layout.anonymize as anonymize_module
import ecf_layout.cli as cli
from ecf_layout.anonymize import (
    AnonymizationError,
    anonymize_bytes,
    anonymize_file,
)


def _field(
    number: int,
    name: str,
    *,
    field_type: str = "C",
    size: str = "-",
    decimals: str = "-",
    valid_values: str = "-",
    required: str = "Sim",
) -> dict:
    return {
        "number": number,
        "name": name,
        "description": name,
        "type": field_type,
        "size": size,
        "decimals": decimals,
        "required": required,
        "validValues": valid_values,
    }


def _record(code: str, level: int, *fields: dict, occurrence: str = "1:1") -> dict:
    return {
        "code": code,
        "block": code[0],
        "title": f"Registro {code}",
        "pageStart": 1,
        "pageEnd": 1,
        "level": str(level),
        "occurrence": occurrence,
        "reviewed": True,
        "fields": [_field(1, "REG", size="4", valid_values=f"[{code}]"), *fields],
    }


def _manifest(*records: dict) -> list[dict]:
    return [
        _record(
            "0000",
            0,
            _field(2, "NOME_ESC", size="4", valid_values="[LECF]"),
            _field(3, "COD_VER", size="4"),
        ),
        *records,
        _record("9999", 1, _field(2, "QTD_LIN", field_type="N")),
    ]


def _source(*lines: str) -> bytes:
    return ("\r\n".join(("|0000|LECF|0008|", *lines, "|9999|0|")) + "\r\n").encode(
        "cp1252"
    )


def _source_for_version(version: int, *lines: str) -> bytes:
    return (
        "\r\n".join((f"|0000|LECF|{version:04d}|", *lines, "|9999|0|"))
        + "\r\n"
    ).encode("cp1252")


def test_stable_fixture_scoped_pseudonyms_compare_the_complete_output() -> None:
    manifest = _manifest(
        _record(
            "A100",
            1,
            _field(2, "CHAVE", size="20"),
            _field(3, "CHAVE_RELACIONADA", size="20"),
        )
    )
    source = _source("|A100|CHAVE-77|CHAVE-77|")
    expected = (
        "|0000|LECF|0008|\r\n"
        "|A100|FNFQY-93|FNFQY-93|\r\n"
        "|9999|3|\r\n"
    ).encode("cp1252")

    first = anonymize_bytes(source, fixture_id="stable", denylist=(), manifest=manifest)
    second = anonymize_bytes(source, fixture_id="stable", denylist=(), manifest=manifest)
    other_fixture = anonymize_bytes(source, fixture_id="stable-other", denylist=(), manifest=manifest)

    assert first == expected
    assert second == expected
    assert other_fixture != expected


def test_fields_are_active_only_from_their_declared_layout_version() -> None:
    versioned = _field(2, "POSITION_31", valid_values="[S;N]")
    versioned["sinceVersion"] = 10
    manifest = _manifest(_record("A100", 1, versioned))

    anonymize_bytes(
        _source_for_version(8, "|A100|"),
        fixture_id="before-field",
        denylist=(),
        manifest=manifest,
    )
    anonymize_bytes(
        _source_for_version(10, "|A100|S|"),
        fixture_id="with-field",
        denylist=(),
        manifest=manifest,
    )

    with pytest.raises(AnonymizationError, match="invalid source"):
        anonymize_bytes(
            _source_for_version(8, "|A100|S|"),
            fixture_id="premature-field",
            denylist=(),
            manifest=manifest,
        )


def test_table_required_markers_do_not_infer_tax_obligation_for_blank_values() -> None:
    manifest = _manifest(
        _record(
            "A100",
            1,
            _field(2, "VL_REM_TRAB", field_type="N", size="19"),
            _field(3, "FONE", size="15"),
            _field(4, "IND", size="1", valid_values="[S;N]"),
        )
    )
    source = _source("|A100||||")

    output = anonymize_bytes(
        source,
        fixture_id="conditional-blanks",
        denylist=(),
        manifest=manifest,
    )

    assert output == (
        "|0000|LECF|0008|\r\n"
        "|A100||||\r\n"
        "|9999|3|\r\n"
    ).encode("cp1252")


def test_distinct_short_values_never_collapse_to_the_same_pseudonym() -> None:
    manifest = _manifest(
        _record(
            "A100",
            1,
            _field(2, "CHAVE_1", size="1"),
            _field(3, "CHAVE_2", size="1"),
        )
    )
    source = _source("|A100|C|K|")
    expected = (
        "|0000|LECF|0008|\r\n"
        "|A100|I|L|\r\n"
        "|9999|3|\r\n"
    ).encode("cp1252")

    assert anonymize_bytes(source, fixture_id="collision", denylist=(), manifest=manifest) == expected


def test_saturated_short_token_space_uses_an_injective_derangement() -> None:
    manifest = _manifest(
        _record(
            "A100",
            1,
            *(
                    _field(number, f"QTD_TOKEN_{number}", field_type="N", size="1")
                for number in range(2, 12)
            ),
        )
    )
    original = tuple("0123456789")
    source = _source("|A100|" + "|".join(original) + "|")

    output = anonymize_bytes(
        source,
        fixture_id="saturated",
        denylist=(),
        manifest=manifest,
    )

    transformed = tuple(output.decode("cp1252").splitlines()[1].split("|")[2:-1])
    assert set(transformed) == set(original)
    assert len(set(transformed)) == len(original)
    assert all(before != after for before, after in zip(original, transformed, strict=True))


def test_generic_short_values_expand_within_their_maximum_instead_of_leaking() -> None:
    manifest = _manifest(
        _record(
            "A100",
            1,
            *(_field(number, f"NUM_{number}", size="6") for number in range(2, 12)),
        )
    )
    original = tuple("0123456789")
    source = _source("|A100|" + "|".join(original) + "|")

    output = anonymize_bytes(
        source,
        fixture_id="expandable-generic",
        denylist=(),
        manifest=manifest,
    )

    transformed = tuple(output.decode("cp1252").splitlines()[1].split("|")[2:-1])
    assert len(set(transformed)) == len(original)
    assert set(transformed).isdisjoint(original)
    assert all(1 < len(value) <= 6 and value.isdigit() for value in transformed)


def test_saturated_generic_alphabet_fails_closed_instead_of_leaking() -> None:
    manifest = _manifest(
        _record(
            "A100",
            1,
            *(_field(number, f"TEXT_{number}", size="1") for number in range(2, 28)),
        )
    )
    source = _source("|A100|" + "|".join("ABCDEFGHIJKLMNOPQRSTUVWXYZ") + "|")

    with pytest.raises(AnonymizationError, match="privacy audit failed"):
        anonymize_bytes(
            source,
            fixture_id="saturated-generic",
            denylist=(),
            manifest=manifest,
        )


def test_explicit_singleton_structural_domain_is_preserved() -> None:
    manifest = _manifest(
        _record("A100", 1, _field(2, "IND_ESTRUTURAL", size="1", valid_values="[X]"))
    )

    assert anonymize_bytes(
        _source("|A100|X|"),
        fixture_id="singleton-structural",
        denylist=(),
        manifest=manifest,
    ) == (
        "|0000|LECF|0008|\r\n"
        "|A100|X|\r\n"
        "|9999|3|\r\n"
    ).encode("cp1252")


def test_mixed_sensitive_and_structural_token_is_caught_by_absolute_denylist() -> None:
    manifest = _manifest(
        _record(
            "A100",
            1,
            _field(2, "NOME", size="10"),
            _field(3, "IND", size="5", valid_values="[ALPHA;BETA]"),
        )
    )

    with pytest.raises(AnonymizationError, match="privacy audit failed"):
        anonymize_bytes(
            _source("|A100|ALPHA|ALPHA|"),
            fixture_id="mixed-semantics",
            denylist=("ALPHA",),
            manifest=manifest,
        )


def test_values_from_dropped_occurrences_are_forbidden_as_pseudonyms() -> None:
    manifest = _manifest(_record("A100", 1, _field(2, "CHAVE", size="1"), occurrence="0:N"))
    source = _source("|A100|A|", "|A100|B|")
    expected = (
        "|0000|LECF|0008|\r\n"
        "|A100|O|\r\n"
        "|9999|3|\r\n"
    ).encode("cp1252")

    assert anonymize_bytes(source, fixture_id="collision-2", denylist=(), manifest=manifest) == expected


def test_valid_cpf_and_cnpj_check_digits_compare_the_complete_output() -> None:
    manifest = _manifest(
        _record(
            "A100",
            1,
            _field(2, "CPF", field_type="N", size="11"),
            _field(3, "CNPJ", size="14"),
        )
    )
    source = _source("|A100|12345678909|11222333000181|")
    expected = (
        "|0000|LECF|0008|\r\n"
        "|A100|76521718061|94497952784603|\r\n"
        "|9999|3|\r\n"
    ).encode("cp1252")

    output = anonymize_bytes(source, fixture_id="docs", denylist=(), manifest=manifest)

    assert output == expected
    cells = output.decode("cp1252").splitlines()[1].split("|")
    assert _valid_cpf(cells[2])
    assert _valid_cnpj(cells[3])


def test_document_semantics_are_applied_to_every_equal_related_key() -> None:
    manifest = _manifest(
        _record(
            "A100",
            1,
            _field(2, "CPF", field_type="N", size="11"),
            _field(3, "RELATED_KEY", size="11"),
        )
    )
    source = _source("|A100|12345678909|12345678909|")
    expected = (
        "|0000|LECF|0008|\r\n"
        "|A100|76521718061|76521718061|\r\n"
        "|9999|3|\r\n"
    ).encode("cp1252")

    assert anonymize_bytes(source, fixture_id="docs", denylist=(), manifest=manifest) == expected


def test_one_deterministic_date_offset_compares_the_complete_output() -> None:
    manifest = _manifest(
        _record(
            "A100",
            1,
            _field(2, "DT_INI", field_type="N", size="8"),
            _field(3, "DT_FIN", field_type="N", size="8"),
        )
    )
    source = _source("|A100|01012025|15022025|")
    expected = (
        "|0000|LECF|0008|\r\n"
        "|A100|22122024|05022025|\r\n"
        "|9999|3|\r\n"
    ).encode("cp1252")

    assert anonymize_bytes(source, fixture_id="dates", denylist=(), manifest=manifest) == expected


def test_dat_prefixed_fields_use_the_same_fixture_date_offset() -> None:
    manifest = _manifest(
        _record(
            "A100",
            1,
            _field(2, "DAT_ABERT", field_type="N", size="8"),
            _field(3, "DAT_ENCER", field_type="N", size="8"),
        )
    )
    expected = (
        "|0000|LECF|0008|\r\n"
        "|A100|22122024|05022025|\r\n"
        "|9999|3|\r\n"
    ).encode("cp1252")

    assert anonymize_bytes(
        _source("|A100|01012025|15022025|"),
        fixture_id="dates",
        denylist=(),
        manifest=manifest,
    ) == expected


@pytest.mark.parametrize(
    ("fixture_id", "source_date", "expected_date"),
    [("date-0", "01010001", "01060001"), ("date-1", "31129999", "28029999")],
)
def test_date_offset_selects_one_deterministic_direction_valid_at_calendar_boundaries(
    fixture_id: str, source_date: str, expected_date: str
) -> None:
    manifest = _manifest(
        _record("A100", 1, _field(2, "DT_EVENTO", field_type="N", size="8"))
    )
    source = _source(f"|A100|{source_date}|")
    expected = (
        "|0000|LECF|0008|\r\n"
        f"|A100|{expected_date}|\r\n"
        "|9999|3|\r\n"
    ).encode("cp1252")

    assert anonymize_bytes(source, fixture_id=fixture_id, denylist=(), manifest=manifest) == expected


def test_date_offset_never_reuses_any_sensitive_source_value() -> None:
    manifest = _manifest(
        _record(
            "A100",
            1,
            _field(2, "DT_INI", field_type="N", size="8"),
            _field(3, "DT_FIN", field_type="N", size="8"),
        )
    )
    expected = (
        "|0000|LECF|0008|\r\n"
        "|A100|03012025|04012025|\r\n"
        "|9999|3|\r\n"
    ).encode("cp1252")

    assert anonymize_bytes(
        _source("|A100|01012025|02012025|"),
        fixture_id="date-collision-161",
        denylist=(),
        manifest=manifest,
    ) == expected


def test_generic_pseudonym_cannot_collide_with_a_shifted_date() -> None:
    manifest = _manifest(
        _record(
            "A100",
            1,
            _field(2, "DT_EVENTO", field_type="N", size="8"),
            occurrence="0:N",
        ),
        _record("B100", 1, _field(2, "RELATED_KEY", size="8")),
    )
    years = list(range(1000, 9002, 3))
    source_dates = ("01018281", *(f"0101{year:04d}" for year in years if year != 8281))
    expected = (
        "|0000|LECF|0008|\r\n"
        "|A100|10078281|\r\n"
        "|B100|01151415|\r\n"
        "|9999|4|\r\n"
    ).encode("cp1252")

    assert anonymize_bytes(
        _source(
            *(f"|A100|{source_date}|" for source_date in source_dates),
            "|B100|00010430|",
        ),
        fixture_id="date-generic-collision",
        denylist=(),
        manifest=manifest,
    ) == expected


def test_numeric_sign_and_lexical_scale_compare_the_complete_output() -> None:
    manifest = _manifest(
        _record(
            "A100",
            1,
            _field(2, "VALOR", field_type="N", size="12", decimals="3"),
            _field(3, "OUTRO_VALOR", field_type="N", size="12", decimals="2"),
        )
    )
    source = _source("|A100|-0012,340|+001,20|")
    expected = (
        "|0000|LECF|0008|\r\n"
        "|A100|-4076,865|+667,56|\r\n"
        "|9999|3|\r\n"
    ).encode("cp1252")

    assert anonymize_bytes(source, fixture_id="stable", denylist=(), manifest=manifest) == expected


def test_cp1252_and_maximum_text_size_compare_the_complete_output() -> None:
    manifest = _manifest(_record("A100", 1, _field(2, "NOME", size="9")))
    source = _source("|A100|AÇÃO ÁGIL|")
    expected = (
        "|0000|LECF|0008|\r\n"
        "|A100|PÀÍQ ÃEYD|\r\n"
        "|9999|3|\r\n"
    ).encode("cp1252")

    output = anonymize_bytes(source, fixture_id="stable", denylist=(), manifest=manifest)

    assert output == expected
    assert output.decode("cp1252").encode("cp1252") == output
    assert b"\xc0" in output and b"\xcd" in output and b"\xc3" in output


def test_key_equality_and_ancestor_retention_compare_the_complete_output() -> None:
    manifest = _manifest(
        _record("A100", 1, _field(2, "CHAVE", size="20")),
        _record(
            "A110",
            2,
            _field(2, "CHAVE_PAI", size="20"),
            occurrence="0:N",
        ),
    )
    source = _source(
        "|A100|CLIENTE-9|",
        "|A110|CLIENTE-9|",
        "|A110|CLIENTE-9|",
    )
    expected = (
        "|0000|LECF|0008|\r\n"
        "|A100|PEDCXUY-2|\r\n"
        "|A110|PEDCXUY-2|\r\n"
        "|9999|4|\r\n"
    ).encode("cp1252")

    assert anonymize_bytes(source, fixture_id="keys", denylist=(), manifest=manifest) == expected


def test_each_selected_code_retains_its_actual_source_ancestor_occurrence() -> None:
    manifest = _manifest(
        _record("A100", 1, _field(2, "CHAVE"), occurrence="0:N"),
        _record("A110", 2, _field(2, "CHAVE_PAI"), occurrence="0:N"),
        _record("A120", 2, _field(2, "CHAVE_PAI"), occurrence="0:N"),
    )
    source = _source(
        "|A100|PARENT-1|",
        "|A110|PARENT-1|",
        "|A100|PARENT-2|",
        "|A120|PARENT-2|",
    )
    expected = (
        "|0000|LECF|0008|\r\n"
        "|A100|WJXJRL-2|\r\n"
        "|A110|WJXJRL-2|\r\n"
        "|A100|QKOVPG-6|\r\n"
        "|A120|QKOVPG-6|\r\n"
        "|9999|6|\r\n"
    ).encode("cp1252")

    assert anonymize_bytes(source, fixture_id="ancestor", denylist=(), manifest=manifest) == expected


def test_structural_closure_and_totalizers_are_recomputed_in_the_complete_output() -> None:
    manifest = _manifest(
        _record("0001", 1, _field(2, "IND_DAD", field_type="N", valid_values="[0;1]")),
        _record("0990", 1, _field(2, "QTD_LIN", field_type="N")),
        _record("9001", 1, _field(2, "IND_DAD", field_type="N", valid_values="[0;1]")),
        _record(
            "9900",
            2,
            _field(2, "REG_BLC", size="4"),
            _field(3, "QTD_REG_BLC", field_type="N"),
            _field(4, "VERSAO", size="4", required="Não"),
            _field(5, "ID_TAB_DIN", required="Não"),
            occurrence="0:N",
        ),
        _record("9990", 1, _field(2, "QTD_LIN", field_type="N")),
    )
    source = _source(
        "|0001|0|",
        "|0990|99|",
        "|9001|0|",
        "|9900|0000|999|||",
        "|9990|99|",
    )
    expected = (
        "|0000|LECF|0008|\r\n"
        "|0001|0|\r\n"
        "|0990|3|\r\n"
        "|9001|0|\r\n"
        "|9900|0000|1|||\r\n"
        "|9900|0001|1|||\r\n"
        "|9900|0990|1|||\r\n"
        "|9900|9001|1|||\r\n"
        "|9900|9900|7|||\r\n"
        "|9900|9990|1|||\r\n"
        "|9900|9999|1|||\r\n"
        "|9990|9|\r\n"
        "|9999|13|\r\n"
    ).encode("cp1252")

    assert anonymize_bytes(source, fixture_id="counts", denylist=(), manifest=manifest) == expected


def test_denylist_and_equal_hash_abort_without_output() -> None:
    manifest = _manifest(_record("A100", 1, _field(2, "NOME", size="30")))
    source = _source("|A100|EMPRESA SINTÉTICA|")

    with pytest.raises(AnonymizationError, match="privacy audit failed"):
        anonymize_bytes(
            source,
            fixture_id="other",
            denylist=("BNNEHPC EDJBÇCSFZ",),
            manifest=manifest,
        )

    fixed_manifest = _manifest()
    fixed_source = "|0000|LECF|0008|\r\n|9999|2|\r\n".encode("cp1252")
    with pytest.raises(AnonymizationError, match="privacy audit failed"):
        anonymize_bytes(fixed_source, fixture_id="fixed", denylist=(), manifest=fixed_manifest)


def test_logs_are_limited_to_counts_and_codes_and_output_is_golden(
    tmp_path: Path, capsys: pytest.CaptureFixture[str]
) -> None:
    private_root = tmp_path / "private"
    private_root.mkdir()
    source = private_root / "source.bin"
    denylist = private_root / "audit.bin"
    output = tmp_path / "fixture.txt"
    manifest_path = tmp_path / "manifest.json"
    source_bytes = _source("|A100|EMPRESA SINTÉTICA|")
    source.write_bytes(source_bytes)
    source.with_name(source.name + ".sha256").write_text(
        hashlib.sha256(source_bytes).hexdigest() + "\n", encoding="ascii"
    )
    denylist.write_bytes("VALOR AUSENTE\r\n".encode("cp1252"))
    manifest_path.write_text(
        __import__("json").dumps(_manifest(_record("A100", 1, _field(2, "NOME", size="30")))),
        encoding="utf-8",
    )
    expected = (
        "|0000|LECF|0008|\r\n"
        "|A100|BNNEHPC EDJBÇCSFZ|\r\n"
        "|9999|3|\r\n"
    ).encode("cp1252")

    result = anonymize_file(
        source,
        output,
        fixture_id="other",
        denylist_path=denylist,
        manifest_path=manifest_path,
        private_root=private_root,
    )
    print(result.log_line)

    assert output.read_bytes() == expected
    assert capsys.readouterr().out == "anonymized: records=3 codes=0000,A100,9999\n"


def test_anonymize_cli_uses_only_explicit_private_inputs_and_generic_logs(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch, capsys: pytest.CaptureFixture[str]
) -> None:
    private_root = tmp_path / ".local" / "ecf-layout" / "private"
    private_root.mkdir(parents=True)
    manifest_path = tmp_path / "sped" / "ecf" / "layout-12-manifest.json"
    manifest_path.parent.mkdir(parents=True)
    source = private_root / "source.bin"
    denylist = private_root / "audit.bin"
    output = tmp_path / "fixture.txt"
    source_bytes = _source("|A100|CHAVE-77|")
    source.write_bytes(source_bytes)
    source.with_name(source.name + ".sha256").write_text(
        hashlib.sha256(source_bytes).hexdigest(), encoding="ascii"
    )
    denylist.write_bytes(b"NEVER-MATCH\r\n")
    manifest_path.write_text(
        __import__("json").dumps(_manifest(_record("A100", 1, _field(2, "CHAVE")))),
        encoding="utf-8",
    )
    monkeypatch.chdir(tmp_path)

    exit_code = cli.main(
        [
            "anonymize",
            "--source",
            str(source),
            "--output",
            str(output),
            "--fixture-id",
            "stable",
            "--denylist",
            str(denylist),
        ]
    )

    assert exit_code == 0
    assert output.read_bytes() == (
        "|0000|LECF|0008|\r\n"
        "|A100|FNFQY-93|\r\n"
        "|9999|3|\r\n"
    ).encode("cp1252")
    assert capsys.readouterr().out == "anonymized: records=3 codes=0000,A100,9999\n"


@pytest.mark.parametrize(
    "source_bytes",
    [
        b"\x81",
        b"0000|LECF|0008|\r\n|9999|2|\r\n",
        b"|0000|LECF|0008|\r\n|ZZZZ|x|\r\n|9999|3|\r\n",
        b"|0000|ABCD|0008|\r\n|9999|2|\r\n",
    ],
)
def test_hostile_encoding_malformed_and_unknown_records_fail_closed(source_bytes: bytes) -> None:
    with pytest.raises(AnonymizationError, match="invalid source"):
        anonymize_bytes(source_bytes, fixture_id="hostile", denylist=(), manifest=_manifest())


def test_non_ascii_utf8_source_is_rejected_instead_of_becoming_cp1252_mojibake() -> None:
    manifest = _manifest(_record("A100", 1, _field(2, "NOME", size="30")))
    source = (
        "|0000|LECF|0008|\r\n|A100|AÇÃO SINTÉTICA|\r\n|9999|3|\r\n"
    ).encode("utf-8")

    with pytest.raises(AnonymizationError, match="invalid source"):
        anonymize_bytes(source, fixture_id="hostile", denylist=(), manifest=manifest)


@pytest.mark.parametrize(
    "source_lines",
    [
        ("|A110|CHAVE|",),
        ("|A100|CHAVE|", "|A100|OUTRA|")
    ],
)
def test_missing_or_duplicate_required_ancestors_fail_closed(source_lines: tuple[str, ...]) -> None:
    manifest = _manifest(
        _record("A100", 1, _field(2, "CHAVE")),
        _record("A110", 2, _field(2, "CHAVE")),
    )

    with pytest.raises(AnonymizationError, match="invalid source"):
        anonymize_bytes(_source(*source_lines), fixture_id="hierarchy", denylist=(), manifest=manifest)


def test_singleton_occurrence_is_scoped_to_each_repeated_parent() -> None:
    manifest = _manifest(
        _record(
            "A100",
            1,
            _field(2, "IND", field_type="N", valid_values="[0;1]"),
            occurrence="0:N",
        ),
        _record(
            "A110",
            2,
            _field(2, "IND", field_type="N", valid_values="[0;1]"),
            occurrence="1:1",
        ),
    )
    source = _source("|A100|0|", "|A110|0|", "|A100|1|", "|A110|1|")
    expected = (
        "|0000|LECF|0008|\r\n"
        "|A100|0|\r\n"
        "|A110|0|\r\n"
        "|9999|4|\r\n"
    ).encode("cp1252")

    assert anonymize_bytes(source, fixture_id="cardinality", denylist=(), manifest=manifest) == expected


@pytest.mark.parametrize("occurrence,count", [("0:1", 2), ("0:2", 3)])
def test_finite_occurrence_maximum_is_enforced_per_parent(
    occurrence: str, count: int
) -> None:
    manifest = _manifest(
        _record("A100", 1, occurrence="0:N"),
        _record(
            "A110",
            2,
            _field(2, "IND", field_type="N", valid_values="[0;1]"),
            occurrence=occurrence,
        ),
    )
    children = tuple(f"|A110|{position % 2}|" for position in range(count))

    with pytest.raises(AnonymizationError, match="invalid source"):
        anonymize_bytes(
            _source("|A100|", *children),
            fixture_id="maximum",
            denylist=(),
            manifest=manifest,
        )


def test_occurrence_minimum_does_not_infer_a_conditional_child() -> None:
    manifest = _manifest(
        _record("A100", 1, occurrence="0:N"),
        _record("A110", 2, occurrence="1:2"),
    )

    assert anonymize_bytes(
        _source("|A100|"),
        fixture_id="minimum",
        denylist=(),
        manifest=manifest,
    ) == (
        "|0000|LECF|0008|\r\n"
        "|A100|\r\n"
        "|9999|3|\r\n"
    ).encode("cp1252")


def test_compaction_retains_source_codes_and_ancestors_without_conditional_children() -> None:
    manifest = _manifest(
        _record("A100", 1, occurrence="0:N"),
        _record("A110", 2, occurrence="1:1"),
        _record("A120", 2, occurrence="0:N"),
    )
    source = _source(
        "|A100|",
        "|A110|",
        "|A100|",
        "|A110|",
        "|A120|",
    )
    expected = (
        "|0000|LECF|0008|\r\n"
        "|A100|\r\n"
        "|A110|\r\n"
        "|A100|\r\n"
        "|A120|\r\n"
        "|9999|6|\r\n"
    ).encode("cp1252")

    assert anonymize_bytes(
        source, fixture_id="ancestor-closure", denylist=(), manifest=manifest
    ) == expected


@pytest.mark.parametrize("occurrence", ["0:X", "01:1", "1:01", "1:n", " 1:1"])
def test_invalid_occurrence_grammar_fails_closed(occurrence: str) -> None:
    manifest = _manifest(_record("A100", 1, occurrence=occurrence))

    with pytest.raises(AnonymizationError, match="invalid manifest"):
        anonymize_bytes(
            _source("|A100|"),
            fixture_id="occurrence-grammar",
            denylist=(),
            manifest=manifest,
        )


def test_record_must_follow_the_canonical_parent_code_not_only_a_numeric_level() -> None:
    manifest = _manifest(
        _record("A100", 1, _field(2, "CHAVE"), occurrence="0:N"),
        _record("A110", 2, _field(2, "CHAVE"), occurrence="0:N"),
        _record("B100", 1, _field(2, "CHAVE"), occurrence="0:N"),
        _record("B110", 2, _field(2, "CHAVE"), occurrence="0:N"),
    )

    with pytest.raises(AnonymizationError, match="invalid source"):
        anonymize_bytes(
            _source("|A100|A|", "|B110|B|"),
            fixture_id="parent",
            denylist=(),
            manifest=manifest,
        )


def test_sibling_records_must_follow_manifest_order_within_each_parent() -> None:
    manifest = _manifest(
        _record("A100", 1, occurrence="0:N"),
        _record("A110", 2, occurrence="0:N"),
        _record("A120", 2, occurrence="0:N"),
    )

    with pytest.raises(AnonymizationError, match="invalid source"):
        anonymize_bytes(
            _source("|A100|", "|A120|", "|A110|"),
            fixture_id="sibling-order",
            denylist=(),
            manifest=manifest,
        )


@pytest.mark.parametrize(
    "record_line",
    [
        "|A100|KEY|9|50|",
        "|A100|KEY|1|101|",
        "|A100|KEY|1|1e1|",
        "|A100|KEY|1|NaN|",
    ],
)
def test_required_closed_domain_and_simple_range_are_strict_syntax(
    record_line: str,
) -> None:
    manifest = _manifest(
        _record(
            "A100",
            1,
            _field(2, "CHAVE", size="10"),
            _field(3, "IND", field_type="N", size="1", valid_values="[0;1]"),
            _field(4, "PERCENTUAL", field_type="N", size="3", valid_values="[0 a 100]"),
        )
    )

    with pytest.raises(AnonymizationError, match="invalid source"):
        anonymize_bytes(
            _source(record_line), fixture_id="domains", denylist=(), manifest=manifest
        )


def test_zero_decimal_numeric_metadata_rejects_alphabetic_values() -> None:
    manifest = _manifest(
        _record(
            "A100",
            1,
            _field(2, "VL_REC", field_type="N", size="10", decimals="0"),
        )
    )

    with pytest.raises(AnonymizationError, match="invalid source"):
        anonymize_bytes(
            _source("|A100|ALPHA|"),
            fixture_id="integer-metadata",
            denylist=(),
            manifest=manifest,
        )


@pytest.mark.parametrize(
    "field_name,value",
    [
        ("CNPJ", "ABCDEFGHIJKLMN"),
        ("CNPJ", "1122233300018"),
        ("CNPJ", "11222333000180"),
        ("CPF_REP_LEG", "ABCDEFGHIJK"),
        ("CPF_REP_LEG", "12345678900"),
        ("CPF_CNPJ", "ABCDEFGHIJKLMN"),
        ("IDENT_CPF_CNPJ", "1234567890"),
    ],
)
def test_unambiguous_national_document_fields_require_valid_digits(
    field_name: str, value: str
) -> None:
    size = "11" if field_name.startswith("CPF_") and field_name != "CPF_CNPJ" else "14"
    manifest = _manifest(_record("A100", 1, _field(2, field_name, size=size)))

    with pytest.raises(AnonymizationError, match="invalid source"):
        anonymize_bytes(
            _source(f"|A100|{value}|"),
            fixture_id="invalid-document",
            denylist=(),
            manifest=manifest,
        )


def test_manifest_field_reference_is_not_misread_as_a_closed_domain() -> None:
    manifest = _manifest(
        _record("A100", 1, _field(2, "RELATED_KEY", valid_values="[B100_KEY]"))
    )
    expected = (
        "|0000|LECF|0008|\r\n"
        "|A100|GTWNUH|\r\n"
        "|9999|3|\r\n"
    ).encode("cp1252")

    assert anonymize_bytes(
        _source("|A100|ACTUAL|"),
        fixture_id="narrative",
        denylist=(),
        manifest=manifest,
    ) == expected


def test_record_code_identity_wins_over_a_mistyped_reg_valid_values_cell() -> None:
    record = _record("Y681", 1)
    record["fields"][0]["validValues"] = "[X681]"
    manifest = _manifest(record)
    expected = (
        "|0000|LECF|0008|\r\n"
        "|Y681|\r\n"
        "|9999|3|\r\n"
    ).encode("cp1252")

    assert anonymize_bytes(
        _source("|Y681|"), fixture_id="reg-code", denylist=(), manifest=manifest
    ) == expected


def test_explicit_multi_character_composite_domain_is_preserved() -> None:
    manifest = _manifest(
        _record("A100", 1, _field(2, "FORMA", size="4", valid_values="[0;R;P]"))
    )
    expected = (
        "|0000|LECF|0008|\r\n"
        "|A100|R0PR|\r\n"
        "|9999|3|\r\n"
    ).encode("cp1252")

    assert anonymize_bytes(
        _source("|A100|R0PR|"), fixture_id="composite", denylist=(), manifest=manifest
    ) == expected


def test_scalar_domain_rejects_repeated_option_characters_without_composite_size() -> None:
    manifest = _manifest(
        _record("A100", 1, _field(2, "IND", valid_values="[D;C]"))
    )

    with pytest.raises(AnonymizationError, match="invalid source"):
        anonymize_bytes(
            _source("|A100|DD|"), fixture_id="scalar", denylist=(), manifest=manifest
        )


@pytest.mark.parametrize("stage", ["after_write", "after_validation", "after_fsync"])
def test_write_validation_or_fsync_interruption_preserves_existing_output(
    tmp_path: Path, stage: str
) -> None:
    private_root = tmp_path / "private"
    private_root.mkdir()
    source = private_root / "source.bin"
    denylist = private_root / "audit.bin"
    output = tmp_path / "fixture.txt"
    manifest_path = tmp_path / "manifest.json"
    source_bytes = _source("|A100|CHAVE-77|")
    source.write_bytes(source_bytes)
    source.with_name(source.name + ".sha256").write_text(
        hashlib.sha256(source_bytes).hexdigest(), encoding="ascii"
    )
    denylist.write_bytes(b"NEVER-MATCH\r\n")
    manifest_path.write_text(
        __import__("json").dumps(_manifest(_record("A100", 1, _field(2, "CHAVE")))),
        encoding="utf-8",
    )
    output.write_bytes(b"OLD\r\n")

    def interrupt(current: str) -> None:
        if current == stage:
            raise OSError("synthetic interruption")

    with pytest.raises(AnonymizationError, match="atomic promotion failed"):
        anonymize_file(
            source,
            output,
            fixture_id="stable",
            denylist_path=denylist,
            manifest_path=manifest_path,
            private_root=private_root,
            interrupt=interrupt,
        )

    assert output.read_bytes() == b"OLD\r\n"
    assert list(output.parent.glob(f".{output.name}.*.tmp")) == []


def test_keyboard_interrupt_after_write_preserves_output_and_cleans_temp(
    tmp_path: Path,
) -> None:
    private_root = tmp_path / "private"
    private_root.mkdir()
    source = private_root / "source.bin"
    denylist = private_root / "audit.bin"
    output = tmp_path / "fixture.txt"
    manifest_path = tmp_path / "manifest.json"
    source_bytes = _source("|A100|CHAVE-77|")
    source.write_bytes(source_bytes)
    source.with_name(source.name + ".sha256").write_text(
        hashlib.sha256(source_bytes).hexdigest(), encoding="ascii"
    )
    denylist.write_bytes(b"NEVER-MATCH\r\n")
    manifest_path.write_text(
        __import__("json").dumps(_manifest(_record("A100", 1, _field(2, "CHAVE")))),
        encoding="utf-8",
    )
    output.write_bytes(b"OLD\r\n")

    def interrupt(current: str) -> None:
        if current == "after_write":
            raise KeyboardInterrupt

    with pytest.raises(KeyboardInterrupt):
        anonymize_file(
            source,
            output,
            fixture_id="stable",
            denylist_path=denylist,
            manifest_path=manifest_path,
            private_root=private_root,
            interrupt=interrupt,
        )

    assert output.read_bytes() == b"OLD\r\n"
    assert list(output.parent.glob(f".{output.name}.*.tmp")) == []


def test_parent_directory_fsync_failure_reports_complete_promoted_output(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    private_root = tmp_path / "private"
    private_root.mkdir()
    source = private_root / "source.bin"
    denylist = private_root / "audit.bin"
    output = tmp_path / "fixture.txt"
    manifest_path = tmp_path / "manifest.json"
    manifest = _manifest(_record("A100", 1, _field(2, "CHAVE")))
    source_bytes = _source("|A100|CHAVE-77|")
    source.write_bytes(source_bytes)
    source.with_name(source.name + ".sha256").write_text(
        hashlib.sha256(source_bytes).hexdigest(), encoding="ascii"
    )
    denylist.write_bytes(b"NEVER-MATCH\r\n")
    manifest_path.write_text(__import__("json").dumps(manifest), encoding="utf-8")
    output.write_bytes(b"OLD\r\n")
    expected = anonymize_bytes(
        source_bytes, fixture_id="stable", denylist=("NEVER-MATCH",), manifest=manifest
    )

    def fail_directory_fsync(directory: Path) -> None:
        raise OSError("synthetic directory fsync failure")

    monkeypatch.setattr(anonymize_module, "_fsync_parent_directory", fail_directory_fsync)

    with pytest.raises(AnonymizationError, match="atomic promotion failed"):
        anonymize_file(
            source,
            output,
            fixture_id="stable",
            denylist_path=denylist,
            manifest_path=manifest_path,
            private_root=private_root,
        )

    assert output.read_bytes() == expected
    assert list(output.parent.glob(f".{output.name}.*.tmp")) == []


def test_output_parent_failure_is_reported_without_a_private_path(tmp_path: Path) -> None:
    private_root = tmp_path / "private"
    private_root.mkdir()
    source = private_root / "source.bin"
    denylist = private_root / "audit.bin"
    source_bytes = _source("|A100|CHAVE-77|")
    source.write_bytes(source_bytes)
    source.with_name(source.name + ".sha256").write_text(
        hashlib.sha256(source_bytes).hexdigest(), encoding="ascii"
    )
    denylist.write_bytes(b"NEVER-MATCH\r\n")
    manifest_path = tmp_path / "manifest.json"
    manifest_path.write_text(
        __import__("json").dumps(_manifest(_record("A100", 1, _field(2, "CHAVE")))),
        encoding="utf-8",
    )
    blocked_parent = tmp_path / "not-a-directory"
    blocked_parent.write_bytes(b"occupied")

    with pytest.raises(AnonymizationError, match="atomic promotion failed") as failure:
        anonymize_file(
            source,
            blocked_parent / "fixture.txt",
            fixture_id="stable",
            denylist_path=denylist,
            manifest_path=manifest_path,
            private_root=private_root,
        )

    assert str(source) not in str(failure.value)
    assert str(blocked_parent) not in str(failure.value)


def test_source_hash_sidecar_is_required_and_must_match_without_exposing_paths(tmp_path: Path) -> None:
    private_root = tmp_path / "private"
    private_root.mkdir()
    source = private_root / "source.bin"
    denylist = private_root / "audit.bin"
    source.write_bytes(_source("|A100|CHAVE-77|"))
    denylist.write_bytes(b"NEVER-MATCH\r\n")
    manifest_path = tmp_path / "manifest.json"
    manifest_path.write_text(
        __import__("json").dumps(_manifest(_record("A100", 1, _field(2, "CHAVE")))),
        encoding="utf-8",
    )

    with pytest.raises(AnonymizationError, match="source authorization failed") as missing:
        anonymize_file(
            source,
            tmp_path / "out.txt",
            fixture_id="stable",
            denylist_path=denylist,
            manifest_path=manifest_path,
            private_root=private_root,
        )
    assert str(source) not in str(missing.value)

    source.with_name(source.name + ".sha256").write_text("0" * 64, encoding="ascii")
    with pytest.raises(AnonymizationError, match="source authorization failed"):
        anonymize_file(
            source,
            tmp_path / "out.txt",
            fixture_id="stable",
            denylist_path=denylist,
            manifest_path=manifest_path,
            private_root=private_root,
        )

    source.with_name(source.name + ".sha256").write_text(
        " " + hashlib.sha256(source.read_bytes()).hexdigest() + " \n", encoding="ascii"
    )
    with pytest.raises(AnonymizationError, match="source authorization failed"):
        anonymize_file(
            source,
            tmp_path / "out.txt",
            fixture_id="stable",
            denylist_path=denylist,
            manifest_path=manifest_path,
            private_root=private_root,
        )


def test_root_source_path_fails_with_a_generic_authorization_error(tmp_path: Path) -> None:
    root = Path(tmp_path.anchor)

    with pytest.raises(AnonymizationError, match="source authorization failed"):
        anonymize_file(
            root,
            tmp_path / "out.txt",
            fixture_id="stable",
            denylist_path=tmp_path / "audit.bin",
            manifest_path=tmp_path / "manifest.json",
            private_root=tmp_path,
        )


def test_hardlinked_source_and_output_are_rejected_before_promotion(tmp_path: Path) -> None:
    private_root = tmp_path / "private"
    private_root.mkdir()
    source = private_root / "source.bin"
    denylist = private_root / "audit.bin"
    output = tmp_path / "output.bin"
    source_bytes = _source("|A100|CHAVE-77|")
    source.write_bytes(source_bytes)
    source.with_name(source.name + ".sha256").write_text(
        hashlib.sha256(source_bytes).hexdigest(), encoding="ascii"
    )
    denylist.write_bytes(b"NEVER-MATCH\r\n")
    output.hardlink_to(source)
    manifest_path = tmp_path / "manifest.json"
    manifest_path.write_text(
        __import__("json").dumps(_manifest(_record("A100", 1, _field(2, "CHAVE")))),
        encoding="utf-8",
    )

    with pytest.raises(AnonymizationError, match="output authorization failed"):
        anonymize_file(
            source,
            output,
            fixture_id="stable",
            denylist_path=denylist,
            manifest_path=manifest_path,
            private_root=private_root,
        )

    assert source.read_bytes() == source_bytes
    assert output.read_bytes() == source_bytes


@pytest.mark.parametrize("alias", ["denylist", "sidecar", "manifest"])
def test_source_cannot_alias_an_authorization_or_audit_input(
    tmp_path: Path, alias: str
) -> None:
    private_root = tmp_path / "private"
    private_root.mkdir()
    source = private_root / "source.bin"
    source_bytes = _source("|A100|CHAVE-77|")
    source.write_bytes(source_bytes)
    sidecar = source.with_name(source.name + ".sha256")
    denylist = private_root / "audit.bin"
    manifest_path = private_root / "manifest.json"
    sidecar.write_text(hashlib.sha256(source_bytes).hexdigest(), encoding="ascii")
    denylist.write_bytes(b"NEVER-MATCH\r\n")
    manifest_path.write_text(
        __import__("json").dumps(_manifest(_record("A100", 1, _field(2, "CHAVE")))),
        encoding="utf-8",
    )
    target = {"denylist": denylist, "sidecar": sidecar, "manifest": manifest_path}[alias]
    target.unlink()
    target.hardlink_to(source)

    with pytest.raises(AnonymizationError, match="source authorization failed"):
        anonymize_file(
            source,
            tmp_path / "output.txt",
            fixture_id="stable",
            denylist_path=denylist,
            manifest_path=manifest_path,
            private_root=private_root,
        )


@pytest.mark.parametrize(
    "denylist_bytes",
    ["VALOR NÃO PRESENTE\r\n".encode("utf-8"), b"NEVER-MATCH\r\n\r\n"],
)
def test_hostile_or_blank_denylist_encoding_fails_closed(
    tmp_path: Path, denylist_bytes: bytes
) -> None:
    private_root = tmp_path / "private"
    private_root.mkdir()
    source = private_root / "source.bin"
    denylist = private_root / "audit.bin"
    source_bytes = _source("|A100|CHAVE-77|")
    source.write_bytes(source_bytes)
    source.with_name(source.name + ".sha256").write_text(
        hashlib.sha256(source_bytes).hexdigest(), encoding="ascii"
    )
    denylist.write_bytes(denylist_bytes)
    manifest_path = tmp_path / "manifest.json"
    manifest_path.write_text(
        __import__("json").dumps(_manifest(_record("A100", 1, _field(2, "CHAVE")))),
        encoding="utf-8",
    )

    with pytest.raises(AnonymizationError, match="privacy audit failed"):
        anonymize_file(
            source,
            tmp_path / "output.txt",
            fixture_id="stable",
            denylist_path=denylist,
            manifest_path=manifest_path,
            private_root=private_root,
        )


def test_direct_api_rejects_whitespace_only_denylist() -> None:
    manifest = _manifest(_record("A100", 1, _field(2, "CHAVE", size="10")))
    with pytest.raises(AnonymizationError, match="privacy audit failed"):
        anonymize_bytes(
            _source("|A100|VALUE|"),
            fixture_id="denylist",
            denylist=("   ",),
            manifest=manifest,
        )


@pytest.mark.parametrize("alias", ["source", "sidecar", "denylist", "manifest"])
def test_output_cannot_alias_any_authorization_or_audit_input(tmp_path: Path, alias: str) -> None:
    private_root = tmp_path / "private"
    private_root.mkdir()
    source = private_root / "source.bin"
    sidecar = source.with_name(source.name + ".sha256")
    denylist = private_root / "audit.bin"
    manifest_path = tmp_path / "manifest.json"
    source_bytes = _source("|A100|CHAVE-77|")
    source.write_bytes(source_bytes)
    sidecar.write_text(hashlib.sha256(source_bytes).hexdigest(), encoding="ascii")
    denylist.write_bytes(b"NEVER-MATCH\r\n")
    manifest_path.write_text(
        __import__("json").dumps(_manifest(_record("A100", 1, _field(2, "CHAVE")))),
        encoding="utf-8",
    )
    output = {
        "source": source,
        "sidecar": sidecar,
        "denylist": denylist,
        "manifest": manifest_path,
    }[alias]

    with pytest.raises(AnonymizationError, match="output authorization failed"):
        anonymize_file(
            source,
            output,
            fixture_id="stable",
            denylist_path=denylist,
            manifest_path=manifest_path,
            private_root=private_root,
        )


def test_cli_private_material_is_ignored_by_git() -> None:
    ignored = subprocess.run(
        ["git", "check-ignore", "--quiet", ".local/ecf-layout/private/source.txt"],
        check=False,
    )
    assert ignored.returncode == 0


def _valid_cpf(value: str) -> bool:
    if len(value) != 11 or len(set(value)) == 1:
        return False
    first = _cpf_digit(value[:9], range(10, 1, -1))
    second = _cpf_digit(value[:9] + first, range(11, 1, -1))
    return value[-2:] == first + second


def _cpf_digit(value: str, weights: range) -> str:
    digit = 11 - sum(int(char) * weight for char, weight in zip(value, weights)) % 11
    return "0" if digit >= 10 else str(digit)


def _valid_cnpj(value: str) -> bool:
    if len(value) != 14 or len(set(value)) == 1:
        return False
    first = _cnpj_digit(value[:12], (5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2))
    second = _cnpj_digit(value[:12] + first, (6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2))
    return value[-2:] == first + second


def _cnpj_digit(value: str, weights: tuple[int, ...]) -> str:
    remainder = sum(int(char) * weight for char, weight in zip(value, weights)) % 11
    return "0" if remainder < 2 else str(11 - remainder)
