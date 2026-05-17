# Stage 8 — EFD ICMS-IPI incremento V018 (Leiaute 2024)

> Incremento sobre V017 (`sped/STAGE_8_INCR_V017.md`). Pré-requisito: V017 concluído.
>
> **Vigência fiscal:** períodos de apuração a partir de janeiro/2024.
> **Guides publicados durante este leiaute:** 3.1.5, 3.1.6.
> **Fontes neste guia v3.2.2:** Subseção 13 (p. 17-18) e "Principais alterações no Guia Prático da EFD-ICMS/IPI – versão 3.1.5/3.1.6" (p. 360-361).
>
> **Atributos:** `[CampoSped(DesdeVersao = (int)LayoutEfdIcmsIpi.V018)]`. Membro `V018 = 18` criado first-use.

## Sub-stages

### Campos novos em registros existentes (UPDATE/Campo)

| Feito | Sub-stage | Tipo | Registro | Bloco | Campos novos | Fonte |
| --- | --- | --- | --- | --- | --- | --- |
| [ ] | 8.018.001 | UPDATE/Campo | Registro 1391 | 1 | 21 `QTD_RESIDUO_DDG`, 22 `QTD_RESIDUO_WDG`, 23 `QTD_RESIDUO_CANA`. *Coordenar com 8.017.045 (campo 09 já tocado em V017).* | Subseção 13 + 3.1.4 item 9 |

### Mudanças estruturais (UPDATE)

| Feito | Sub-stage | Tipo | Registro | Bloco | Delta | Fonte |
| --- | --- | --- | --- | --- | --- | --- |
| [ ] | 8.018.002 | UPDATE/Validação | Registro 1400 | 1 | Orientação geral + descrição campo 02 + validação campos 02 e 03. | 3.1.5 itens 1-3 |
| [ ] | 8.018.003 | UPDATE/Obrig | Registro D700 | D | Campo 07 OC→O. Campo 22 OC→O. Orientação geral. Descrição campo 10. Validação campo 11. Exclusão da validação do campo 23. | 3.1.5 itens 4-6 + 3.1.6 itens 1-3 |
| [ ] | 8.018.004 | UPDATE/Doc | Registro D730 | D | Orientação geral. Descrição campo 05. Orientação campo 05. | 3.1.5 item 7 + 3.1.6 itens 4-5 |
| [ ] | 8.018.005 | UPDATE/Subclasse | Registro D750 | D | Orientação geral. Tipo campo 03 C→N (`RegistroD750V018 : RegistroD750`). Chave do registro retira `COD_MUN_DEST`. Orientação campo 07. | 3.1.5 itens 8-10 + 3.1.6 item 6 |
| [ ] | 8.018.006 | UPDATE/Doc | Registro D760 | D | Orientação geral. Descrição campo 05. Orientação campo 05. | 3.1.5 item 11 + 3.1.6 itens 7-8 |
| [ ] | 8.018.007 | UPDATE/Validação | Registro E110 | E | Validação campos 02 (inclui D700/D730/D750/D760), 03 (C800/C857/C860/C897/D700/D737), 06 (D700/D730), 07 (mesmos), 12 (mesmos) e 15 (C857/C897/D737). | 3.1.5 itens 12-17 |
| [ ] | 8.018.008 | UPDATE/Validação | Registro E113 | E | Orientação e validação do campo 10. | 3.1.5 item 18 |
| [ ] | 8.018.009 | UPDATE/Validação | Registro E210 | E | Validação campos 07 e 10 (C800/C857/C860/C897/D700/D737). Orientação/validação campo 15 (C857/C897/D737). | 3.1.5 itens 19-21 |
| [ ] | 8.018.010 | UPDATE/Validação | Registro E240 | E | Orientação e validação do campo 10. | 3.1.5 item 22 |
| [ ] | 8.018.011 | UPDATE/Doc | Registro C700 | C | Orientação geral. *Coordenar com 8.017.032 — mesmo registro tocado em V017.* | 3.1.5 item 23 |
| [ ] | 8.018.012 | UPDATE/Doc | Cabeçalho Seção 2 | — | Convênio 115/03 NF3e cód. 66 + C700 (UF cuja legislação permite escrituração consolidada). Atualizar documentação/README do módulo — não é registro. | 3.1.5 item 24 |

## Notas arquiteturais

1. **D700 em V018 já existe da V017 (8.017.007).** Modo UPDATE encadeia sobre o que V017 produziu. Se V017 ainda não estiver concluído, parar — incrementos são sequenciais.
2. **D750 mudança de chave** (retira `COD_MUN_DEST`) afeta deduplicação de duplicidade em parser. Verificar `RegistroD750.ChaveSped` ou helper equivalente.
3. **Tipo `C→N` em D750/03:** estratégia subclasse confirma ARCHITECTURE §4.7. Round-trip de arquivos V017 ainda lê campo como string; V018 e posteriores como numérico.

## Sumário

- **Total sub-stages:** 12 (0 NEW + 1 UPDATE/Campo + 11 UPDATE diversos).
- **Pendentes:** 12.
- **Concluídos:** 0.
