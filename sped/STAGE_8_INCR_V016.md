# Stage 8 — EFD ICMS-IPI incremento V016 (Leiaute 2022)

> Incremento sobre o baseline V015 (`sped/STAGE_8_EFD_ICMS_IPI_V015.md`). **Não** repete a lista de registros do baseline — descreve apenas o delta.
>
> **Modo:** read-only (ARCHITECTURE §2.5 + §4.7). Modelo único do leiaute mais recente — sem subclasses por versão. Atributos `DesdeVersao`/`IntroduzidoEm`/`Descontinuado` são informacionais.
> **Vigência fiscal:** períodos de apuração a partir de janeiro/2022.
> **Guides publicados durante este leiaute:** 3.0.7, 3.0.8, 3.0.9. Cada item do tracking referencia a publicação que introduziu a mudança.
> **Fontes neste guia v3.2.2:** Subseção 11 (p. 16) e "Principais alterações no Guia Prático da EFD-ICMS/IPI – versão 3.0.7/3.0.8/3.0.9" (p. 358-359).
>
> **Atributos:** novos campos usam `[CampoSped(DesdeVersao = (int)LayoutEfdIcmsIpi.V016)]`; registros novos usam `[RegistroSped(IntroduzidoEm = (int)LayoutEfdIcmsIpi.V016)]`. O membro `V016 = 16` deve ser criado em `LayoutEfdIcmsIpi.cs` pelo PR do primeiro sub-stage que o consumir (first-use).

## Como usar este incremento

1. Selecionar próximo sub-stage `[ ]` (smallest `8.016.NNN`).
2. Determinar **modo de operação** pela coluna *Tipo*:
   - **NEW** — registro inexistente em V015. Modo CREATE do `/implementar-registro` (template baseline). Classe nova em `Registros/Bloco{X}/Registro{CODE}.cs`. `[RegistroSped(IntroduzidoEm = (int)LayoutEfdIcmsIpi.V016)]`.
   - **UPDATE/Campo** — campo novo em registro existente OU mudança de atributo do `[CampoSped]` (tamanho, decimais) in-place na property existente. `Edit` adiciona property nova no fim com `DesdeVersao = (int)LayoutEfdIcmsIpi.V016` ou atualiza o atributo da property existente. Doc-comment atualizado.
   - **UPDATE/Descontinuado** — registro deixa de ser usado em versões futuras. Marca `[Descontinuado(EmVersao = (int)LayoutEfdIcmsIpi.V016)]` (informacional — parser continua aceitando para ler arquivos históricos, ARCHITECTURE §4.7 read-only).
   - **UPDATE/Doc** — qualquer mudança puramente textual ou que afete apenas validação/obrigatoriedade fiscal (`UPDATE/Validação`, `UPDATE/Obrig` do tracker antigo). ARCHITECTURE §2.3 mantém validações fiscais fora do escopo da library — registro como doc-comment XML no campo/registro afetado.
3. Abrir PDF (`sped/guides/Guia Prático EFD - Versão 3.2.2.pdf`) só nas páginas indicadas. Para NEW: página do registro em Cap. III. Para UPDATE: ler a página do registro **e** a entrada correspondente em p. 358-359 (alterações do guide).
4. Tests:
   - NEW: cobertura completa (igual baseline). Como pacote é read-only, não há test de geração — apenas parser + catálogo + atributos.
   - UPDATE/Campo: adicionar fixtures que exercitam o delta sem tocar nos tests baseline. Parse com campo novo populado + vazio (compat baseline).
   - UPDATE/Doc: nenhum teste novo. Doc-comment é prosa.
5. **Commits granulares** dentro do PR (1 commit por step coeso — implementação, tests, tracking). Squash-Merge no `dev` é regra dura do repo — o merge consolida o branch em um único commit. **Não** rebase/amend antes do merge.
6. Tracking marcado **antes** do commit final do PR.

