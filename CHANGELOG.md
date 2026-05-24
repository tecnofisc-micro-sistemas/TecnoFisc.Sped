# Changelog

Todas as mudanças relevantes deste repositório são documentadas neste arquivo.

O formato segue [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/) e o projeto adota [Semantic Versioning](https://semver.org/lang/pt-BR/). Cada pacote NuGet possui versão independente; as seções abaixo agrupam as mudanças por release do repositório.

## [Não publicado]

## [0.5.0] — 2026-05-24

Release breaking. Revisa a convenção de nomenclatura da API pública (verbos, factories estáticos e predicados booleanos passam a usar inglês idiomático; substantivos do domínio SPED permanecem em português) e adiciona três helpers de persistência sobre o `IAsyncEnumerable<RegistroSped>` produzido pelos parsers: `OfType<T>()`, `Batch(n)` e `WithContext()`, mais um dispatcher Visitor source-generated por leiaute.

### Documentação

#### Alterado (breaking)

- Convenção de nomenclatura revisada (ARCHITECTURE §1.3): verbos, factories estáticos e predicados booleanos passam a usar inglês idiomático; substantivos do domínio SPED permanecem em português. API pública renomeada — ver detalhes abaixo.

### TecnoFisc.Sped.Core 0.5.0

#### Adicionado

- Namespace `TecnoFisc.Sped.Core.Streaming` com dois extension methods sobre o `IAsyncEnumerable<RegistroSped>` produzido pelos parsers: `OfType<T>()` filtra pelo tipo concreto de registro (zero reflection — pattern matching resolvido em compile-time) e `Batch(int size)` agrupa em lotes para bulk-insert em banco (EF Core `AddRangeAsync`, Dapper, `SqlBulkCopy`). Cobre o caso de uso mais comum de ingestão SPED → banco sem o consumidor precisar implementar boilerplate de cast + buffer manual. Memória continua bounded — só o lote corrente fica em memória. (#414)
- `WithContext()` (mesmo namespace) enriquece o stream com `ContextoPersistencia { IdRegistroAtual, IdPai }` contendo IDs surrogate sequenciais já amarrados à hierarquia. Resolve persistência relacional (PK/FK) sem o consumidor precisar manter stack manual de IDs. Overload `WithContext(startAt: ...)` permite retomar import multi-arquivo. (#416)

#### Alterado (breaking)

- Value objects: `Criar` → `Create` em `Cnpj`, `Cpf`, `Cfop`, `Ncm`, `Cst`, `ChaveAcesso`, `InscricaoEstadual`, `ModeloDocumento`, `GeneroItem`.
- `Cfop.EhEntrada` / `Cfop.EhSaida` → `Cfop.IsEntrada` / `Cfop.IsSaida`.
- `InscricaoEstadual.EhIsento` → `IsIsento`.
- `CodigosUf.EhValido` → `IsValid`.
- `ILeitorSped.LerStreamingAsync` / `LerAsync` → `ReadStreamingAsync` / `ReadAsync`. `LeitorSpedTxt` segue o mesmo.
- `IEscritorSped.EscreverAsync` → `WriteAsync`. `EscritorSpedTxt` segue.
- `CatalogoBuilder.ConstruirMetadadosDoTipo` → `BuildMetadataForType`.

### TecnoFisc.Sped.EfdContribuicoes 0.5.0

#### Alterado (breaking)

- `ParserEfdContribuicoes.LerAsync` / `LerStreamingAsync` → `ReadAsync` / `ReadStreamingAsync`.
- `GeradorEfdContribuicoes.EscreverAsync` → `WriteAsync`.
- `ArquivoEfdContribuicoes.CarregarAsync` → `LoadAsync`.

### TecnoFisc.Sped.EfdIcmsIpi 0.5.0

#### Alterado (breaking)

- `ParserEfdIcmsIpi.LerAsync` / `LerStreamingAsync` → `ReadAsync` / `ReadStreamingAsync`.
- `ArquivoEfdIcmsIpi.CarregarAsync` → `LoadAsync` (se aplicável).

### TecnoFisc.Sped.Core.SourceGenerators 0.5.0

#### Adicionado

- Source generator passa a emitir, por assembly consumidor, uma interface `IRegistroSpedVisitor` com um overload `VisitAsync(TipoConcreto)` default vazio para cada classe decorada com `[RegistroSped]`, mais `VisitUnknownAsync(RegistroSped)` para registros fora do assembly. Acompanha extension `RegistroSpedVisitorExtensions.DispatchAsync(IAsyncEnumerable<RegistroSped>, IRegistroSpedVisitor, CancellationToken)` que despacha cada registro para o overload correto via `switch` resolvido em compile-time. Permite ao consumidor evitar o `switch` gigante (200+ casos no EFD Contribuições, 255+ no EFD ICMS-IPI) sobrescrevendo apenas os tipos que importam. Zero reflection, zero boxing. (#415)

#### Alterado (breaking)

- Código gerado passa a invocar `Create` em vez de `Criar` nos value objects.

## [0.4.0] — 2026-05-24

Consolida três incrementos do leiaute EFD ICMS-IPI (V018, V019, V020 — vigente em 2026) e oficializa o pacote como **read-only**, alinhado ao caso de uso real (ingestão rápida + modelo tipado). Também esclarece o escopo dos pacotes XML (NF-e, NFC-e, CT-e), que passam a ser planejados também como read-only — o único pacote SPED com geração de arquivo confirmada permanece sendo o `TecnoFisc.Sped.EfdContribuicoes`.

### TecnoFisc.Sped.EfdIcmsIpi 0.4.0

#### Adicionado

- Suporte ao leiaute **V018** (Guias Práticos 3.1.5/3.1.6, vigência fiscal jan/2024): novos campos 21-23 `QTD_RESIDUO_DDG/WDG/CANA` no `Registro1391` e doc-comments mecânicos cobrindo NF3-e (modelo 66 no `RegistroC700`), Convênio 115/03 e escrituração consolidada NFCom (`D700`/`D730`/`D750`/`D760`), com reflexos nas apurações `E110`/`E113`/`E210`/`E240` e no `Registro1400`.
- Suporte ao leiaute **V019** (Guias Práticos 3.1.7/3.1.8/3.1.9, vigência fiscal jan/2025): novo campo `DED` (valor das deduções) em `RegistroD700` (32) e `RegistroD750` (17), e doc-comments mecânicos cobrindo CT-e Simplificado (`D130`), DSI no `C120`, observação sobre Reforma Tributária do Consumo (IBS/CBS/IS) no `C100`/`C190`, DIFAL EC 87/2015 no `0150` e revisões de obrigatoriedade/validação em `C700`/`D100`/`E113`/`D700`/`D750`.
- Suporte ao leiaute **V020** (Guias Práticos 3.2.0/3.2.1/3.2.2, vigência fiscal jan/2026 — leiaute vigente): novo campo `CAP_TANQUE` (capacidade do tanque em litros) em `Registro1310` (11) e doc-comments mecânicos cobrindo Reforma Tributária do Consumo + Ajuste SINIEF 49/25 no `C100`, orientações de preenchimento em `0150`/`D100`/`D700`/`K230` e mudança de tipo N→C do campo `SER` no `D700` (já modelado lazy como `string?` desde V017).

#### Alterado (breaking)

- Pacote passa a ser **read-only** (ARCHITECTURE §2.5). API pública `GeradorEfdIcmsIpi` removida; `IEscritorSped` deixa de ser implementado neste pacote. Consumidores que precisam emitir arquivos EFD ICMS-IPI devem usar o PVA da Receita ou outro caminho — o propósito do pacote é ingestão rápida + modelo tipado.
- `[Descontinuado(EmVersao=...)]` vira informacional no read path — registros descontinuados continuam sendo reconhecidos pelo parser para que arquivos históricos sejam lidos sem erro de leiaute.
- Testes de round-trip parse → generate → parse removidos (`RoundTripFixtureRealTests` renomeado para `ParserFixtureRealTests`, cobrindo apenas o caminho de leitura).

### Documentação

#### Alterado

- `ARCHITECTURE.md` §2.5 e §4.7, `README.md` e `CLAUDE.md` atualizados para refletir que os pacotes XML planejados (`TecnoFisc.Sped.NFe`, `NFCe`, `CTe`) também serão **read-only**. O caso de uso confirmado nos três é ingestão de XMLs já emitidos (parser + validação de assinatura + modelo tipado). Geração/emissão para SEFAZ depende de confirmação externa e, quando ocorrer, entra como stage dedicada (igual a ECD/ECF). Resultado: o único pacote SPED com geração de arquivo confirmada hoje é `TecnoFisc.Sped.EfdContribuicoes`.
- Stages 14/15/16 (NFe/NFCe/CTe) em `ARCHITECTURE.md` reescritos como pacotes read-only — `GeradorNFe`/`GeradorNFCe`/`GeradorCTe` saem do escopo inicial.
- README do repositório atualizado para refletir EFD ICMS-IPI 0.4.0 (V015 baseline + V016-V020 incrementos, parser apenas) e marcar pacotes XML como `planejado (XML, read-only)`.

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

[Não publicado]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/compare/v0.5.0...HEAD
[0.5.0]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.5.0
[0.4.0]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.4.0
[0.3.1]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.3.1
[0.3.0]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.3.0
[0.2.0]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.2.0
[0.1.0]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.1.0
