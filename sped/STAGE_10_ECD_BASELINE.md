# Stage 10 — ECD baseline (sub-stage decomposition)

> Operational appendix to `ARCHITECTURE.md` §10. **Scope: ECD baseline leiaute 9 only** (project `TecnoFisc.Sped.Ecd`, **read-only**). Each row below is one sub-stage of Stage 10.
>
> **Leiaute 9, vigente a partir do ano-calendário 2020** (Capítulo 3 do manual: "Leiaute 9 – A partir do Ano-Calendário 2020"). Source of truth: `sped/guides/Manual_de_Orientação_da_ECD_Leiaute_20_maio_2026.pdf` (Anexo ao Ato Declaratório Executivo Cofis nº 01/2026, atualização maio/2026). Encoding do arquivo `.txt`: **ASCII ISO-8859-1 (Latin-1)** — manual §2 (não aceita packed decimal, EBCDIC, etc.).
>
> **Versão do leiaute ≠ campo do `0000`.** Diferente de EFD, o `Registro0000` da ECD **não** carrega `COD_VER` — seu campo 02 é o literal `"LECD"`. O código da versão do leiaute mora em **`I010.COD_VER_LC`** (valor `"9.00"` para AC2024+). `LayoutEcd` enum (`V009 = 9`) é informacional (read-only, `ARCHITECTURE.md` §4.7) — não filtra parse.
>
> **Sem incrementos.** A Receita não publicou leiaute posterior ao 9 (vigente desde AC2020). Stage 11 (incrementos ECD) fica em standby até surgir um leiaute 10. Existe **uma única modelagem** = leiaute 9.
>
> Registros aqui são **independentes** dos homônimos em EFD Contribuições / EFD ICMS-IPI. `Registro0150` da ECD e o `Registro0150` do EFD ICMS-IPI são duas classes distintas em assemblies diferentes. Compartilhamento só via `TecnoFisc.Sped.Core` (value objects fiscais transversais — `Cnpj`, `Cpf`, `InscricaoEstadual`, `CodigoMunicipio` já existem desde Stage 1).

## Modo de operação (read-only)

- Pacote expõe apenas **parser + modelo tipado**. Não existe `GeradorEcd`, não existe pasta `Gerador/`, não existem testes de round-trip parse→generate→parse — apenas fixture-load + asserts sobre o modelo lido (`ARCHITECTURE.md` §2.5).
- Estrutura interna = `TecnoFisc.Sped.EfdIcmsIpi` **menos `Gerador/`**: `Registros/Bloco{0,C,I,J,K,9}/`, `Enums/`, `Versionamento/LayoutEcd.cs`, `Parser/ParserEcd.cs`, `ArquivoEcd.cs`.
- Migrar para read+write no futuro = stage dedicada (ativa gerador + round-trip). Não criar gerador sem promover oficialmente.

## How to use this document

1. Pick the next un-implemented sub-stage (smallest `10.NNN` com status `[ ]`).
2. Abra `sped/guides/Manual_de_Orientação_da_ECD_Leiaute_20_maio_2026.pdf` direto na página listada — não leia o manual inteiro. Use `Read` com `pages` (e.g., `pages: "117-122"` para `RegistroI050`). O **índice de página do PDF = página impressa** (rodapé "Página N de 236"), sem offset. Alguns registros ocupam várias páginas; leia até o próximo header `Registro NNNN`.
3. Implemente a classe sob a pasta do bloco correto (`Registros/Bloco0/`, `Registros/BlocoI/`, etc.) seguindo `ARCHITECTURE.md` (Portuguese para domínio, `partial class`, `[RegistroSped]`/`[CampoSped]`).
4. Tests no projeto `tests/TecnoFisc.Sped.Ecd.Tests/`: validação de campos + fixture-load do exemplo do manual. **Sem round-trip** (read-only).
5. PR com prefixo `feat:` e corpo em português. Ver **PR granularity** abaixo para quando agrupar sub-stages.

As demais convenções operacionais (o que a página do manual te diz além da tabela de campos — nível, ocorrência, regras, severidade erro/advertência; enums e value objects on-demand com first-use; naming PT/EN) são **idênticas** às de `STAGE_8_EFD_ICMS_IPI_V015.md` — consultar lá, não duplicado aqui.

## Bootstrap (primeiras sub-stages)

O projeto `TecnoFisc.Sped.Ecd` ainda não existe. As primeiras sub-stages bootstrapam a infra mínima (igual Stage 4 fez para EFD Contribuições):

- `10.001` (Registro 0000) cria: projeto `src/TecnoFisc.Sped.Ecd/` + `tests/TecnoFisc.Sped.Ecd.Tests/`, registro no `.slnx`, `Versionamento/LayoutEcd.cs` (`V009 = 9`), `Parser/ParserEcd.cs` + `ArquivoEcd.cs` mínimos, wiring no catálogo/`LeitorSpedTxt` do Core.
- Sub-stages seguintes só acrescentam classes de registro + estendem `ArquivoEcd`/catálogo.

