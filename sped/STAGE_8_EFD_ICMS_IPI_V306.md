# Stage 8 — EFD ICMS-IPI registros V306 (sub-stage decomposition)

> Operational appendix to `ARCHITECTURE.md` §12. **Scope: EFD ICMS-IPI baseline V306 only** (project `TecnoFisc.Sped.EfdIcmsIpi`, layout v3.0.6, guide `sped/guides/Guia Prático EFD - Versão 3.0.6.pdf`). Each row below is one sub-stage of Stage 8 baseline.
>
> Incrementos V307..V322 vivem em arquivos próprios (`STAGE_8_INCR_V307.md`, …, `STAGE_8_INCR_V322.md`), criados conforme cada versão é tackled. Eles **não** repetem a lista de registros; cada incremento descreve apenas o delta sobre a versão anterior (novos campos, novos registros, mudanças de obrigatoriedade) via `[CampoSped(DesdeVersao = (int)LayoutEfdIcmsIpi.VXXX)]` e `[RegistroSped(IntroduzidoEm = (int)LayoutEfdIcmsIpi.VXXX)]`.
>
> Registros aqui são **independentes** dos homônimos em EFD Contribuições. `RegistroC100` em EFD ICMS-IPI e em EFD Contribuições são duas classes distintas em assemblies diferentes — campos, filhos, hierarquia e validações divergem. Compartilhamento só via `TecnoFisc.Sped.Core` (value objects fiscais transversais, enums regidos pelo Ato COTEPE/ICMS — EFD ICMS-IPI **é** o regente).

## How to use this document

1. Pick the next un-implemented sub-stage (smallest 8.NNN com status `[ ]`).
2. Abra `sped/guides/Guia Prático EFD - Versão 3.0.6.pdf` direto na página listada — não leia o guia inteiro. Use `Read` com `pages` (e.g., `pages: "59-64"` para `RegistroC100`). Alguns registros ocupam várias páginas; leia até o próximo header `REGISTRO NNNN`.
3. As páginas listadas são **anchored** quando observadas diretamente durante o bootstrap (ver "Página PDF" exata) e **estimadas** caso contrário (toleram ±2 páginas). Se a página estimada estiver vazia ou apontar para outro registro, role 1–3 páginas — a Tabela 2.6.1 (p303-320) é a fonte de ordem oficial.
4. Implemente a classe sob a pasta do bloco correto (`Registros/Bloco0/`, `Registros/BlocoC/`, etc.) seguindo `ARCHITECTURE.md` (Portuguese para domínio, `partial class`, `[RegistroSped]`/`[CampoSped]`).
5. Tests no projeto `tests/TecnoFisc.Sped.EfdIcmsIpi.Tests/`: validação de campos, round-trip (parse → generate → parse) e pelo menos uma fixture do exemplo do guia.
6. PR com prefixo `feat:` e corpo em português. Ver **PR granularity** abaixo para quando agrupar sub-stages.

## What the PDF page tells you (read every section, not just the field table)

A página de um registro contém mais que a tabela de campos. **Tudo abaixo deve ser respeitado pela implementação:**

- **Nível hierárquico (`Nível`)** — declarado no header (e.g., `Nível: 2`). Mapeia para `Nivel` no `[RegistroSped]` para `PilhaHierarquica` aninhar sob o pai correto. Nível errado quebra a cadeia em parse time.
- **Ocorrência (`Ocorrência`)** — declarada ao lado do nível (e.g., `1:N`, `0:1`, `1:1`, `V`). Determina se é obrigatório, opcional ou repetível.
- **Tabela de campos** — ordem, tipo (`C`/`N`), tamanho (`Tam`), decimais (`Dec`), obrigatoriedade (`Obrig: S/N/O/OC`). A ordem é a posição na linha SPED; não reordenar. `*` em tamanho = comprimento fixo.
- **Regras e observações** — texto abaixo ou ao lado da tabela ("Observações", "Regras de Validação", "Validação do Registro"). Restringem valores, cruzam campos, dependem do perfil (A/B/C), referenciam Tabela 4.1.1 (modelos), 4.1.2 (situação do documento), CST, CFOP, etc. Codifique como validação na propriedade ou value object — não só como comentário.
- **Validação por perfil (A/B/C)** — Bloco C, D e outros distinguem obrigatoriedade por perfil do declarante (campo `PERFIL` no `Registro0000`). A coluna "Obrigatoriedade do registro" na Tabela 2.6.1 (p303-320) tem 6 colunas: Entrada/Saída × Perfil A/B/C. Implementação: a obrigatoriedade condicional roda na validação cross-registro, não no `[CampoSped]`.
- **Severidade da validação** — guia e PVA-EFD-ICMS/IPI distinguem:
  - **Erro** (bloqueante): rejeita o arquivo. Throw ou `Result<T>.Failure` no parser.
  - **Advertência** (aviso): arquivo aceito, warning reportado. Surface em `IList<Aviso>` no resultado do parse/generate — nunca throw nem drop silencioso. Round-trip preserva input mesmo com advertência.
  - Quando o guia não for explícito: *formato malformado* (tipo errado, tamanho, campo obrigatório ausente) = **erro**; *consistência cross-registro / códigos sugeridos* = **advertência**. Em dúvida, `// TODO severidade` e perguntar antes do merge.

## PR granularity

Default: **one sub-stage per PR.** Correto para qualquer registro com filhos hierárquicos, campos condicionais, totalizadores calculados, validação de value object ou cross-field não trivial.

