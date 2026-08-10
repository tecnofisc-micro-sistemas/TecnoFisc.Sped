import json
from pathlib import Path

import pytest

import ecf_layout.manifest as manifest
from ecf_layout.cache import sha256_file
from ecf_layout.manifest import ManifestValidationError, validate_and_promote
from ecf_layout.fragmenter import FragmentationResult, RecordFragment


FIELD_KEYS = {
    "number",
    "name",
    "description",
    "type",
    "size",
    "decimals",
    "required",
    "validValues",
}
RECORD_KEYS = {
    "code",
    "block",
    "title",
    "pageStart",
    "pageEnd",
    "level",
    "occurrence",
    "fields",
    "reviewed",
}


def test_m300_and_m350_preserve_official_dynamic_label_read_domain() -> None:
    repository = Path(__file__).resolve().parents[3]
    records = json.loads(
        (repository / "sped/ecf/layout-12-manifest.json").read_text(encoding="utf-8")
    )

    for code in ("M300", "M350"):
        record = next(item for item in records if item["code"] == code)
        field = next(item for item in record["fields"] if item["name"] == "TIPO_LANCAMENTO")
        assert field["validValues"] == "[A; E; P; R; L]"


def test_0020_position_31_preserves_layout_10_positional_compatibility() -> None:
    repository = Path(__file__).resolve().parents[3]
    records = json.loads(
        (repository / "sped/ecf/layout-12-manifest.json").read_text(encoding="utf-8")
    )

    record = next(item for item in records if item["code"] == "0020")
    field = next(item for item in record["fields"] if item["number"] == 31)
    # The V12 manifest names the current semantic. V10/V11 used the same
    # position and shape as IND_PR_TRANSF; model code exposes both aliases.
    assert field["name"] == "POSSUI_CEBRAS"
    assert field["type"] == "C"
    assert field["size"] == "1"
    assert field["required"] == "Sim"
    assert field["validValues"] == "[S;N]"
    assert field["sinceVersion"] == 10


def test_c050_preserves_the_official_ecd_i050_account_nature_domain() -> None:
    repository = Path(__file__).resolve().parents[3]
    records = json.loads(
        (repository / "sped/ecf/layout-12-manifest.json").read_text(encoding="utf-8")
    )

    domains = {}
    for code in ("C050", "J050"):
        record = next(item for item in records if item["code"] == code)
        field = next(item for item in record["fields"] if item["name"] == "COD_NAT")
        domains[code] = field["validValues"]

    assert domains == {
        "C050": "[01; 02; 03; 04; 05; 09]",
        "J050": "[01; 02; 03; 04; 05; 09]",
    }


def test_k356_preserves_the_official_debit_credit_domain() -> None:
    repository = Path(__file__).resolve().parents[3]
    records = json.loads(
        (repository / "sped/ecf/layout-12-manifest.json").read_text(encoding="utf-8")
    )

    record = next(item for item in records if item["code"] == "K356")
    field = next(item for item in record["fields"] if item["name"] == "IND_VL_SLD_FIN")
    assert field["type"] == "C"
    assert field["size"] == "1"
    assert field["validValues"] == "[D; C]"


def test_y620_and_y800_preserve_domains_documented_in_all_supported_layouts() -> None:
    repository = Path(__file__).resolve().parents[3]
    records = json.loads(
        (repository / "sped/ecf/layout-12-manifest.json").read_text(encoding="utf-8")
    )

    expected_domains = {
        ("Y620", "IND_RELAC"): "[1; 2; 3; 4; 5]",
        ("Y800", "TIPO_DOC"): "[001; 002; 003]",
    }

    actual_domains = {}
    for (code, field_name), expected_domain in expected_domains.items():
        record = next(item for item in records if item["code"] == code)
        field = next(item for item in record["fields"] if item["name"] == field_name)
        actual_domains[(code, field_name)] = field["validValues"]
        assert field["validValues"] == expected_domain

    assert actual_domains == expected_domains


