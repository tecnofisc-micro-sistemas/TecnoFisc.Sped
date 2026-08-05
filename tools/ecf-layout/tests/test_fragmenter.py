from pathlib import Path

from ecf_layout.fragmenter import RecordFragment, fragment_pages, fragment_pages_with_errors, write_fragments


FIXTURES = Path(__file__).parent / "fixtures"


def test_ignores_summary_occurrence_and_keeps_detailed_section() -> None:
    markdown = (FIXTURES / "summary-duplicate.md").read_text(encoding="utf-8")

    assert fragment_pages([(12, markdown)]) == [
        RecordFragment("0000", None, 12, 12, "0", "1:1", ["REG"], """# **Registro 0000: Abertura do Arquivo Digital e Identificação da Pessoa Jurídica**

# **REGISTRO 0000: ABERTURA DO ARQUIVO DIGITAL E IDENTIFICAÇÃO DA PESSOA JURÍDICA**

||**Nível Hierárquico – 0**|||**Ocorrência – 1:1**||

|**Nº**|**Campo**|**Descrição**|**Tipo**|**Tamanho**|**Decimal**|**Valores Válidos**|**Obrigatório**|
|---|---|---|---|---|---|---|---|
|**1**|REG|Texto fixo contendo a identificação do registro(0000).|C|4|-<br>[0000]|Sim|
"""),
        RecordFragment("0001", None, 12, 12, "1", "1:1", ["REG"], """# **Registro 0001: Abertura do Bloco 0**

||**Nível Hierárquico – 1**|||**Ocorrência – 1:1**||

|**Nº**|**Campo**|**Descrição**|**Tipo**|**Tamanho**|**Decimal**|**Valores Válidos**|**Obrigatório**|
|---|---|---|---|---|---|---|---|
|**1**|REG|Texto fixo contendo a identificação do registro (0001).|C|4|-|[0001]|Sim|
"""),
    ]


def test_fragment_ends_immediately_before_next_detailed_record() -> None:
    markdown = (FIXTURES / "summary-duplicate.md").read_text(encoding="utf-8")

    assert [fragment.markdown for fragment in fragment_pages([(12, markdown)])] == [
        """# **Registro 0000: Abertura do Arquivo Digital e Identificação da Pessoa Jurídica**

# **REGISTRO 0000: ABERTURA DO ARQUIVO DIGITAL E IDENTIFICAÇÃO DA PESSOA JURÍDICA**

||**Nível Hierárquico – 0**|||**Ocorrência – 1:1**||

|**Nº**|**Campo**|**Descrição**|**Tipo**|**Tamanho**|**Decimal**|**Valores Válidos**|**Obrigatório**|
|---|---|---|---|---|---|---|---|
|**1**|REG|Texto fixo contendo a identificação do registro(0000).|C|4|-<br>[0000]|Sim|
""",
        """# **Registro 0001: Abertura do Bloco 0**

||**Nível Hierárquico – 1**|||**Ocorrência – 1:1**||

|**Nº**|**Campo**|**Descrição**|**Tipo**|**Tamanho**|**Decimal**|**Valores Válidos**|**Obrigatório**|
|---|---|---|---|---|---|---|---|
|**1**|REG|Texto fixo contendo a identificação do registro (0001).|C|4|-|[0001]|Sim|
""",
    ]


def test_accepts_plain_bold_detailed_record_title() -> None:
    markdown = (FIXTURES / "plain-record-title.md").read_text(encoding="utf-8")

    assert [fragment.code for fragment in fragment_pages([(273, markdown)])] == ["N030"]


def test_accepts_record_title_from_picture_text() -> None:
    markdown = (FIXTURES / "picture-text-record-title.md").read_text(encoding="utf-8")

    assert [fragment.code for fragment in fragment_pages([(577, markdown)])] == ["Y730"]


def test_merges_continued_field_table_headers() -> None:
    markdown = (FIXTURES / "continued-field-table.md").read_text(encoding="utf-8")

    assert fragment_pages([(122, markdown)])[0].fields == ["REG", "DT_INI", "DT_FIN", "PER_APUR"]


