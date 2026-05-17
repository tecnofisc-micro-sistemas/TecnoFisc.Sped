# Stage 8 — EFD ICMS-IPI incremento V020 (Leiaute 2026)

> Incremento sobre V019 (`sped/STAGE_8_INCR_V019.md`). Pré-requisito: V019 concluído.
>
> **Vigência fiscal:** períodos de apuração a partir de janeiro/2026 — **vigente agora** (data corrente 2026-05).
> **Guides publicados durante este leiaute:** 3.2.0, 3.2.1, 3.2.2.
> **Fontes neste guia v3.2.2:** Subseção 15 (p. 18) e "Principais alterações no Guia Prático da EFD-ICMS/IPI – versão 3.2.0/3.2.1/3.2.2" (p. 361-362).
>
> **Atributos:** `[CampoSped(DesdeVersao = (int)LayoutEfdIcmsIpi.V020)]`. Membro `V020 = 20` criado first-use.

## Sub-stages

### Campos novos em registros existentes (UPDATE/Campo)

| Feito | Sub-stage | Tipo | Registro | Bloco | Campos novos | Fonte |
| --- | --- | --- | --- | --- | --- | --- |
| [ ] | 8.020.001 | UPDATE/Campo | Registro 1310 | 1 | 11 `CAP_TANQUE`. Equivalente a "Criação do campo 11 no registro 1310" (3.1.9 item 1) — vigência fiscal só em V020 (Subseção 15). | Subseção 15 + 3.1.9 item 1 |

### Mudanças estruturais (UPDATE)

| Feito | Sub-stage | Tipo | Registro | Bloco | Delta | Fonte |
| --- | --- | --- | --- | --- | --- | --- |
| [ ] | 8.020.002 | UPDATE/Doc | Registro D700 | D | Orientação de preenchimento campo 04. *Coordenar com cadeia V017/V018/V019 — mesmo registro tocado em todas as versões.* | 3.2.0 item 1 |
| [ ] | 8.020.003 | UPDATE/Doc | Cap. I Seção 10 (Reforma Tributária) | — | Inclusão de Capítulo I Seção 10 — "Informações sobre a Reforma Tributária sobre o Consumo". Atualizar doc do módulo (README/CHANGELOG). Não é registro. | 3.2.0 item 2 + 3.2.1 item 3 |
| [ ] | 8.020.004 | UPDATE/Doc | Registro K230 | K | Orientação adicionada. | 3.2.0 item 3 |
| [ ] | 8.020.005 | UPDATE/Subclasse | Registro D700 | D | Tipo campo 07 `SER` N→C. `RegistroD700V020 : RegistroD700`. **Quebra round-trip** entre V019 e V020 — campo serializado como C aceita texto. | 3.2.1 item 1 |
| [ ] | 8.020.006 | UPDATE/Doc | Registro 0150 | 0 | Orientação de preenchimento. *Coordenar com 8.019.012 (V019 — DIFAL EC 87/2015).* | 3.2.1 item 2 |
| [ ] | 8.020.007 | UPDATE/Doc | Registro C100 | C | Orientação campo 12 (`VL_DOC`). Exceção nº 11. Orientação Reforma Tributária + Ajuste SINIEF 49/25 (operações que envolvem ambos novos tributos e ICMS/IPI devem ser regularmente escrituradas). *Coordenar com 8.019.010 (V019 — observação Reforma Tributária inicial).* | 3.2.1 item 4 + 3.2.2 itens 1-2 |
| [ ] | 8.020.008 | UPDATE/Doc | Registro D100 | D | Orientação campo 25 (`COD_MUN_DEST`). *Coordenar com 8.019.007 (V019 — D100 amplo).* | 3.2.1 item 5 |

## Notas arquiteturais

1. **D700 tipo N→C campo 07 (V020):** primeiro caso real de mudança de tipo na cadeia. Valida o padrão ARCHITECTURE §4.7. Documentar no PR como referência para futuras mudanças.
2. **Reforma Tributária do Consumo:** ICMS/IPI **e** novos tributos (IBS/CBS/IS) na mesma operação ⇒ escriturar normalmente. Documentos exclusivos dos novos tributos ⇒ não escriturar. Implementação: validador opcional cross-doc (não bloqueia parse). Stages futuras (módulo separado `TecnoFisc.Sped.EfdReformaTributaria`?) podem cobrir os novos tributos. **Fora do escopo deste incremento.**
3. **C100 exceção 11 (V020) + exceções anteriores:** o registro C100 acumula exceções desde V015. Catalogar todas em local único (doc-comment ou arquivo `RegistroC100Excecoes.md`).

## Sumário

- **Total sub-stages:** 8 (0 NEW + 1 UPDATE/Campo + 7 UPDATE diversos).
- **Pendentes:** 8.
- **Concluídos:** 0.

---

## Estado consolidado da cadeia V016 → V020

| Versão | Leiaute | Guides cobertos | NEW | UPDATE | Total | Status |
| --- | --- | --- | --- | --- | --- | --- |
| V015 | 2021 | 3.0.6 (e anteriores dentro da janela) | 255 | — | 255 | ✅ 100% |
| V016 | 2022 | 3.0.7, 3.0.8, 3.0.9 | 1 | 26 | 27 | ⏳ pendente |
| V017 | 2023 | 3.1.0, 3.1.1, 3.1.2, 3.1.3, 3.1.4 | 14 | 34 | 48 | ⏳ pendente |
| V018 | 2024 | 3.1.5, 3.1.6 | 0 | 12 | 12 | ⏳ pendente |
| V019 | 2025 | 3.1.7, 3.1.8, 3.1.9 | 0 | 12 | 12 | ⏳ pendente |
| V020 | 2026 | 3.2.0, 3.2.1, 3.2.2 | 0 | 8 | 8 | ⏳ pendente |
| **Total incrementos V016-V020** | | | **15** | **92** | **107** | |

Incrementos devem ser executados **em ordem** (V016 → V017 → … → V020). Cada incremento assume baseline anterior concluído.
