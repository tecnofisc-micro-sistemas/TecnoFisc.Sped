# Stage 8 — EFD ICMS-IPI incremento V019 (Leiaute 2025)

> Incremento sobre V018 (`sped/STAGE_8_INCR_V018.md`). Pré-requisito: V018 concluído.
>
> **Modo:** read-only (ARCHITECTURE §2.5 + §4.7). Modelo único do leiaute mais recente.
> **Vigência fiscal:** períodos de apuração a partir de janeiro/2025.
> **Guides publicados durante este leiaute:** 3.1.7, 3.1.8, 3.1.9.
> **Fontes neste guia v3.2.2:** Subseção 14 (p. 18) e "Principais alterações no Guia Prático da EFD-ICMS/IPI – versão 3.1.7/3.1.8/3.1.9" (p. 361).
>
> **Atributos:** `[CampoSped(DesdeVersao = (int)LayoutEfdIcmsIpi.V019)]`. Membro `V019 = 19` criado first-use.

## Sub-stages

### Campos novos em registros existentes (UPDATE/Campo)

| Feito | Sub-stage | Tipo | Registro | Bloco | Campos novos | Fonte |
| --- | --- | --- | --- | --- | --- | --- |
| [ ] | 8.019.001 | UPDATE/Campo | Registro D700 | D | 32 `DED` | Subseção 14 + 3.1.7 item 3 |
| [ ] | 8.019.002 | UPDATE/Campo | Registro D750 | D | 17 `DED` | Subseção 14 + 3.1.7 item 5 |

### Documentação (UPDATE/Doc — antigos UPDATE/Validação, UPDATE/Obrig)

**PR único agregador (`8.019.003`):** doc-comments mecânicos.

| Feito | Sub-stage | Registro | Bloco | Delta documentado | Fonte |
| --- | --- | --- | --- | --- | --- |
| [ ] | 8.019.003.a | Registro D700 | D | Campos 23 e 24 obrigatoriedade. Validação campo 11. *Coordenar com 8.019.001 — mesmo registro.* | 3.1.7 itens 2, 4 |
| [ ] | 8.019.003.b | Registro D750 | D | Campos 15 e 16 obrigatoriedade. Validação campo 07. *Coordenar com 8.019.002 — mesmo registro.* | 3.1.7 itens 6-7 |
| [ ] | 8.019.003.c | Registro C700 | C | Validação revisada. *Coordenar com 8.017.052.m/8.018.002.j.* | 3.1.7 item 1 |
| [ ] | 8.019.003.d | Registro E113 | E | Validação campo 02. | 3.1.7 item 8 |
| [ ] | 8.019.003.e | Registro D100 | D | Preenchimento campos 14, 24, 25. Validação campos 14, 18. Exceção nº 4. | 3.1.7 itens 9, 12-13 + 3.1.8 itens 1, 3 |
| [ ] | 8.019.003.f | Registro D130 | D | Inclusão de instrução sobre Conhecimento de transporte eletrônico simplificado (CT-e Simplificado). Preenchimento campos 02, 03, 05, 06. Descrição do registro alterada. | 3.1.7 itens 10-11 + 3.1.8 item 2 |
| [ ] | 8.019.003.g | Registro C120 | C | Valor válido "2" no campo 02. | 3.1.9 item 2 |
| [ ] | 8.019.003.h | Registro C100 | C | Regra desabilitada no campo 12. Observação sobre Reforma Tributária do Consumo (não escriturar documentos exclusivamente de novos tributos). | 3.1.9 itens 3-4 |
| [ ] | 8.019.003.i | Registro C190 | C | Regra desabilitada no campo 05. *Coordenar com 8.017.052.f (V017 — orientação FCP).* | 3.1.9 item 3 |
| [ ] | 8.019.003.j | Registro 0150 | 0 | Orientação adicionada: DIFAL EC 87/2015 (hipótese § 30 art. 19 Convênio SN/1970). | 3.1.9 item 5 |

## Notas arquiteturais

1. **Reforma Tributária do Consumo:** observação introduzida em V019 (C100). Documenta que documentos fiscais exclusivos de IBS/CBS/IS (novos tributos) **não** entram em EFD ICMS-IPI. Versão V020 (3.2.0/3.2.1/3.2.2) refina essa regra com Ajuste SINIEF 49/25. Regra é orientação de uso (consumidor) — fora do escopo de parse.

## Sumário

- **Total sub-stages:** 2 UPDATE/Campo + 1 PR agregador (10 itens de doc).
- **Pendentes:** 2 + 1 PR agregador.
- **Concluídos:** 0.