**Batching permitido** quando *todos* os sub-stages do batch atendem:

- 2-3 campos, sem decimais, sem enums, sem validação além de formatação.
- Sem filhos hierárquicos (leaf ou parent-only).
- Sem bloco "Regras de Validação"/"Observações" além da tabela de campos.
- Agrupamento lógico: contíguos no mesmo bloco (e.g., todos os `X001` aberturas, todos os `X990` encerramentos, totalizadores triviais do Bloco 9).

Cap: ~10 registros por PR batch. PR description lista todo `8.NNN` coberto.

## Enums e value objects (on demand, EFD ICMS-IPI é regente do Ato COTEPE)

- **First-use creates.** Enums (e.g., `ModeloDocumento` da Tabela 4.1.1, `SituacaoDocumento` da Tabela 4.1.2) e value objects (`Cnpj`, `Cfop`, `Ncm`, `ChaveAcesso`) são criados no primeiro sub-stage que os consome.
- **Core, não duplicar.** Como EFD ICMS-IPI é o **regente** do Ato COTEPE/ICMS nº 44/2018, tabelas referenciadas por outros leiautes (EFD Contribuições inclusive) vivem em `TecnoFisc.Sped.Core/Tabelas/` e value objects fiscais transversais em `TecnoFisc.Sped.Core/ValueObjects/`. Duplicar = drift bug.
- **Reuso > redeclaração.** Sub-stages posteriores referenciam — nunca recriam, nunca forkam "near-equivalent".
- **Extensão tardia é aditiva.** Novo código no PDF para um campo = extensão do enum no PR do sub-stage que descobriu. Round-trip test mandatório.
- **Exaustividade.** Enums mapeiam exatamente os valores listados. Sem `Outros`/`Desconhecido` sentinel — código desconhecido = parse error.
- **Naming.** Portuguese para enums e value objects fiscais (`ARCHITECTURE.md` §1.3).

## Convenções para estes sub-stages

- **Classe:** `Registro<CODE>` (e.g., `Registro0000`, `RegistroC100`, `Registro1990`).
- **Pasta:** `src/TecnoFisc.Sped.EfdIcmsIpi/Registros/Bloco<X>/` onde `<X>` é a letra/dígito do bloco.
- **Layout version (baseline):** todas as linhas são V306. `[CampoSped]` e `[RegistroSped]` sem `DesdeVersao`/`IntroduzidoEm` (zero = baseline).
- **Encerramento (`X990`, `9990`, `9999`):** contadores triviais; só declaração de campos — `TotalizadorBlocos` (Stage 3) popula. Elegível para batching.
- **Tests:** cobertura mínima por sub-stage descrita em `ARCHITECTURE.md` §13.3. Round-trip mandatório.

## Sub-stages

Block headers são informacionais; numeração é global e contígua (8.001 → 8.255).

### Bloco 0 — Abertura, Identificação e Referências (22 registros)

| Feito | Sub-stage | Registro | Descrição | Página PDF |
| --- | --- | --- | --- | --- |
| [x] | 8.001 | Registro 0000 | Abertura do Arquivo Digital e Identificação da Entidade | 26 |
| [x] | 8.002 | Registro 0001 | Abertura do Bloco 0 | 27 |
| [x] | 8.003 | Registro 0002 | Classificação do Estabelecimento Industrial ou Equiparado a Industrial | 27 |
| [x] | 8.004 | Registro 0005 | Dados Complementares da Entidade | 28 |
| [x] | 8.005 | Registro 0015 | Dados do Contribuinte Substituto ou Responsável pelo ICMS Destino | 28 |
| [x] | 8.006 | Registro 0100 | Dados do Contabilista | 29 |
| [x] | 8.007 | Registro 0150 | Tabela de Cadastro do Participante | 30 |
| [x] | 8.008 | Registro 0175 | Alteração da Tabela de Cadastro de Participante | 31 |
| [x] | 8.009 | Registro 0190 | Identificação das Unidades de Medida | 32 |
| [x] | 8.010 | Registro 0200 | Tabela de Identificação do Item (Produtos e Serviços) | 33 |
| [x] | 8.011 | Registro 0205 | Alteração do Item | 35 |
| [x] | 8.012 | Registro 0206 | Código de Produto Conforme Tabela ANP | 36 |
| [x] | 8.013 | Registro 0210 | Consumo Específico Padronizado | 36 |
| [x] | 8.014 | Registro 0220 | Fatores de Conversão de Unidades | 37 |
| [x] | 8.015 | Registro 0300 | Cadastro de Bens ou Componentes do Ativo Imobilizado | 38 |
| [x] | 8.016 | Registro 0305 | Informação sobre a Utilização do Bem | 39 |
| [x] | 8.017 | Registro 0400 | Tabela de Natureza da Operação/Prestação | 40 |
| [x] | 8.018 | Registro 0450 | Tabela de Informação Complementar do Documento Fiscal | 41 |
| [x] | 8.019 | Registro 0460 | Tabela de Observações do Lançamento Fiscal | 41 |
| [x] | 8.020 | Registro 0500 | Plano de Contas Contábeis | 42 |
| [x] | 8.021 | Registro 0600 | Centro de Custos | 43 |
| [x] | 8.022 | Registro 0990 | Encerramento do Bloco 0 | 43 |

### Bloco B — ISS (Contribuintes do DF) (13 registros)

