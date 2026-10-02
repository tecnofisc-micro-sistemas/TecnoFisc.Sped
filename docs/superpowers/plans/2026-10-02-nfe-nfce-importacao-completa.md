# NF-e/NFC-e completa e importação em streaming — Implementation Plan

> Execução: tarefas 1–10 implementadas na branch `feat/nfe-importacao-completa`, com verificações consolidadas em `docs/performance/nfe-importacao.md`. A referência reconciliada é PL_010f_v1.04. Os nomes de testes propostos abaixo foram consolidados em testes de folhas/grupos/repetições derivados do inventário e testes de integração/adversariais; as listas abaixo preservam o planejamento original, não são uma lista atual de pendências. Revisão final independente: próxima etapa.

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Completar a leitura tipada de NF-e/NFC-e 4.00 e entregar importação de XML/diretórios/ZIP com memória limitada para o FiscTax.

**Architecture:** Ampliar os modelos e leitores forward-only existentes. Separar leitura de um documento, enumeração de documentos de um envelope e pipeline de importação com canais limitados. A persistência pertence ao consumidor.

**Tech Stack:** C#/.NET 10, System.Xml, System.IO.Compression, System.Threading.Channels, xUnit v3, FluentAssertions e BenchmarkDotNet. Sem dependências adicionais de runtime.

**Spec:** `docs/superpowers/specs/2026-10-02-nfe-nfce-importacao-completa-design.md` (aprovada).

**Status:** plano para revisão. Execução recomendada: nativa, nesta sessão; tarefas dependem dos mesmos modelos, leitores e contratos. Não há autorização de publicação ou alteração do FiscTax.

## Global Constraints

- Performance e streaming são requisitos de aceitação. A biblioteca não acessa o banco do consumidor.
- Manter `XmlReader` forward-only, sem DOM, reflexão ou desserialização intermediária no caminho de leitura.
- Uma nota tipada é materializada por vez por trabalhador; a memória depende do tamanho das notas em processamento, e não do total de arquivos do lote.
- NFe e NFCe permanecem tipos distintos, reutilizando os grupos comuns.
- Omissão não deve virar zero: preservar nulabilidade e precisão decimal.
- Streams fornecidos pelo chamador permanecem abertos; streams abertos pela biblioteca são fechados por ela.
- ZIP dentro de ZIP fica fora do contrato inicial e gera diagnóstico explícito.
- Não chamar uma cobertura de completa sem identificar exatamente os esquemas cobertos.
- Não estabelecer promessa de notas/segundo sem medir corpus e hardware.
- Preservar APIs existentes sempre que possível; nenhuma referência a pacotes EFD ou banco de dados.

## Review Focus

1. Consumidor abandona a enumeração com canais cheios: cancelar e aguardar tarefas sem deadlock (tarefa 8).
2. Protocolo aparece antes da nota ou pertence a outra chave: preservar associação correta, rejeitar inconsistência (tarefa 6).
3. ZIP tem diretório central enorme, ZIP64 ou tamanhos declarados falsos: limitar antes de materializar metadados e durante a descompressão (tarefa 9).
4. XML em stream não posicionável com `ide` ao final: ler uma vez, identificar modelo corretamente e manter o stream aberto (tarefa 6).
5. Schema acrescenta um campo em um tipo compartilhado: detectar ausência na matriz, não aprovar cobertura só por número de propriedades (tarefas 1 e 10).

## Contratos e organização

Prefixos usados abaixo: `S = src/TecnoFisc.Sped.NFeNFCe`, `T = tests/TecnoFisc.Sped.NFeNFCe.Tests`, `B = benchmarks/TecnoFisc.Sped.Benchmarks`. Os caminhos com esses prefixos são relativos à raiz do repositório.

Preservar `Task<NFe> ReadNFeAsync(Stream entrada, CancellationToken cancelamento = default)` e `Task<IDocumentoFiscalXml> ReadAsync(Stream entrada, CancellationToken cancelamento = default)`. Acrescentar em `ParserNFe`:

