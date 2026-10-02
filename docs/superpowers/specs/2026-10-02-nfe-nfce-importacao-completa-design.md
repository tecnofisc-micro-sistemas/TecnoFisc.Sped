# Leitura completa de NF-e/NFC-e e importação em grande volume

Status: desenho aprovado pelo usuário em 2026-10-02; plano de implementação em revisão; implementação ainda não iniciada.

## Objetivo e decisões de escopo

Receber grandes quantidades de XML de NF-e/NFC-e, inclusive ZIP, e entregar dados tipados para o FiscTax importar as operações. Performance e streaming são requisitos de aceitação. A biblioteca não acessa o banco do consumidor.

A solicitação atual amplia o escopo de `sped/STAGE_14_NFE_NFCE.md`: os grupos anteriormente excluídos da v1 passam a fazer parte da cobertura. Preservar APIs existentes sempre que possível, sem perpetuar omissões silenciosas em documentos declarados suportados.

Proposta de referência: leiaute 4.00, modelos 55 e 65, com cobertura verificável do pacote local `PL_010c_NT2022_002v1.30` e seus includes. Antes da implementação, comparar esse pacote com os esquemas oficiais disponíveis e registrar versões, origem e hashes da referência efetivamente adotada. Não chamar uma cobertura de completa sem identificar exatamente os esquemas cobertos. Leiautes históricos, como 3.10, exigem cobertura própria e não ficam implicitamente incluídos em 4.00.

Fonte oficial para reconciliação: https://www.nfe.fazenda.gov.br/portal/listaConteudo.aspx?tipoConteudo=BMPFMBoln3w%3D

## Estado verificado

- Parser atual usa `XmlReader` assíncrono, proíbe DTD e mantém aberto o stream do chamador.
- Leitura independente da ordem dos elementos, incluindo envelope SERPRO de uma nota.
- Existem identificação, participantes, produtos, itens, protocolo e diversos impostos tradicionais.
- Faltam grupos documentais, especializados, NFC-e como tipo distinto, eventos e ingestão em lote/ZIP.
- XSD local contém ICMS02/15/53/61, IBSCBS, ISTot e IBSCBSTot; a marcação antiga de impostos completos não comprova cobertura desses grupos.
- `Parallelism` e `FailFast` existem nas opções, mas não há API de diretório implementada.
- Base de validação: `dotnet test tests/TecnoFisc.Sped.NFeNFCe.Tests/TecnoFisc.Sped.NFeNFCe.Tests.csproj -c Release --nologo`: 98 aprovados, zero falhas e zero ignorados, em 2026-10-02.

## Alternativas e escolha proposta

1. Ampliar os leitores e modelos existentes, com manifesto de cobertura XSD e pipeline limitado. Recomendada: preserva compatibilidade e permite medir cada mudança no caminho atual.
2. Gerar todos os modelos e leitores a partir de XSD. Reduz repetição, mas demanda resolver escolhas, tipos compartilhados e particularidades SERPRO; avaliar somente se a expansão manual demonstrar custo ou risco maior.
3. Expor árvores XML ou dicionários genéricos. Não atende ao objetivo principal de dados tipados para importação e acrescenta custo de interpretação ao consumidor.

## Cobertura e organização dos dados

Criar matriz verificável por caminho de schema: elemento/atributo, tipo CLR, propriedade pública, leitor e teste. Resolver includes, tipos nomeados, escolhas e ocorrências repetidas. Cada campo fiscal do schema de referência precisa ter representação tipada; armazenar XML bruto não conta como cobertura tipada.

Incluir identificação completa e referências; emitente/destinatário; retirada/entrega; autorizados; produtos e itens; DI/adições; exportação; rastreabilidade; medicamentos; veículos; armas; combustíveis; cana; transporte; cobrança; pagamentos/cartões; informações adicionais; intermediador; responsável técnico; compras e demais grupos presentes no schema.

Completar tributos e totais, incluindo monofásicos, partilha, devolução, retenções e IBS/CBS/IS conforme a referência. Omissão não deve virar zero: preservar nulabilidade e precisão decimal. Preservar repetições, identificadores textuais e offsets de datas.

NFe e NFCe permanecem tipos distintos, reutilizando os grupos comuns. NFCe inclui `infNFeSupl`, QR Code e destinatário opcional. `ReadAsync` identifica o tipo sem reler ou copiar todo o stream; métodos específicos rejeitam modelo incompatível.

Ler protocolos e eventos com sua própria identidade e vínculo com a chave. Incluir cancelamento tipado e fallback documentado para eventos de outros tipos. Nunca associar o primeiro protocolo encontrado a uma nota de outra chave. Preservar o contrato atual de `IsAutorizada` até revisão explícita de compatibilidade; oferecer status efetivo baseado no protocolo sem tratar sua mera presença como autorização fiscal.

## Streaming e orçamento de memória