def test_ignores_reference_table_after_detailed_fields() -> None:
    markdown = (FIXTURES / "field-and-reference-table.md").read_text(encoding="utf-8")

    assert fragment_pages([(539, markdown)])[0].fields == [
        "REG",
        "CNPJ_FON",
        "NOM_EMP",
        "IND_ORG_PUB",
        "COD_REC",
        "VL_REND",
        "IR_RET",
        "CSLL_RET",
    ]


def test_ignores_fused_rules_table_after_detailed_fields() -> None:
    markdown = (FIXTURES / "fused-rules-table.md").read_text(encoding="utf-8")

    assert fragment_pages([(572, markdown)])[0].fields == ["REG", "MES", "ACRES_PATR"]


def test_quarantines_restarted_competing_field_table() -> None:
    markdown = (FIXTURES / "competing-field-tables.md").read_text(encoding="utf-8")

    result = fragment_pages_with_errors([(100, markdown)])

    assert result.fragments == []
    assert result.errors == ["record TEST: competing field tables"]


def test_quarantines_field_ordinal_gap_after_table_interruption() -> None:
    markdown = """# **4.5. Leiaute dos Registros**

# **Registro TEST: Registro interrompido**

||**Nível Hierárquico – 1**|||**Ocorrência – 1:1**||

|**Nº**|**Campo**|**Descrição**|**Tipo**|**Tamanho**|**Decimal**|**Valores Válidos**|**Obrigatório**|
|---|---|---|---|---|---|---|---|
|**1**|REG|Identificação do registro.|C|4|-|[TEST]|Sim|

|**3**|CAMPO_3|Campo depois da interrupção.|C|10|-|-|Não|
"""

    result = fragment_pages_with_errors([(100, markdown)])

    assert result.fragments == []
    assert result.errors == ["record TEST: non-contiguous field table at 3"]


def test_recovers_metadata_split_across_table_cells() -> None:
    markdown = (FIXTURES / "split-metadata.md").read_text(encoding="utf-8")

    fragment = fragment_pages([(407, markdown)])[0]

    assert (fragment.level, fragment.occurrence) == ("2", "0:1")


def test_recovers_occurrence_split_by_interleaved_cell_text() -> None:
    markdown = (FIXTURES / "interleaved-metadata.md").read_text(encoding="utf-8")

    fragment = fragment_pages([(513, markdown)])[0]

    assert (fragment.level, fragment.occurrence) == ("3", "0:N")


def test_recovers_occurrence_label_with_one_lost_letter() -> None:
    markdown = (FIXTURES / "truncated-occurrence-label.md").read_text(encoding="utf-8")

    fragment = fragment_pages([(523, markdown)])[0]

    assert (fragment.level, fragment.occurrence) == ("2", "0:N")


def test_extracts_field_from_number_field_and_description_merged_cell() -> None:
    markdown = (FIXTURES / "fully-merged-field-row.md").read_text(encoding="utf-8")

    fragment = fragment_pages([(508, markdown)])[0]

    assert fragment.fields == [
        "REG",
        "VL_SERV_ASSIST_BR",
        "VL_SERV_SEM_ASSIST_BR",
        "VL_SERV_ASSIST_EXT",
        "VL_SERV_SEM_ASSIST_EXT",
    ]


def test_quarantines_duplicate_detailed_record_code() -> None:
    markdown = (FIXTURES / "duplicate-detailed-code.md").read_text(encoding="utf-8")

    result = fragment_pages_with_errors([(200, markdown)])

    assert result.fragments == []
    assert result.errors == ["record DUPL: duplicate detailed sections"]


def test_write_fragments_removes_stale_quarantined_record(tmp_path: Path) -> None:
    directory = tmp_path / "fragments"
    directory.mkdir()
    (directory / "STALE.md").write_text("stale\n", encoding="utf-8")
    fragment = RecordFragment("0000", "0", 58, 58, "0", "1:1", ["REG"], "# Registro 0000\n")

    write_fragments([fragment], directory)

    assert sorted(path.name for path in directory.glob("*.md")) == ["0000.md"]