```csharp
Task<NFCe> ReadNFCeAsync(Stream entrada, CancellationToken cancelamento = default);
Task<Evento> ReadEventoAsync(Stream entrada, CancellationToken cancelamento = default);
IAsyncEnumerable<IDocumentoFiscalXml> ReadDocumentsAsync(Stream entrada, CancellationToken cancelamento = default);
IAsyncEnumerable<ResultadoImportacaoNFe> ReadFilesAsync(IEnumerable<string> caminhos, CancellationToken cancelamento = default);
IAsyncEnumerable<ResultadoImportacaoNFe> ReadDirectoryAsync(string caminho, bool recursivo = false, CancellationToken cancelamento = default);
IAsyncEnumerable<ResultadoImportacaoNFe> ReadZipAsync(Stream entrada, string origem, CancellationToken cancelamento = default);
```

Métodos de documento único rejeitam multiplicidade de notas; `ReadAsync` retorna a nota de envelope SERPRO com eventos, preservando o uso existente. `ReadDocumentsAsync` entrega nota e eventos como documentos distintos e é a base da importação. Em um `nfeProc`, só entregar a nota após concluir a unidade e associar seu protocolo. Em envelopes com múltiplas unidades, um erro posterior não desfaz resultados já entregues; documentar essa semântica.

`S/Importacao/OrigemDocumentoNFe.cs`: `sealed record OrigemDocumentoNFe(string Arquivo, string? EntradaZip = null)`.
`S/Importacao/ResultadoImportacaoNFe.cs`: classe imutável com `OrigemDocumentoNFe Origem`, `IDocumentoFiscalXml? Documento`, `Exception? Erro`, `bool IsSuccess`; factories internas garantem exatamente documento ou erro.
`S/Importacao/LimiteLeituraException.cs`: exceção específica derivada de `IOException`, distinguível de erros de I/O.

Acrescentar opções init-only em `ParserNFeOptions`: `BufferCapacity = 64`, `MaxXmlBytes = 64L * 1024 * 1024`, `MaxXmlDepth = 128`, `MaxZipEntries = 100_000`, `MaxZipEntryBytes = 64L * 1024 * 1024`, `MaxZipUncompressedBytes = 100L * 1024 * 1024 * 1024`, `MaxZipSpoolBytes = 20L * 1024 * 1024 * 1024`, `MaxZipCentralDirectoryBytes = 64L * 1024 * 1024`, `string? TemporaryDirectory = null`. Todos os limites são positivos e ajustáveis; bytes comprimidos do spool e bytes descompactados são orçamentos diferentes. Preservar defaults existentes de `Parallelism`, `FailFast`, `Strict` e `ValidateChecksums`.

Diagnóstico opt-in: `Action<CampoXmlIgnorado>? OnIgnoredElement` nas opções; `S/Parser/CampoXmlIgnorado.cs` define record com namespace, nome, linha e posição. Não capturar texto bruto nem acumular avisos. Callback pode executar concorrentemente e suas exceções propagam, sem serem classificadas como erro fiscal recuperável.

## Task 1 — Referência verificável e baseline

**Files:** criar `sped/nfe/schema-reference.json`, `sped/nfe/coverage.json`, `tools/nfe-schema/inventory.py`, `tools/nfe-schema/test_inventory.py`, `T/SchemaCoverageTests.cs`, `docs/performance/nfe-baseline.md`; usar `B/ParserNFeBenchmark.cs` existente.

**Interfaces:** `inventory.py --schema-dir PATH --output PATH` exporta entradas estáveis por caminho expandido (QName, atributo/elemento, tipo, ocorrência efetiva, ramo de choice). `coverage.json` acrescenta tipo/propriedade CLR, método leitor e caso de teste; omissões ficam explícitas até a tarefa 10.