Manter `XmlReader` forward-only, sem DOM, reflexão ou desserialização intermediária no caminho de leitura. Uma nota tipada é materializada por vez por trabalhador; a memória depende do tamanho das notas em processamento, e não do total de arquivos do lote.

Expor enumeração assíncrona de resultados para diretórios, fontes enumeráveis e ZIP. Descoberta de arquivos lazy, fila de trabalho limitada, número configurável de trabalhadores e fila de resultados limitada. A lentidão do FiscTax suspende os produtores por backpressure. Ordem de conclusão não garantida; origem acompanha cada resultado.

Limitar bytes descompactados por entrada e total por ZIP, tamanho máximo de XML, profundidade XML e número de entradas. Validar limites ao consumir os bytes, sem confiar apenas nos tamanhos declarados. Limites e paralelismo configuráveis e validados antes de iniciar I/O.

ZIP: leitura incremental das entradas, sem extração geral e sem `MemoryStream` do arquivo inteiro. ZIPs distintos podem executar em paralelo. Não compartilhar um stream de arquivo compactado entre leitores concorrentes. O diretório central do `ZipArchive` também ocupa memória proporcional às entradas: limitar explicitamente essa quantidade e documentar esse custo; não anunciar memória constante para ZIP arbitrário. Streams ZIP não posicionáveis devem usar spool temporário em disco com tamanho limitado e limpeza garantida, nunca cópia integral em RAM. XML não posicionável deve funcionar diretamente. ZIP dentro de ZIP fica fora do contrato inicial e gera diagnóstico explícito.

Cancelamento e encerramento antecipado do `await foreach` encerram produtores/trabalhadores, liberam arquivos e removem temporários. Propagar cancelamento ao I/O subjacente onde suportado. Streams fornecidos pelo chamador permanecem abertos; streams abertos pela biblioteca são fechados por ela.

## Erros e integração com FiscTax

Cada resultado contém origem (arquivo e, quando aplicável, entrada ZIP), documento ou diagnóstico. Em `FailFast=true`, interromper o pipeline com a causa original; em `false`, emitir a falha e continuar as unidades independentes. ZIP corrompido pode inviabilizar o restante daquele arquivo, mas não outros arquivos. Não usar lista global de erros nem logging como único modo de observação.

Elementos desconhecidos preservam a semântica de `Strict`. Acrescentar diagnóstico opt-in de campos ignorados para que novas versões não passem despercebidas ao consumidor. Erros de infraestrutura e programação não devem ser ocultados como simples XML inválido.

O FiscTax controla transações, tamanho de batch, deduplicação e persistência de eventos. Exemplo de integração deve consumir resultados incrementalmente, gravar batches limitados e liberar as notas já importadas. Correlação global de notas/eventos não pode exigir manter todas as notas em memória.

## Validação e critérios de entrega

- Cobertura dos campos do schema documentada e testada, inclusive escolhas, atributos e listas; fixtures sintéticas não contêm dados privados.
- Casos canônico e SERPRO, NF-e e NFC-e, protocolos antes/depois da nota, namespaces, XML truncado, grupos vazios e modelo incompatível.
- Precisão de valores, nulabilidade, grupos especializados e tributos da referência.
- XML não posicionável, ZIP posicionável/não posicionável, entradas ignoráveis, ZIP corrompido, limites de descompressão e descarte antecipado.
- `FailFast` e continuação, cancelamento durante I/O e produtor bloqueado, consumidor lento, ausência de tarefas órfãs e arquivos presos.
- BenchmarkDotNet: tempo e bytes alocados por nota, notas/segundo para diretório e ZIP; matrizes por tamanho de nota e paralelismo. Medir baseline antes das alterações e comparar no mesmo hardware/configuração.
- Ensaio de carga com lotes crescentes e consumidor lento mede pico de memória de processo e retenção após consumo; separar memória de documentos, buffers, metadados ZIP e consumidor. Demonstrar que o conjunto de XML não é acumulado na RAM.
- Não estabelecer promessa de notas/segundo sem medir corpus e hardware. Qualquer regressão precisa ser quantificada e explicada antes de concluir.
- Testes existentes e novos em Release, build com analyzers, documentação de APIs e exemplo de importação para o FiscTax.

## Sequência proposta

1. Fixar referência XSD, gerar inventário de lacunas e registrar baseline de performance.
2. Completar modelos/leitores por famílias de grupos, com testes de cobertura.
3. Completar NFC-e, roteamento de documentos, protocolos e eventos.
4. Implementar resultados de importação, diretórios e pipeline com memória limitada.
5. Integrar ZIP com limites, spool quando necessário e cancelamento.
6. Executar carga, corrigir gargalos demonstrados e entregar exemplo de consumo e relatório de cobertura/performance.

Esta sequência é o desenho proposto, não um registro de implementação concluída. O plano executável deve especificar as interfaces e tarefas após a revisão deste documento.