| Feito | Sub-stage | Registro | Descrição | Página PDF |
| --- | --- | --- | --- | --- |
| [x] | 8.023 | Registro B001 | Abertura do Bloco B | 44 |
| [x] | 8.024 | Registro B020 | NF (cód. 01), NFS (03), NFS-Avulsa (3B), NF-Produtor (04), CT-Rod-Cargas (08), NF-e (55), NFC-e (65) | 45 |
| [x] | 8.025 | Registro B025 | Detalhamento por Combinação de Alíquota e Item da Lista de Serviços (LC 116/2003) | 47 |
| [x] | 8.026 | Registro B030 | Nota Fiscal de Serviços Simplificada (código 3A) | 48 |
| [x] | 8.027 | Registro B035 | Detalhamento por Combinação de Alíquota e Item da Lista de Serviços (LC 116/2003) | 50 |
| [x] | 8.028 | Registro B350 | Serviços Prestados por Instituições Financeiras | 51 |
| [x] | 8.029 | Registro B420 | Totalização dos Valores de Serviços Prestados por Combinação de Alíquota e Item da Lista (LC 116/2003) | 52 |
| [x] | 8.030 | Registro B440 | Totalização dos Valores Retidos | 53 |
| [x] | 8.031 | Registro B460 | Deduções do ISS | 54 |
| [x] | 8.032 | Registro B470 | Apuração do ISS | 55 |
| [x] | 8.033 | Registro B500 | Apuração do ISS Sociedade Uniprofissional | 56 |
| [x] | 8.034 | Registro B510 | Uniprofissional — Empregados e Sócios | 57 |
| [x] | 8.035 | Registro B990 | Encerramento do Bloco B | 58 |

### Bloco C — Documentos Fiscais I — Mercadorias (78 registros)