- [ ] Testar inventário em mini-XSD: incluir tipo compartilhado, ref, atributo herdado, choice opcional e lista; caminhos de dois usos do mesmo tipo devem aparecer, com ocorrências distintas. Ciclos devem produzir erro explícito, sem recursão infinita.
- [ ] Executar `python -m unittest discover -s tools/nfe-schema -p test_*.py`; comprovar falha antes da implementação.
- [ ] Implementar o inventário offline; comparar o pacote local com o download oficial, registrar URL, data, versão e SHA-256 de cada arquivo adotado. Não baixar schemas em runtime. Fixar referência reproduzível no repositório ou artefato de teste, sem depender de arquivos ignorados presentes apenas nesta máquina.
- [ ] Preencher mapeamento dos campos já existentes e classificar todos os restantes nas tarefas 3–5. Excluir apenas assinatura criptográfica da cobertura fiscal, explicitamente; não excluir grupos fiscais especializados. Guardar inventário de referência independente do mapeamento.
- [ ] Executar os testes do inventário e `dotnet test tests/TecnoFisc.Sped.NFeNFCe.Tests -c Release`; os testes de inventário passam e a matriz inicial registra lacunas sem alegar completude.
- [ ] Executar `dotnet run -c Release --project benchmarks/TecnoFisc.Sped.Benchmarks -- --filter '*ParserNFeBenchmark*' --job short`; registrar médias, alocações, ambiente e artefatos no baseline antes de alterar o parser.
- [ ] Commit: `test(nfe): pin schema coverage and parser baseline`.

## Task 2 — Limites de leitura, cancelamento e diagnóstico

**Files:** modificar `S/ParserNFeOptions.cs`, `S/ParserNFe.cs`, `S/Parser/XmlReaderExtensions.cs`; criar `S/IO/LimitedReadStream.cs`, `S/IO/GuardedXmlReader.cs`, `S/Importacao/LimiteLeituraException.cs`, `S/Parser/CampoXmlIgnorado.cs`, `T/Streaming/ReadLimitsTests.cs`, `T/Streaming/TestStreams.cs`.

**Interfaces:** `LimitedReadStream(Stream inner, long maxBytes, bool leaveOpen, CancellationToken cancellationToken)` limita bytes efetivamente lidos; `GuardedXmlReader` aplica profundidade também em operações de skip/subtree. Opções e callback conforme contratos acima.

- [ ] Testes: XML de tamanho exatamente igual ao limite funciona; um byte além falha; profundidade excessiva em grupo desconhecido falha em modo não estrito; cancelamento chega ao `ReadAsync` do stream bloqueado; stream do chamador permanece aberto após sucesso/erro. Defaults existentes continuam iguais.
- [ ] Rodar `dotnet test tests/TecnoFisc.Sped.NFeNFCe.Tests -c Release --filter FullyQualifiedName~ReadLimitsTests` e confirmar falhas.
- [ ] Implementar limites de bytes sem confundir fim artificial com EOF; validar limites antes de I/O. DTD proibido e resolver externo desabilitado. Nenhum caminho síncrono/assíncrono de leitura ou skip pode contornar a contagem/profundidade.
- [ ] Testar callback: um elemento ignorado gera um aviso; `Strict=true` falha; callback que lança exceção propaga. Aplicar a política também nos envelopes, sem avisos para wrappers reconhecidos.
- [ ] Reexecutar testes focados e suite NFe; commit `feat(nfe): bound XML reads and propagate cancellation`.

## Task 3 — Grupos da nota e referências

**Files:** modificar `S/Modelos/NFe.cs`, `S/Modelos/Identificacao.cs`, `S/Modelos/Emitente.cs`, `S/Modelos/Destinatario.cs`, `S/Modelos/Endereco.cs`, `S/Parser/NFeXmlReader.cs`; criar modelos em `S/Modelos/GruposDocumento.cs`, leitores em `S/Parser/NFeXmlReader.Documento.cs`, testes `T/GruposDocumentoTests.cs`.

**Interfaces:** propriedades adicionais `Retirada`, `Entrega`, `AutXML`, `Transp`, `Cobr`, `Pag`, `InfAdic`, `InfRespTec`, `InfIntermed`, `Compra`, `Exporta`, `Cana`; grupos opcionais nulos, coleções vazias quando ausentes. Tipos nomeados em português; cada grupo reflete os filhos da referência, sem bags de `object`/XML. `Identificacao.NFref` contém todas as alternativas de referência do schema.

