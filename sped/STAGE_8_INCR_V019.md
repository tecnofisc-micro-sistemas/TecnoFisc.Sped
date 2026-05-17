# Stage 8 — EFD ICMS-IPI incremento V019 (Leiaute 2025)

> Incremento sobre V018 (`sped/STAGE_8_INCR_V018.md`). Pré-requisito: V018 concluído.
>
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

### Mudanças estruturais (UPDATE)

| Feito | Sub-stage | Tipo | Registro | Bloco | Delta | Fonte |
| --- | --- | --- | --- | --- | --- | --- |
| [ ] | 8.019.003 | UPDATE/Obrig | Registro D700 | D | Campos 23 e 24 obrigatoriedade. Validação campo 11. *Coordenar com 8.019.001 — mesmo registro.* | 3.1.7 itens 2, 4 |
| [ ] | 8.019.004 | UPDATE/Obrig | Registro D750 | D | Campos 15 e 16 obrigatoriedade. Validação campo 07. *Coordenar com 8.019.002 — mesmo registro.* | 3.1.7 itens 6-7 |
| [ ] | 8.019.005 | UPDATE/Validação | Registro C700 | C | Validação revisada. *Coordenar com 8.017.032/8.018.011.* | 3.1.7 item 1 |
| [ ] | 8.019.006 | UPDATE/Validação | Registro E113 | E | Validação campo 02. | 3.1.7 item 8 |
| [ ] | 8.019.007 | UPDATE/Validação | Registro D100 | D | Preenchimento campos 14, 24, 25. Validação campos 14, 18. Exceção nº 4. | 3.1.7 itens 9, 12-13 + 3.1.8 itens 1, 3 |
| [ ] | 8.019.008 | UPDATE/Doc | Registro D130 | D | Inclusão de instrução sobre Conhecimento de transporte eletrônico simplificado (CT-e Simplificado). Preenchimento campos 02, 03, 05, 06. Descrição do registro alterada. | 3.1.7 itens 10-11 + 3.1.8 item 2 |
| [ ] | 8.019.009 | UPDATE/Validação | Registro C120 | C | Valor válido "2" no campo 02. *Coordenar com 8.016.006 (subclasse V016) — V019 estende subclasse ou cria nova V019.* | 3.1.9 item 2 |
| [ ] | 8.019.010 | UPDATE/Validação | Registro C100 | C | Regra desabilitada no campo 12. Observação sobre Reforma Tributária do Consumo (não escriturar documentos exclusivamente de novos tributos). | 3.1.9 itens 3-4 |
| [ ] | 8.019.011 | UPDATE/Validação | Registro C190 | C | Regra desabilitada no campo 05. *Coordenar com 8.017.025 (V017 — orientação FCP).* | 3.1.9 item 3 |
| [ ] | 8.019.012 | UPDATE/Doc | Registro 0150 | 0 | Orientação adicionada: DIFAL EC 87/2015 (hipótese § 30 art. 19 Convênio SN/1970). | 3.1.9 item 5 |

## Notas arquiteturais

1. **Reforma Tributária do Consumo:** observação introduzida em V019 (C100). Documenta que documentos fiscais exclusivos de IBS/CBS/IS (novos tributos) **não** entram em EFD ICMS-IPI. Versões posteriores (V020 — 3.2.0/3.2.1/3.2.2) refinam essa regra com Ajuste SINIEF 49/25. Atenção: regra **fora do escopo de parse/round-trip** — é orientação de uso, mas alguns validadores cross-registro podem precisar.
2. **C120 cascata V016→V019:** se 8.016.006 criou subclasse `RegistroC120V016`, V019 adiciona valor "2" sobre essa subclasse. Decidir: estender ou criar `RegistroC120V019`. ARCHITECTURE §4.7 manda criar nova quando o delta muda **structure** — só valor válido novo cabe na mesma classe sob enum atualizado.

## Sumário

- **Total sub-stages:** 12 (0 NEW + 2 UPDATE/Campo + 10 UPDATE diversos).
- **Pendentes:** 12.
- **Concluídos:** 0.
