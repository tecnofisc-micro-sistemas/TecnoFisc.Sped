# Stage 8 — EFD ICMS-IPI incremento V016 (Leiaute 2022)

> Incremento sobre o baseline V015 (`sped/STAGE_8_EFD_ICMS_IPI_V015.md`). **Não** repete a lista de registros do baseline — descreve apenas o delta.
>
> **Vigência fiscal:** períodos de apuração a partir de janeiro/2022.
> **Guides publicados durante este leiaute:** 3.0.7, 3.0.8, 3.0.9. Cada item do tracking referencia a publicação que introduziu a mudança.
> **Fontes neste guia v3.2.2:** Subseção 11 (p. 16) e "Principais alterações no Guia Prático da EFD-ICMS/IPI – versão 3.0.7/3.0.8/3.0.9" (p. 358-359).
>
> **Atributos:** novos campos usam `[CampoSped(DesdeVersao = (int)LayoutEfdIcmsIpi.V016)]`; registros novos usam `[RegistroSped(IntroduzidoEm = (int)LayoutEfdIcmsIpi.V016)]`. O membro `V016 = 16` deve ser criado em `LayoutEfdIcmsIpi.cs` pelo PR do primeiro sub-stage que o consumir (first-use).

## Como usar este incremento

1. Selecionar próximo sub-stage `[ ]` (smallest `8.016.NNN`).
2. Determinar **modo de operação** pela coluna *Tipo*:
   - **NEW** — registro inexistente em V015. Modo CREATE do `/implementar-registro` (template baseline). Classe nova em `Registros/Bloco{X}/Registro{CODE}.cs`. `[RegistroSped(IntroduzidoEm = (int)LayoutEfdIcmsIpi.V016)]`.
   - **UPDATE/Campo** — campo novo em registro existente. `Edit` adiciona property no fim com `DesdeVersao = (int)LayoutEfdIcmsIpi.V016`. Doc-comment atualizado.
   - **UPDATE/Subclasse** — mudança incompatível com versão anterior (tipo, tamanho, decimais, formato). Cria subclasse `Registro{CODE}V016 : Registro{CODE}` em `Registros/Bloco{X}/Versionado/`. ARCHITECTURE §4.7.
   - **UPDATE/Obrig** — mudança de obrigatoriedade (S↔OC↔O). Validador cross-versão em `Validadores/`; atributo `[CampoSped]` permanece.
   - **UPDATE/Validação** — regra de validação alterada ou adicionada. Cria/edita validador específico; doc-comment do campo.
   - **UPDATE/Doc** — mudança puramente textual (orientação, descrição). Apenas doc-comment XML. Sem código novo.
   - **UPDATE/Descontinuado** — registro/campo deixa de ser usado. Não remove; marca `[Descontinuado(EmVersao = (int)LayoutEfdIcmsIpi.V016)]` (atributo a criar first-use) + validador que rejeita uso em `>= V016`.
3. Abrir PDF (`sped/guides/Guia Prático EFD - Versão 3.2.2.pdf`) só nas páginas indicadas. Para NEW: página do registro em Cap. III. Para UPDATE: ler a página do registro **e** a entrada correspondente em p. 358-359 (alterações do guide).
4. Tests:
   - NEW: cobertura completa (igual baseline).
   - UPDATE: adicionar fixtures que exercitam o delta, sem tocar nos tests baseline. Round-trip com campo novo populado + vazio (compat baseline).
   - Subclasse: arquivo de teste próprio `Registro{CODE}V016Tests.cs`.
5. **Commits granulares** dentro do PR (1 commit por step coeso — implementação, tests, tracking). Squash-Merge no `dev` é regra dura do repo — o merge consolida o branch em um único commit. **Não** rebase/amend antes do merge.
6. Tracking marcado **antes** do commit final do PR.

## Sub-stages

