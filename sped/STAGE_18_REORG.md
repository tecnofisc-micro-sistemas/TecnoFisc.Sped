# Stage 18 — Reorganização em camadas (tracking)

> Operational appendix to `ARCHITECTURE.md` §4.9 + Stage 18. Refatoração estrutural:
> Core monolítico → 4 camadas (Core universal + `Txt.Engine`/`Xml.Engine` + leiautes + guarda-chuvas).
> Passos 1–3 executados em PRs; o passo 4 agora criou `TecnoFisc.Sped.Txt` +
> `TecnoFisc.Sped`. `TecnoFisc.Sped.Xml` permanece adiado para depois do CT-e.

## Baseline (antes de tocar em código)

- Build: ✅ 0 warnings / 0 errors.
- Baseline pré-cleanup: 4693 testes verdes — Core 274, NFeNFCe 98, Ecd 515, EfdContribuições 1661, EfdIcmsIpi 2145.
- Estado pós-cleanup: 4707 testes verdes — Core 129, Txt.Engine 147, Xml.Engine 12, NFeNFCe 98, Ecd 515, EfdContribuições 1661, EfdIcmsIpi 2145.
- **Invariante do refactor:** cobertura preservada a cada checkpoint (move + repoint, sem mudança de comportamento); a contagem final aumentou por separação de projetos de teste, sem remoção de cobertura.

## Triagem dos enums de `Core/Enums` (regra dos três níveis, §4.9)

Baseada no uso real (grep por pacote consumidor):

**Fica no Core (universal — cross-mundo ou Ato COTEPE):**
- `IndicadorOperacao` (EfdIcmsIpi + NFeNFCe)
- `CodigoSituacaoDocumentoFiscal` (Tabela 4.1.2 Ato COTEPE)
- `OrigemMercadoria` (tabela fiscal `orig`)
- `ModalidadeFrete` (`modFrete`/IND_FRT convergente; órfão hoje, usado pelo slice 14.5)
- `TipoAmbiente`, `TipoEmissao` (`ChaveAcesso` consome `TipoEmissao`; mover criaria dependência invertida Core→Xml.Engine)

**→ `Txt.Engine` (transversal a ≥2 leiautes TXT) — PR 2:**
- `CodigoNaturezaContaContabil` (EfdContribuições + EfdIcmsIpi)
- `IndicadorApuracaoIpi` (EfdContribuições + EfdIcmsIpi)
- `IndicadorMovimentacaoFisica` (EfdContribuições + EfdIcmsIpi)
- `IndicadorMovimentoBloco` (Ecd + EfdContribuições + EfdIcmsIpi — IND_MOV)
- `IndicadorSimNao` (Ecd + EfdIcmsIpi)
- `TipoItem` (EfdContribuições + EfdIcmsIpi)

**→ `Xml.Engine` (transversal XML, NFe + CT-e futuro) — PR 3:**
- Nenhum enum movido; PR 3 moveu os tipos de `Core/Xml/`.

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

### PR 2 — Criar `Txt.Engine` — `refactor/stage18-pr2-txt-engine` ✅

**De-risking (confirmado por análise):**
- NFeNFCe **não** importa nenhuma máquina TXT do Core → corte limpo, sem refactor de NFe.
- Nenhum tipo que fica no Core (`ValueObjects`/`Erros`/`Xml`/4 enums/`DescontinuadoAttribute`) referencia tipo que move → sem dependência invertida.
- `Erros/` (`ResultadoParse` etc.) é autossuficiente → **fica no Core** (universal).
- Source generator casa atributos por **string FQN** (`TecnoFisc.Sped.Core.Atributos.RegistroSpedAttribute` etc.) e **gera** `using TecnoFisc.Sped.Core.{Abstracoes,Catalogo,Gerador,Parser}` → atualizar para `Txt.Engine.*` (ValueObjects ficam Core).
- `[Descontinuado]` usado em 6 arquivos (Registro0210, Registro1600 + 2 testes + interno CatalogoBuilder/MetadadosRegistro/LeitorSpedTxt): namespace `Core.Atributos` **fica**; esses arquivos precisam dos dois usings.
- `Enums` e `Atributos` **dividem** entre Core e Txt.Engine; consumidor que usa um que move + um que fica precisa dos dois usings (compilador aponta).

