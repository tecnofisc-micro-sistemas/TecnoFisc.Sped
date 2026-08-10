# **4.5. Leiaute dos Registros**

**Registro Y682: Informações de Optantes pelo Refis – Imunes ou Isentas**

# **REGISTRO Y682: INFORMAÇÕES DE OPTANTES PELO REFIS (IMUNES OU ISENTAS)**

**Nível Hierárquico – 2 Ocorrência – 0:12**

|**Nº**|**Campo**|**Descrição**<br>**Tipo**|**Tamanho**|<br>**Decimal**|<br>**Válidos**|**Obrigatório**||
|**1**|REG|Texto Fixo Contendo a Identificação do Registro (Y682).<br>C|4|-|[Y682]|Sim||
|**2**|MES|Mês.<br>C|2|-|[01; 02; 03; 04; 05; 06; 07; 08; 09; 10; 11; 12]|Sim||
|**3**|ACRES_PATR|Acréscimo Patrimonial no Mês.<br>N|19|2|**-**|Sim||
|**I – Regra**<br>**Nº **|**s de Validação de Ca**<br>**Campo**|**mpos:**<br>**Regras de Validação do Campo**|||||**Tipo**|
|**2**<br>M|ES|**REGRA_MES_FORA_PERIODO:**Verifica se Y682.MES está compreendido no período|informado e|ntre 0000.D|T_INI e 0000.D|T_FIN.|Erro|