Numeração: `8.{versão}.{seq}`. Sequência é local da versão (V016: 8.016.001 → 8.016.027).

### Registros novos (NEW)

| Feito | Sub-stage | Tipo | Registro | Bloco | Resumo | Fonte |
| --- | --- | --- | --- | --- | --- | --- |
| [x] | 8.016.001 | NEW | Registro 1601 | 1 | Operações com instrumentos de pagamentos eletrônicos. Nível 2, 1:N, OC. Substitui Registro 1600. | Subseção 11 (p.16) + 3.0.7 item 6 |

### Campos novos em registros existentes (UPDATE/Campo)

| Feito | Sub-stage | Tipo | Registro | Bloco | Campos novos | Fonte |
| --- | --- | --- | --- | --- | --- | --- |
| [x] | 8.016.002 | UPDATE/Campo | Registro 0220 | 0 | 04 `COD_BARRA` | Subseção 11 (p.16) + 3.0.7 item 10 |
| [ ] | 8.016.003 | UPDATE/Campo | Registro C500 | C | 34 `COD_MOD_DOC_REF`, 35 `HASH_DOC_REF`, 36 `SER_DOC_REF`, 37 `NUM_DOC_REF`, 38 `MES_DOC_REF`, 39 `ENER_INJET`, 40 `OUTRAS_DED` | Subseção 11 (p.16) + 3.0.7 itens 11-12 |

### Mudanças estruturais (UPDATE)

| Feito | Sub-stage | Tipo | Registro | Bloco | Delta | Fonte |
| --- | --- | --- | --- | --- | --- | --- |
| [ ] | 8.016.004 | UPDATE/Descontinuado | Registro 1600 | 1 | Término da utilização. Substituído por Registro 1601 a partir de V016. | 3.0.7 item 6 |
| [ ] | 8.016.005 | UPDATE/Descontinuado | Registro 0210 | 0 | Término da utilização. | 3.0.7 item 22 |
| [ ] | 8.016.006 | UPDATE/Subclasse | Registro C120 | C | Campo 03 tamanho 12→15. Subclasse `RegistroC120V016 : RegistroC120`. | 3.0.7 item 4 |
| [ ] | 8.016.007 | UPDATE/Obrig | Registro D100 | D | Campos 24 `VL_PIS` e 25 `VL_COFINS` OC→O. Inclui validação alterada do campo 11. | 3.0.7 itens 1-2, 29 |
| [ ] | 8.016.008 | UPDATE/Obrig | Registro D410 | D | `VL_BC_ICMS` e `VL_ICMS` O→OC. | 3.0.7 item 3 |
| [ ] | 8.016.009 | UPDATE/Obrig | Registro D420 | D | `VL_BC_ICMS` e `VL_ICMS` O→OC. | 3.0.7 item 3 |
| [ ] | 8.016.010 | UPDATE/Obrig | Registro D500 | D | `VL_BC_ICMS` e `VL_ICMS` O→OC. | 3.0.7 item 3 |
| [ ] | 8.016.011 | UPDATE/Obrig | Registro D600 | D | `VL_BC_ICMS` e `VL_ICMS` O→OC. | 3.0.7 item 3 |
| [ ] | 8.016.012 | UPDATE/Obrig | Registro C176 | C | Campos 12-15 OC→O. Orientação 12/14/15. Descrição campo 18. Validação retira ">0" e exige `COD_RESP_RET="2"` no campo 14. | 3.0.7 itens 16-18 + 3.0.9 itens 1-2 |
| [ ] | 8.016.013 | UPDATE/Validação | Registro C170 | C | Validação adicional no campo 06. | 3.0.7 item 8 |
| [ ] | 8.016.014 | UPDATE/Validação | Registro C425 | C | Validação adicional no campo 04. | 3.0.7 item 9 |
| [ ] | 8.016.015 | UPDATE/Validação | Registro C500 | C | Validação alterada nos campos 13, 15 e 30. Orientação dos campos 12, 16, 17, 20 e 22. *Coordenar com 8.016.003 — mesmo registro, pode entrar no mesmo PR.* | 3.0.7 itens 12-13 |
| [ ] | 8.016.016 | UPDATE/Doc | Registro C590 | C | Orientação do campo 05 (entrega de NF3-e). | 3.0.7 item 14 |
| [ ] | 8.016.017 | UPDATE/Validação | Registro 0200 | 0 | Validação alterada. | 3.0.7 item 15 |
| [ ] | 8.016.018 | UPDATE/Validação | Registro B020 | B | Inclusão de NF3-e (cód. 66). Validações dos campos 04, 07 e 09. | 3.0.7 itens 19-20 |
| [ ] | 8.016.019 | UPDATE/Doc | Registro 1010 | 1 | Redação do campo 08. | 3.0.7 item 21 |
| [ ] | 8.016.020 | UPDATE/Doc | Registro C180 | C | Descrição do campo 11. | 3.0.7 item 23 |
| [ ] | 8.016.021 | UPDATE/Validação | Registro E250 | E | Nova validação do campo 05 (`MES_REF`). | 3.0.7 item 5 |
| [ ] | 8.016.022 | UPDATE/Validação | Registro E316 | E | Nova validação do campo 05 (`MES_REF`). | 3.0.7 item 5 |
| [ ] | 8.016.023 | UPDATE/Validação | Registro E530 | E | Validação alterada no campo 04. | 3.0.7 item 7 |
| [ ] | 8.016.024 | UPDATE/Validação | Registro K235 | K | Regra de validação revisada. | 3.0.9 item 4 |
| [ ] | 8.016.025 | UPDATE/Validação | Registro K255 | K | Regra de validação revisada. | 3.0.9 item 4 |
| [ ] | 8.016.026 | UPDATE/Validação | Registro K292 | K | Regra de validação revisada. | 3.0.9 item 4 |
| [ ] | 8.016.027 | UPDATE/Validação | Registro K302 | K | Regra de validação revisada. | 3.0.9 item 4 |