| Feito | Sub-stage | Registro | Descrição | Página PDF |
| --- | --- | --- | --- | --- |
| [x] | 8.036 | Registro C001 | Abertura do Bloco C | 58 |
| [x] | 8.037 | Registro C100 | NF (cód. 01), NF Avulsa (1B), NF-Produtor (04), NF-e (55), NF-e ao Consumidor Final (65) | 59 |
| [x] | 8.038 | Registro C101 | Informação Complementar — Operações Interestaduais a Consumidor Final Não Contribuinte EC 87/15 (cód. 55) | 65 |
| [x] | 8.039 | Registro C105 | Operações com ICMS ST Recolhido para UF Diversa do Destinatário (cód. 55) | 66 |
| [x] | 8.040 | Registro C110 | Complemento — Informação Complementar da Nota Fiscal (cód. 01, 1B, 55) | 67 |
| [ ] | 8.041 | Registro C111 | Complemento — Processo Referenciado | 68 |
| [ ] | 8.042 | Registro C112 | Complemento — Documento de Arrecadação Referenciado | 68 |
| [ ] | 8.043 | Registro C113 | Complemento — Documento Fiscal Referenciado | 69 |
| [ ] | 8.044 | Registro C114 | Complemento — Cupom Fiscal Referenciado | 70 |
| [ ] | 8.045 | Registro C115 | Local de Coleta e/ou Entrega (cód. 01, 1B e 04) | 70 |
| [ ] | 8.046 | Registro C116 | Cupom Fiscal Eletrônico — CF-e Referenciado | 71 |
| [ ] | 8.047 | Registro C120 | Complemento — Operações de Importação (cód. 01 e 55) | 72 |
| [ ] | 8.048 | Registro C130 | Complemento — ISSQN, IRRF e Previdência Social | 73 |
| [ ] | 8.049 | Registro C140 | Complemento — Fatura (cód. 01) | 74 |
| [ ] | 8.050 | Registro C141 | Complemento — Vencimento da Fatura (cód. 01) | 74 |
| [ ] | 8.051 | Registro C160 | Complemento — Volumes Transportados (cód. 01 e 04) Exceto Combustíveis | 75 |
| [ ] | 8.052 | Registro C165 | Complemento — Operações com Combustíveis (cód. 01) | 76 |
| [ ] | 8.053 | Registro C170 | Complemento — Itens do Documento (cód. 01, 1B, 04 e 55) | 77 |
| [ ] | 8.054 | Registro C171 | Complemento de Item — Armazenamento de Combustíveis (cód. 01, 55) | 85 |
| [ ] | 8.055 | Registro C172 | Complemento de Item — Operações com ISSQN (cód. 01) | 86 |
| [ ] | 8.056 | Registro C173 | Complemento de Item — Operações com Medicamentos (cód. 01, 55) | 87 |
| [ ] | 8.057 | Registro C174 | Complemento de Item — Operações com Armas de Fogo (cód. 01) | 88 |
| [ ] | 8.058 | Registro C175 | Complemento de Item — Operações com Veículos Novos (cód. 01, 55) | 89 |
| [ ] | 8.059 | Registro C176 | Complemento de Item — Ressarcimento de ICMS e FCP em Operações com ST (cód. 01, 55) | 90 |
| [ ] | 8.060 | Registro C177 | Complemento de Item — Outras Informações (cód. 01, 55) | 91 |
| [ ] | 8.061 | Registro C178 | Complemento de Item — Operações com Produtos Sujeitos a IPI por Unidade ou Quantidade | 92 |
| [ ] | 8.062 | Registro C179 | Complemento de Item — Informações Complementares ST (cód. 01) | 93 |
| [ ] | 8.063 | Registro C180 | Informações Complementares de Entrada de Mercadorias Sujeitas a ST (cód. 01, 1B, 04 e 55) | 94 |
| [ ] | 8.064 | Registro C181 | Informações Complementares de Devolução de Saída de Mercadorias Sujeitas a ST (cód. 01, 1B, 04 e 55) | 95 |
| [ ] | 8.065 | Registro C185 | Informações Complementares de Saída de Mercadorias Sujeitas a ST (cód. 01, 1B, 04, 55 e 65) | 96 |
| [ ] | 8.066 | Registro C186 | Informações Complementares de Devolução de Entrada de Mercadorias Sujeitas a ST (cód. 01, 1B, 04 e 55) | 97 |
| [ ] | 8.067 | Registro C190 | Registro Analítico do Documento (cód. 01, 1B, 04, 55 e 65) | 98 |
| [ ] | 8.068 | Registro C191 | Informações do Fundo de Combate à Pobreza — FCP na NF-e (cód. 55) e NFC-e (cód. 65) | 100 |
| [ ] | 8.069 | Registro C195 | Complemento do Registro Analítico — Observações do Lançamento Fiscal (cód. 01, 1B, 04, 55 e 65) | 101 |
| [ ] | 8.070 | Registro C197 | Outras Obrigações Tributárias, Ajustes e Informações Provenientes do Documento Fiscal | 102 |
| [ ] | 8.071 | Registro C300 | Documento — Resumo Diário das NF de Venda a Consumidor (cód. 02) | 103 |
| [ ] | 8.072 | Registro C310 | Documentos Cancelados de NF de Venda a Consumidor (cód. 02) | 104 |
| [ ] | 8.073 | Registro C320 | Registro Analítico das NF de Venda a Consumidor (cód. 02) | 105 |
| [ ] | 8.074 | Registro C321 | Itens dos Resumos Diários dos Documentos (cód. 02) | 106 |
| [ ] | 8.075 | Registro C330 | Informações Complementares das Operações de Saída de Mercadorias Sujeitas a ST (cód. 02) | 107 |
| [ ] | 8.076 | Registro C350 | Nota Fiscal de Venda a Consumidor (cód. 02) | 108 |
| [ ] | 8.077 | Registro C370 | Itens do Documento (cód. 02) | 109 |
| [ ] | 8.078 | Registro C380 | Informações Complementares das Operações de Saída de Mercadorias Sujeitas a ST (cód. 02) | 110 |
| [ ] | 8.079 | Registro C390 | Registro Analítico das NF de Venda a Consumidor (cód. 02) | 111 |
| [ ] | 8.080 | Registro C400 | Equipamento ECF (cód. 02, 2D e 60) | 112 |
| [ ] | 8.081 | Registro C405 | Redução Z (cód. 02, 2D e 60) | 113 |
| [ ] | 8.082 | Registro C410 | PIS e COFINS Totalizados no Dia (cód. 02 e 2D) | 114 |
| [ ] | 8.083 | Registro C420 | Registro dos Totalizadores Parciais da Redução Z (cód. 02, 2D e 60) | 115 |
| [ ] | 8.084 | Registro C425 | Resumo de Itens do Movimento Diário (cód. 02 e 2D) | 116 |
| [ ] | 8.085 | Registro C430 | Informações Complementares das Operações de Saída de Mercadorias Sujeitas a ST (cód. 02, 2D e 60) | 117 |
| [ ] | 8.086 | Registro C460 | Documento Fiscal Emitido por ECF (cód. 02, 2D e 60) | 118 |
| [ ] | 8.087 | Registro C465 | Complemento do Cupom Fiscal Eletrônico Emitido por ECF — CF-e-ECF (cód. 60) | 119 |
| [ ] | 8.088 | Registro C470 | Itens do Documento Fiscal Emitido por ECF (cód. 02 e 2D) | 120 |
| [ ] | 8.089 | Registro C480 | Informações Complementares das Operações de Saída de Mercadorias Sujeitas a ST (cód. 02, 2D e 60) | 121 |
| [ ] | 8.090 | Registro C490 | Registro Analítico do Movimento Diário (cód. 02, 2D e 60) | 122 |
| [ ] | 8.091 | Registro C495 | Resumo Mensal de Itens do ECF por Estabelecimento (cód. 02, 2D e 2E) | 123 |
| [ ] | 8.092 | Registro C500 | NF/Conta de Energia Elétrica (cód. 06), NF-e Eletrônica (66), NF/Conta de Água Canalizada (29), NF/Consumo Gás (28) | 124 |
| [ ] | 8.093 | Registro C510 | Itens do Documento — NF/Conta de Energia Elétrica (cód. 06), Água (29), Gás (28) | 128 |
| [ ] | 8.094 | Registro C590 | Registro Analítico — NF/Conta de Energia Elétrica (06, 66), Água (29), Gás (28) | 130 |
| [ ] | 8.095 | Registro C591 | Informações do Fundo de Combate à Pobreza — FCP na NF3e (cód. 66) | 132 |
| [ ] | 8.096 | Registro C595 | Observações do Lançamento Fiscal (cód. 06, 28, 29 e 66) | 133 |
| [ ] | 8.097 | Registro C597 | Outras Obrigações Tributárias, Ajustes e Informações de Valores Provenientes do Documento Fiscal | 134 |
| [ ] | 8.098 | Registro C600 | Consolidação Diária de NF/Contas (06, 29, 28) — Empresas Não Obrigadas ao Convênio ICMS 115/03 | 135 |
| [ ] | 8.099 | Registro C601 | Documentos Cancelados — Consolidação Diária (06, 29, 28) | 138 |
| [ ] | 8.100 | Registro C610 | Itens do Documento Consolidado — NF/Contas (06, 29, 28) — Empresas Não Obrigadas ao Convênio 115/03 | 139 |
| [ ] | 8.101 | Registro C690 | Registro Analítico dos Documentos — NF/Contas (06, 29, 28) | 141 |
| [ ] | 8.102 | Registro C700 | Consolidação dos Documentos NF/Conta Energia (06) Emitidas em Via Única — Convênio 115/03 e NF/Conta Gás (28) | 142 |
| [ ] | 8.103 | Registro C790 | Registro Analítico dos Documentos — NF/Conta Energia (06) Emitidas em Via Única | 144 |
| [ ] | 8.104 | Registro C791 | Registro de Informações de ICMS ST por UF | 145 |
| [ ] | 8.105 | Registro C800 | Registro Cupom Fiscal Eletrônico — CF-e-SAT (cód. 59) | 146 |
| [ ] | 8.106 | Registro C810 | Itens do Documento do Cupom Fiscal Eletrônico — SAT (CF-E-SAT) (cód. 59) | 148 |
| [ ] | 8.107 | Registro C815 | Informações Complementares das Operações de Saída de Mercadorias Sujeitas a ST (CF-SAT) (cód. 59) | 149 |
| [ ] | 8.108 | Registro C850 | Registro Analítico do CF-e-SAT (cód. 59) | 150 |
| [ ] | 8.109 | Registro C860 | Identificação do Equipamento SAT-CF-e (cód. 59) | 151 |
| [ ] | 8.110 | Registro C870 | Itens do Documento do Cupom Fiscal Eletrônico — SAT (CF-E-SAT) (cód. 59) | 152 |
| [ ] | 8.111 | Registro C880 | Informações Complementares das Operações de Saída de Mercadorias Sujeitas a ST (CF-E-SAT) (cód. 59) | 153 |
| [ ] | 8.112 | Registro C890 | Resumo Diário de CF-e-SAT (cód. 59) por Equipamento SAT-CF-e | 154 |
| [ ] | 8.113 | Registro C990 | Encerramento do Bloco C | 155 |

