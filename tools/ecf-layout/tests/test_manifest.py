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

    assert record["fields"][1]["name"] == "IND_ VL_LCTO_PARTEB"
    assert record["fields"][1]["description"] == "Indicador Somatorio dos Lancamentos da Parte B."
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