def _valid_records(*, reviewed: bool = False) -> list[dict]:
    return [
        {
            "code": code,
            "block": manifest.block_for_code(code),
            "title": f"Registro {code}",
            "pageStart": position + 10,
            "pageEnd": position + 10,
            "level": "1",
            "occurrence": "1:1",
            "fields": [
                {
                    "number": 1,
                    "name": "REG",
                    "description": f"Identificacao {code}",
                    "type": "C",
                    "size": "4",
                    "decimals": "-",
                    "required": "Sim",
                    "validValues": f"[{code}]",
                }
            ],
            "reviewed": reviewed,
        }
        for position, code in enumerate(manifest.EXPECTED_CODES)
    ]


@pytest.mark.parametrize("defect", ["duplicate", "missing", "unknown"])
def test_rejects_duplicate_missing_or_unknown_record_codes(tmp_path: Path, defect: str) -> None:
    records = _valid_records(reviewed=True)
    if defect == "duplicate":
        records[-1] = dict(records[0])
    elif defect == "missing":
        records.pop()
    else:
        records[-1] = {**records[-1], "code": "ZZZZ"}
    promoted = tmp_path / "reviewed" / "manifest.json"

    with pytest.raises(ManifestValidationError):
        validate_and_promote(records, tmp_path, promote_to=promoted)

    report = json.loads((tmp_path / "quarantine.json").read_text(encoding="utf-8"))
    assert report["items"]
    assert not promoted.exists()


def test_requires_180_records_and_17_blocks_in_canonical_order(tmp_path: Path) -> None:
    records = _valid_records()
    records[manifest.EXPECTED_CODES.index("M001")]["block"] = "L"

    with pytest.raises(ManifestValidationError):
        validate_and_promote(records, tmp_path)

    assert len(manifest.EXPECTED_CODES) == 180
    assert manifest.CANONICAL_BLOCKS == ("0", "C", "E", "J", "K", "L", "M", "N", "P", "Q", "T", "U", "V", "W", "X", "Y", "9")
    assert json.loads((tmp_path / "quarantine.json").read_text(encoding="utf-8"))["items"]


def test_preserves_supported_layout_version_metadata_and_omits_defaults(tmp_path: Path) -> None:
    records = _valid_records(reviewed=True)
    records[0]["introducedIn"] = 9
    records[0]["fields"][0]["sinceVersion"] = 10

    candidate = validate_and_promote(records, tmp_path)
    payload = json.loads(candidate.read_text(encoding="utf-8"))

    assert payload[0]["introducedIn"] == 9
    assert payload[0]["fields"][0]["sinceVersion"] == 10
    assert "introducedIn" not in payload[1]
    assert "sinceVersion" not in payload[1]["fields"][0]


@pytest.mark.parametrize(
    ("location", "value"),
    [("record", 7), ("record", 13), ("field", 7), ("field", 13), ("record", True), ("field", "10")],
)
def test_rejects_unsupported_layout_version_metadata(
    tmp_path: Path, location: str, value: object
) -> None:
    records = _valid_records(reviewed=True)
    if location == "record":
        records[0]["introducedIn"] = value
    else:
        records[0]["fields"][0]["sinceVersion"] = value

    with pytest.raises(ManifestValidationError):
        validate_and_promote(records, tmp_path)

    report = json.loads((tmp_path / "quarantine.json").read_text(encoding="utf-8"))
    assert report["items"]


@pytest.mark.parametrize("numbers", [[2], [1, 3]])
def test_rejects_missing_or_non_contiguous_field_numbers(tmp_path: Path, numbers: list[int]) -> None:
    records = _valid_records()
    records[0]["fields"] = [
        {**records[0]["fields"][0], "number": number, "name": f"FIELD_{number}"}
        for number in numbers
    ]

    with pytest.raises(ManifestValidationError):
        validate_and_promote(records, tmp_path)

    assert json.loads((tmp_path / "quarantine.json").read_text(encoding="utf-8"))["items"][0]["code"] == "0000"


