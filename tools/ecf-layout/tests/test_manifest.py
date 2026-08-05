import json
from pathlib import Path

import pytest

import ecf_layout.manifest as manifest
from ecf_layout.manifest import ManifestValidationError, validate_and_promote
from ecf_layout.fragmenter import RecordFragment


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
