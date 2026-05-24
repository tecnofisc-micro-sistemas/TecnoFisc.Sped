# Stage 8 — EFD ICMS-IPI incremento V017 (Leiaute 2023)

> Incremento sobre V016 (`sped/STAGE_8_INCR_V016.md`). Pré-requisito: V016 concluído.
>
> **Modo:** read-only (ARCHITECTURE §2.5 + §4.7). Modelo único do leiaute mais recente — sem subclasses por versão. Atributos `DesdeVersao`/`IntroduzidoEm`/`Descontinuado` são informacionais.
> **Vigência fiscal:** períodos de apuração a partir de janeiro/2023.
> **Guides publicados durante este leiaute:** 3.1.0, 3.1.1, 3.1.2, 3.1.3, 3.1.4.
> **Fontes neste guia v3.2.2:** Subseção 12 (p. 17) e "Principais alterações no Guia Prático da EFD-ICMS/IPI – versão 3.1.0/3.1.1/3.1.2/3.1.3/3.1.4" (p. 359-360).
>
> **Atributos:** `[CampoSped(DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]`, `[RegistroSped(IntroduzidoEm = (int)LayoutEfdIcmsIpi.V017)]`. Membro `V017 = 17` criado first-use.

## Sub-stages

### Registros novos (NEW) — Bloco D NFCom (código 62) + extras

| Feito | Sub-stage | Tipo | Registro | Bloco | Resumo | Fonte |
| --- | --- | --- | --- | --- | --- | --- |
| [ ] | 8.017.002 | NEW | Registro 0221 | 0 | Correlação entre códigos de itens comercializados. Nível 3, 1:N, OC. | Subseção 12 + 3.1.0 item 2 |
| [ ] | 8.017.003 | NEW | Registro C855 | C | Observações do lançamento fiscal (código 59). Nível 3, 1:N, OC. | Subseção 12 + 3.1.0 item 2 |
| [ ] | 8.017.004 | NEW | Registro C857 | C | Outras obrigações tributárias, ajustes e informações de valores provenientes de documento fiscal. Nível 4, 1:N, OC. | Subseção 12 + 3.1.0 item 2 |
| [ ] | 8.017.005 | NEW | Registro C895 | C | Observações do lançamento fiscal (código 59) — bloco 89x. Nível 3, 1:N, OC. | Subseção 12 + 3.1.0 item 2 |
| [ ] | 8.017.006 | NEW | Registro C897 | C | Outras obrigações tributárias, ajustes — bloco 89x. Nível 4, 1:N, OC. | Subseção 12 + 3.1.0 item 2 |
| [x] | 8.017.007 | NEW | Registro D700 | D | Nota Fiscal Fatura Eletrônica de Serviços de Comunicação — NFCom (código 62). Nível 2, V, OC. **Campo 07 `SER` declarar como `string?` lazy** — V020 muda tipo N→C; modelo único aceita texto sempre. | Subseção 12 + 3.1.0 item 2 |
| [x] | 8.017.008 | NEW | Registro D730 | D | Registro analítico NFCom. Nível 3, 1:N, OC. | Subseção 12 + 3.1.0 item 2 |
| [x] | 8.017.009 | NEW | Registro D731 | D | Informações do fundo de combate à pobreza FCP. Nível 4, 1:1, OC. | Subseção 12 + 3.1.0 item 2 |
| [x] | 8.017.010 | NEW | Registro D735 | D | Observações do lançamento fiscal (código 62). Nível 3, 1:N, OC. | Subseção 12 + 3.1.0 item 2 |
| [x] | 8.017.011 | NEW | Registro D737 | D | Outras obrigações tributárias / ajustes — NFCom. Nível 4, 1:N, OC. | Subseção 12 + 3.1.0 item 2 |
| [x] | 8.017.012 | NEW | Registro D750 | D | Escrituração consolidada NFCom (código 62). Nível 2, 1:N, OC. **Campo 03 `COD_MOD` declarar como `string?` lazy** — V018 muda tipo C→N; modelo único aceita texto sempre. | Subseção 12 + 3.1.0 item 2 |
| [x] | 8.017.013 | NEW | Registro D760 | D | Registro analítico da escrituração consolidada NFCom. Nível 3, 1:N, OC. | Subseção 12 + 3.1.0 item 2 |
| [x] | 8.017.014 | NEW | Registro D761 | D | Informações do fundo de combate à pobreza FCP — consolidada. Nível 4, 1:1, OC. | Subseção 12 + 3.1.0 item 2 |