### Bloco D — Documentos Fiscais II — Serviços (40 registros)

| Feito | Sub-stage | Registro | Descrição | Página PDF |
| --- | --- | --- | --- | --- |
| [ ] | 8.114 | Registro D001 | Abertura do Bloco D | 164 |
| [ ] | 8.115 | Registro D100 | NF Serviço de Transporte (07), CT (08, 8B, 09, 10, 11, 26, 27), CT-e (57, 67), Bilhete Passagem (63) | 165 |
| [ ] | 8.116 | Registro D101 | Informação Complementar — Prestações Interestaduais a Consumidor Final Não Contribuinte EC 87/15 (57, 63, 67) | 169 |
| [ ] | 8.117 | Registro D110 | Itens do Documento — NF Serviço de Transporte (cód. 07) | 170 |
| [ ] | 8.118 | Registro D120 | Complemento da NF Serviço de Transporte (cód. 07) | 171 |
| [ ] | 8.119 | Registro D130 | Complemento do CT Rodoviário de Cargas (cód. 08) e CT Cargas Avulso (8B) | 172 |
| [ ] | 8.120 | Registro D140 | Complemento do CT Aquaviário de Cargas (cód. 09) | 173 |
| [ ] | 8.121 | Registro D150 | Complemento do CT Aéreo de Cargas (cód. 10) | 174 |
| [ ] | 8.122 | Registro D160 | Carga Transportada (cód. 08, 8B, 09, 10, 11, 26 e 27) | 175 |
| [ ] | 8.123 | Registro D161 | Local de Coleta e Entrega (cód. 08, 8B, 09, 10, 11 e 26) | 176 |
| [ ] | 8.124 | Registro D162 | Identificação dos Documentos Fiscais (cód. 08, 8B, 09, 10, 11, 26 e 27) | 177 |
| [ ] | 8.125 | Registro D170 | Complemento do Conhecimento Multimodal de Cargas (cód. 26) | 178 |
| [ ] | 8.126 | Registro D180 | Modais (cód. 26) | 179 |
| [ ] | 8.127 | Registro D190 | Registro Analítico dos Documentos (cód. 07, 08, 8B, 09, 10, 11, 26, 27, 57, 63 e 67) | 180 |
| [ ] | 8.128 | Registro D195 | Observações do Lançamento Fiscal | 181 |
| [ ] | 8.129 | Registro D197 | Outras Obrigações Tributárias, Ajustes e Informações de Valores Provenientes do Documento Fiscal | 182 |
| [ ] | 8.130 | Registro D300 | Registro Analítico dos Bilhetes Consolidados de Passagem Rodoviário (13), Aquaviário (14), Passagem e Bagagem (15), Ferroviário (16) | 183 |
| [ ] | 8.131 | Registro D301 | Documentos Cancelados dos Bilhetes (13, 14, 15, 16) | 184 |
| [ ] | 8.132 | Registro D310 | Complemento dos Bilhetes (cód. 13, 14, 15 e 16) | 185 |
| [ ] | 8.133 | Registro D350 | Equipamento ECF (cód. 2E, 13, 14, 15 e 16) | 186 |
| [ ] | 8.134 | Registro D355 | Redução Z (cód. 2E, 13, 14, 15 e 16) | 187 |
| [ ] | 8.135 | Registro D360 | PIS e COFINS Totalizados no Dia (cód. 2E, 13, 14, 15 e 16) | 188 |
| [ ] | 8.136 | Registro D365 | Registro dos Totalizadores Parciais da Redução Z (cód. 2E, 13, 14, 15 e 16) | 189 |
| [ ] | 8.137 | Registro D370 | Complemento dos Documentos Informados (cód. 13, 14, 15, 16 e 2E) | 190 |
| [ ] | 8.138 | Registro D390 | Registro Analítico do Movimento Diário (cód. 13, 14, 15, 16 e 2E) | 191 |
| [ ] | 8.139 | Registro D400 | Resumo do Movimento Diário (cód. 18) | 192 |
| [ ] | 8.140 | Registro D410 | Documentos Informados (cód. 13, 14, 15 e 16) | 193 |
| [ ] | 8.141 | Registro D411 | Documentos Cancelados dos Documentos Informados (cód. 13, 14, 15 e 16) | 194 |
| [ ] | 8.142 | Registro D420 | Complemento dos Documentos Informados (cód. 13, 14, 15 e 16) | 195 |
| [ ] | 8.143 | Registro D500 | NF de Serviço de Comunicação (cód. 21) e Serviço de Telecomunicação (cód. 22) | 196 |
| [ ] | 8.144 | Registro D510 | Itens do Documento — NF Serviço Comunicação (21) e Telecomunicação (22) | 198 |
| [ ] | 8.145 | Registro D530 | Terminal Faturado | 199 |
| [ ] | 8.146 | Registro D590 | Registro Analítico do Documento (cód. 21 e 22) | 200 |
| [ ] | 8.147 | Registro D600 | Consolidação da Prestação de Serviços — NF Serviço Comunicação (21) e Telecomunicação (22) | 201 |
| [ ] | 8.148 | Registro D610 | Itens do Documento Consolidado (cód. 21 e 22) | 202 |
| [ ] | 8.149 | Registro D690 | Registro Analítico dos Documentos (cód. 21 e 22) | 203 |
| [ ] | 8.150 | Registro D695 | Consolidação da Prestação de Serviços — NF Serviço Comunicação (21) e Telecomunicação (22) | 204 |
| [ ] | 8.151 | Registro D696 | Registro Analítico dos Documentos (cód. 21 e 22) | 205 |
| [ ] | 8.152 | Registro D697 | Informações de Outras UFs — Serviços "Não Medidos" de Televisão por Assinatura via Satélite | 205 |
| [ ] | 8.153 | Registro D990 | Encerramento do Bloco D | 205 |

