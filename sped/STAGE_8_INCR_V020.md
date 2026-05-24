# Stage 8 — EFD ICMS-IPI incremento V020 (Leiaute 2026)

> Incremento sobre V019 (`sped/STAGE_8_INCR_V019.md`). Pré-requisito: V019 concluído.
>
> **Modo:** read-only (ARCHITECTURE §2.5 + §4.7). Modelo único do leiaute mais recente — **este é o leiaute alvo do modelo** (data corrente 2026-05).
> **Vigência fiscal:** períodos de apuração a partir de janeiro/2026 — **vigente agora**.
> **Guides publicados durante este leiaute:** 3.2.0, 3.2.1, 3.2.2.
> **Fontes neste guia v3.2.2:** Subseção 15 (p. 18) e "Principais alterações no Guia Prático da EFD-ICMS/IPI – versão 3.2.0/3.2.1/3.2.2" (p. 361-362).
>
> **Atributos:** `[CampoSped(DesdeVersao = (int)LayoutEfdIcmsIpi.V020)]`. Membro `V020 = 20` criado first-use.

## Sub-stages

### Campos novos em registros existentes (UPDATE/Campo)

| Feito | Sub-stage | Tipo | Registro | Bloco | Campos novos | Fonte |
| --- | --- | --- | --- | --- | --- | --- |
| [ ] | 8.020.001 | UPDATE/Campo | Registro 1310 | 1 | 11 `CAP_TANQUE`. Equivalente a "Criação do campo 11 no registro 1310" (3.1.9 item 1) — vigência fiscal só em V020 (Subseção 15). | Subseção 15 + 3.1.9 item 1 |

### Documentação (UPDATE/Doc — antigos UPDATE/Validação, UPDATE/Subclasse, UPDATE/Doc)

**PR único agregador (`8.020.002`):** doc-comments mecânicos.

| Feito | Sub-stage | Registro | Bloco | Delta documentado | Fonte |
| --- | --- | --- | --- | --- | --- |
| [ ] | 8.020.002.a | Registro D700 | D | Orientação de preenchimento campo 04. **Campo 07 `SER` tipo N→C — já está modelado como `string?` lazy desde 8.017.007, nada a alterar no código.** *Coordenar com cadeia V017/V018/V019 — mesmo registro tocado em todas as versões.* | 3.2.0 item 1 + 3.2.1 item 1 |
| [ ] | 8.020.002.b | Cap. I Seção 10 (Reforma Tributária — README/CHANGELOG) | — | Inclusão de Capítulo I Seção 10 — "Informações sobre a Reforma Tributária sobre o Consumo". Atualizar README/CHANGELOG do módulo. Não é registro. | 3.2.0 item 2 + 3.2.1 item 3 |
| [ ] | 8.020.002.c | Registro K230 | K | Orientação adicionada. | 3.2.0 item 3 |
| [ ] | 8.020.002.d | Registro 0150 | 0 | Orientação de preenchimento. *Coordenar com 8.019.003.j (V019 — DIFAL EC 87/2015).* | 3.2.1 item 2 |
| [ ] | 8.020.002.e | Registro C100 | C | Orientação campo 12 (`VL_DOC`). Exceção nº 11. Orientação Reforma Tributária + Ajuste SINIEF 49/25 (operações que envolvem ambos novos tributos e ICMS/IPI devem ser regularmente escrituradas). *Coordenar com 8.019.003.h (V019 — observação Reforma Tributária inicial).* | 3.2.1 item 4 + 3.2.2 itens 1-2 |
| [ ] | 8.020.002.f | Registro D100 | D | Orientação campo 25 (`COD_MUN_DEST`). *Coordenar com 8.019.003.e (V019 — D100 amplo).* | 3.2.1 item 5 |

## Notas arquiteturais

1. **D700/07 tipo N→C (V020):** modelo único declara `string?` lazy desde a criação em V017 (sub-stage 8.017.007). Decisão registrada em 2026-05-23 como padrão para campos com regressão/mudança de tipo entre versões em pacotes read-only (ARCHITECTURE §4.7).
2. **Reforma Tributária do Consumo:** ICMS/IPI **e** novos tributos (IBS/CBS/IS) na mesma operação ⇒ escriturar normalmente. Documentos exclusivos dos novos tributos ⇒ não escriturar. Regra de uso/consumidor. Eventual pacote separado `TecnoFisc.Sped.EfdReformaTributaria` cobriria os novos tributos. **Fora do escopo deste incremento.**
3. **C100 exceção 11 (V020) + exceções anteriores:** o registro C100 acumula exceções desde V015. Catalogar todas em local único (doc-comment ou arquivo `RegistroC100Excecoes.md`).

## Sumário

- **Total sub-stages:** 1 UPDATE/Campo + 1 PR agregador (6 itens de doc).
- **Pendentes:** 1 + 1 PR agregador.
- **Concluídos:** 0.

---

## Estado consolidado da cadeia V016 → V020 (read-only)

| Versão | Leiaute | Guides cobertos | NEW | UPDATE/Campo | UPDATE/Descontinuado | UPDATE/Doc agregado | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| V015 | 2021 | 3.0.6 (e anteriores dentro da janela) | 255 | — | — | — | ✅ 100% |
| V016 | 2022 | 3.0.7, 3.0.8, 3.0.9 | 2 (Registro1601, K010) | 3 (0220, C500, C120 tam) | 2 (1600, 0210) | 1 PR (21 itens) | ⏳ 5 atômicos prontos, 2 + 1 PR pendentes |
| V017 | 2023 | 3.1.0, 3.1.1, 3.1.2, 3.1.3, 3.1.4 | 13 (NFCom + extras) | 11 (9 tamanhos + 2 enums Core) | 0 | 1 PR (21 itens) | ⏳ pendente |
| V018 | 2024 | 3.1.5, 3.1.6 | 0 | 1 (1391) | 0 | 1 PR (11 itens) | ⏳ pendente |
| V019 | 2025 | 3.1.7, 3.1.8, 3.1.9 | 0 | 2 (D700, D750 DED) | 0 | 1 PR (10 itens) | ⏳ pendente |
| V020 | 2026 | 3.2.0, 3.2.1, 3.2.2 | 0 | 1 (1310 CAP_TANQUE) | 0 | 1 PR (6 itens) | ⏳ pendente |
| **Total incrementos V016-V020 read-only** | | | **15 NEW** | **18 UPDATE/Campo** | **2 UPDATE/Descontinuado** | **5 PRs agregadores (~80 doc-comments)** | |

**Comparação com plano antigo (pré read-only):** 107 sub-stages → **~40 atômicos + 5 PRs agregadores de doc**. Redução de ~63% em complexidade de tracking sem perda de cobertura de leitura.

Incrementos devem ser executados **em ordem** (V016 → V017 → … → V020). Cada incremento assume baseline anterior concluído.
