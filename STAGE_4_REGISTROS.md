# Stage 4 — EFD Contribuições registros (sub-stage decomposition)

> Operational appendix to `ARCHITECTURE.md` §12. **Scope: EFD Contribuições only** (project `TecnoFisc.Sped.EfdContribuicoes`, layout V006, guide v1.35). Each row below is one sub-stage of Stage 4.
>
> **Other SPED leiautes (EFD ICMS-IPI, EFD-Reinf, ECD, ECF, eSocial, …) get their own analogous appendix** when their stage starts (e.g., `STAGE_8_REGISTROS_FISCAL.md` for EFD ICMS-IPI under Stage 8). Each appendix is built from the Section-3 (or equivalent) TOC of that project's own *Guia Prático* PDF and follows the same conventions as this document. Do **not** mix registros across leiautes — `RegistroC100` of EFD Contribuições and `RegistroC100` of EFD ICMS-IPI are unrelated classes in independent assemblies.

## How to use this document

1. Pick the next un-implemented sub-stage (smallest 4.NNN with no PR merged yet).
2. Open `Guia_Pratico_EFD_Contribuicoes_Versao_1_35 - 18_06_2021.pdf` directly at the PDF page listed for that sub-stage — do **not** read the whole guide. Use the `Read` tool's `pages` parameter (e.g., `pages: "117-122"` for `RegistroC170`). Some records span multiple pages; read until the next `Registro NNNN` heading.
3. Implement the record class under the correct block folder (`Registros/Bloco0/`, `Registros/BlocoC/`, etc.) following naming and code conventions in `ARCHITECTURE.md`.
4. Write tests in the matching `tests/` project: field validation, round-trip (parse → generate → parse), and at least one fixture line copied from the guide's example.
5. PR with `feat: ` prefix and Portuguese body. See **PR granularity** below for when batching multiple sub-stages into one PR is allowed.

## What the PDF page tells you (read every section, not just the field table)

The page that documents a registro contains more than the field table. **All of the following must be respected by the implementation:**

- **Hierarchical level (`Nível`)** — declared in the registro header (e.g., `Nível: 2`). Maps to `Nivel` on `[RegistroSped]` so `PilhaHierarquica` links the record under the correct parent. Wrong nível breaks the parent chain at parse time.
- **Repetition (`Ocorrência`)** — declared next to the nível (e.g., `1:N`, `0:1`, `1:1`). Determines whether the record is required, optional, or repeated, and whether it is a child collection on the parent.
- **Field table** — order, type (`C`/`N`), length (`Tam`), decimals (`Dec`), obligatoriness (`Obrig: S/N/O`). The order is the position in the SPED line; do not reorder. `*` next to length means fixed-length.
- **Per-field rules and observations** — text below or beside the table (often "Observações", "Regras de Validação", "Tabela de Códigos"). These restrict valid values, cross-reference other fields, depend on `Registro0110` regime, or require check-digit validation. Encode these as validation logic on the property setter or the value object — not just as comments.
- **Validation severity** — the guide and the PVA distinguish two classes of validation result:
  - **Erro** (blocking): rejects the file. Implementation: throw or return `Result<T>.Failure` from the parser; refuse to serialize via the generator.
  - **Advertência** (warning): file is accepted but a warning is reported. Implementation: surface as a non-fatal entry in the parse/generate result (e.g., a `IList<Aviso>` on the parse outcome) — never throw and never silently drop. Round-trip must preserve the input even when warnings fire.
  - When the guide is not explicit about which class a rule belongs to, treat *malformed-format* (wrong type, length, missing required field) as **erro** and *cross-record consistency / suggested values / preferred codes* as **advertência**. When in doubt, leave a `// TODO severidade` comment and ask before merging.

## PR granularity

Default: **one sub-stage per PR.** This is correct for any registro with hierarchical children, conditional fields, calculated totalizers, value-object validation, or non-trivial cross-field rules.

**Batching is allowed** when *all* of the following hold for every sub-stage in the batch:

- Two or three fields, no decimals, no enums, no value-object validation beyond formatting.
- No hierarchical children (parent-only or leaf, never has a child registro hanging off it).
- No "Regras de Validação" / observation block beyond the field table.
- Logically grouped: a contiguous run within the same bloco (e.g., all `Processo Referenciado` records of Bloco C: `C111`, `C188`, `C198`, `C489`, `C499`, `C509`, `C609`, `C830`, `C890`), or all bloco openers (`X001`), or all bloco closers (`X990`/`9990`/`9999`).