### Bloco E — Apuração do ICMS e do IPI (26 registros)

| Feito | Sub-stage | Registro | Descrição | Página PDF |
| --- | --- | --- | --- | --- |
| [ ] | 8.154 | Registro E001 | Abertura do Bloco E | 206 |
| [ ] | 8.155 | Registro E100 | Período de Apuração do ICMS | 206 |
| [ ] | 8.156 | Registro E110 | Apuração do ICMS — Operações Próprias | 207 |
| [ ] | 8.157 | Registro E111 | Ajuste/Benefício/Incentivo da Apuração do ICMS | 209 |
| [ ] | 8.158 | Registro E112 | Informações Adicionais dos Ajustes da Apuração do ICMS | 210 |
| [ ] | 8.159 | Registro E113 | Informações Adicionais dos Ajustes da Apuração do ICMS — Identificação dos Documentos Fiscais | 211 |
| [ ] | 8.160 | Registro E115 | Informações Adicionais da Apuração do ICMS — Valores Declaratórios | 212 |
| [ ] | 8.161 | Registro E116 | Obrigações do ICMS a Recolher — Obrigações Próprias | 213 |
| [ ] | 8.162 | Registro E200 | Período de Apuração do ICMS — Substituição Tributária | 214 |
| [ ] | 8.163 | Registro E210 | Apuração do ICMS — Substituição Tributária | 215 |
| [ ] | 8.164 | Registro E220 | Ajuste/Benefício/Incentivo da Apuração do ICMS — Substituição Tributária | 217 |
| [ ] | 8.165 | Registro E230 | Informações Adicionais dos Ajustes da Apuração do ICMS Substituição Tributária | 218 |
| [ ] | 8.166 | Registro E240 | Informações Adicionais dos Ajustes da Apuração do ICMS ST — Identificação dos Documentos Fiscais | 219 |
| [ ] | 8.167 | Registro E250 | Obrigações do ICMS a Recolher — Substituição Tributária | 220 |
| [ ] | 8.168 | Registro E300 | Período de Apuração do ICMS Diferencial de Alíquota — UF Origem/Destino EC 87/15 | 222 |
| [ ] | 8.169 | Registro E310 | Apuração do ICMS Diferencial de Alíquota — UF Origem/Destino EC 87/15 | 223 |
| [ ] | 8.170 | Registro E311 | Ajuste/Benefício/Incentivo da Apuração do ICMS Diferencial de Alíquota — UF Origem/Destino EC 87/15 | 225 |
| [ ] | 8.171 | Registro E312 | Informações Adicionais dos Ajustes da Apuração do ICMS Diferencial de Alíquota — EC 87/15 | 226 |
| [ ] | 8.172 | Registro E313 | Informações Adicionais da Apuração do ICMS Difal — EC 87/15 Identificação dos Documentos Fiscais | 227 |
| [ ] | 8.173 | Registro E316 | Obrigações do ICMS Recolhido ou a Recolher — Diferencial de Alíquota — UF Origem/Destino EC 87/15 | 228 |
| [ ] | 8.174 | Registro E500 | Período de Apuração do IPI | 230 |
| [ ] | 8.175 | Registro E510 | Consolidação dos Valores de IPI | 232 |
| [ ] | 8.176 | Registro E520 | Apuração do IPI | 233 |
| [ ] | 8.177 | Registro E530 | Ajustes da Apuração do IPI | 235 |
| [ ] | 8.178 | Registro E531 | Informações Adicionais dos Ajustes da Apuração do IPI — Identificação dos Documentos Fiscais (01 e 55) | 237 |
| [ ] | 8.179 | Registro E990 | Encerramento do Bloco E | 238 |

