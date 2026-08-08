# **4.5. Leiaute dos Registros**

# **Registro Y570: Demonstrativo do Imposto de Renda e CSLL Retidos na Fonte**

# **REGISTRO Y570: DEMONSTRATIVO DO IMPOSTO DE RENDA E CSLL RETIDOS NA FONTE**

|**Nível Hierárquico – 2**|**Ocorrência – 0:N**|

|**Nº**|**Campo**|**Descrição**|**Tipo**|**Tamanho**|**Decimal**|**Valores**<br>**Válidos**|**Obrigatório**|
|---|---|---|---|---|---|---|---|
|**1**|REG|Texto Fixo Contendo a Identificação do Registro (Y570).|C|4|-|[Y570]|Sim|
|**2**|CNPJ_FON|CNPJ da Fonte Pagadora.|C|14|-|-|Sim|
|**3**|NOM_EMP|Nome Empresarial.|C|-|-|-|Sim|
|**4**|IND_ORG_PUB|Indicador de Órgão Público.|C|1|-|-|Sim|
|**5**|COD_REC|Código da Receita.|C|4|-|-|Sim|
|**6**|VL_REND|Valor do Rendimento.|N|19|2|-|Sim|

|**Nº**|**Campo**|**Descrição**|**Tipo**|**Tamanho**|<br>**Decimal**|**Valores**<br>**Válidos**|**Obrigatório**|
|---|---|---|---|---|---|---|---|
|**7**|IR_RET|IR Retido na Fonte.|N|19|2|-|Não|
|**8**|CSLL_RET|CSLL Retida na Fonte.|N|19|2|-|Não|
|||<br>período.<br>**Tabela de Códigos de Retenção na Fonte**||||||
|**Códigos**||**Nome da Receita**|||**Órgão Públic**|**o**<br>**IRR**|**F**<br>**CSLL**|
|4085|RET CONTRIB PA|GT EST/DF/MUNIC-BENS/SERVIÇOS-CSLL/COFINS/PIS|||Sim|Nã|o<br>Sim|
