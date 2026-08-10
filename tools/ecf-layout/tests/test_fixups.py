from pathlib import Path

from ecf_layout.fixups import apply


FIXTURES = Path(__file__).parent / "fixtures"


def test_removes_ecf_header_and_footer_only_as_full_lines() -> None:
    markdown = (FIXTURES / "ecf-chrome.md").read_text(encoding="utf-8")

    assert apply(markdown) == """

# **4.5. Leiaute dos Registros**

# **Registro 0000: Abertura do Arquivo Digital e Identificação da Pessoa Jurídica**


Texto que menciona Página 58 de 594 fora do chrome.
"""


def test_repairs_uppercase_field_name_split_by_br() -> None:
    markdown = (FIXTURES / "broken-field.md").read_text(encoding="utf-8")

    assert apply(markdown) == """# **4.5. Leiaute dos Registros**

# **REGISTRO 0000: ABERTURA DO ARQUIVO DIGITAL E IDENTIFICAÇÃO DA PESSOA JURÍDICA**

|**Nº**|**Campo**|**Descrição**|**Tipo**|**Tamanho**|**Decimal**|**Valores Válidos**|**Obrigatório**|
|---|---|---|---|---|---|---|---|
|**6**|IND_SIT_INI_PER|Indicador do Início do Período:|N|1|**-**<br>[0; 1; 2; 3; 4]|Sim|
|**7**|NOME|Nome empresarial da pessoa jurídica.|C|-|-|Sim|
"""


def test_reconstructs_merged_field_table_header() -> None:
    markdown = (FIXTURES / "merged-header.md").read_text(encoding="utf-8")

    assert apply(markdown) == """# **4.5. Leiaute dos Registros**

# **Registro 0000: Abertura do Arquivo Digital e Identificação da Pessoa Jurídica**

|**Nº**|**Campo**|**Descrição**|**Tipo**|**Tamanho**|**Decimal**|**Valores Válidos**|**Obrigatório**|
|---|---|---|---|---|---|---|---|
|**1**|REG|Texto fixo contendo a identificação do registro(0000).|C|4|-<br>[0000]|Sim|
"""


def test_repairs_uppercase_field_name_split_across_cells() -> None:
    markdown = (FIXTURES / "cross-cell-field.md").read_text(encoding="utf-8")

    assert apply(markdown) == """|**Nº**|**Campo**|**Descrição**|**Tipo**|**Tamanho**|**Decimal**|**Valores Válidos**|**Obrigatório**|
|---|---|---|---|---|---|---|---|
|**2**|IND_AVAL_ESTOQ|Método de Avaliação do Estoque Final.|C|1|-|[1; 2; 3; 4; 5; 6; 7; 8]|Sim|
|**5**|TIPO_DESTINATARIO|Identificação se o destinatário da dedução é pessoa física ou jurídica.|C|2|-|[PF;PJ]|Sim|

Texto fora da tabela: TIPO_DESTINATARI|O<br> permanece idêntico.
"""


def test_reconstructs_header_with_number_field_and_description_merged() -> None:
    markdown = (FIXTURES / "fully-merged-field-row.md").read_text(encoding="utf-8")

    repaired = apply(markdown)

    assert repaired.count(
        "|**Nº**|**Campo**|**Descrição**|**Tipo**|**Tamanho**|**Decimal**|**Valores Válidos**|**Obrigatório**|"
    ) == 2
    assert "|**Nº**<br>**Campo**<br>**Descrição**|" not in repaired
    assert "|**5**<br>VL_SERV_SEM_ASSIST_EXT<br>Valor dos Serviços Técnicos" in repaired


def test_preserves_metadata_fused_before_field_header() -> None:
    markdown = (FIXTURES / "contaminated-field-header.md").read_text(encoding="utf-8")

    assert apply(markdown) == markdown