> **K010 movido para V016 (sub-stage 8.016.028).** Era 8.017.001 — não está mais neste tracker.

### Mudanças estruturais (UPDATE/Campo — tamanho in-place)

Registros que tiveram `Tam` aumentado em V017 (15→60 em vários casos). Modelo único atualiza o `[CampoSped(Tamanho=...)]` da property existente para o valor mais recente. Sem subclasses.

**PR único agregador (`8.017.049`):** todos os tamanhos podem ir em um único PR mecânico porque a edição é homogênea.

| Feito | Sub-stage | Registro | Bloco | Campo | Mudança | Fonte |
| --- | --- | --- | --- | --- | --- | --- |
| [ ] | 8.017.049.a | Registro C111 | C | 02 | Tam 15→60 | 3.1.0 item 8 |
| [ ] | 8.017.049.b | Registro E112 | E | 03 | Tam 15→60 | 3.1.0 item 9 |
| [ ] | 8.017.049.c | Registro E116 | E | 06 | Tam 15→60 | 3.1.0 item 10 |
| [ ] | 8.017.049.d | Registro E230 | E | 03 | Tam 15→60 | 3.1.0 item 9 |
| [ ] | 8.017.049.e | Registro E250 | E | 06 | Tam 15→60 | 3.1.0 item 10 |
| [ ] | 8.017.049.f | Registro E312 | E | 03 | Tam 15→60 | 3.1.0 item 9 |
| [ ] | 8.017.049.g | Registro E316 | E | 06 | Tam 15→60 | 3.1.0 item 10 |
| [ ] | 8.017.049.h | Registro 1922 | 1 | 03 | Tam 15→60 | 3.1.0 item 9 |
| [ ] | 8.017.049.i | Registro 1926 | 1 | 06 | Tam 15→60 | 3.1.0 item 10 |

### Enums Core (UPDATE/Campo — Core)

| Feito | Sub-stage | Alvo | Delta | Fonte |
| --- | --- | --- | --- | --- |
| [ ] | 8.017.050 | enum `SituacaoDocumento` (Core, Tabela 4.1.2) | Códigos 04 e 05 marcados com `[Descontinuado(EmVersao = V017)]` (informacional, parser continua reconhecendo). Atualizar doc-comment dos membros. | 3.1.0 item 1 |
| [ ] | 8.017.051 | Tabela CST ICMS (Core, Cap IV §1.1) | Atualizar `CstIcms` (enum/value object) com valores vigentes do guide 3.1.3. | 3.1.3 item 1 |

### Documentação (UPDATE/Doc — antigos UPDATE/Validação, UPDATE/Obrig)

**PR único agregador (`8.017.052`):** doc-comments mecânicos.