def test_ambiguous_record_is_quarantined_and_not_promoted(tmp_path: Path) -> None:
    records = _valid_records(reviewed=True)
    records[0]["ambiguities"] = ["multiple candidate field tables"]
    promoted = tmp_path / "reviewed" / "manifest.json"
    promoted.parent.mkdir()
    promoted.write_text("previous reviewed manifest\n", encoding="utf-8")

    with pytest.raises(ManifestValidationError):
        validate_and_promote(records, tmp_path, promote_to=promoted)

    report = json.loads((tmp_path / "quarantine.json").read_text(encoding="utf-8"))
    assert report["items"][0]["pages"] == [10]
    assert promoted.read_text(encoding="utf-8") == "previous reviewed manifest\n"


def test_promotion_uses_atomic_replace_only_after_full_validation(tmp_path: Path, monkeypatch) -> None:
    replacements: list[tuple[Path, Path]] = []
    real_replace = manifest.os.replace

    def replace(source, destination) -> None:
        replacements.append((Path(source), Path(destination)))
        real_replace(source, destination)

    monkeypatch.setattr(manifest.os, "replace", replace)
    promoted = tmp_path / "reviewed" / "manifest.json"

    result = validate_and_promote(_valid_records(reviewed=True), tmp_path, promote_to=promoted)

    payload = json.loads(promoted.read_text(encoding="utf-8"))
    assert result == promoted
    assert len(payload) == 180
    assert set(payload[0]) == RECORD_KEYS
    assert set(payload[0]["fields"][0]) == FIELD_KEYS
    assert replacements[-1][1] == promoted
    assert replacements[-1][0].parent == promoted.parent
    assert not replacements[-1][0].exists()


def test_builds_only_structural_manifest_data_from_fragment() -> None:
    fragment = RecordFragment(
        code="0000",
        block="0",
        page_start=58,
        page_end=59,
        level="0",
        occurrence="1:1",
        fields=["REG", "NOME"],
        markdown="""# **Registro 0000: Abertura do Arquivo**
|**Nº**|**Campo**|**Descrição**|**Tipo**|**Tamanho**|**Decimal**|**Valores Válidos**|**Obrigatório**|
|---|---|---|---|---|---|---|---|
|**1**|REG|Identificacao do registro.|C|4|-|[0000]|Sim|
|**2**<br>NOME<br>Nome empresarial.|C|100|-|-|Sim|
# **I - Regras de Validação dos Campos:**
|**1**|REG|**REGRA_PROIBIDA:** texto fiscal extenso.|Erro|
""",
    )

    record = manifest.record_from_fragment(fragment)

    assert record == {
        "code": "0000",
        "block": "0",
        "title": "Abertura do Arquivo",
        "pageStart": 58,
        "pageEnd": 59,
        "level": "0",
        "occurrence": "1:1",
        "fields": [
            {
                "number": 1,
                "name": "REG",
                "description": "Identificacao do registro.",
                "type": "C",
                "size": "4",
                "decimals": "-",
                "required": "Sim",
                "validValues": "[0000]",
            },
            {
                "number": 2,
                "name": "NOME",
                "description": "Nome empresarial.",
                "type": "C",
                "size": "100",
                "decimals": "-",
                "required": "Sim",
                "validValues": "-",
            },
        ],
        "reviewed": False,
    }
    assert "REGRA_PROIBIDA" not in json.dumps(record, ensure_ascii=False)


def test_parses_metadata_when_field_ordinal_is_split_across_adjacent_cells() -> None:
    fragment = RecordFragment(
        code="0001",
        block="0",
        page_start=564,
        page_end=564,
        level="1",
        occurrence="1:1",
        fields=["REG", "IND_AVAL_ESTOQ"],
        markdown="""# **Registro 0001: Abertura**
|**1**|REG|Identificacao.|C|4|-|[0001]|Sim|
|**0**|**2**<br>IND_AVAL_ESTOQ|Metodo de avaliacao.<br>C<br>1<br>-|[1;2;3]|Nao||
""",
    )

    record = manifest.record_from_fragment(fragment)

    assert record["fields"][1] == {
        "number": 2,
        "name": "IND_AVAL_ESTOQ",
        "description": "Metodo de avaliacao.",
        "type": "C",
        "size": "1",
        "decimals": "-",
        "required": "Nao",
        "validValues": "[1;2;3]",
    }
    assert "ambiguities" not in record


