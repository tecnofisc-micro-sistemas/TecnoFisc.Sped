# Changelog

Todas as mudanças relevantes deste repositório são documentadas neste arquivo.

O formato segue [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/) e o projeto adota [Semantic Versioning](https://semver.org/lang/pt-BR/). Cada pacote NuGet possui versão independente; as seções abaixo agrupam as mudanças por release do repositório.

## [Não publicado]

## [0.3.1] — 2026-05-17

Corrige nomenclatura das versões do leiaute EFD ICMS-IPI: o que estava sendo chamado de `V306` é, na verdade, o leiaute **V015** (`COD_VER` do registro `0000`, conforme Tabela "Versão do Leiaute" da Nota Técnica EFD ICMS-IPI nº 2020.001 — Ato COTEPE/ICMS nº 44/2018). O número `306` é a versão do Guia Prático (3.0.6) que descreve o leiaute, não o leiaute em si. Múltiplas versões do Guia (3.0.6, 3.1.x, 3.2.x) descrevem o mesmo leiaute 015.

### TecnoFisc.Sped.EfdIcmsIpi 0.3.1

#### Alterado (breaking)

- Enum público `LayoutEfdIcmsIpi`: constante `V306` renomeada para `V015` (valor `306` → `15`). As 16 constantes anteriormente listadas como `V307`..`V322` foram removidas — elas mapeavam atualizações textuais do Guia Prático, não leiautes. Incrementos reais (`V016`, `V017`, …) serão adicionados conforme a Receita publicar novas Notas Técnicas com leiaute novo.
- Documentação (`ARCHITECTURE.md` §12, `README.md`, `CLAUDE.md`, `sped/STAGE_8_EFD_ICMS_IPI_V015.md`) reescrita para distinguir versão do leiaute (`COD_VER` do `0000`) de versão do Guia Prático.

#### Notas de migração

- Consumidores que usavam `LayoutEfdIcmsIpi.V306` devem trocar para `LayoutEfdIcmsIpi.V015`. Nenhum impacto em arquivos SPED gerados/lidos — o `COD_VER` correto no registro `0000` sempre foi `015`.
- Tracking file `sped/STAGE_8_EFD_ICMS_IPI_V306.md` renomeado para `sped/STAGE_8_EFD_ICMS_IPI_V015.md`.

## [0.3.0] — 2026-05-17

Conclui a Stage 8 baseline do `ARCHITECTURE.md`: EFD ICMS-IPI leiaute V015 (`COD_VER` do registro `0000`) com todos os 255 registros tipados, API pública de parser/gerador e validação round-trip end-to-end contra arquivo real emitido pelo PVA da Receita. *Nota: esta release foi originalmente publicada referindo-se ao leiaute como "V306"; ver release 0.3.1 para a correção de nomenclatura.*

### TecnoFisc.Sped.EfdIcmsIpi 0.3.0

#### Adicionado

- Pacote novo. Cobre a Stage 8 baseline V015 (Ato COTEPE/ICMS nº 44/2018, NT 2020.001, descrito no Guia Prático v3.0.6 e posteriores): 255 registros distribuídos nos 10 blocos (`0`, `B`, `C`, `D`, `E`, `G`, `H`, `K`, `1`, `9`), com `[RegistroSped]`/`[CampoSped]` declarados, validação de níveis hierárquicos e fixtures por bloco.
- API pública: `ArquivoEfdIcmsIpi`, `BlocoEfdIcmsIpi`, `ParserEfdIcmsIpi`, `GeradorEfdIcmsIpi`. Espelha o contrato de `EfdContribuicoes` — leitura streaming via `IAsyncEnumerable<RegistroSped>`, leitura buffered para o modelo tipado, escrita pipe-delimitada em Latin1/Windows-1252.
- Validação round-trip end-to-end (`RoundTripFixtureRealTests`) contra arquivo SPED real emitido pelo PVA, exercitando os 10 blocos. Invariante: `parse → serialize → parse → serialize` é byte-idêntica entre as duas passagens de serialização.
- Suporte para o registro `9999` final seguido de bloco PKCS#7 anexo: parser encerra silenciosamente no marcador `|9999|` e descarta o trailer binário da assinatura digital do PVA, sem perder registros nem cuspir erro de layout.

### TecnoFisc.Sped.Core.SourceGenerators 0.3.0

#### Corrigido

- `RegistroSpedCatalogoGenerator` agora honra `[SpedValor("S")]`/`[SpedValor("N")]` em membros de enum. Setter emitido vira sequência de `valor.SequenceEqual("X".AsSpan())` com despacho para o membro do enum; serializador vira `switch` por valor. Antes da correção, o gerador sempre emitia `int.Parse(valor)` para qualquer enum, o que quebrava em runtime qualquer campo SPED textual (`IndicadorSimNao` no EFD ICMS-IPI, descoberto via round-trip real). O caminho integral via `EnumUnderlyingType` continua intacto para enums sem `[SpedValor]`.
- Teste de regressão: `CatalogoSpedGeradoEnumTextualTests` exercita o `CatalogoSpedGerado` direto (não o builder reflexivo) com `Registro1010.IndExp`, garantindo que o caminho gerado lê/escreve `"S"`/`"N"` corretamente.

### Documentação

#### Alterado

- README e tabela de status do repositório passam a refletir EFD ICMS-IPI 0.3.0 publicada.
- Registrada regra dura de integração: merges para `dev` devem usar sempre Squash and Merge; branches de trabalho podem manter commits granulares.

## [0.2.0] — 2026-05-06

Conclui as Stages 5 e 6 de `ARCHITECTURE.md`: API streaming pública e source generator do catálogo de registros, com migração do `ParserEfdContribuicoes` para o catálogo gerado em compile-time.

### TecnoFisc.Sped.Core 0.2.0

#### Adicionado

- Contrato `ILeitorSped.LerStreamingAsync` (com `ReadOnlySpan<char>` no caminho dos campos) — semântica explícita de leitura registro-a-registro sem bufferizar o arquivo todo. Memória consumida fica limitada ao buffer do `PipeReader`, independente do tamanho do arquivo.
- Helper público `CatalogoBuilder.ConstruirMetadadosDoTipo` — usado pelo source generator para reutilizar a extração de campos via reflexão one-time durante a inicialização do catálogo gerado, mantendo zero reflexão na hot path.

#### Alterado

- `MetadadosCampo` passa a expor apenas dois delegates compostos: `Action<RegistroSped, ReadOnlySpan<char>>` para parse + atribuição e `Func<RegistroSped, string>` para serialização. O caminho reflexivo (`CatalogoBuilder`) adapta a API antiga preservando comportamento; o caminho gerado (Stage 6) implementa os delegates inline com casts diretos para o tipo concreto, sem boxing.
- `LeitorSpedTxt.LerAsync` renomeado para `LeitorSpedTxt.LerStreamingAsync`. Mantém a mesma assinatura (`IAsyncEnumerable<RegistroSped>`); o nome novo deixa claro que é o caminho memory-bounded.

### TecnoFisc.Sped.Core.SourceGenerators 0.2.0

#### Adicionado

- Pacote novo (`netstandard2.0`, `IsRoslynComponent=true`). Distribuído como Analyzer (`OutputItemType=Analyzer ReferenceOutputAssembly=false`), não embarca no runtime do consumidor.
- `RegistroSpedCatalogoGenerator` — `IIncrementalGenerator` que detecta classes decoradas com `[RegistroSped]` no projeto consumidor e emite, em compile-time, `CatalogoSpedGerado : CatalogoSpedBase` com o dicionário de registros já populado. Cada propriedade decorada com `[CampoSped]` vira um par de helpers privados estáticos (parse + serialize) com cast direto para o tipo concreto — sem `Assembly.GetTypes()`, sem `Expression.Compile`, sem boxing.
- Cobertura zero-alloc para os tipos de campo: `string`, `int/long/short` (e nullables), `decimal`, `DateOnly` (com `Formato`), `bool`, `char`, enums (com `EnumUnderlyingType` real) e os value objects fiscais expostos pelo Core (`Cnpj`, `Cpf`, `Cfop`, `Ncm`, `ChaveAcesso`, `InscricaoEstadual`, `ModeloDocumento`, `GeneroItem` — todos com `Criar(ReadOnlySpan<char>)`). Tipos fora dessa lista caem num fallback que delega para `ConversoresPrimitivosCatalogo`.

### TecnoFisc.Sped.EfdContribuicoes 0.2.0

#### Adicionado

- `ParserEfdContribuicoes.LerStreamingAsync(Stream)` — caminho streaming explicito, retorna `IAsyncEnumerable<RegistroSped>`. Os registros saem com Pai/Filhos já vinculados.
- `ParserEfdContribuicoes.LerAsync(Stream)` — conveniência buffered que devolve `Task<ArquivoEfdContribuicoes>` com todos os blocos populados; encapsula `LerStreamingAsync` + `ArquivoEfdContribuicoes.CarregarAsync`.

#### Alterado

- O parser passa a usar `new CatalogoSpedGerado()` por padrão (catálogo gerado em compile-time). Elimina o scan reflexivo via `Assembly.GetTypes()` que era feito na primeira chamada do parser. O construtor `ParserEfdContribuicoes(IRegistroSpedCatalogo)` continua disponível para injeção de catálogo customizado.

### Benchmarks

#### Adicionado

- Projeto `benchmarks/TecnoFisc.Sped.Benchmarks` (BenchmarkDotNet, .NET 10) com:
  - `StreamingVsBufferedBenchmark` — comparação memory-bounded entre `LerStreamingAsync` e `LerAsync` (buffered) sobre fluxos sintéticos de até 1M de registros, com `MemoryDiagnoser` ativo.
  - `PeakHeapProbe` (acionado por `--probe peak`) — sonda standalone que amostra `GC.GetTotalMemory` em segundo plano para evidenciar o pico de memória viva, métrica que a coluna `Allocated` do BDN não captura.
  - `InicializacaoCatalogoBenchmark` — comparação de tempo e alocação na inicialização do catálogo entre o caminho reflexivo (`CatalogoBuilder.BuildFromAssembly`) e o catálogo gerado (`CatalogoSpedGerado`).
  - `ParserCatalogoBenchmark` — comparação no caminho quente do parser entre os dois catálogos.

### Notas de release

- Stage 7 (`Layout V007`) descrito em `ARCHITECTURE.md` foi descartado: a Receita não publicou novo leiaute do EFD Contribuições desde a versão V006 (2021). O suporte a leiautes futuros volta à roadmap quando houver um.

## [0.1.0] — 2026-05-06

Release inicial. Conclui a Stage 4 de `ARCHITECTURE.md`: implementação completa do leiaute V006 da EFD Contribuições (Guia Prático v1.35) com parser e gerador capazes de fazer round-trip de um arquivo real anonimizado.

### TecnoFisc.Sped.Core 0.1.0

#### Adicionado

- Abstrações base: `RegistroSped`, `IArquivoSped`, `IBlocoSped`, `ILeitorSped`, `IEscritorSped`, `IRegistroSpedCatalogo`.
- Atributos de metadados: `[RegistroSped]`, `[CampoSped]`, `[BlocoSped]`.
- Catálogo dinâmico de registros (`CatalogoBuilder`, `CatalogoSpedBase`, `MetadadosRegistro`, `MetadadosCampo`) com cache via reflexão em startup — sem reflexão no caminho quente. O source generator (Stage 6) substituirá esta camada mantendo a API.
- Parser binário baseado em `PipeReader` + `Utf8Parser` (`LeitorSpedTxt`, `PilhaHierarquica`, `ParseadoresPrimitivos`, `EncodingSped` com Latin1/Windows-1252).
- Gerador binário (`EscritorSpedTxt`, `SerializadoresPrimitivos`, `TotalizadorBlocos`).
- Tipo `ResultadoParse` e hierarquia `ErroFormato` / `ErroLayout` para falhas esperadas; exceções reservadas para erros de programador.
- Value objects fiscais transversais com validação de dígito verificador e formatação canônica: `Cnpj`, `Cpf`, `Cfop`, `Ncm`, `Cst`, `ChaveAcesso`, `InscricaoEstadual`, `CodigosUf`, `TipoTributo`, `GeneroItem`, `ModeloDocumento` (Tabela 4.1.1, regida pelo Ato COTEPE/ICMS).
- Enums transversais regidos pelo Ato COTEPE/ICMS: `CodigoSituacaoDocumentoFiscal` (Tabela 4.1.2), `IndicadorApuracaoIpi`, `IndicadorMovimentacaoFisica`.

### TecnoFisc.Sped.EfdContribuicoes 0.1.0

#### Adicionado

- 203 classes de registro cobrindo todo o leiaute V006:
  - **Bloco 0** (abertura, identificação, tabelas): `0000`, `0001`, `0035`, `0100`, `0110`, `0111`, `0120`, `0140`, `0145`, `0150`, `0190`, `0200`, `0205`, `0206`, `0208`, `0400`, `0450`, `0500`, `0600`, `0900`, `0990`.
  - **Bloco A** (serviços): `A001`, `A010`, `A100`, `A110`, `A111`, `A120`, `A170`, `A990`.
  - **Bloco C** (documentos fiscais — mercadorias): `C001`, `C010`, `C100`, `C110`, `C111`, `C120`, `C170`, `C175`, `C180`, `C181`, `C185`, `C188`, `C190`, `C191`, `C195`, `C198`, `C199`, `C380`, `C381`, `C385`, `C395`, `C396`, `C400`, `C405`, `C481`, `C485`, `C489`, `C490`, `C491`, `C495`, `C499`, `C500`, `C501`, `C505`, `C509`, `C600`, `C601`, `C605`, `C609`, `C800`, `C810`, `C820`, `C830`, `C860`, e demais conforme `sped/STAGE_4_REGISTROS.md`.
  - **Bloco D** (serviços de comunicação/transporte): conjunto completo, incluindo registros referenciados.
  - **Bloco F** (demais documentos e operações): conjunto completo.
  - **Bloco I** (operações de instituições financeiras e seguros): conjunto completo.
  - **Bloco M** (apuração da contribuição e do crédito): conjunto completo, incluindo detalhamentos por CST.
  - **Bloco P** (apuração da contribuição previdenciária sobre receita bruta): conjunto completo.
  - **Bloco 1** (complemento da escrituração): `1001`, `1010`, `1011`, `1020`, `1050`, `1100`, `1101`, `1102`, `1200`, `1210`, `1220`, `1300`, `1500`, `1501`, `1502`, `1600`, `1610`, `1620`, `1700`, `1800`, `1809`, `1900`, `1990`.
  - **Bloco 9** (controle e encerramento): `9001`, `9900`, `9990`, `9999`.
- `ParserEfdContribuicoes` — leitura de arquivo `.txt` em Latin1/Windows-1252 com construção da árvore hierárquica e detecção de erros de formato e leiaute.
- `GeradorEfdContribuicoes` — escrita de arquivo `.txt` com totalizadores por bloco (`9900`) e contagem global (`9999`) calculados automaticamente.
- `ArquivoEfdContribuicoes` e `BlocoEfdContribuicoes` — modelo em memória para manipulação após o parse e antes da geração.
- Round-trip end-to-end (`parse → generate → parse`) validado contra fixture real anonimizada quando disponível em `sped/fixtures/` (PR #106, #107).

### Notas de release

- Distribuição: ainda não publicado em feed NuGet remoto. Pacotes `.nupkg` são gerados pelo CI (`pack` job em push para `main`) e disponibilizados como artefato. A escolha entre Azure Artifacts e GitHub Packages permanece aberta (`ARCHITECTURE.md` §15).
- Performance: parser e gerador implementados sobre `PipeReader` + `ReadOnlySpan<byte>`; benchmarks dedicados (`benchmarks/TecnoFisc.Sped.Benchmarks`) entram na Stage 6 junto com o source generator.
- API streaming (`IAsyncEnumerable<RegistroSped>`) é objetivo da Stage 5 e não está disponível neste release.
- Suporte a leiautes mais novos (V007+) entra na Stage 7.

[Não publicado]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/compare/v0.3.1...HEAD
[0.3.1]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.3.1
[0.3.0]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.3.0
[0.2.0]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.2.0
[0.1.0]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.1.0
