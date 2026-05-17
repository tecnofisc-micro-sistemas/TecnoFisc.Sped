# Stage 8 — EFD ICMS-IPI incremento V017 (Leiaute 2023)

> Incremento sobre V016 (`sped/STAGE_8_INCR_V016.md`). Pré-requisito: V016 concluído.
>
> **Vigência fiscal:** períodos de apuração a partir de janeiro/2023.
> **Guides publicados durante este leiaute:** 3.1.0, 3.1.1, 3.1.2, 3.1.3, 3.1.4.
> **Fontes neste guia v3.2.2:** Subseção 12 (p. 17) e "Principais alterações no Guia Prático da EFD-ICMS/IPI – versão 3.1.0/3.1.1/3.1.2/3.1.3/3.1.4" (p. 359-360).
>
> **Atributos:** `[CampoSped(DesdeVersao = (int)LayoutEfdIcmsIpi.V017)]`, `[RegistroSped(IntroduzidoEm = (int)LayoutEfdIcmsIpi.V017)]`. Membro `V017 = 17` criado first-use.

## Sub-stages

### Registros novos (NEW) — Bloco D NFCom (código 62) + extras

| Feito | Sub-stage | Tipo | Registro | Bloco | Resumo | Fonte |
| --- | --- | --- | --- | --- | --- | --- |
| [ ] | 8.017.001 | NEW | Registro K010 | K | Tipo de leiaute K (simplificado/completo). Nível 2, 1, OC. (Decisão de vigência: ver nota 1 — pode migrar para V016.) | Subseção 12 + 3.0.9 item 3 |
| [ ] | 8.017.002 | NEW | Registro 0221 | 0 | Correlação entre códigos de itens comercializados. Nível 3, 1:N, OC. | Subseção 12 + 3.1.0 item 2 |
| [ ] | 8.017.003 | NEW | Registro C855 | C | Observações do lançamento fiscal (código 59). Nível 3, 1:N, OC. | Subseção 12 + 3.1.0 item 2 |
| [ ] | 8.017.004 | NEW | Registro C857 | C | Outras obrigações tributárias, ajustes e informações de valores provenientes de documento fiscal. Nível 4, 1:N, OC. | Subseção 12 + 3.1.0 item 2 |
| [ ] | 8.017.005 | NEW | Registro C895 | C | Observações do lançamento fiscal (código 59) — bloco 89x. Nível 3, 1:N, OC. | Subseção 12 + 3.1.0 item 2 |
| [ ] | 8.017.006 | NEW | Registro C897 | C | Outras obrigações tributárias, ajustes — bloco 89x. Nível 4, 1:N, OC. | Subseção 12 + 3.1.0 item 2 |
| [ ] | 8.017.007 | NEW | Registro D700 | D | Nota Fiscal Fatura Eletrônica de Serviços de Comunicação — NFCom (código 62). Nível 2, V, OC. | Subseção 12 + 3.1.0 item 2 |
| [ ] | 8.017.008 | NEW | Registro D730 | D | Registro analítico NFCom. Nível 3, 1:N, OC. | Subseção 12 + 3.1.0 item 2 |
| [ ] | 8.017.009 | NEW | Registro D731 | D | Informações do fundo de combate à pobreza FCP. Nível 4, 1:1, OC. | Subseção 12 + 3.1.0 item 2 |
| [ ] | 8.017.010 | NEW | Registro D735 | D | Observações do lançamento fiscal (código 62). Nível 3, 1:N, OC. | Subseção 12 + 3.1.0 item 2 |
| [ ] | 8.017.011 | NEW | Registro D737 | D | Outras obrigações tributárias / ajustes — NFCom. Nível 4, 1:N, OC. | Subseção 12 + 3.1.0 item 2 |
| [ ] | 8.017.012 | NEW | Registro D750 | D | Escrituração consolidada NFCom (código 62). Nível 2, 1:N, OC. | Subseção 12 + 3.1.0 item 2 |
| [ ] | 8.017.013 | NEW | Registro D760 | D | Registro analítico da escrituração consolidada NFCom. Nível 3, 1:N, OC. | Subseção 12 + 3.1.0 item 2 |
| [ ] | 8.017.014 | NEW | Registro D761 | D | Informações do fundo de combate à pobreza FCP — consolidada. Nível 4, 1:1, OC. | Subseção 12 + 3.1.0 item 2 |