## Particularidades da ECD (respeitar na implementação)

- **`Registro0000` campo 02 = `"LECD"` literal** (texto fixo), não versão. É o discriminador do sniffer (Stage 12) para reconhecer "isto é ECD" pela primeira linha. A versão (`COD_VER_LC`) só aparece em `I010`.
- **`IND_ESC` (campo 02 do `I010`, valores `G`/`R`/`A`/`B`/`Z`) = forma da escrituração contábil.** Dirige a obrigatoriedade condicional de vários registros e regras cruzadas (análogo ao `PERFIL` do EFD ICMS-IPI). Por `ARCHITECTURE.md` §2.3 → **responsabilidade do consumidor**: só doc-comment XML descrevendo a regra, **sem código de validação** e sem pasta `Validadores/`.
- **Bloco C = informações recuperadas da ECD anterior.** O manual diz que "não precisam ser importados" (preenchidos pelo PGE do Sped Contábil após recuperação). Mesmo assim **devem ser parseáveis** no read path — implementar normalmente.
- **Regras `REGRA_*` nomeadas no manual** (e.g., `REGRA_VALIDA_CNPJ`, `REGRA_BATE_SALDO`) → tratadas como `UPDATE/Doc`: viram doc-comment XML para o consumidor, sem validador (§2.3). Formato malformado (tipo/tamanho/obrigatório ausente) continua sendo erro de parse.
- **Drift interno do manual:** o registro **`C052`** aparece na seção de detalhe 3.6 (p94) mas foi **omitido** na Tabela de Registros resumida (3.2). A Seção 3.6 (Leiaute dos Registros) é a fonte autoritativa de ordem e existência — `C052` entra (por isso o Bloco C tem 10 registros, não 9). Se outras divergências 3.2 × 3.6 aparecerem, **3.6 vence**.

## PR granularity

Default: **one sub-stage per PR** (qualquer registro com filhos hierárquicos, campos condicionais, value-object, ou bloco "Regras de Validação"). **Batching permitido** (cap ~10) para aberturas (`X001`), encerramentos (`X990`/`9990`/`9999`) e leaf triviais contíguos no mesmo bloco — mesmos critérios de `STAGE_8_EFD_ICMS_IPI_V015.md`.

## Convenções para estes sub-stages

- **Classe:** `Registro<CODE>` (e.g., `Registro0000`, `RegistroI050`, `RegistroJ100`, `Registro9999`).
- **Pasta:** `src/TecnoFisc.Sped.Ecd/Registros/Bloco<X>/` onde `<X>` ∈ {`0`,`C`,`I`,`J`,`K`,`9`}.
- **Layout version (baseline):** todas as linhas são V009. `[CampoSped]`/`[RegistroSped]` sem `DesdeVersao`/`IntroduzidoEm` (zero = baseline; não há incrementos).
- **Encerramento (`X990`, `9990`, `9999`):** contadores triviais; elegíveis para batching.

## Sub-stages

Block headers são informacionais; numeração é global e contígua (`10.001` → `10.072`). Colunas: `Nível` e `Ocorrência` conforme Tabela 3.2 / detalhe 3.6 do manual.

### Bloco 0 — Abertura, Identificação e Referências (8 registros)

| Feito | Sub-stage | Registro | Descrição | Nível | Ocorrência | Página PDF |
| --- | --- | --- | --- | --- | --- | --- |
| [ ] | 10.001 | Registro 0000 | Abertura do Arquivo Digital e Identificação do Empresário ou da Sociedade Empresária | 0 | 1:1 | 64 |
| [ ] | 10.002 | Registro 0001 | Abertura do Bloco 0 | 1 | 1 | 75 |
| [ ] | 10.003 | Registro 0007 | Outras Inscrições Cadastrais da Pessoa Jurídica | 2 | 0:N | 76 |
| [ ] | 10.004 | Registro 0020 | Escrituração Contábil Descentralizada | 2 | 0:N | 78 |
| [ ] | 10.005 | Registro 0035 | Identificação das SCP | 2 | 0:N | 81 |
| [ ] | 10.006 | Registro 0150 | Tabela de Cadastro do Participante | 2 | 0:N | 82 |
| [ ] | 10.007 | Registro 0180 | Identificação do Relacionamento com o Participante | 3 | 1:N | 85 |
| [ ] | 10.008 | Registro 0990 | Encerramento do Bloco 0 | 1 | 1 | 87 |

### Bloco C — Informações Recuperadas da Escrituração Contábil Anterior (10 registros)