- [ ] Criar fixtures por grupo com valores distintos, duas ocorrências para cada coleção repetível, opcionais ausentes e zero explícito. Assertar propriedades, ordem, offsets, zeros à esquerda e null versus zero; executar `dotnet test tests/TecnoFisc.Sped.NFeNFCe.Tests -c Release --filter FullyQualifiedName~GruposDocumentoTests` antes de implementar.
- [ ] Completar todos os caminhos de identificação/participantes e grupos documentais da matriz da tarefa 1; adicionar `Read{Grupo}Async(CancellationToken ct)` no partial correspondente, reutilizando o contrato de posicionamento existente. Propagar os grupos pelo modelo intermediário e pelo retorno público.
- [ ] Executar a mesma bateria com filhos reordenados e Id/versao como elemento SERPRO. Nenhum novo campo obrigatório no construtor público existente; dados novos opcionais preservam compatibilidade de código.
- [ ] Atualizar linhas correspondentes de `coverage.json`, rodar suite NFe e commit `feat(nfe): complete document groups and references`.

## Task 4 — Produtos, itens e grupos especializados

**Files:** modificar `S/Modelos/Produto.cs`, `S/Modelos/Item.cs`, `S/Parser/NFeXmlReader.cs`; criar `S/Modelos/GruposProduto.cs`, `S/Parser/NFeXmlReader.Produto.cs`, `T/GruposProdutoTests.cs`.

**Interfaces:** acrescentar Produto.DI, DetExport, Rastro, VeicProd, Med, Arma, Comb e demais filhos apontados pelo inventário; Item.InfAdProd e demais campos do det; coleções `IReadOnlyList<T>`, números fiscais `decimal`, datas sem hora `DateOnly` quando o XSD especificar data.

- [ ] Criar testes para DI com duas adições, duas exportações, dois lotes, combustível com subgrupos, veículo, medicamento e armas. Cada campo folha recebe um valor reconhecível; duas alternativas de choice são testadas em documentos separados. Rodar `dotnet test tests/TecnoFisc.Sped.NFeNFCe.Tests -c Release --filter FullyQualifiedName~GruposProdutoTests` e confirmar falhas.
- [ ] Implementar os leitores especializados e completar campos escalares do produto/itens a partir da matriz, sem arredondar quantidade/preço. Manter independência da ordem.
- [ ] Cobrir os demais caminhos especializados existentes no schema (inclusive agropecuários); não usar a lista de exemplos acima como limite de cobertura.
- [ ] Atualizar matriz, executar testes focados e suite NFe; commit `feat(nfe): complete products and specialized groups`.

## Task 5 — Tributos e totais completos

**Files:** modificar `S/Modelos/Imposto.cs`, `S/Modelos/Total.cs`, `S/Modelos/TotalIcms.cs`, `S/Parser/NFeXmlReader.Icms.cs`, `S/Parser/NFeXmlReader.Tributos.cs`; criar `S/Modelos/Icms02.cs`, `Icms15.cs`, `Icms53.cs`, `Icms61.cs`, `S/Modelos/TributosRtc.cs`, `S/Modelos/TotaisComplementares.cs`, `S/Parser/NFeXmlReader.Rtc.cs`, `S/Parser/NFeXmlReader.Totais.cs`, `T/TributosCompletosTests.cs`.

**Interfaces:** variantes novas derivam de `Icms`; `Imposto` expõe `IcmsUfDest`, `IBSCBS`, `IS`; `Total` expõe `ISSQNtot`, `RetTrib`, `IBSCBSTot`, `ISTot` e demais totais da referência. `Item.ImpostoDevol` separado de `Imposto`. Tipos RTC distintos por grupo/choice, sem enum fechado que perca códigos suportados pelo XSD.

