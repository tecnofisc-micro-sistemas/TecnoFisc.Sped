# Stage 14 — TecnoFisc.Sped.NFeNFCe

Estado implementado em 02/10/2026. O desenho de importação aprovado em `docs/superpowers/specs/2026-10-02-nfe-nfce-importacao-completa-design.md` substitui o antigo escopo essencial/piloto deste stage. NF-e e NFC-e compartilham o leiaute 4.00 e permanecem em um pacote read-only, com modelos públicos distintos.

## 1. Escopo e referência

Cobertura tipada de `TNFe` e `TProtNFe` do **PL_010f_v1.04**, incluindo grupos especializados, variantes monofásicas, IBS/CBS/IS, totais, protocolo e suplemento NFC-e. Os schemas oficiais e SHA-256 estão versionados em [nfe](nfe). `inventory.json` enumera campos e ocorrências; `field-bindings.json` vincula cada caminho a modelo, propriedade, leitor e teste.

Cancelamento 110111 é tipado; demais eventos usam detalhes estruturados em `EventoGenerico`. O pacote oficial de cancelamento também está fixado com hashes. Assinatura digital é excluída do inventário fiscal: validação criptográfica, XSD em produção, regras fiscais, emissão, NF-e 3.10 e CT-e/MDF-e não fazem parte deste módulo.

## 2. Modelos e identidade

`NFe` e `NFCe` implementam `IDocumentoFiscalXml`; igualdade das notas permanece baseada em chave. Compartilham identificação, participantes, itens, produtos, tributos, transporte, cobrança, pagamentos, informações adicionais e os demais grupos do schema. Coleções repetidas preservam ocorrências; novos escalares opcionais são nullable. Defaults de escalares legados não nullable permanecem por compatibilidade.

`Protocolo` preserva os metadados e mensagens repetidas. A associação exige a mesma chave. `IsAutorizada` mantém o contrato de presença; `IsAutorizacaoEfetiva` usa status 100/150. `EventoCancelamento.IsEffective` exige retorno da mesma chave/tipo/sequência com status 135/155. Leitura não equivale a comprovação criptográfica de autorização/cancelamento.

## 3. Parser core

`XmlReader` forward-only, `Async=true`, DTD proibido, comentários/instruções de processamento ignorados, entrada do chamador mantida aberta. Despacho de filhos por nome, independente da ordem, com namespace fiscal ou sem namespace para SERPRO. Atributos também podem chegar como elementos. Sem DOM, reflexão, serializer intermediário ou geração dinâmica no caminho de produção.

A expansão mecânica dos leitores/modelos foi feita por ferramenta offline, revisada e versionada como C# comum; não há source generator no build ou runtime. Testes lexicais independentes verificam cada folha em duas ordens e todos os caminhos repetidos; uma fixture canônica é validada por XSD somente nos testes.

## 4. APIs de leitura

`ReadNFeAsync`, `ReadNFCeAsync`, `ReadEventoAsync` e `ReadAsync` devolvem unidades singulares. `ReadDocumentsAsync` emite notas e eventos separados de um XML, incluindo envelopes SERPRO. Não há sniffer preliminar que exija rewind. `ReadAsync` preserva a seleção da nota quando ela vem acompanhada de eventos; múltiplas notas exigem enumeração.

`ReadFilesAsync`, `ReadDirectoryAsync` e `ReadZipAsync` devolvem `ResultadoImportacaoNFe`, com documento ou erro e origem. `FailFast=true` continua sendo o padrão. `false` permite recuperar falhas documentais, sem ocultar falhas globais de I/O ou de callback.

## 5. Streaming e limites

Descoberta lazy de caminhos, dois canais limitados e quantidade fixa de trabalhadores. A espera do consumidor aplica backpressure. Cancelamento, descarte e interrupção antecipada aguardam o encerramento de produtor e trabalhadores. Não existe lista global de caminhos/documentos nem uma Task por arquivo.

XML tem limites de bytes e profundidade. ZIP tem limites de entradas, diretório central, bytes efetivamente descompactados por entrada/total e spool. Metadados EOCD/ZIP64 são inspecionados antes de materializar `ZipArchive.Entries`. Entradas XML são lidas sequencialmente e verificadas por CRC32, sem extração por nome. Uploads não posicionáveis usam temporário limitado e removido em todos os caminhos de saída.

Memória é proporcional às notas em voo, filas e metadados ZIP, não ao total de XMLs do diretório. Resumos SERPRO sem chave podem ser retidos até o fechamento do NFeLog, dentro do limite de bytes, para exigir contexto inequívoco de uma nota.

## 6. Integração e correlação

Persistência, deduplicação, transações e correlação histórica pertencem ao FiscTax. Eventos podem chegar antes/depois da nota ou em outro arquivo. Nenhum parser mantém estado fiscal entre chamadas. Não foi implementado o `Correlator` proposto no desenho antigo: ele não é necessário para a API de importação aprovada e uma coleção global contrariaria a ingestão limitada.

O [guia de integração](../docs/nfe-importacao.md) apresenta contratos, defaults, tratamento de falhas e limites. O [exemplo executável](../samples/TecnoFisc.Sped.NFeImportacao/Program.cs) consome lotes limitados com destino demonstrativo. O [relatório de desempenho](../docs/performance/nfe-importacao.md) registra as medições e limitações do corpus.

## 7. Verificação e evolução

Execute testes .NET da solução e `python -m unittest discover -s tools/nfe-schema -p test_*.py`. O CI verifica inventário, hashes, valores de campos e regressões. O inventário é recalculado dos schemas fixados; atualizar apenas a matriz manual não encobre uma nova folha do XSD.

Novas Notas Técnicas exigem atualizar pacote/hash, inventário, modelos/leitores e expectativas independentes. Elementos desconhecidos podem ser diagnosticados por `OnIgnoredElement` ou rejeitados com `Strict=true`; esse modo não substitui validação fiscal/XSD. Encoding segue a declaração XML da BCL, sem tentativa silenciosa de reinterpretar bytes inválidos.