def test_parses_field_when_empty_valid_values_column_is_omitted() -> None:
    fragment = RecordFragment(
        code="0001",
        block="0",
        page_start=523,
        page_end=523,
        level="1",
        occurrence="1:1",
        fields=["REG", "CNPJ_INCORP"],
        markdown="""# **Registro 0001: Abertura**
|**1**|REG|Identificacao.|C|4|-|[0001]|Sim|
|**2**|CNPJ_INCORP|Inscricao da incorporacao afetada.|C|014|-|Não|
""",
    )

    record = manifest.record_from_fragment(fragment)

    assert record["fields"][1] == {
        "number": 2,
        "name": "CNPJ_INCORP",
        "description": "Inscricao da incorporacao afetada.",
        "type": "C",
        "size": "014",
        "decimals": "-",
        "required": "Não",
        "validValues": "",
    }
    assert "ambiguities" not in record


def test_parses_field_when_empty_decimal_column_is_omitted() -> None:
    fragment = RecordFragment(
        code="0001",
        block="0",
        page_start=525,
        page_end=525,
        level="1",
        occurrence="1:1",
        fields=["REG", "DT_VIGENCIA"],
        markdown="""# **Registro 0001: Abertura**
|**1**|REG|Identificacao.|C|4|-|[0001]|Sim|
|**2**|DT_VIGENCIA|Data no formato DD/MM/AAAA.|N|008|DDMMAAAA|Não|
""",
    )

    record = manifest.record_from_fragment(fragment)

    assert record["fields"][1] == {
        "number": 2,
        "name": "DT_VIGENCIA",
        "description": "Data no formato DD/MM/AAAA.",
        "type": "N",
        "size": "008",
        "decimals": "",
        "required": "Não",
        "validValues": "DDMMAAAA",
    }
    assert "ambiguities" not in record


def test_parses_numeric_decimal_when_empty_valid_values_column_is_omitted() -> None:
    fragment = RecordFragment(
        code="0001",
        block="0",
        page_start=525,
        page_end=525,
        level="1",
        occurrence="1:1",
        fields=["REG", "VALOR"],
        markdown="""# **Registro 0001: Abertura**
|**1**|REG|Identificacao.|C|4|-|[0001]|Sim|
|**2**|VALOR|Valor monetario.|N|019|2|Não|
""",
    )

    record = manifest.record_from_fragment(fragment)

    assert record["fields"][1]["decimals"] == "2"
    assert record["fields"][1]["validValues"] == ""
    assert "ambiguities" not in record


@pytest.mark.parametrize("metadata", ["2A", "DMAA", "DMMAAA", "DDMAA"])
def test_flags_ambiguous_seven_cell_metadata_instead_of_guessing_column(
    metadata: str,
) -> None:
    fragment = RecordFragment(
        code="0001",
        block="0",
        page_start=525,
        page_end=525,
        level="1",
        occurrence="1:1",
        fields=["REG", "VALOR"],
        markdown=f"""# **Registro 0001: Abertura**
|**1**|REG|Identificacao.|C|4|-|[0001]|Sim|
|**2**|VALOR|Metadado sem coluna identificavel.|N|019|{metadata}|Não|
""",
    )

    record = manifest.record_from_fragment(fragment)

    assert [field["name"] for field in record["fields"]] == ["REG"]
    assert record["ambiguities"] == [
        f"field 2 has ambiguous seven-cell metadata {metadata!r}",
        "expected 2 structured fields, parsed 1",
    ]


