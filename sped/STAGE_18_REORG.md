# Stage 18 — Reorganização em camadas (tracking)

> Operational appendix to `ARCHITECTURE.md` §4.9 + Stage 18. Refatoração estrutural:
> Core monolítico → 4 camadas (Core universal + `Txt.Engine`/`Xml.Engine` + leiautes + guarda-chuvas).
> Executada em **3 PRs** (passos 1–3); guarda-chuvas (passo 4) ficam para depois.

## Baseline (antes de tocar em código)

- Build: ✅ 0 warnings / 0 errors.
- Testes: ✅ **4693** verdes — Core 274, NFeNFCe 98, Ecd 515, EfdContribuições 1661, EfdIcmsIpi 2145.
- **Invariante do refactor:** mesmos 4693 testes verdes a cada checkpoint (move + repoint, sem mudança de comportamento).

## Triagem dos enums de `Core/Enums` (regra dos três níveis, §4.9)

Baseada no uso real (grep por pacote consumidor):

**Fica no Core (universal — cross-mundo ou Ato COTEPE):**
- `IndicadorOperacao` (EfdIcmsIpi + NFeNFCe)
- `CodigoSituacaoDocumentoFiscal` (Tabela 4.1.2 Ato COTEPE)
- `OrigemMercadoria` (tabela fiscal `orig`)
- `ModalidadeFrete` (`modFrete`/IND_FRT convergente; órfão hoje, usado pelo slice 14.5)

**→ `Txt.Engine` (transversal a ≥2 leiautes TXT) — PR 2:**
- `CodigoNaturezaContaContabil` (EfdContribuições + EfdIcmsIpi)
- `IndicadorApuracaoIpi` (EfdContribuições + EfdIcmsIpi)
- `IndicadorMovimentacaoFisica` (EfdContribuições + EfdIcmsIpi)
- `IndicadorMovimentoBloco` (Ecd + EfdContribuições + EfdIcmsIpi — IND_MOV)
- `IndicadorSimNao` (Ecd + EfdIcmsIpi)
- `TipoItem` (EfdContribuições + EfdIcmsIpi)

**→ `Xml.Engine` (transversal XML, NFe + CT-e futuro) — PR 3:**
- `TipoAmbiente`, `TipoEmissao`

**→ pacote do leiaute (um único leiaute) — PR 1:**
- EfdIcmsIpi (8): `IndicadorAtividadeIpi`, `IndicadorEmissorDocumento`, `IndicadorFinalidadeArquivo`, `IndicadorOrigemDocumentoAjusteIpi`, `IndicadorSubApuracaoIcms`, `IndicadorTipoAjusteIpi`, `IndicadorTipoTitulo`, `TipoMovimentacaoBemCiAp`
- NFeNFCe (3): `FinalidadeEmissao`, `IndicadorIntermediador`, `IndicadorPresenca`

## Achados / decisões

- **Doc desatualizado:** `ARCHITECTURE.md` §4.9 + Stage 18 citam `CodigoNaturezaContaContabil`→Ecd como exemplo de enum de leiaute único. Uso real = EfdContribuições+EfdIcmsIpi → `Txt.Engine`. **Corrigir o exemplo no doc** (trocar por um enum de leiaute único real, ex.: `IndicadorApuracaoIpi`... não — esse é 2 leiautes; usar `TipoMovimentacaoBemCiAp`→EfdIcmsIpi).
- **`Atributos/` divide:** `DescontinuadoAttribute` **fica no Core** (marcador de versionamento universal; usado por `CodigoSituacaoDocumentoFiscal` que fica no Core). Só `RegistroSpedAttribute`/`CampoSpedAttribute`/`BlocoSpedAttribute`/`SpedValorAttribute` vão para `Txt.Engine`. Senão Core→Txt.Engine (dependência invertida).

## PRs

### PR 1 — Enxugar Core (enums de leiaute único) — `refactor/stage18-pr1-slim-core` ✅ PR #509
- [x] Mover 8 enums só-EfdIcmsIpi → `src/TecnoFisc.Sped.EfdIcmsIpi/Enums/` (namespace `TecnoFisc.Sped.EfdIcmsIpi.Enums`).
- [x] Mover 3 enums só-NFeNFCe → `src/TecnoFisc.Sped.NFeNFCe/Enums/` (namespace `TecnoFisc.Sped.NFeNFCe.Enums`).
- [x] Adicionar `using` do novo namespace nos consumidores (só onde faltava); remover `Core.Enums` órfão.
- [x] Build 0/0 + 4693 testes verdes (verificado de forma independente). PR #509 aberto para `dev`.

### PR 2 — Criar `Txt.Engine` — `refactor/stage18-pr2-txt-engine`
- [ ] Novo projeto `src/TecnoFisc.Sped.Txt.Engine/`; mover `Parser/`, `Gerador/`, `Catalogo/`, `Abstracoes/` (`RegistroSped`+`I*Sped`), `Streaming/`, atributos SPED (menos `Descontinuado`), `Erros/`? (avaliar — `ResultadoParse` é universal, fica no Core) + 6 enums TXT-transversais.
- [ ] Renomear `Core.SourceGenerators` → `Txt.Engine.SourceGenerators`.
- [ ] Repontar EFD Contribuições/ICMS-IPI/ECD para `Core + Txt.Engine + analyzer`. Atualizar `.slnx`.
- [ ] Build + testes verdes. PR.

### PR 3 — Criar `Xml.Engine` — `refactor/stage18-pr3-xml-engine`
- [ ] Novo projeto `src/TecnoFisc.Sped.Xml.Engine/`; mover `Core/Xml/` + `TipoAmbiente`/`TipoEmissao`.
- [ ] Repontar `NFeNFCe` para `Core + Xml.Engine`. Atualizar `.slnx`.
- [ ] Build + testes verdes. PR.

### Pós-PRs
- [ ] Atualizar `ARCHITECTURE.md`: marcar Stage 18 (passos 1–3) concluída; corrigir exemplo `CodigoNaturezaContaContabil`; nota sobre `DescontinuadoAttribute` no Core.
- [ ] CHANGELOG por pacote.
- [ ] Guarda-chuvas (passo 4) + Stage 12 sniffer TXT: pendências separadas.
