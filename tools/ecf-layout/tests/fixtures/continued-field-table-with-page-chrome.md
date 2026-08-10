# **4.5. Leiaute dos Registros**

# **Registro TEST: Registro com continuação sem cabeçalho**

**Nível Hierárquico – 2 Ocorrência – 0:N**

|**Nº**|**Campo**|**Descrição**|**Tipo**|**Tamanho**|**Decimal**|**Valores Válidos**|**Obrigatório**|
|---|---|---|---|---|---|---|---|
|**1**|REG|Identificação do registro.|C|4|-|[TEST]|Sim|
|**2**|CAMPO_INICIAL|Campo antes da quebra de página.|C|6|-|-|Sim|

<u>Atualização: janeiro/2026</u>

|103|Item de uma tabela incorporada à descrição|S|
|||599|Outro item incorporado||||||
|---|---|---|---|---|---|---|---|
|**3**|CAMPO_CONTINUADO|Campo retomado sem cabeçalho.|N|19|2|-|Sim|

<u>Atualização: janeiro/2026</u>

|**4**|CAMPO_FINAL|Campo após outro cabeçalho de página.|C|1|-|[S;N]|Não|

# **I – Regras de Validação de Campos:**

|**3**|CAMPO_CONTINUADO|**REGRA_TESTE:** não pertence à tabela de campos.|Erro|