## Sub-stages

Numeração: `8.{versão}.{seq}`. Sequência é local da versão.

### Registros novos (NEW)

| Feito | Sub-stage | Tipo | Registro | Bloco | Resumo | Fonte |
| --- | --- | --- | --- | --- | --- | --- |
| [x] | 8.016.001 | NEW | Registro 1601 | 1 | Operações com instrumentos de pagamentos eletrônicos. Nível 2, 1:N, OC. Substitui Registro 1600. | Subseção 11 (p.16) + 3.0.7 item 6 |
| [ ] | 8.016.028 | NEW | Registro K010 | K | Tipo de leiaute K (simplificado/completo). Nível 2, 1, OC. Movido de V017 — facultativo em 2022 (3.0.9 item 3) antes da obrigatoriedade em V017. `IntroduzidoEm = V016`. | Subseção 11 + 3.0.9 item 3 |

### Campos novos / mudança de atributo em registros existentes (UPDATE/Campo)

| Feito | Sub-stage | Tipo | Registro | Bloco | Campos / Delta | Fonte |
| --- | --- | --- | --- | --- | --- | --- |
| [x] | 8.016.002 | UPDATE/Campo | Registro 0220 | 0 | 04 `COD_BARRA` | Subseção 11 (p.16) + 3.0.7 item 10 |
| [x] | 8.016.003 | UPDATE/Campo | Registro C500 | C | 34 `COD_MOD_DOC_REF`, 35 `HASH_DOC_REF`, 36 `SER_DOC_REF`, 37 `NUM_DOC_REF`, 38 `MES_DOC_REF`, 39 `ENER_INJET`, 40 `OUTRAS_DED` | Subseção 11 (p.16) + 3.0.7 itens 11-12 |
| [x] | 8.016.006 | UPDATE/Campo | Registro C120 | C | Campo 03 `Tamanho` atualizado 12→15 in-place no `[CampoSped]` da property existente. Pacote read-only não distingue versões para tamanho — valor máximo do leiaute mais recente. | 3.0.7 item 4 |

### Mudanças estruturais (UPDATE/Descontinuado)

| Feito | Sub-stage | Tipo | Registro | Bloco | Delta | Fonte |
| --- | --- | --- | --- | --- | --- | --- |
| [x] | 8.016.004 | UPDATE/Descontinuado | Registro 1600 | 1 | Término da utilização. Substituído por Registro 1601 a partir de V016. Anotação informacional — parser continua aceitando para arquivos históricos. | 3.0.7 item 6 |
| [x] | 8.016.005 | UPDATE/Descontinuado | Registro 0210 | 0 | Término da utilização. Anotação informacional. | 3.0.7 item 22 |

### Documentação (UPDATE/Doc — antigos UPDATE/Validação, UPDATE/Obrig)

Conforme ARCHITECTURE §2.3, validações fiscais (obrigatoriedade condicional, regras cross-registro) ficam com o consumidor. Cada item abaixo vira doc-comment XML no campo/registro afetado, descrevendo a regra para referência do consumidor — sem código de validação no projeto.

**PR único agregador (`8.016.029`):** todos os itens podem entrar como um único PR de doc-comments porque a edição é mecânica e não há lógica nova. Subdividir só se um registro tiver mais de uma página de prosa.