### Mudanças estruturais (UPDATE)

| Feito | Sub-stage | Tipo | Alvo | Bloco | Delta | Fonte |
| --- | --- | --- | --- | --- | --- | --- |
| [ ] | 8.017.015 | UPDATE/Descontinuado | enum `SituacaoDocumento` | Core | Códigos 04 e 05 da Tabela 4.1.2 descontinuados a partir de 31/12/2022. Marcar `[Descontinuado(EmVersao = V017)]`. | 3.1.0 item 1 |
| [ ] | 8.017.016 | UPDATE/Doc | Tabela CST ICMS Cap IV §1.1 | Core | Atualização tabela de CST ICMS. Atualizar `CstIcms` (enum/value object) com valores vigentes. | 3.1.3 item 1 |
| [ ] | 8.017.017 | UPDATE/Doc | Registro C100 | C | Orientação ICMS monofásico (Nota Orientativa 01/2023). | 3.1.3 item 2 |
| [ ] | 8.017.018 | UPDATE/Validação | Registro C100 | C | Validação de duplicidade (`IND_EMIT`, `COD_SIT`, `COD_PART`, `SER`, `NUM_DOC` com exceção `COD_MOD` 55/65). | 3.1.4 item 1 |
| [ ] | 8.017.019 | UPDATE/Doc | Registro C105 | C | Instrução do registro + valor válido "2" no campo 02. | 3.1.4 itens 4-5 |
| [ ] | 8.017.020 | UPDATE/Subclasse | Registro C111 | C | Tamanho campo 02 15→60. `RegistroC111V017 : RegistroC111`. | 3.1.0 item 8 |
| [ ] | 8.017.021 | UPDATE/Doc | Registro C170 | C | Orientação campo 05. | 3.1.3 item 6 |
| [ ] | 8.017.022 | UPDATE/Validação | Registro C170 | C | Validação campo 06. *Coordenar com 8.017.021.* | 3.1.0 item 4 |
| [ ] | 8.017.023 | UPDATE/Validação | Registro C181 | C | Validação campo 02. | 3.1.0 item 6 |
| [ ] | 8.017.024 | UPDATE/Validação | Registro C185 | C | Validação campo 06. | 3.1.0 item 7 |
| [ ] | 8.017.025 | UPDATE/Doc | Registro C190 | C | Orientação campo 05 (retira termo FCP). | 3.1.1 item 1 |
| [ ] | 8.017.026 | UPDATE/Validação | Registro C330 | C | Validação campo 02. | 3.1.0 item 6 |
| [ ] | 8.017.027 | UPDATE/Validação | Registro C380 | C | Validação campo 02. | 3.1.0 item 6 |
| [ ] | 8.017.028 | UPDATE/Validação | Registro C430 | C | Validação campo 02. | 3.1.0 item 6 |
| [ ] | 8.017.029 | UPDATE/Validação | Registro C480 | C | Validação campo 02. | 3.1.0 item 6 |
| [ ] | 8.017.030 | UPDATE/Doc | Registro C500 | C | NF3-e sem CST não escriturada neste registro. | 3.1.4 item 2 |
| [ ] | 8.017.031 | UPDATE/Doc | Registro C590 | C | NF3-e sem CST nem energia injetada. | 3.1.4 item 3 |
| [ ] | 8.017.032 | UPDATE/Obrig | Registro C700 | C | Modelo 66 (NF3-e). Campos 08/09 O→OC. Orientação 06-09. NF3-e sem CST não escriturada. | 3.1.2 itens 1-3 + 3.1.4 item 2 |
| [ ] | 8.017.033 | UPDATE/Doc | Registro C790 | C | Modelo 66. NF3-e sem CST nem energia injetada. | 3.1.2 item 1 + 3.1.4 item 3 |
| [ ] | 8.017.034 | UPDATE/Doc | Registro C791 | C | Modelo 66. | 3.1.2 item 1 |
| [ ] | 8.017.035 | UPDATE/Validação | Registro C800 | C | Exceção nº 2 + validação campo 09. | 3.1.0 item 3 + 3.1.4 item 6 |
| [ ] | 8.017.036 | UPDATE/Validação | Registro C815 | C | Validação campo 02. | 3.1.0 item 6 |
| [ ] | 8.017.037 | UPDATE/Validação | Registro C880 | C | Validação campo 02. | 3.1.0 item 6 |
| [ ] | 8.017.038 | UPDATE/Subclasse | Registro E112 | E | Tamanho campo 03 15→60. | 3.1.0 item 9 |
| [ ] | 8.017.039 | UPDATE/Subclasse | Registro E116 | E | Tamanho campo 06 15→60. | 3.1.0 item 10 |
| [ ] | 8.017.040 | UPDATE/Validação | Registro E210 | E | Validação campo 08. | 3.1.4 item 7 |
| [ ] | 8.017.041 | UPDATE/Subclasse | Registro E230 | E | Tamanho campo 03 15→60. | 3.1.0 item 9 |
| [ ] | 8.017.042 | UPDATE/Subclasse | Registro E250 | E | Tamanho campo 06 15→60. | 3.1.0 item 10 |
| [ ] | 8.017.043 | UPDATE/Subclasse | Registro E312 | E | Tamanho campo 03 15→60. | 3.1.0 item 9 |
| [ ] | 8.017.044 | UPDATE/Subclasse | Registro E316 | E | Tamanho campo 06 15→60. | 3.1.0 item 10 |
| [ ] | 8.017.045 | UPDATE/Validação | Registro 1391 | 1 | Valor válido "4" no campo 09. | 3.1.4 item 8 |
| [ ] | 8.017.046 | UPDATE/Obrig | Registro 1900 | 1 | Obrigatoriedade inclui registros C597, C857, C897, D737. | 3.1.1 item 2 |
| [ ] | 8.017.047 | UPDATE/Subclasse | Registro 1922 | 1 | Tamanho campo 03 15→60. | 3.1.0 item 9 |
| [ ] | 8.017.048 | UPDATE/Subclasse | Registro 1926 | 1 | Tamanho campo 06 15→60. | 3.1.0 item 10 |