### Bloco G — CIAP — Controle de Crédito de ICMS do Ativo Permanente (7 registros)

| Feito | Sub-stage | Registro | Descrição | Página PDF |
| --- | --- | --- | --- | --- |
| [ ] | 8.180 | Registro G001 | Abertura do Bloco G | 239 |
| [ ] | 8.181 | Registro G110 | ICMS — Ativo Permanente — CIAP | 239 |
| [ ] | 8.182 | Registro G125 | Movimentação de Bem do Ativo Imobilizado | 240 |
| [ ] | 8.183 | Registro G126 | Outros Créditos CIAP | 241 |
| [ ] | 8.184 | Registro G130 | Identificação do Documento Fiscal | 242 |
| [ ] | 8.185 | Registro G140 | Identificação do Item do Documento Fiscal | 243 |
| [ ] | 8.186 | Registro G990 | Encerramento do Bloco G | 244 |

### Bloco H — Inventário Físico (6 registros)

| Feito | Sub-stage | Registro | Descrição | Página PDF |
| --- | --- | --- | --- | --- |
| [ ] | 8.187 | Registro H001 | Abertura do Bloco H | 245 |
| [ ] | 8.188 | Registro H005 | Totais do Inventário | 245 |
| [ ] | 8.189 | Registro H010 | Inventário | 246 |
| [ ] | 8.190 | Registro H020 | Informação Complementar do Inventário | 248 |
| [ ] | 8.191 | Registro H030 | Informações Complementares do Inventário de Mercadorias Sujeitas ao Regime de ST | 248 |
| [ ] | 8.192 | Registro H990 | Encerramento do Bloco H | 249 |

### Bloco K — Controle da Produção e do Estoque (22 registros)

| Feito | Sub-stage | Registro | Descrição | Página PDF |
| --- | --- | --- | --- | --- |
| [ ] | 8.193 | Registro K001 | Abertura do Bloco K | 249 |
| [ ] | 8.194 | Registro K100 | Período de Apuração do ICMS/IPI | 250 |
| [ ] | 8.195 | Registro K200 | Estoque Escriturado | 250 |
| [ ] | 8.196 | Registro K210 | Desmontagem de Mercadorias — Item de Origem | 251 |
| [ ] | 8.197 | Registro K215 | Desmontagem de Mercadorias — Item de Destino | 252 |
| [ ] | 8.198 | Registro K220 | Outras Movimentações Internas entre Mercadorias | 253 |
| [ ] | 8.199 | Registro K230 | Itens Produzidos | 254 |
| [ ] | 8.200 | Registro K235 | Insumos Consumidos | 255 |
| [ ] | 8.201 | Registro K250 | Industrialização Efetuada por Terceiros — Itens Produzidos | 256 |
| [ ] | 8.202 | Registro K255 | Industrialização em Terceiros — Insumos Consumidos | 257 |
| [ ] | 8.203 | Registro K260 | Reprocessamento/Reparo de Produto/Insumo | 258 |
| [ ] | 8.204 | Registro K265 | Reprocessamento/Reparo — Mercadorias Consumidas e/ou Retornadas | 259 |
| [ ] | 8.205 | Registro K270 | Correção de Apontamento dos Registros K210, K220, K230, K250, K260, K291, K292, K301 e K302 | 259 |
| [ ] | 8.206 | Registro K275 | Correção de Apontamento e Retorno de Insumos dos Registros K215, K220, K235, K255 e K265 | 261 |
| [ ] | 8.207 | Registro K280 | Correção de Apontamento — Estoque Escriturado | 263 |
| [ ] | 8.208 | Registro K290 | Produção Conjunta — Ordem de Produção | 265 |
| [ ] | 8.209 | Registro K291 | Produção Conjunta — Itens Produzidos | 266 |
| [ ] | 8.210 | Registro K292 | Produção Conjunta — Insumos Consumidos | 267 |
| [ ] | 8.211 | Registro K300 | Produção Conjunta — Industrialização Efetuada por Terceiros | 268 |
| [ ] | 8.212 | Registro K301 | Produção Conjunta — Industrialização Efetuada por Terceiros — Itens Produzidos | 270 |
| [ ] | 8.213 | Registro K302 | Produção Conjunta — Industrialização Efetuada por Terceiros — Insumos Consumidos | 271 |
| [ ] | 8.214 | Registro K990 | Encerramento do Bloco K | 273 |

### Bloco 1 — Outras Informações (37 registros)