- [ ] Testar cada novo ramo, null versus zero, valores com maior escala aceita, totais não zerados e grupos de diferimento/redução/monofasia/crédito da referência. Rodar `dotnet test tests/TecnoFisc.Sped.NFeNFCe.Tests -c Release --filter FullyQualifiedName~TributosCompletosTests` e confirmar falhas.
- [ ] Implementar leitores e modelos; auditar também campos ausentes nas variantes tradicionais já existentes. Validar que nenhum ramo passa pelos skips de desconhecidos em modo estrito.
- [ ] Vincular testes a cada campo/ramo na matriz, executar suite NFe e commit `feat(nfe): complete tax variants and totals`.

## Task 6 — NFC-e, envelopes e protocolos

**Files:** criar `S/Modelos/NFCe.cs`, `S/Modelos/InformacaoSuplementar.cs`, `S/Parser/DocumentoXmlReader.cs`, `T/DocumentoRoutingTests.cs`; modificar `S/ParserNFe.cs`, `S/Parser/NFeXmlReader.cs`, `S/Modelos/NFe.cs`.

**Interfaces:** implementar ReadNFCeAsync e ReadDocumentsAsync dos contratos; converter o núcleo compartilhado em NFe ou NFCe conforme Ide.Mod.Codigo e verificar modelo da chave. NFCe reutiliza todos os grupos; não herda de NFe. `NFe.IsAutorizacaoEfetiva` e `NFCe.IsAutorizacaoEfetiva` usam `Protocolo?.IsAutorizada == true`, mantendo `IsAutorizada` existente.

- [ ] Testar NFC-e sem destinatário, QR Code CDATA e urlChave; NF-e/SERPRO com ide após det em stream que lança em Seek/Length. Assertar tipo correto, leitura única, grupos preservados e stream aberto. Rodar `dotnet test tests/TecnoFisc.Sped.NFeNFCe.Tests -c Release --filter FullyQualifiedName~DocumentoRoutingTests` e confirmar falhas.
- [ ] Implementar roteamento no mesmo leitor, sem sniffer que consome a entrada e exige rewind; associar protocolo dentro da unidade documental e por chave, independente da ordem. Rejeitar protocolo de chave divergente e versões/modelos incompatíveis.
- [ ] Testar dois nfeProc em envelope reconhecido: ReadDocumentsAsync emite duas notas corretas, método singular rejeita multiplicidade. Elementos `infNFe` de namespace estranho não são tratados como nota fiscal; tolerância sem namespace permanece para SERPRO.
- [ ] Testar protocolo de status não autorizado: presença preservada e status efetivo falso. Rodar suite NFe e commit `feat(nfe): route NFCe and correlate protocols`.

## Task 7 — Eventos e envelopes SERPRO mistos

**Files:** criar `S/Modelos/Evento.cs`, `EventoCancelamento.cs`, `EventoGenerico.cs`, `RetornoEvento.cs`, `S/Parser/EventoXmlReader.cs`, `T/EventoTests.cs`; modificar roteador da tarefa 6.

**Interfaces:** Evento implementa IDocumentoFiscalXml com ChaveAcesso, TpEvento, NSeqEvento nullable para resumo, DhEvento, identificador/versão e retorno; identidade inclui chave, tipo e sequência. EventoCancelamento expõe NProt e XJust. EventoGenerico preserva detalhes desconhecidos de forma limitada, com nomes qualificados, atributos e repetições; esse fallback de evento não substitui modelos fiscais tipados da nota.

- [ ] Fixar também o XSD de cancelamento e as fontes oficiais da semântica dos status no registro da tarefa 1. Criar testes de evento puro/processado, rejeitado, efetivo, sequência distinta e retorno com chave/tipo/seq divergente. Rodar `dotnet test tests/TecnoFisc.Sped.NFeNFCe.Tests -c Release --filter FullyQualifiedName~EventoTests` antes da implementação.
- [ ] Implementar ReadEventoAsync e integração com ReadDocumentsAsync, sem acumular todos os eventos do arquivo. IsEffective só considera retorno que confirma o cancelamento conforme fonte adotada; solicitação sem retorno não é cancelamento efetivo.
- [ ] Testar SERPRO com nota e eventos completos/resumidos (210210/610600). Resumo sem chave herda apenas a chave do contexto documental inequivocamente identificado; não inventar sequência/Id. Se os eventos vierem antes do contexto, usar pendências limitadas ao XML e documentar esse custo.
- [ ] Testar nota + evento + XML truncado: nunca emitir um documento incompleto como sucesso; resultados completos anteriores permanecem válidos. Rodar suite NFe e commit `feat(nfe): stream standalone and embedded events`.