def test_ambiguous_field_with_continuation_and_rules_stays_quarantined() -> None:
    fragment = RecordFragment(
        code="0001",
        block="0",
        page_start=525,
        page_end=526,
        level="1",
        occurrence="1:1",
        fields=["REG", "VALOR"],
        markdown="""# **Registro 0001: Abertura**
|**1**|REG|Identificacao.|C|4|-|[0001]|Sim|
|**2**|VALOR|Metadado sem coluna identificavel.|N|019|2A|Não|
| | |Continuação que não pode ser anexada a um campo descartado.| | | | |
**Regras de Validação**
|**2**|VALOR|Linha estrutural de uma regra posterior.|N|019|2|Não|
""",
    )

    record = manifest.record_from_fragment(fragment)

    assert [field["name"] for field in record["fields"]] == ["REG"]
    assert record["ambiguities"] == [
        "field 2 has ambiguous seven-cell metadata '2A'",
        "expected 2 structured fields, parsed 1",
    ]


def test_does_not_treat_packed_size_and_decimal_cells_as_omitted_column() -> None:
    fragment = RecordFragment(
        code="0001",
        block="0",
        page_start=58,
        page_end=58,
        level="1",
        occurrence="1:1",
        fields=["REG"],
        markdown="""# **Registro 0001: Abertura**
|**1**|REG|Identificacao.|C|4<br>-|[0001]|Sim|
""",
    )

    record = manifest.record_from_fragment(fragment)

    assert record["fields"][0] == {
        "number": 1,
        "name": "REG",
        "description": "Identificacao.",
        "type": "C",
        "size": "4",
        "decimals": "-",
        "required": "Sim",
        "validValues": "[0001]",
    }
    assert "ambiguities" not in record


def test_keeps_conditional_required_marker_in_its_column() -> None:
    fragment = RecordFragment(
        code="0001",
        block="0",
        page_start=67,
        page_end=67,
        level="1",
        occurrence="1:1",
        fields=["REG", "DETALHE"],
        markdown="""# **Registro 0001: Abertura**
|**1**|REG|Identificacao.|C|4|-|[0001]|Sim|
|**2**|DETALHE|Numero do certificado.|C|255|-||OC<br>Deve ser preenchido caso o indicador seja igual a “S –<br>Sim”|
""",
    )

    record = manifest.record_from_fragment(fragment)

    assert record["fields"][1] == {
        "number": 2,
        "name": "DETALHE",
        "description": (
            "Numero do certificado. Deve ser preenchido caso o indicador seja igual a “S – Sim”"
        ),
        "type": "C",
        "size": "255",
        "decimals": "-",
        "required": "OC",
        "validValues": "",
    }


def test_preserves_normative_lowercase_required_marker_for_w300() -> None:
    fragment = RecordFragment(
        code="W300",
        block="W",
        page_start=436,
        page_end=439,
        level="2",
        occurrence="0:N",
        fields=["REG", "FIM_OBSERVACAO"],
        markdown="""# **Registro W300: Observacoes**
|**1**|REG|Identificacao.|C|4|-|[W300]|Sim|
|**2**|FIM_OBSERVACAO|Indicador de fim.|C|7|-|[W300FIM]|sim|
""",
    )

    record = manifest.record_from_fragment(fragment)

    assert record["fields"][1]["required"] == "sim"


def test_quarantines_unknown_required_marker_before_promotion(tmp_path: Path) -> None:
    records = _valid_records(reviewed=True)
    records[0]["fields"][0]["required"] = "Sim”"

    with pytest.raises(ManifestValidationError):
        validate_and_promote(records, tmp_path, promote_to=tmp_path / "reviewed.json")

    report = json.loads((tmp_path / "quarantine.json").read_text(encoding="utf-8"))
    assert report["items"][0]["code"] == "0000"
    assert report["items"][0]["reasons"] == [
        "field 1 required must be one of: Sim, sim, S, Não, N, -, OC"
    ]