**Move para `Txt.Engine` (namespace `Core.X` → `Txt.Engine.X`):** `Abstracoes/` (6), `Atributos/` exceto `DescontinuadoAttribute` (4), `Catalogo/` (5), `Parser/` (5), `Gerador/` (3), `Streaming/` (2), 6 enums TXT (`CodigoNaturezaContaContabil`, `IndicadorApuracaoIpi`, `IndicadorMovimentacaoFisica`, `IndicadorMovimentoBloco`, `IndicadorSimNao`, `TipoItem`).
**Fica no Core:** `ValueObjects/`, `Erros/`, `Xml/`, 4 enums, `Atributos/DescontinuadoAttribute`.

- [x] Criar `src/TecnoFisc.Sped.Txt.Engine/` (csproj ref Core) + mover arquivos (git mv) + ajustar namespaces.
- [x] Renomear projeto `Core.SourceGenerators` → `Txt.Engine.SourceGenerators` + atualizar FQN/usings gerados.
- [x] Repontar EFD Contribuições/ICMS-IPI/ECD (csproj: + Txt.Engine, analyzer novo) + renames de `using` nos consumidores (~1184 arquivos). Atualizar `.slnx`.
- [x] Build 0/0 + 4693 testes verdes (verificado independente). Dependência confirmada: Core sem refs; NFeNFCe e Txt.Engine só → Core.

**Cleanup concluído:** testes de Parser/Gerador/Catalogo/Abstracoes/Streaming foram extraídos para `Txt.Engine.Tests`; testes XML do sniffer foram extraídos para `Xml.Engine.Tests`; `Core.Tests` referencia somente `Core`.

### PR 3 — Criar `Xml.Engine` — `refactor/stage18-pr3-xml-engine` ✅
**Correção de triagem:** `TipoAmbiente`/`TipoEmissao` **ficam no Core** — `ChaveAcesso` (Core) consome `TipoEmissao` (mover criaria dep invertida Core→Xml.Engine). `XmlReaderExtensions` fica no NFeNFCe (helper de NF-e). Logo o PR só move os 3 arquivos de `Core/Xml/`.
- [x] Novo projeto `src/TecnoFisc.Sped.Xml.Engine/`; mover `Core/Xml/` (IdentificadorXmlFiscal, IDocumentoFiscalXml, TipoDocumentoFiscalXml).
- [x] Repontar `NFeNFCe` para `Core + Xml.Engine`; testes do sniffer XML extraídos para `Xml.Engine.Tests`. Atualizar `.slnx`.
- [x] Build 0/0 + 4693 testes verdes (verificado independente). Dependência: Core/Txt.Engine não referenciam Xml.Engine.
- [x] ARCHITECTURE.md: Stage 18 (passos 1–3) marcada concluída; exemplos corrigidos (`CodigoNaturezaContaContabil`→Txt.Engine; `TipoAmbiente`/`TipoEmissao`→Core); §7 Xml.Engine tree corrigida.

### Pós-PRs (pendências separadas)
- [x] CHANGELOG por pacote (próximo release — novos pacotes `Txt.Engine`/`Xml.Engine`/`Txt.Engine.SourceGenerators`; breaking de namespace).
- [x] Passo 4 — guarda-chuvas (Stage 13): `TecnoFisc.Sped.Txt` + `TecnoFisc.Sped` criados; `TecnoFisc.Sped.Xml` permanece adiado para depois do CT-e.
- [x] Stage 12 — sniffer TXT (`SnifferSped`) implementado em `TecnoFisc.Sped.Txt.Engine`; XML ja existia em `TecnoFisc.Sped.Xml.Engine`.
- [x] Cleanup opcional: `Txt.Engine.Tests` e `Xml.Engine.Tests` extraidos; `Core.Tests` voltou a referenciar somente `Core`.