## Task 8 — Resultados e pipeline de diretório/arquivos

**Files:** criar `S/Importacao/OrigemDocumentoNFe.cs`, `ResultadoImportacaoNFe.cs`, `PipelineImportacaoNFe.cs`, `S/ParserNFe.Importacao.cs`, `T/Streaming/DirectoryStreamingTests.cs`; tornar ParserNFe partial.

**Interfaces:** ReadFilesAsync/ReadDirectoryAsync e tipos dos contratos; pipeline interno consome caminhos lazy e ReadDocumentsAsync. `.xml` e `.zip` com extensão ordinal case-insensitive; diretórios só via API explícita, arquivos não suportados informados como resultado de erro quando fornecidos diretamente.

- [ ] Testar produtor lazy, paralelismo 1/2, consumidor lento, capacidade 1, erro entre dois arquivos válidos e FailFast true/false. Assertar origem, documentos e erros sem duplicação. Rodar `dotnet test tests/TecnoFisc.Sped.NFeNFCe.Tests -c Release --filter FullyQualifiedName~DirectoryStreamingTests` antes de implementar.
- [ ] Implementar dois Channel bounded de BufferCapacity, produtores/trabalhadores supervisionados por CTS vinculado; consumidores fazem await de cada emissão. Não criar uma Task por arquivo nem materializar caminhos em lista.
- [ ] XML/formato/limites recuperáveis geram resultado por origem em FailFast=false. Falhas globais de enumeração, permissões/spool/disco e callbacks propagam; não capturar todas as exceções como XML inválido. FailFast preserva a causa original, cancelando demais tarefas.
- [ ] Testar break após primeiro resultado com filas cheias, dispose do enumerador, cancelamento durante I/O e erro do consumidor. Usar prazos apenas para detectar travamento, e assertar workers encerrados/arquivos reabertos sem compartilhamento. Rodar suite NFe e commit `feat(nfe): add bounded file import pipeline`.

## Task 9 — ZIP limitado e spool de streams não posicionáveis

**Files:** criar `S/Importacao/ZipImportReader.cs`, `S/IO/ZipMetadataGuard.cs`, `S/IO/TemporarySeekableStream.cs`, `T/Streaming/ZipStreamingTests.cs`; integrar com ParserNFe.Importacao e pipeline.

**Interfaces:** ReadZipAsync dos contratos; ZipMetadataGuard valida contagem e bytes de diretório central antes de acessar ZipArchive. TemporarySeekableStream produz stream posicionável, usando o original quando possível ou arquivo temporário com limite e limpeza; nunca fecha a entrada do chamador.

- [ ] Testar XML e entradas não XML, nomes duplicados (duas entradas produzem dois resultados), ZIP aninhado, dados corrompidos, stream não posicionável, ZIP64, limites exatos e excedidos. Rodar `dotnet test tests/TecnoFisc.Sped.NFeNFCe.Tests -c Release --filter FullyQualifiedName~ZipStreamingTests` e confirmar falhas.
- [ ] Implementar inspeção incremental de EOCD/ZIP64 e registros centrais dentro do orçamento, validando contagem real e offsets antes de materializar Entries. Rejeitar ZIP multipart/encriptado e estruturas inconsistentes com diagnóstico; não confiar apenas na contagem anunciada no EOCD.
- [ ] Ler entradas sequencialmente por ZIP. Usar limite efetivo mínimo de MaxXmlBytes/MaxZipEntryBytes para XML e contador compartilhado do total; bytes descartados após falha de parse também contam. Ao exceder limite total, encerrar esse ZIP, mesmo em FailFast=false; outros arquivos independentes continuam.
- [ ] Spool usa buffer fixo, MaxZipSpoolBytes, FileOptions.Asynchronous e limpeza em finally. Não extrair entradas em caminhos derivados dos nomes ZIP. Entradas não XML comuns são ignoradas sem descompressão; ZIP aninhado gera erro explícito.
- [ ] Testar cancelamento durante spool/entrada, consumidor interrompido, XML inválido seguido por válido e diretório central adulterado. Assertar ausência de temporários/handles após todos os caminhos. Rodar suite NFe e commit `feat(nfe): import ZIP archives with bounded resources`.