| Feito | Sub-stage | Registro | Descrição | Página PDF |
| --- | --- | --- | --- | --- |
| [ ] | 8.215 | Registro 1001 | Abertura do Bloco 1 | 275 |
| [ ] | 8.216 | Registro 1010 | Obrigatoriedade de Registros do Bloco 1 | 275 |
| [ ] | 8.217 | Registro 1100 | Registro de Informações sobre Exportação | 276 |
| [ ] | 8.218 | Registro 1105 | Documentos Fiscais de Exportação | 277 |
| [ ] | 8.219 | Registro 1110 | Operações de Exportação Indireta — Mercadorias de Terceiros | 278 |
| [ ] | 8.220 | Registro 1200 | Controle de Créditos Fiscais — ICMS | 279 |
| [ ] | 8.221 | Registro 1210 | Utilização de Créditos Fiscais — ICMS | 280 |
| [ ] | 8.222 | Registro 1250 | Informações Consolidadas de Saldos de Restituição, Ressarcimento e Complementação do ICMS | 281 |
| [ ] | 8.223 | Registro 1255 | Informações Consolidadas de Saldos de Restituição, Ressarcimento e Complementação do ICMS por Motivo | 281 |
| [ ] | 8.224 | Registro 1300 | Movimentação Diária de Combustíveis | 282 |
| [ ] | 8.225 | Registro 1310 | Movimentação Diária de Combustíveis por Tanque | 282 |
| [ ] | 8.226 | Registro 1320 | Volume de Vendas | 283 |
| [ ] | 8.227 | Registro 1350 | Bombas | 283 |
| [ ] | 8.228 | Registro 1360 | Lacres das Bombas | 284 |
| [ ] | 8.229 | Registro 1370 | Bicos da Bomba | 284 |
| [ ] | 8.230 | Registro 1390 | Controle de Produção de Usina | 284 |
| [ ] | 8.231 | Registro 1391 | Produção Diária da Usina | 285 |
| [ ] | 8.232 | Registro 1400 | Informação sobre Valor Agregado | 285 |
| [ ] | 8.233 | Registro 1500 | NF/Conta de Energia Elétrica (cód. 06) — Operações Interestaduais | 285 |
| [ ] | 8.234 | Registro 1510 | Itens do Documento NF/Conta de Energia Elétrica (cód. 06) | 285 |
| [ ] | 8.235 | Registro 1600 | Total das Operações com Cartão de Crédito e/ou Débito | 286 |
| [ ] | 8.236 | Registro 1700 | Documentos Fiscais Utilizados | 286 |
| [ ] | 8.237 | Registro 1710 | Documentos Fiscais Cancelados/Inutilizados | 287 |
| [ ] | 8.238 | Registro 1800 | DCTA — Demonstrativo de Crédito do ICMS Sobre Transporte Aéreo | 288 |
| [ ] | 8.239 | Registro 1900 | Indicador de Sub-Apuração do ICMS | 288 |
| [ ] | 8.240 | Registro 1910 | Período da Sub-Apuração do ICMS | 290 |
| [ ] | 8.241 | Registro 1920 | Sub-Apuração do ICMS | 290 |
| [ ] | 8.242 | Registro 1921 | Ajuste/Benefício/Incentivo da Sub-Apuração do ICMS | 293 |
| [ ] | 8.243 | Registro 1922 | Informações Adicionais dos Ajustes da Sub-Apuração do ICMS | 293 |
| [ ] | 8.244 | Registro 1923 | Informações Adicionais dos Ajustes da Sub-Apuração do ICMS — Identificação dos Documentos Fiscais | 294 |
| [ ] | 8.245 | Registro 1925 | Informações Adicionais da Sub-Apuração — Valores Declaratórios | 295 |
| [ ] | 8.246 | Registro 1926 | Obrigações do ICMS a Recolher — Operações Referentes à Sub-Apuração | 295 |
| [ ] | 8.247 | Registro 1960 | GIAF 1 — Guia de Informação e Apuração de Incentivos Fiscais e Financeiros: Indústria (Crédito Presumido) | 296 |
| [ ] | 8.248 | Registro 1970 | GIAF 3 — Importação (Diferimento na Entrada e Crédito Presumido na Saída Subsequente) | 298 |
| [ ] | 8.249 | Registro 1975 | GIAF 3 — Importação (Saídas Internas por Faixa de Alíquota) | 299 |
| [ ] | 8.250 | Registro 1980 | GIAF 4 — Central de Distribuição (Entradas/Saídas) | 300 |
| [ ] | 8.251 | Registro 1990 | Encerramento do Bloco 1 | 301 |

### Bloco 9 — Controle e Encerramento do Arquivo Digital (4 registros)

| Feito | Sub-stage | Registro | Descrição | Página PDF |
| --- | --- | --- | --- | --- |
| [ ] | 8.252 | Registro 9001 | Abertura do Bloco 9 | 302 |
| [ ] | 8.253 | Registro 9900 | Registros do Arquivo | 302 |
| [ ] | 8.254 | Registro 9990 | Encerramento do Bloco 9 | 302 |
| [ ] | 8.255 | Registro 9999 | Encerramento do Arquivo Digital | 303 |

## Total

255 sub-stages (8.001 → 8.255), distribuídos:

| Bloco | Registros | Sub-stages |
| --- | --- | --- |
| 0 | 22 | 8.001 – 8.022 |
| B | 13 | 8.023 – 8.035 |
| C | 78 | 8.036 – 8.113 |
| D | 40 | 8.114 – 8.153 |
| E | 26 | 8.154 – 8.179 |
| G | 7 | 8.180 – 8.186 |
| H | 6 | 8.187 – 8.192 |
| K | 22 | 8.193 – 8.214 |
| 1 | 37 | 8.215 – 8.251 |
| 9 | 4 | 8.252 – 8.255 |
| **Total** | **255** | **8.001 – 8.255** |

Fonte oficial da ordem: Seção 2.6.1 do `Guia Prático EFD - Versão 3.0.6.pdf` (Tabela de Obrigatoriedade dos Registros, p303-320). Sub-stages numerados na ordem em que aparecem na tabela oficial.