def test_keeps_field_row_when_rules_table_is_fused_into_that_row() -> None:
    fragment = RecordFragment(
        code="Y620",
        block="Y",
        page_start=555,
        page_end=555,
        level="2",
        occurrence="0:N",
        fields=["REG", "NUM_PROC_RFB"],
        markdown="""# **Registro Y620: Participacoes**
|**1**|REG|Identificacao.|C|4|-|[Y620]|Sim|
|**2**<br>NUM_PROC_RFB<br>**I - Regras de Validacao do Regi**<br>**REGRA_FUNDIDA**|Numero do Processo.<br>**stro:**<br>texto de regra|C<br>CONTAMINACAO|-<br>REGRA|-<br>REGRA|-<br>REGRA|Nao<br>REGRA|
""",
    )

    record = manifest.record_from_fragment(fragment)

    assert record["fields"][1] == {
        "number": 2,
        "name": "NUM_PROC_RFB",
        "description": "Numero do Processo.",
        "type": "C",
        "size": "-",
        "decimals": "-",
        "required": "Nao",
        "validValues": "-",
    }
    assert "ambiguities" not in record
    assert "REGRA_FUNDIDA" not in json.dumps(record, ensure_ascii=False)


def test_recovers_metadata_when_pdf_cells_are_merged_or_split() -> None:
    fragment = RecordFragment(
        code="0001",
        block="0",
        page_start=67,
        page_end=67,
        level="1",
        occurrence="1:1",
        fields=["REG", "IND_DAD"],
        markdown="""# **Registro 0001: Abertura**
|**1**|REG|Texto fixo contendo a iden|tificacao do registro.<br>C|4|-|[0001]|Sim|
|**2**|IND_DAD|Indicador de movimento.|N<br>1|-|[0;1]|Sim|
""",
    )

    record = manifest.record_from_fragment(fragment)

    assert [(field["type"], field["size"], field["decimals"], field["validValues"], field["required"]) for field in record["fields"]] == [
        ("C", "4", "-", "[0001]", "Sim"),
        ("N", "1", "-", "[0;1]", "Sim"),
    ]
    assert record["fields"][0]["description"] == "Texto fixo contendo a iden tificacao do registro."


@pytest.mark.parametrize(
    "defect",
    ["missing_record_key", "unknown_record_key", "wrong_record_type", "missing_field_key", "unknown_field_key", "wrong_field_type"],
)
def test_rejects_invalid_exact_schemas_before_candidate_or_promotion_write(
    tmp_path: Path, defect: str
) -> None:
    records = _valid_records(reviewed=True)
    if defect == "missing_record_key":
        del records[0]["title"]
    elif defect == "unknown_record_key":
        records[0]["markdown"] = "must not be accepted"
    elif defect == "wrong_record_type":
        records[0]["reviewed"] = "true"
    elif defect == "missing_field_key":
        del records[0]["fields"][0]["description"]
    elif defect == "unknown_field_key":
        records[0]["fields"][0]["ruleText"] = "must not be accepted"
    else:
        records[0]["fields"][0]["number"] = "1"
    candidate = tmp_path / "candidate" / "layout-12-manifest.json"
    candidate.parent.mkdir(parents=True)
    candidate.write_text("previous candidate\n", encoding="utf-8")
    promoted = tmp_path / "reviewed" / "manifest.json"
    promoted.parent.mkdir()
    promoted.write_text("previous reviewed manifest\n", encoding="utf-8")

    with pytest.raises(ManifestValidationError):
        validate_and_promote(records, tmp_path, promote_to=promoted)

    report = json.loads((tmp_path / "quarantine.json").read_text(encoding="utf-8"))
    assert report["items"]
    assert candidate.read_text(encoding="utf-8") == "previous candidate\n"
    assert promoted.read_text(encoding="utf-8") == "previous reviewed manifest\n"