| Feito | Sub-stage | Registro | Descrição | Nível | Ocorrência | Página PDF |
| --- | --- | --- | --- | --- | --- | --- |
| [ ] | 10.009 | Registro C001 | Abertura do Bloco C | 1 | 1 | 88 |
| [ ] | 10.010 | Registro C040 | Identificação da ECD Recuperada | 2 | 1 | 89 |
| [ ] | 10.011 | Registro C050 | Plano de Contas Recuperado | 3 | 1:N | 92 |
| [ ] | 10.012 | Registro C051 | Plano de Contas Referencial Recuperado | 4 | 0:N | 93 |
| [ ] | 10.013 | Registro C052 | Indicação dos Códigos de Aglutinação Recuperados | 4 | 0:N | 94 |
| [ ] | 10.014 | Registro C150 | Saldos Periódicos Recuperados – Identificação do Período | 3 | 1 | 95 |
| [ ] | 10.015 | Registro C155 | Detalhe dos Saldos Periódicos Recuperados | 4 | 1:N | 96 |
| [ ] | 10.016 | Registro C600 | Demonstrações Contábeis Recuperadas | 3 | 1:N | 98 |
| [ ] | 10.017 | Registro C650 | Demonstração do Resultado do Exercício Recuperada | 4 | 1:N | 99 |
| [ ] | 10.018 | Registro C990 | Encerramento do Bloco C | 1 | 1 | 100 |

### Bloco I — Lançamentos Contábeis (26 registros)

| Feito | Sub-stage | Registro | Descrição | Nível | Ocorrência | Página PDF |
| --- | --- | --- | --- | --- | --- | --- |
| [ ] | 10.019 | Registro I001 | Abertura do Bloco I | 1 | 1 | 101 |
| [ ] | 10.020 | Registro I010 | Identificação da Escrituração Contábil | 2 | 1 | 102 |
| [ ] | 10.021 | Registro I012 | Livros Auxiliares ao Diário ou Livro Principal | 3 | 0:N | 104 |
| [ ] | 10.022 | Registro I015 | Identificação das Contas da Escrituração Resumida a que se Refere a Escrituração Auxiliar | 4 | 1:N | 107 |
| [ ] | 10.023 | Registro I020 | Campos Adicionais | 3 | 0:N | 109 |
| [ ] | 10.024 | Registro I030 | Termo de Abertura do Livro | 3 | 1 | 113 |
| [ ] | 10.025 | Registro I050 | Plano de Contas | 3 | 1:N | 117 |
| [ ] | 10.026 | Registro I051 | Plano de Contas Referencial | 4 | 1:N | 122 |
| [ ] | 10.027 | Registro I052 | Indicação dos Códigos de Aglutinação | 4 | 1:N | 124 |
| [ ] | 10.028 | Registro I053 | Subcontas Correlatas | 4 | 1:N | 126 |
| [ ] | 10.029 | Registro I075 | Tabela de Histórico Padronizado | 3 | 0:N | 129 |
| [ ] | 10.030 | Registro I100 | Centro de Custos | 3 | 0:N | 130 |
| [ ] | 10.031 | Registro I150 | Saldos Periódicos – Identificação do Período | 3 | 1:12 | 131 |
| [ ] | 10.032 | Registro I155 | Detalhe dos Saldos Periódicos | 4 | 1:N | 133 |
| [ ] | 10.033 | Registro I157 | Transferência de Saldos de Plano de Contas Anterior | 5 | 1:N | 140 |
| [ ] | 10.034 | Registro I200 | Lançamento Contábil | 3 | 1:N | 142 |
| [ ] | 10.035 | Registro I250 | Partidas do Lançamento | 4 | 1:N | 147 |
| [ ] | 10.036 | Registro I300 | Balancetes Diários – Identificação da Data | 3 | 0:N | 152 |
| [ ] | 10.037 | Registro I310 | Detalhes do Balancete Diário | 4 | 1:N | 153 |
| [ ] | 10.038 | Registro I350 | Saldo das Contas de Resultado Antes do Encerramento – Identificação da Data | 3 | 1:12 | 155 |
| [ ] | 10.039 | Registro I355 | Detalhes dos Saldos das Contas de Resultado Antes do Encerramento | 4 | 1:N | 157 |
| [ ] | 10.040 | Registro I500 | Parâmetros de Impressão e Visualização do Razão Auxiliar com Leiaute Parametrizável | 3 | 0:N | 160 |
| [ ] | 10.041 | Registro I510 | Definição de Campos do Livro Razão Auxiliar com Leiaute Parametrizável | 3 | 0:N | 161 |
| [ ] | 10.042 | Registro I550 | Detalhes do Livro Razão Auxiliar com Leiaute Parametrizável | 3 | 0:N | 163 |
| [ ] | 10.043 | Registro I555 | Totais no Livro Razão Auxiliar com Leiaute Parametrizável | 4 | 0:N | 166 |
| [ ] | 10.044 | Registro I990 | Encerramento do Bloco I | 1 | 1 | 168 |