## Task 10 — Fechamento de cobertura, carga e integração documentada

**Files:** completar `T/SchemaCoverageTests.cs`, `sped/nfe/coverage.json`; criar `T/NFeFullCoverageTests.cs`, `B/ParserNFeImportBenchmark.cs`, `B/NFeImportHeapProbe.cs`, `docs/performance/nfe-importacao.md`, `samples/TecnoFisc.Sped.NFeImportacao/Program.cs` e `.csproj`; atualizar `B/Program.cs`, README e Stage 14.

**Interfaces:** probe `--probe nfe-import --count N --parallelism P --buffer B --delay-ms D`; exemplo recebe diretório e processa resultados em lotes de no máximo 500, com sink demonstrativo explicitamente substituível pelo repositório do FiscTax, sem simular gravação real.

- [ ] Teste de cobertura deve falhar para qualquer caminho fiscal sem modelo/leitor/teste; mutar mini-schema com uma folha nova em tipo compartilhado e comprovar detecção. Assertar valores de cada campo via fixtures e expectativas independentes, não só existência de propriedades.
- [ ] Completar casos faltantes apontados pelo inventário; validar fixtures canônicas com XSD somente nos testes, nunca no hot path. SERPRO usa assert de equivalência sem forçar ordem XSD.
- [ ] Benchmark de importação varia 1/50/500 itens, paralelismo 1/4 e XML/ZIP; preparação do corpus fora do trecho medido. Repetir baseline do parser com o mesmo comando da tarefa 1; registrar diferenças de tempo e alocação com hardware/configuração.
- [ ] Executar probe com 10.000 e 100.000 documentos, capacidade 64, paralelismo 1 e 4, consumidor imediato e lento. Registrar throughput, GC, pico de working set, heap retido e bytes/temporários; geração de corpus lazy e fora das medidas de parsing. Verificar que retenção após GC não cresce com total de notas consumidas. Não tratar memória de metadados ZIP como constante.
- [ ] Exemplo demonstra pattern matching NFe/NFCe/Evento, batches limitados, erros por origem, cancelamento, deduplicação/persistência deixadas ao FiscTax. Documentar que sucesso indica leitura, não validação fiscal, XSD ou assinatura.
- [ ] Executar `dotnet build TecnoFisc.Sped.slnx -c Release --nologo`, `dotnet test TecnoFisc.Sped.slnx -c Release --no-build --nologo`, testes Python do inventário e `dotnet build samples/TecnoFisc.Sped.NFeImportacao -c Release`. Confirmar analyzers e suites passam; falhas precisam ser diagnosticadas e resolvidas ou atribuídas com evidência antes de declarar conclusão.
- [ ] Rever documentação para remover escopo piloto e limitações superadas, preservar limitações reais e reportar cobertura/performance medidas. Fazer revisão final do diff e `git diff --check`; commit `docs(nfe): publish coverage and streaming performance evidence`.

## Revisão do plano

Cobertura do desenho: schema/manifesto (1/10), limites e desconhecidos (2), grupos/tributos completos (3–5), NFC-e/protocolos/eventos (6–7), diretórios e backpressure (8), ZIP/spool (9), integração/carga (10). Os cinco casos de Review Focus têm testes nas tarefas indicadas. Novos defaults são decisões propostas neste plano e podem ser alterados na revisão; não são alegações de capacidade medida.

As dez tarefas são entregas verificáveis do mesmo fluxo de importação e devem ser executadas em sequência. Commits ficam locais; publicação, push e PR não são necessários para atender à solicitação. Uma tarefa só é marcada concluída com testes executados e evidência registrada. A aprovação do desenho não é registrada como aprovação automática deste plano.