@pytest.mark.parametrize(
    "defect",
    ["list_code", "integer_ambiguities", "object_field_type", "numeric_record_key", "numeric_field_key"],
)
def test_hostile_json_values_become_current_quarantine_evidence(
    tmp_path: Path, defect: str
) -> None:
    records = _valid_records(reviewed=True)
    if defect == "list_code":
        records[0]["code"] = []
    elif defect == "integer_ambiguities":
        records[0]["ambiguities"] = 7
    elif defect == "object_field_type":
        records[0]["fields"][0]["type"] = {}
    elif defect == "numeric_record_key":
        records[0][7] = "hostile key"
    else:
        records[0]["fields"][0][7] = "hostile key"
    (tmp_path / "quarantine.json").write_text(
        json.dumps({"items": [{"code": "OLD", "reasons": ["stale failure"], "pages": [99]}]}),
        encoding="utf-8",
    )
    candidate = tmp_path / "candidate" / "layout-12-manifest.json"
    candidate.parent.mkdir(parents=True)
    candidate.write_text("previous candidate\n", encoding="utf-8")
    promoted = tmp_path / "reviewed" / "manifest.json"
    promoted.parent.mkdir()
    promoted.write_text("previous reviewed manifest\n", encoding="utf-8")

    with pytest.raises(ManifestValidationError):
        validate_and_promote(records, tmp_path, promote_to=promoted)

    report = json.loads((tmp_path / "quarantine.json").read_text(encoding="utf-8"))
    assert report["items"]
    assert all("stale failure" not in reason for item in report["items"] for reason in item["reasons"])
    assert candidate.read_text(encoding="utf-8") == "previous candidate\n"
    assert promoted.read_text(encoding="utf-8") == "previous reviewed manifest\n"


def test_rejects_non_array_manifest_root_with_current_quarantine(tmp_path: Path) -> None:
    (tmp_path / "quarantine.json").write_text(
        json.dumps({"items": [{"code": "OLD", "reasons": ["stale failure"], "pages": [99]}]}),
        encoding="utf-8",
    )

    with pytest.raises(ManifestValidationError):
        validate_and_promote(None, tmp_path)  # type: ignore[arg-type]

    assert json.loads((tmp_path / "quarantine.json").read_text(encoding="utf-8")) == {
        "items": [{"code": None, "reasons": ["manifest root must be an array"], "pages": []}]
    }


def test_reconstructs_split_uppercase_field_name_suffix_without_record_exception() -> None:
    fragment = RecordFragment(
        code="M500",
        block="M",
        page_start=267,
        page_end=267,
        level="3",
        occurrence="0:N",
        fields=["REG", "IND_ VL_LCTO_PARTEB"],
        markdown="""# **Registro M500: Controle de Saldos**
|**1**|REG|Identificacao.|C|4|-|[M500]|Sim|
|**2**|IND_ VL_LCTO_PARTE<br>B|Indicador Somatorio dos Lancamentos da Parte B.|C|1|-|[C; D]|Sim|
""",
    )

    record = manifest.record_from_fragment(fragment)

    assert record["fields"][1]["name"] == "IND_VL_LCTO_PARTEB"
    assert record["fields"][1]["description"] == "Indicador Somatorio dos Lancamentos da Parte B."
    assert "ambiguities" not in record


def test_normalizes_hyphenated_ecommerce_field_name_to_valid_identifier() -> None:
    """0020.18 e "IND_E-COM_TI" no manual (pag. 81) - hifen deliberado do RFB,
    nao ruido de extracao, mas invalido como identificador (IsFieldNameValid).
    A reextracao a partir do PDF deve normalizar para "IND_E_COM_TI" sem
    depender de edicao manual do manifesto gerado."""
    fragment = RecordFragment(
        code="0020",
        block="0",
        page_start=77,
        page_end=91,
        level="0",
        occurrence="1:1",
        fields=["REG", "IND_E-COM_TI"],
        markdown="""# **Registro 0020: Parâmetros Complementares**
|**1**|REG|Identificacao.|C|4|-|[0020]|Sim|
|**2**|IND_E-COM_TI|Comércio Eletrônico e Tecnologia da Informação.|C|1|-|[S;N]|Sim|
""",
    )

    record = manifest.record_from_fragment(fragment)

    assert record["fields"][1]["name"] == "IND_E_COM_TI"
    assert "ambiguities" not in record