## Notas arquiteturais

1. **K010 — vigência 2022 vs 2023.** Guide 3.0.9 item 3 inclui K010 (= V016 fiscal). Subseção 12 (Leiaute 2023) também lista K010 como novo. Comportamento provável: facultativo 2022, obrigatório 2023. Decidir antes do PR: alocar a V016 (mais conservador para arquivos antigos) ou V017 (vigência obrigatória). Posicionado em V017 por default — mover para V016 se PVA aceitar K010 em arquivos V016.
2. **Mudanças de tamanho 15→60 (C111, E112, E230, E312, 1922 / E116, E250, E316, 1926):** padrão repetitivo de 8 registros. Bom candidato a batch (1 PR cobrindo todos os UPDATE/Subclasse), desde que a estratégia de subclasse esteja consolidada. Por ora 1 sub-stage por registro — agrupar em PR único se ARCHITECTURE §4.7 estiver fechada.
3. **NFCom (D700, D730, D731, D735, D737, D750, D760, D761):** hierarquia D700→D730→D731 e D700→D735→D737; consolidada D750→D760→D761. Implementar D700 primeiro (pai); demais dependem.
4. **C597:** mencionado em 3.1.1 item 2 mas não criado por este incremento. Conferir baseline V015 — provavelmente existente.

## Sumário

- **Total sub-stages:** 48 (14 NEW + 34 UPDATE).
- **Pendentes:** 48.
- **Concluídos:** 0.