## Notas arquiteturais (resolver antes do primeiro sub-stage)

1. **Atributo `LayoutEfdIcmsIpi.V016`:** criar no PR de `8.016.001` (first-use). Atualizar doc-comment do `Registro0000.cs` linha 23 (exemplo ainda diz "ex.: 306" — resíduo do rename V306→V015).
2. **Estratégia subclasse §4.7:** confirmar como `RegistroXxxxV016 : RegistroXxxx` se relaciona com:
   - Catálogo (`[RegistroSped(Codigo = "C120")]` colide — usar `[RegistroSpedVersionado]` novo? Ou registrar a subclasse e o parser escolhe por `LayoutEfdIcmsIpi`?)
   - Source generator do Stage 6.
   - Round-trip — `Registro0000.CodVer` decide qual variante o parser instancia.
3. **Atributo `[Descontinuado(EmVersao = ...)]`:** não existe ainda no Core. Criar no PR de `8.016.004` (first-use) junto com lógica que rejeita parse de `1600`/`0210` em arquivos `COD_VER >= V016`.
4. **Validadores cross-versão:** pasta `src/TecnoFisc.Sped.EfdIcmsIpi/Validadores/Versionados/` (a criar). Padrão a definir antes do primeiro sub-stage de validação.
5. **Subseção 12 (Leiaute 2023) reaparece K010 como novo:** decidir se K010 entra em V016 (3.0.9 item 3 — vigência possivelmente facultativa em 2022) ou V017 (Subseção 12 — vigência obrigatória 2023). Padrão atual: **V017** (vigência fiscal manda). Confirmar com NT.

## Sumário

- **Total sub-stages:** 27 (1 NEW + 2 UPDATE/Campo + 24 UPDATE diversos).
- **Pendentes:** 27.
- **Concluídos:** 0.