def test_normalizes_composite_nif_cnpj_field_name_to_valid_identifier() -> None:
    """X357.3 e "NIF/CNPJ" no manual (pag. 474) - nome composto legitimo do
    RFB porque o campo aceita NIF ou CNPJ conforme o pais da investidora, mas
    a barra e invalida como identificador. Precisa normalizar para
    "NIF_CNPJ" na reextracao, nao so no JSON promovido a mao."""
    fragment = RecordFragment(
        code="X357",
        block="X",
        page_start=474,
        page_end=476,
        level="3",
        occurrence="0:N",
        fields=["REG", "PAIS", "NIF/CNPJ", "RAZAO_SOCIAL", "PERCENTUAL"],
        markdown="""# **Registro X357: Investidoras Diretas**
|**1**|REG|Identificacao.|C|4|-|[X357]|Sim|
|**2**|PAIS|Pais de cada investidora direta.|N|3|-|-|Sim|
|**3**|NIF/CNPJ|NIF ou CNPJ da investidora direta.|C|-|-|-|Sim|
|**4**|RAZAO_SOCIAL|Razao social da investidora direta.|C|-|-|-|Sim|
|**5**|PERCENTUAL|Percentual de participacao.|N|8|4|-|Sim|
""",
    )

    record = manifest.record_from_fragment(fragment)

    assert record["fields"][2]["name"] == "NIF_CNPJ"
    assert "ambiguities" not in record


def test_title_stops_at_first_visual_line_when_picture_text_fuses_narrative() -> None:
    fragment = RecordFragment(
        code="Y730",
        block="Y",
        page_start=577,
        page_end=581,
        level="2",
        occurrence="0:N",
        fields=["REG"],
        markdown="""Registro Y730: Identificação de donatários/destinatários<br>Texto narrativo que não pertence ao título.<br><!-- End of picture text -->
|**1**|REG|Identificação do registro.|C|4|-|[Y730]|Sim|
""",
    )

    record = manifest.record_from_fragment(fragment)

    assert record["title"] == "Identificação de donatários/destinatários"


def test_cache_selection_is_bound_to_supplied_pdf_hash(tmp_path: Path, monkeypatch) -> None:
    pdf = tmp_path / "manual.pdf"
    pdf.write_bytes(b"normative manual")
    work_dir = tmp_path / "work"
    fragments_dir = work_dir / "fragments"
    fragments_dir.mkdir(parents=True)
    fragments = []
    for code in manifest.EXPECTED_CODES:
        (fragments_dir / f"{code}.md").write_text("fragment\n", encoding="utf-8")
        fragments.append(RecordFragment(code, None, 1, 1, "1", "1:1", ["REG"], "fragment\n"))
    correct_cache = work_dir / "cache" / sha256_file(pdf) / "converter"
    correct_cache.mkdir(parents=True)
    (correct_cache / "page-0001.md").write_text("correct source", encoding="utf-8")
    wrong_cache = work_dir / "cache" / ("0" * 64) / "newer-converter"
    wrong_cache.mkdir(parents=True)
    (wrong_cache / "page-0001.md").write_text("wrong source", encoding="utf-8")
    seen_pages: list[str] = []

    def fragment_pages(pages) -> FragmentationResult:
        seen_pages.extend(markdown for _page, markdown in pages)
        return FragmentationResult(fragments, [])

    monkeypatch.setattr(manifest, "fragment_pages_with_errors", fragment_pages)
    monkeypatch.setattr(manifest, "record_from_fragment", lambda fragment: {"code": fragment.code})

    records = manifest.records_from_work_dir(work_dir, pdf)

    assert len(records) == 180
    assert seen_pages == ["correct source"]