### Bloco J — Demonstrações Contábeis (13 registros)

| Feito | Sub-stage | Registro | Descrição | Nível | Ocorrência | Página PDF |
| --- | --- | --- | --- | --- | --- | --- |
| [ ] | 10.045 | Registro J001 | Abertura do Bloco J | 1 | 1 | 169 |
| [ ] | 10.046 | Registro J005 | Demonstrações Contábeis | 2 | 1:12 | 171 |
| [ ] | 10.047 | Registro J100 | Balanço Patrimonial | 3 | 1:N | 174 |
| [ ] | 10.048 | Registro J150 | Demonstração do Resultado do Exercício (DRE) | 3 | 1:N | 180 |
| [ ] | 10.049 | Registro J210 | DLPA – Demonstração de Lucros ou Prejuízos Acumulados / DMPL – Demonstração de Mutações do Patrimônio Líquido | 3 | 1:N | 186 |
| [ ] | 10.050 | Registro J215 | Fato Contábil que Altera a Conta Lucros Acumulados ou Prejuízos Acumulados ou Todo o Patrimônio Líquido | 4 | 1:N | 189 |
| [ ] | 10.051 | Registro J800 | Outras Informações | 3 | 1:N | 190 |
| [ ] | 10.052 | Registro J801 | Termo de Verificação para Fins de Substituição da ECD | 3 | 0:1 | 192 |
| [ ] | 10.053 | Registro J900 | Termo de Encerramento | 2 | 1 | 195 |
| [ ] | 10.054 | Registro J930 | Signatários da Escrituração | 3 | 1:N | 197 |
| [ ] | 10.055 | Registro J932 | Signatários do Termo de Verificação para Fins de Substituição da ECD | 3 | 1:N | 203 |
| [ ] | 10.056 | Registro J935 | Identificação dos Auditores Independentes | 3 | 1:N | 206 |
| [ ] | 10.057 | Registro J990 | Encerramento do Bloco J | 1 | 1 | 207 |

### Bloco K — Conglomerados Econômicos (11 registros)

| Feito | Sub-stage | Registro | Descrição | Nível | Ocorrência | Página PDF |
| --- | --- | --- | --- | --- | --- | --- |
| [ ] | 10.058 | Registro K001 | Abertura do Bloco K | 1 | 1 | 208 |
| [ ] | 10.059 | Registro K030 | Período da Escrituração Contábil Consolidada | 2 | 0:1 | 209 |
| [ ] | 10.060 | Registro K100 | Relação das Empresas Consolidadas | 3 | 0:N | 211 |
| [ ] | 10.061 | Registro K110 | Relação dos Eventos Societários | 4 | 0:N | 215 |
| [ ] | 10.062 | Registro K115 | Empresas Participantes do Evento Societário | 5 | 0:N | 217 |
| [ ] | 10.063 | Registro K200 | Plano de Contas Consolidado | 2 | 1:N | 219 |
| [ ] | 10.064 | Registro K210 | Mapeamento para Planos de Contas das Empresas Consolidadas | 3 | 1:N | 222 |
| [ ] | 10.065 | Registro K300 | Saldos das Contas Consolidadas | 3 | 0:N | 223 |
| [ ] | 10.066 | Registro K310 | Empresas Detentoras das Parcelas do Valor Eliminado Total | 4 | 0:N | 225 |
| [ ] | 10.067 | Registro K315 | Empresas Contrapartes das Parcelas do Valor Eliminado Total | 5 | 0:N | 226 |
| [ ] | 10.068 | Registro K990 | Encerramento do Bloco K | 1 | 1 | 228 |

### Bloco 9 — Controle e Encerramento do Arquivo Digital (4 registros)

| Feito | Sub-stage | Registro | Descrição | Nível | Ocorrência | Página PDF |
| --- | --- | --- | --- | --- | --- | --- |
| [ ] | 10.069 | Registro 9001 | Abertura do Bloco 9 | 1 | 1 | 229 |
| [ ] | 10.070 | Registro 9900 | Registros do Arquivo | 2 | 1:N | 230 |
| [ ] | 10.071 | Registro 9990 | Encerramento do Bloco 9 | 1 | 1 | 231 |
| [ ] | 10.072 | Registro 9999 | Encerramento do Arquivo Digital | 0 | 1 | 232 |

## Publishing

`ARCHITECTURE.md` §10: publica `TecnoFisc.Sped.Ecd` (bump apropriado no release — alinhar com o estado atual do versionamento do repo no momento) quando todas as 72 sub-stages estiverem merged e o `ParserEcd` ler um arquivo ECD real anonimizado. SPED é all-or-nothing — sem release intermediário (todo código de registro presente no arquivo precisa ser reconhecido).