| Feito | Sub-stage | Registro | Bloco | Delta documentado | Fonte |
| --- | --- | --- | --- | --- | --- |
| [ ] | 8.016.029.a | Registro D100 | D | Campos 24 `VL_PIS` e 25 `VL_COFINS` OC→O. Validação alterada do campo 11. | 3.0.7 itens 1-2, 29 |
| [ ] | 8.016.029.b | Registro D410 | D | `VL_BC_ICMS` e `VL_ICMS` O→OC. | 3.0.7 item 3 |
| [ ] | 8.016.029.c | Registro D420 | D | `VL_BC_ICMS` e `VL_ICMS` O→OC. | 3.0.7 item 3 |
| [ ] | 8.016.029.d | Registro D500 | D | `VL_BC_ICMS` e `VL_ICMS` O→OC. | 3.0.7 item 3 |
| [ ] | 8.016.029.e | Registro D600 | D | `VL_BC_ICMS` e `VL_ICMS` O→OC. | 3.0.7 item 3 |
| [ ] | 8.016.029.f | Registro C176 | C | Campos 12-15 OC→O. Orientação 12/14/15. Descrição campo 18. Validação retira ">0" e exige `COD_RESP_RET="2"` no campo 14. | 3.0.7 itens 16-18 + 3.0.9 itens 1-2 |
| [ ] | 8.016.029.g | Registro C170 | C | Validação adicional no campo 06. | 3.0.7 item 8 |
| [ ] | 8.016.029.h | Registro C425 | C | Validação adicional no campo 04. | 3.0.7 item 9 |
| [ ] | 8.016.029.i | Registro C500 | C | Validação alterada nos campos 13, 15 e 30. Orientação dos campos 12, 16, 17, 20 e 22. *Coordenar com 8.016.003 — mesmo registro tocado.* | 3.0.7 itens 12-13 |
| [ ] | 8.016.029.j | Registro C590 | C | Orientação do campo 05 (entrega de NF3-e). | 3.0.7 item 14 |
| [ ] | 8.016.029.k | Registro 0200 | 0 | Validação alterada. | 3.0.7 item 15 |
| [ ] | 8.016.029.l | Registro B020 | B | Inclusão de NF3-e (cód. 66). Validações dos campos 04, 07 e 09. | 3.0.7 itens 19-20 |
| [ ] | 8.016.029.m | Registro 1010 | 1 | Redação do campo 08. | 3.0.7 item 21 |
| [ ] | 8.016.029.n | Registro C180 | C | Descrição do campo 11. | 3.0.7 item 23 |
| [ ] | 8.016.029.o | Registro E250 | E | Nova validação do campo 05 (`MES_REF`). | 3.0.7 item 5 |
| [ ] | 8.016.029.p | Registro E316 | E | Nova validação do campo 05 (`MES_REF`). | 3.0.7 item 5 |
| [ ] | 8.016.029.q | Registro E530 | E | Validação alterada no campo 04. | 3.0.7 item 7 |
| [ ] | 8.016.029.r | Registro K235 | K | Regra de validação revisada. | 3.0.9 item 4 |
| [ ] | 8.016.029.s | Registro K255 | K | Regra de validação revisada. | 3.0.9 item 4 |
| [ ] | 8.016.029.t | Registro K292 | K | Regra de validação revisada. | 3.0.9 item 4 |
| [ ] | 8.016.029.u | Registro K302 | K | Regra de validação revisada. | 3.0.9 item 4 |

## Notas arquiteturais

1. **`LayoutEfdIcmsIpi.V016`:** criado em PR de `8.016.001` (concluído). Atualizar doc-comment do `Registro0000.cs` linha 23 se ainda referenciar a numeração antiga `306` (resíduo do rename V306→V015).
2. **K010 — V016 ou V017?** Decidido em 2026-05-23: V016 (sub-stage 8.016.028). Razão: Guide 3.0.9 item 3 introduziu K010 já em 2022 (facultativo). V017 reforça obrigatoriedade, mas isso vira UPDATE/Doc ou regra do consumidor. Pacote read-only aceita K010 em qualquer arquivo a partir de V016.

## Sumário

- **Total sub-stages:** 7 atômicos (2 NEW + 3 UPDATE/Campo + 2 UPDATE/Descontinuado) + 1 PR agregador (21 itens de doc, todos triviais).
- **Pendentes:** 1 atômico (8.016.028 K010 NEW) + 1 PR agregador (8.016.029).
- **Concluídos:** 6 atômicos (8.016.001-006).