| Feito | Sub-stage | Registro | Bloco | Delta documentado | Fonte |
| --- | --- | --- | --- | --- | --- |
| [ ] | 8.017.052.a | Registro C100 | C | Orientação ICMS monofásico (Nota Orientativa 01/2023). Validação de duplicidade (`IND_EMIT`, `COD_SIT`, `COD_PART`, `SER`, `NUM_DOC` com exceção `COD_MOD` 55/65). | 3.1.3 item 2 + 3.1.4 item 1 |
| [ ] | 8.017.052.b | Registro C105 | C | Instrução do registro + valor válido "2" no campo 02. | 3.1.4 itens 4-5 |
| [ ] | 8.017.052.c | Registro C170 | C | Orientação e validação campos 05-06. | 3.1.0 item 4 + 3.1.3 item 6 |
| [ ] | 8.017.052.d | Registro C181 | C | Validação campo 02. | 3.1.0 item 6 |
| [ ] | 8.017.052.e | Registro C185 | C | Validação campo 06. | 3.1.0 item 7 |
| [ ] | 8.017.052.f | Registro C190 | C | Orientação campo 05 (retira termo FCP). | 3.1.1 item 1 |
| [ ] | 8.017.052.g | Registro C330 | C | Validação campo 02. | 3.1.0 item 6 |
| [ ] | 8.017.052.h | Registro C380 | C | Validação campo 02. | 3.1.0 item 6 |
| [ ] | 8.017.052.i | Registro C430 | C | Validação campo 02. | 3.1.0 item 6 |
| [ ] | 8.017.052.j | Registro C480 | C | Validação campo 02. | 3.1.0 item 6 |
| [ ] | 8.017.052.k | Registro C500 | C | NF3-e sem CST não escriturada neste registro. | 3.1.4 item 2 |
| [ ] | 8.017.052.l | Registro C590 | C | NF3-e sem CST nem energia injetada. | 3.1.4 item 3 |
| [ ] | 8.017.052.m | Registro C700 | C | Modelo 66 (NF3-e). Campos 08/09 O→OC. Orientação 06-09. NF3-e sem CST não escriturada. | 3.1.2 itens 1-3 + 3.1.4 item 2 |
| [ ] | 8.017.052.n | Registro C790 | C | Modelo 66. NF3-e sem CST nem energia injetada. | 3.1.2 item 1 + 3.1.4 item 3 |
| [ ] | 8.017.052.o | Registro C791 | C | Modelo 66. | 3.1.2 item 1 |
| [ ] | 8.017.052.p | Registro C800 | C | Exceção nº 2 + validação campo 09. | 3.1.0 item 3 + 3.1.4 item 6 |
| [ ] | 8.017.052.q | Registro C815 | C | Validação campo 02. | 3.1.0 item 6 |
| [ ] | 8.017.052.r | Registro C880 | C | Validação campo 02. | 3.1.0 item 6 |
| [ ] | 8.017.052.s | Registro E210 | E | Validação campo 08. | 3.1.4 item 7 |
| [ ] | 8.017.052.t | Registro 1391 | 1 | Valor válido "4" no campo 09. | 3.1.4 item 8 |
| [ ] | 8.017.052.u | Registro 1900 | 1 | Obrigatoriedade inclui registros C597, C857, C897, D737. | 3.1.1 item 2 |

## Notas arquiteturais

1. **NFCom (D700, D730, D731, D735, D737, D750, D760, D761):** hierarquia D700→D730→D731 e D700→D735→D737; consolidada D750→D760→D761. Implementar D700 e D750 **primeiro** (pais) — atenção aos campos `string?` lazy (D700/07 SER, D750/03 COD_MOD) para cobrir mudanças de tipo em V018/V020.
2. **C597:** mencionado em 3.1.1 item 2 mas não criado neste incremento. Já existe no baseline V015.
3. **Tamanhos 15→60 não exigem subclasse no read-only:** o parser não valida tamanho hoje (apenas truncates na escrita), e o pacote é read-only. Atualizar `Tamanho` in-place no `[CampoSped]`.

## Sumário

- **Total sub-stages:** 13 NEW + 9 UPDATE/Campo tamanho (1 PR agregador) + 2 UPDATE/Campo enum Core + 21 UPDATE/Doc (1 PR agregador). Em PRs: ~16 individuais para NEW/enums + 2 agregadores.
- **Pendentes:** 13 NEW + 2 enums Core + 2 PRs agregadores (tamanho + doc).
- **Concluídos:** 8 NEW (D700, D730, D731, D735, D737, D750, D760, D761).