Cap a batched PR at ~10 registros. Anything larger is harder to review than the time saved. The PR description must list every sub-stage code it covers.

## Enums and value objects (on demand, not upfront)

There is **no dedicated stage zero** that pre-creates every fiscal enum or value object before sub-stage `4.001`. Pre-cataloging would force reading ~150 PDF pages eagerly to harvest every `Ind*`/`Cod*`/referenced table — expensive in tokens and in review effort, and the resulting catalog rots before it is consumed (rules vary per registro; valid values only become unambiguous in the registro context).

**Rules:**

- **First-use creates.** Enums (e.g., `IndicadorOperacao`, `ModeloDocumento`, `SituacaoDocumento`) and value objects (e.g., `Cnpj`, `Cfop`, `Ncm`, `ChaveAcesso`) are created in the **first sub-stage that consumes them**. They ship in the same PR as that registro, with their own unit tests independent of the registro tests.
- **Reuse, don't redeclare.** Subsequent sub-stages that need the same type just reference it — never recreate, never fork "near-equivalent" enums per registro.
- **Late extension is allowed and additive.** When a later sub-stage discovers a value missing from an existing enum (new code in the PDF for that registro), the extension goes in the PR of *that* sub-stage. A round-trip test exercising the new value is mandatory; without it the gap is invisible.
- **Exhaustiveness, not escape hatches.** Enums map exactly the values listed in the guide for that field. No `Desconhecido`/`Outros` sentinel — an unknown code in input is a parse error (severity per the registro's rule). This is the safety net that turns missing values into loud failures instead of silent miscodings.
- **Naming follows the convention.** Portuguese for fiscal enums and value objects (per `ARCHITECTURE.md` §1.3). Place under `src/TecnoFisc.Sped.EfdContribuicoes/Enums/` or `src/TecnoFisc.Sped.EfdContribuicoes/ValueObjects/` (or shared `TecnoFisc.Sped.Core` when the type is non-format-specific, e.g., `Cnpj`).

Trade-off accepted: a few sub-stages will be larger than average (registro + 2–3 enums + tests). Still atomic, still reviewable.

## Conventions for these sub-stages

- **Class name:** `Registro<CODE>` (e.g., `Registro0000`, `RegistroC170`, `Registro1990`).
- **Folder:** `src/TecnoFisc.Sped.EfdContribuicoes/Registros/Bloco<X>/` where `<X>` is the block letter/digit.
- **Layout version:** all rows are V006 (layout 006, guide v1.35). Versioning per Stage 7 happens later.
- **Dependencies between registros:** the `partial` class infrastructure and the `RegistroSpedAttribute`/`CampoSpedAttribute` must already exist (Stages 2–3). If they do not, finish Stages 2–3 first.
- **Encerramento records (`X990`, `9999`, `9990`):** trivial counters; field declarations only — `TotalizadorBlocos` (Stage 3) drives population. Eligible for batching.
- **Tests:** minimum coverage per sub-stage is described in `ARCHITECTURE.md` §13.3. Round-trip is mandatory; warnings (advertências) must be exercised by at least one fixture when the registro defines them.

## Sub-stages

Block headers are informational; numbering is global and contiguous (4.001 → 4.203).

### Bloco 0 — Abertura, Identificação e Referências

| Feito | Sub-stage | Registro | Descrição | Página PDF |
| --- | --- | --- | --- | --- |
| [x] | 4.001 | Registro 0000 | Abertura do Arquivo Digital e Identificação da Pessoa Jurídica | 66 |
| [x] | 4.002 | Registro 0001 | Abertura do Bloco 0 | 69 |
| [x] | 4.003 | Registro 0035 | Identificação de Sociedade em Conta de Participação – SCP | 69 |
| [x] | 4.004 | Registro 0100 | Dados do Contabilista | 70 |
| [x] | 4.005 | Registro 0110 | Regimes de Apuração da Contribuição Social e de Apropriação de Crédito | 71 |
| [x] | 4.006 | Registro 0111 | Tabela de Receita Bruta Mensal Para Fins de Rateio de Créditos Comuns | 74 |
| [x] | 4.007 | Registro 0120 | Identificação de EFD-Contribuições Sem Dados a Escriturar | 75 |
| [x] | 4.008 | Registro 0140 | Tabela de Cadastro de Estabelecimentos | 78 |
| [x] | 4.009 | Registro 0145 | Regime de Apuração da Contribuição Previdenciária Sobre a Receita Bruta | 79 |
| [x] | 4.010 | Registro 0150 | Tabela de Cadastro do Participante | 81 |
| [x] | 4.011 | Registro 0190 | Identificação das Unidades de Medida | 83 |
| [x] | 4.012 | Registro 0200 | Tabela de Identificação do Item (Produtos e Serviços) | 83 |
| [x] | 4.013 | Registro 0205 | Alteração do Item | 85 |
| [x] | 4.014 | Registro 0206 | Código de Produto Conforme Tabela ANP (Combustíveis) | 86 |
| [x] | 4.015 | Registro 0208 | Código de Grupos por Marca Comercial – Refri (bebidas frias) | 86 |
| [x] | 4.016 | Registro 0400 | Tabela de Natureza da Operação/Prestação | 88 |
| [x] | 4.017 | Registro 0450 | Tabela de Informação Complementar do Documento Fiscal | 88 |
| [ ] | 4.018 | Registro 0500 | Plano de Contas Contábeis | 89 |
| [ ] | 4.019 | Registro 0600 | Centro de Custos | 91 |
| [ ] | 4.020 | Registro 0900 | Composição das Receitas do Período – Receita Bruta e Demais Receitas | 91 |
| [x] | 4.021 | Registro 0990 | Encerramento do Bloco 0 | 94 |

### Bloco A — Documentos Fiscais (Serviços Sujeitos ao ISS)

| Feito | Sub-stage | Registro | Descrição | Página PDF |
| --- | --- | --- | --- | --- |
| [x] | 4.022 | Registro A001 | Abertura do Bloco A | 95 |
| [ ] | 4.023 | Registro A010 | Identificação do Estabelecimento | 95 |
| [ ] | 4.024 | Registro A100 | Documento - Nota Fiscal de Serviço | 96 |
| [ ] | 4.025 | Registro A110 | Complemento do Documento - Informação Complementar da NF | 98 |
| [ ] | 4.026 | Registro A111 | Processo Referenciado | 99 |
| [ ] | 4.027 | Registro A120 | Informação Complementar - Operações de Importação | 100 |
| [ ] | 4.028 | Registro A170 | Complemento do Documento - Itens do Documento | 101 |
| [x] | 4.029 | Registro A990 | Encerramento do Bloco A | 104 |

### Bloco C — Documentos Fiscais I (Mercadorias / ICMS-IPI)

| Feito | Sub-stage | Registro | Descrição | Página PDF |
| --- | --- | --- | --- | --- |
| [x] | 4.030 | Registro C001 | Abertura do Bloco C | 104 |
| [ ] | 4.031 | Registro C010 | Identificação do Estabelecimento | 105 |
| [ ] | 4.032 | Registro C100 | Documento - Nota Fiscal (01), Avulsa (1B), Produtor (04), NF-e (55), NFC-e (65) | 105 |
| [ ] | 4.033 | Registro C110 | Complemento do Documento - Informação Complementar da Nota Fiscal (01, 1B, 04, 55) | 114 |
| [ ] | 4.034 | Registro C111 | Processo Referenciado | 114 |
| [ ] | 4.035 | Registro C120 | Complemento do Documento - Operações de Importação (Código 01) | 115 |
| [ ] | 4.036 | Registro C170 | Complemento do Documento - Itens do Documento (01, 1B, 04, 55) | 117 |
| [ ] | 4.037 | Registro C175 | Registro Analítico do Documento (Código 65) | 122 |
| [ ] | 4.038 | Registro C180 | Consolidação de NF-e Emitidas Pela Pessoa Jurídica (55, 65) – Vendas | 126 |
| [ ] | 4.039 | Registro C181 | Detalhamento da Consolidação – Vendas – PIS/Pasep | 130 |
| [ ] | 4.040 | Registro C185 | Detalhamento da Consolidação – Vendas – Cofins | 132 |
| [ ] | 4.041 | Registro C188 | Processo Referenciado | 134 |
| [ ] | 4.042 | Registro C190 | Consolidação de NF-e (55) – Aquisições com Crédito e Devoluções | 135 |
| [ ] | 4.043 | Registro C191 | Detalhamento da Consolidação – Aquisições/Devoluções – PIS/Pasep | 139 |
| [ ] | 4.044 | Registro C195 | Detalhamento da Consolidação – Aquisições/Devoluções – Cofins | 142 |
| [ ] | 4.045 | Registro C198 | Processo Referenciado | 145 |
| [ ] | 4.046 | Registro C199 | Complemento do Documento - Operações de Importação (Código 55) | 146 |
| [ ] | 4.047 | Registro C380 | NF Venda a Consumidor (02) - Consolidação de Documentos Emitidos | 147 |
| [ ] | 4.048 | Registro C381 | Detalhamento da Consolidação – PIS/Pasep | 148 |
| [ ] | 4.049 | Registro C385 | Detalhamento da Consolidação – Cofins | 149 |
| [ ] | 4.050 | Registro C395 | NF Venda a Consumidor (02, 2D, 2E, 59, 60, 65) – Aquisições com Crédito | 151 |
| [ ] | 4.051 | Registro C396 | Itens do Documento (02, 2D, 2E, 59, 60, 65) – Aquisições com Crédito | 152 |
| [ ] | 4.052 | Registro C400 | Equipamento ECF (Códigos 02 e 2D) | 154 |
| [ ] | 4.053 | Registro C405 | Redução Z (Códigos 02 e 2D) | 154 |
| [ ] | 4.054 | Registro C481 | Resumo Diário ECF – PIS/Pasep (02, 2D) | 155 |
| [ ] | 4.055 | Registro C485 | Resumo Diário ECF – Cofins (02, 2D) | 157 |
| [ ] | 4.056 | Registro C489 | Processo Referenciado | 159 |
| [ ] | 4.057 | Registro C490 | Consolidação de Documentos Emitidos por ECF (02, 2D, 59, 60) | 160 |
| [ ] | 4.058 | Registro C491 | Detalhamento Consolidação ECF (02, 2D, 59, 60) – PIS/Pasep | 160 |
| [ ] | 4.059 | Registro C495 | Detalhamento Consolidação ECF (02, 2D, 59, 60) – Cofins | 162 |
| [ ] | 4.060 | Registro C499 | Processo Referenciado | 164 |
| [ ] | 4.061 | Registro C500 | NF/Conta Energia (06), NF3e (66), Água (29), Gás (28), NF-e (55) – Entrada com Crédito | 165 |
| [ ] | 4.062 | Registro C501 | Complemento da Operação (06, 28, 29) – PIS/Pasep | 167 |
| [ ] | 4.063 | Registro C505 | Complemento da Operação (06, 28, 29) – Cofins | 169 |
| [ ] | 4.064 | Registro C509 | Processo Referenciado | 171 |
| [ ] | 4.065 | Registro C600 | Consolidação Diária NF Energia/Água/Gás – Saída | 172 |
| [ ] | 4.066 | Registro C601 | Complemento Consolidação Diária (06, 28, 29) – Saídas – PIS/Pasep | 174 |
| [ ] | 4.067 | Registro C605 | Complemento Consolidação Diária (06, 28, 29) – Saídas – Cofins | 175 |
| [ ] | 4.068 | Registro C609 | Processo Referenciado | 176 |
| [ ] | 4.069 | Registro C800 | Cupom Fiscal Eletrônico (Código 59) | 177 |
| [ ] | 4.070 | Registro C810 | Detalhamento CF-e (59) – PIS/Pasep e Cofins | 179 |
| [ ] | 4.071 | Registro C820 | Detalhamento CF-e (59) – PIS/Pasep e Cofins por Unidade de Medida | 182 |
| [ ] | 4.072 | Registro C830 | Processo Referenciado | 184 |
| [ ] | 4.073 | Registro C860 | Identificação do Equipamento SAT-CF-e | 184 |
| [ ] | 4.074 | Registro C870 | Resumo Diário SAT-CF-e (59) – PIS/Pasep e Cofins | 185 |
| [ ] | 4.075 | Registro C880 | Resumo Diário SAT-CF-e (59) – PIS/Pasep e Cofins por Unidade de Medida | 188 |
| [ ] | 4.076 | Registro C890 | Processo Referenciado | 191 |
| [x] | 4.077 | Registro C990 | Encerramento do Bloco C | 192 |

### Bloco D — Documentos Fiscais II (Serviços / ICMS)

| Feito | Sub-stage | Registro | Descrição | Página PDF |
| --- | --- | --- | --- | --- |
| [x] | 4.078 | Registro D001 | Abertura do Bloco D | 193 |
| [ ] | 4.079 | Registro D010 | Identificação do Estabelecimento | 193 |
| [ ] | 4.080 | Registro D100 | Aquisição de Serviços de Transporte (07, 08, 8B, 09, 10, 11, 26, 27, 57, 63, 67) | 194 |
| [ ] | 4.081 | Registro D101 | Complemento do Documento de Transporte – PIS/Pasep | 197 |
| [ ] | 4.082 | Registro D105 | Complemento do Documento de Transporte – Cofins | 200 |
| [ ] | 4.083 | Registro D111 | Processo Referenciado | 202 |
| [ ] | 4.084 | Registro D200 | Resumo Diário – Prestação de Serviços de Transporte | 203 |
| [ ] | 4.085 | Registro D201 | Totalização do Resumo Diário – PIS/Pasep | 204 |
| [ ] | 4.086 | Registro D205 | Totalização do Resumo Diário – Cofins | 206 |
| [ ] | 4.087 | Registro D209 | Processo Referenciado | 207 |
| [ ] | 4.088 | Registro D300 | Resumo Diário - Bilhetes Consolidados de Passagem (13, 14, 15, 16, 18) | 208 |
| [ ] | 4.089 | Registro D309 | Processo Referenciado | 210 |
| [ ] | 4.090 | Registro D350 | Resumo Diário Cupom Fiscal Emitido por ECF (2E, 13, 14, 15, 16) | 211 |
| [ ] | 4.091 | Registro D359 | Processo Referenciado | 214 |
| [ ] | 4.092 | Registro D500 | NF Serviço de Comunicação (21) e Telecomunicação (22) – Aquisição com Crédito | 215 |
| [ ] | 4.093 | Registro D501 | Complemento da Operação (21, 22) – PIS/Pasep | 216 |
| [ ] | 4.094 | Registro D505 | Complemento da Operação (21, 22) – Cofins | 218 |
| [ ] | 4.095 | Registro D509 | Processo Referenciado | 220 |
| [ ] | 4.096 | Registro D600 | Consolidação Prestação Serviços Comunicação/Telecomunicação (21, 22) | 221 |
| [ ] | 4.097 | Registro D601 | Complemento Consolidação (21, 22) – PIS/Pasep | 224 |
| [ ] | 4.098 | Registro D605 | Complemento Consolidação (21, 22) – Cofins | 226 |
| [ ] | 4.099 | Registro D609 | Processo Referenciado | 227 |
| [x] | 4.100 | Registro D990 | Encerramento do Bloco D | 228 |

### Bloco F — Demais Documentos e Operações

| Feito | Sub-stage | Registro | Descrição | Página PDF |
| --- | --- | --- | --- | --- |
| [x] | 4.101 | Registro F001 | Abertura do Bloco F | 229 |
| [ ] | 4.102 | Registro F010 | Identificação do Estabelecimento | 230 |
| [ ] | 4.103 | Registro F100 | Demais Documentos e Operações Geradoras de Contribuição e Créditos | 230 |
| [ ] | 4.104 | Registro F111 | Processo Referenciado | 235 |
| [ ] | 4.105 | Registro F120 | Bens Incorporados ao Ativo Imobilizado – Créditos por Depreciação/Amortização | 237 |
| [ ] | 4.106 | Registro F129 | Processo Referenciado | 240 |
| [ ] | 4.107 | Registro F130 | Bens Incorporados ao Ativo Imobilizado – Créditos por Aquisição/Contribuição | 242 |
| [ ] | 4.108 | Registro F139 | Processo Referenciado | 246 |
| [ ] | 4.109 | Registro F150 | Crédito Presumido sobre Estoque de Abertura | 247 |
| [ ] | 4.110 | Registro F200 | Operações da Atividade Imobiliária - Unidade Imobiliária Vendida | 249 |
| [ ] | 4.111 | Registro F205 | Operações da Atividade Imobiliária – Custo Incorrido da Unidade | 252 |
| [ ] | 4.112 | Registro F210 | Operações da Atividade Imobiliária - Custo Orçado da Unidade Vendida | 255 |
| [ ] | 4.113 | Registro F211 | Processo Referenciado | 257 |
| [ ] | 4.114 | Registro F500 | Consolidação Lucro Presumido – Regime de Caixa | 258 |
| [ ] | 4.115 | Registro F509 | Processo Referenciado | 261 |
| [ ] | 4.116 | Registro F510 | Consolidação Lucro Presumido – Caixa por Unidade de Medida (Alíquota em Reais) | 262 |
| [ ] | 4.117 | Registro F519 | Processo Referenciado | 265 |
| [ ] | 4.118 | Registro F525 | Composição da Receita Escriturada – Receita Recebida pelo Regime de Caixa | 266 |
| [ ] | 4.119 | Registro F550 | Consolidação Lucro Presumido – Regime de Competência | 268 |
| [ ] | 4.120 | Registro F559 | Processo Referenciado | 272 |
| [ ] | 4.121 | Registro F560 | Consolidação Lucro Presumido – Competência por Unidade de Medida | 273 |
| [ ] | 4.122 | Registro F569 | Processo Referenciado | 276 |
| [ ] | 4.123 | Registro F600 | Contribuição Retida na Fonte | 277 |
| [ ] | 4.124 | Registro F700 | Deduções Diversas | 280 |
| [ ] | 4.125 | Registro F800 | Créditos Decorrentes de Eventos de Incorporação, Fusão e Cisão | 282 |
| [x] | 4.126 | Registro F990 | Encerramento do Bloco F | 283 |

### Bloco I — Operações de Instituições Financeiras, Seguradoras e Assemelhados

| Feito | Sub-stage | Registro | Descrição | Página PDF |
| --- | --- | --- | --- | --- |
| [x] | 4.127 | Registro I001 | Abertura do Bloco I | 284 |
| [ ] | 4.128 | Registro I010 | Identificação da Pessoa Jurídica/Estabelecimento | 285 |
| [ ] | 4.129 | Registro I100 | Consolidação das Operações do Período | 286 |
| [ ] | 4.130 | Registro I199 | Processo Referenciado | 288 |
| [ ] | 4.131 | Registro I200 | Composição das Receitas, Deduções e/ou Exclusões do Período | 289 |
| [ ] | 4.132 | Registro I299 | Processo Referenciado | 290 |
| [ ] | 4.133 | Registro I300 | Detalhamento das Receitas, Deduções e/ou Exclusões do Período | 291 |
| [ ] | 4.134 | Registro I399 | Processo Referenciado | 293 |
| [x] | 4.135 | Registro I990 | Encerramento do Bloco I | 294 |

### Bloco M — Apuração da Contribuição e Crédito do PIS/Pasep e Cofins

| Feito | Sub-stage | Registro | Descrição | Página PDF |
| --- | --- | --- | --- | --- |
| [x] | 4.136 | Registro M001 | Abertura do Bloco M | 295 |
| [ ] | 4.137 | Registro M100 | Crédito de PIS/Pasep Relativo ao Período | 295 |
| [ ] | 4.138 | Registro M105 | Detalhamento da Base de Cálculo do Crédito Apurado – PIS/Pasep | 299 |
| [ ] | 4.139 | Registro M110 | Ajustes do Crédito de PIS/Pasep Apurado | 303 |
| [ ] | 4.140 | Registro M115 | Detalhamento dos Ajustes do Crédito de PIS/Pasep Apurado | 304 |
| [ ] | 4.141 | Registro M200 | Consolidação da Contribuição para o PIS/Pasep do Período | 305 |
| [ ] | 4.142 | Registro M205 | PIS/Pasep a Recolher – Detalhamento por Código de Receita | 308 |
| [ ] | 4.143 | Registro M210 | Detalhamento da Contribuição para o PIS/Pasep do Período | 309 |
| [ ] | 4.144 | Registro M211 | Sociedades Cooperativas – Composição da Base de Cálculo – PIS/Pasep | 318 |
| [ ] | 4.145 | Registro M215 | Ajustes da Base de Cálculo da Contribuição para o PIS/Pasep | 319 |
| [ ] | 4.146 | Registro M220 | Ajustes da Contribuição para o PIS/Pasep Apurada | 321 |
| [ ] | 4.147 | Registro M225 | Detalhamento dos Ajustes da Contribuição para o PIS/Pasep | 321 |
| [ ] | 4.148 | Registro M230 | Informações Adicionais de Diferimento | 323 |
| [ ] | 4.149 | Registro M300 | PIS/Pasep Diferida em Períodos Anteriores – Valores a Pagar no Período | 324 |
| [ ] | 4.150 | Registro M350 | PIS/Pasep – Folha de Salários | 325 |
| [ ] | 4.151 | Registro M400 | Receitas Isentas / Alíquota Zero / Suspensão – PIS/Pasep | 326 |
| [ ] | 4.152 | Registro M410 | Detalhamento das Receitas Isentas / Alíquota Zero / Suspensão – PIS/Pasep | 327 |
| [ ] | 4.153 | Registro M500 | Crédito de Cofins Relativo ao Período | 329 |
| [ ] | 4.154 | Registro M505 | Detalhamento da Base de Cálculo do Crédito Apurado – Cofins | 332 |
| [ ] | 4.155 | Registro M510 | Ajustes do Crédito de Cofins Apurado | 337 |
| [ ] | 4.156 | Registro M515 | Detalhamento dos Ajustes do Crédito de Cofins Apurado | 338 |
| [ ] | 4.157 | Registro M600 | Consolidação da Contribuição para a Seguridade Social – Cofins | 339 |
| [ ] | 4.158 | Registro M605 | Cofins a Recolher – Detalhamento por Código de Receita | 342 |
| [ ] | 4.159 | Registro M610 | Detalhamento da Contribuição para a Seguridade Social – Cofins | 343 |
| [ ] | 4.160 | Registro M611 | Sociedades Cooperativas – Composição da Base de Cálculo – Cofins | 352 |
| [ ] | 4.161 | Registro M615 | Ajustes da Base de Cálculo da Cofins Apurada | 354 |
| [ ] | 4.162 | Registro M620 | Ajustes da Cofins Apurada | 355 |
| [ ] | 4.163 | Registro M625 | Detalhamento dos Ajustes da Cofins Apurada | 356 |
| [ ] | 4.164 | Registro M630 | Informações Adicionais de Diferimento | 357 |
| [ ] | 4.165 | Registro M700 | Cofins Diferida em Períodos Anteriores – Valores a Pagar | 358 |
| [ ] | 4.166 | Registro M800 | Receitas Isentas / Alíquota Zero / Suspensão – Cofins | 359 |
| [ ] | 4.167 | Registro M810 | Detalhamento das Receitas Isentas / Alíquota Zero / Suspensão – Cofins | 361 |
| [x] | 4.168 | Registro M990 | Encerramento do Bloco M | 362 |

### Bloco P — Apuração da Contribuição Previdenciária Sobre a Receita Bruta (CPRB)

| Feito | Sub-stage | Registro | Descrição | Página PDF |
| --- | --- | --- | --- | --- |
| [x] | 4.169 | Registro P001 | Abertura do Bloco P | 366 |
| [ ] | 4.170 | Registro P010 | Identificação do Estabelecimento | 367 |
| [ ] | 4.171 | Registro P100 | Contribuição Previdenciária sobre a Receita Bruta | 367 |
| [ ] | 4.172 | Registro P110 | Detalhamento da Apuração da Contribuição | 369 |
| [ ] | 4.173 | Registro P199 | Processo Referenciado | 370 |
| [ ] | 4.174 | Registro P200 | Consolidação da Contribuição Previdenciária Sobre a Receita Bruta | 371 |
| [ ] | 4.175 | Registro P210 | Ajuste da Contribuição Previdenciária Apurada Sobre a Receita Bruta | 373 |
| [x] | 4.176 | Registro P990 | Encerramento do Bloco P | 374 |

### Bloco 1 — Complemento da Escrituração

| Feito | Sub-stage | Registro | Descrição | Página PDF |
| --- | --- | --- | --- | --- |
| [x] | 4.177 | Registro 1001 | Abertura do Bloco 1 | 375 |
| [ ] | 4.178 | Registro 1010 | Processo Referenciado – Ação Judicial | 375 |
| [ ] | 4.179 | Registro 1011 | Detalhamento das Contribuições com Exigibilidade Suspensa | 377 |
| [ ] | 4.180 | Registro 1020 | Processo Referenciado – Processo Administrativo | 381 |
| [ ] | 4.181 | Registro 1050 | Detalhamento de Ajustes de Base de Cálculo – Valores Extra Apuração | 382 |
| [ ] | 4.182 | Registro 1100 | Controle de Créditos Fiscais – PIS/Pasep | 384 |
| [ ] | 4.183 | Registro 1101 | Apuração de Crédito Extemporâneo – Períodos Anteriores – PIS/Pasep | 387 |
| [ ] | 4.184 | Registro 1102 | Detalhamento Crédito Extemporâneo Vinculado a Mais de Um Tipo de Receita – PIS/Pasep | 390 |
| [ ] | 4.185 | Registro 1200 | Contribuição Social Extemporânea – PIS/Pasep | 391 |
| [ ] | 4.186 | Registro 1210 | Detalhamento da Contribuição Social Extemporânea – PIS/Pasep | 392 |
| [ ] | 4.187 | Registro 1220 | Demonstração do Crédito a Descontar a Contribuição Extemporânea – PIS/Pasep | 393 |
| [ ] | 4.188 | Registro 1300 | Controle dos Valores Retidos na Fonte – PIS/Pasep | 394 |
| [ ] | 4.189 | Registro 1500 | Controle de Créditos Fiscais – Cofins | 396 |
| [ ] | 4.190 | Registro 1501 | Apuração de Crédito Extemporâneo – Períodos Anteriores – Cofins | 399 |
| [ ] | 4.191 | Registro 1502 | Detalhamento Crédito Extemporâneo Vinculado a Mais de Um Tipo de Receita – Cofins | 402 |
| [ ] | 4.192 | Registro 1600 | Contribuição Social Extemporânea – Cofins | 403 |
| [ ] | 4.193 | Registro 1610 | Detalhamento da Contribuição Social Extemporânea – Cofins | 404 |
| [ ] | 4.194 | Registro 1620 | Demonstração do Crédito a Descontar da Contribuição Extemporânea – Cofins | 405 |
| [ ] | 4.195 | Registro 1700 | Controle dos Valores Retidos na Fonte – Cofins | 406 |
| [ ] | 4.196 | Registro 1800 | Incorporação Imobiliária – RET | 408 |
| [ ] | 4.197 | Registro 1809 | Processo Referenciado | 409 |
| [ ] | 4.198 | Registro 1900 | Consolidação dos Documentos Emitidos no Período – Lucro Presumido (Caixa/Competência) | 410 |
| [x] | 4.199 | Registro 1990 | Encerramento do Bloco 1 | 413 |

### Bloco 9 — Controle e Encerramento do Arquivo Digital

| Feito | Sub-stage | Registro | Descrição | Página PDF |
| --- | --- | --- | --- | --- |
| [x] | 4.200 | Registro 9001 | Abertura do Bloco 9 | 415 |
| [ ] | 4.201 | Registro 9900 | Registros do Arquivo | 415 |
| [x] | 4.202 | Registro 9990 | Encerramento do Bloco 9 | 415 |
| [x] | 4.203 | Registro 9999 | Encerramento do Arquivo Digital | 416 |

## Notes

- **Page numbers** refer to the physical pages of `Guia_Pratico_EFD_Contribuicoes_Versao_1_35 - 18_06_2021.pdf`. They were extracted from the Section 3 TOC (PDF page 3) and matched 1:1 to PDF pages — there is no offset between TOC and physical pages for this guide.
- **Range tip:** when reading the PDF for a sub-stage, fetch a small range that includes the next registro's start as a safety boundary, e.g., for `Registro 0140` (page 78) read `pages: "78-81"` so the section ends naturally before `Registro 0145` (page 79) and `Registro 0150` (page 81).
- **Newer layout (V007+):** when a newer guide PDF is dropped, regenerate this list from the new TOC and bump the affected sub-stages under Stage 7 (Layout V007).
- **Other SPED leiautes:** this file documents only EFD Contribuições. EFD ICMS-IPI, EFD-Reinf, ECD, ECF, eSocial and the rest are tracked in their own appendices, generated the same way from their own *Guia Prático* PDFs when their stage starts.
