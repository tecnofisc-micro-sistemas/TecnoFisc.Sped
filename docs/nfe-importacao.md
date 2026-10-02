# Importação de NF-e e NFC-e

`TecnoFisc.Sped.NFeNFCe` lê o leiaute 4.00 em objetos tipados para o consumidor persistir. A referência é o pacote oficial **PL_010f_v1.04**, publicado em 31/08/2026. Os XSD, hashes, inventário e vínculos modelo/leitor/teste estão em [`sped/nfe`](../sped/nfe). A cobertura fiscal inclui `TNFe` e `TProtNFe`; assinatura digital é excluída deliberadamente.

São contemplados identificação e referências, participantes e endereços, retirada/entrega, autorizados, produtos, importações/adições, exportações, rastreabilidade, veículos, medicamentos, armamentos, combustíveis, cana, agropecuária, transporte, cobrança, pagamentos, intermediador, informações adicionais, responsável técnico e os demais grupos da referência. Os tributos incluem os ramos tradicionais, ICMS02/15/53/61, partilha, devolução, retenções, IBS/CBS/IS e seus totais.

## APIs

| Método | Resultado |
| --- | --- |
| `ReadNFeAsync(Stream, CancellationToken)` | Uma NF-e modelo 55 |
| `ReadNFCeAsync(Stream, CancellationToken)` | Uma NFC-e modelo 65, com `InfNFeSupl` |
| `ReadEventoAsync(Stream, CancellationToken)` | Um evento |
| `ReadAsync(Stream, CancellationToken)` | Documento identificado sem rewind |
| `ReadDocumentsAsync(Stream, CancellationToken)` | Documentos completos de um XML/envelope |
| `ReadFilesAsync(IEnumerable<string>, CancellationToken)` | Resultados de caminhos XML/ZIP descobertos de forma lazy |
| `ReadDirectoryAsync(string, bool recursivo, CancellationToken)` | Resultados dos arquivos XML/ZIP do diretório |
| `ReadZipAsync(Stream, string origem, CancellationToken)` | Resultados das entradas XML de um ZIP |

`ReadAsync` rejeita múltiplas notas. Para compatibilidade, um envelope com uma nota e eventos retorna a nota; use `ReadDocumentsAsync` para receber também os eventos. Um envelope contendo apenas eventos é aceito pelo método singular somente quando há um evento.

Os métodos de stream mantêm aberta a entrada do chamador. XML não exige `Seek`, `Length` nem cópia integral. São aceitos `nfeProc`/`procNFe`, `NFe`, `procEventoNFe`, `evento`/`eventoNFe` e o envelope SERPRO. O parser fiscal aceita o namespace oficial e a ausência de namespace, preserva a independência da ordem dos campos e reconhece atributos representados como elementos no SERPRO.

## Integração com FiscTax

```csharp
var parser = new ParserNFe(new ParserNFeOptions
{
    Parallelism = 4,
    BufferCapacity = 16,
    FailFast = false
});

await foreach (var resultado in parser.ReadDirectoryAsync(pasta, true, cancelamento))
{
    if (!resultado.IsSuccess)
    {
        RegistrarFalha(resultado.Origem, resultado.Erro!);
        continue;
    }

    // Aguardar a persistência aplica backpressure à leitura.
    await PersistirNoFiscTaxAsync(resultado.Documento!, cancelamento);
}
```

As funções de registro/persistência acima pertencem à aplicação. O [exemplo executável](../samples/TecnoFisc.Sped.NFeImportacao/Program.cs) demonstra lotes limitados a 500 resultados, pattern matching de NF-e/NFC-e/eventos e Ctrl+C; seu destino é demonstrativo, sem acesso a banco.

A biblioteca não deduplica nem impõe transações. FiscTax deve definir idempotência por chave da nota e por chave/tipo/sequência do evento, além de tratar eventos posteriores à nota. Nomes duplicados em ZIP geram resultados distintos; `Origem.Arquivo` e `Origem.EntradaZip` identificam a origem disponível. A ordem de conclusão entre arquivos não é garantida.

`Evento.Equals` usa chave/tipo/sequência quando a identidade está completa, independentemente do `Id` opcional. Eventos sem sequência ou chave têm igualdade por referência: dois resumos importados separadamente permanecem distintos, mesmo com o mesmo conteúdo. O consumidor decide como reconciliá-los.

Os envelopes reconhecidos são SOAP 1.1/1.2 (`Envelope`/`Body`), `nfeConsultaNFeLogResult`, `retConsNFeLog` e `NFeLog`, além das unidades fiscais. Elementos desconhecidos são ignorados como subárvore com diagnóstico, ou rejeitados com `Strict=true`; a mesma política vale fora de `infNFe`. A assinatura XMLDSIG continua dispensada de mapeamento.

## Memória, concorrência e ZIP

O caminho de produção usa `XmlReader` forward-only, sem DOM, reflexão ou desserialização intermediária. Cada trabalhador materializa uma unidade documental. Dois canais limitados controlam caminhos pendentes e resultados; um consumidor lento suspende os trabalhadores e, depois, a descoberta de arquivos. `break`, descarte e cancelamento encerram e aguardam produtor/trabalhadores.

| Opção | Padrão |
| --- | ---: |
| `Parallelism` | `Environment.ProcessorCount` |
| `BufferCapacity` | 64 por canal |
| `MaxXmlBytes` | 64 MiB por XML |
| `MaxXmlDepth` | 128 níveis |
| `MaxZipEntries` | 100.000 entradas |
| `MaxZipEntryBytes` | 64 MiB por entrada lida |
| `MaxZipUncompressedBytes` | 100 GiB efetivamente lidos por ZIP |
| `MaxZipCentralDirectoryBytes` | 64 MiB |
| `MaxZipSpoolBytes` | 20 GiB por upload não posicionável |
| `TemporaryDirectory` | diretório temporário do sistema |

Os limites são positivos e configuráveis. Não constituem reserva nem garantia de RAM: o grafo de objetos pode ocupar mais que o XML. A memória depende do tamanho das notas, filas, paralelismo e metadados dos ZIPs abertos, não da quantidade total de XMLs do diretório. Reduza capacidade/paralelismo para documentos grandes. O limite total de ZIP conta bytes realmente descompactados, inclusive os já lidos antes de uma falha; entradas ignoradas não são descompactadas.

Um ZIP posicionável é inspecionado e reposicionado no início. Um upload não posicionável é copiado para arquivo temporário com buffer fixo e limite de bytes, removido ao concluir, falhar, cancelar ou interromper a enumeração. `TemporaryDirectory` deve existir. O ZIP precisa estar completo antes da enumeração das entradas; isso é uma característica do diretório central do formato.

EOCD/ZIP64, quantidade real de registros e orçamento de metadados são verificados antes da materialização das entradas pela BCL. Dentro de cada ZIP, a leitura é sequencial; vários arquivos ZIP podem ser processados por trabalhadores distintos. Entradas XML têm verificação de tamanho e CRC32. Nenhuma entrada é extraída usando seu nome. ZIPs aninhados, criptografados, multipartes ou com compressão diferente de Store/Deflate geram erro explícito. Arquivos comuns não XML dentro do ZIP são ignorados.

## Erros e semântica fiscal

`FailFast=true` é o padrão. Com `false`, XML inválido, formato fiscal incompatível, ZIP inválido ou limite excedido produzem `ResultadoImportacaoNFe` com `Erro` e origem. O limite total encerra o ZIP atingido; outros arquivos podem continuar. Falhas de enumeração, permissões, disco/spool e callbacks propagam. Exceções do callback são encapsuladas em `FalhaCallbackXmlException`, com causa em `InnerException`.

Uma unidade só é emitida após seu fechamento. Um envelope pode produzir documentos completos e depois falhar no restante do XML ou na integridade da entrada ZIP; portanto, sucesso individual não é confirmação transacional do arquivo inteiro. A aplicação decide se mantém esses documentos ou os associa a uma importação pendente até o fim do arquivo.

`Strict=false` permite elementos futuros desconhecidos. `OnIgnoredElement` recebe nome, namespace e posição, sem armazenar o payload; o callback deve suportar concorrência e evitar retenção ilimitada. `Strict=true` rejeita elementos fiscais não mapeados. DTD é proibido. O parser **não valida XSD, assinatura, cálculos ou regras de autorização fiscal** em produção; sucesso significa leitura estruturada.

Chaves, CNPJ/CPF e tipos fiscais existentes mantêm suas validações de construção. `ValidateChecksums` controla o GTIN opcional; não desliga a validação da chave ou dos documentos. CNPJ e chaves aceitam o alfabeto oficial da [NT conjunta 2025.001](https://www.nfe.fazenda.gov.br/Portal/exibirArquivo.aspx?conteudo=5ZkvIZt10mQ%3D). Chaves com CPF preenchido à esquerda seguem a [NT 2023.002](https://www.nfe.fazenda.gov.br/portal/exibirArquivo.aspx?conteudo=4m9xb5EYa%2Fw%3D); a identificação considera séries aplicáveis e o indicador de CPF da NFF ([NT 2021.002](https://www.nfe.fazenda.gov.br/Portal/exibirArquivo.aspx?conteudo=hFtSfuCjY0A%3D)). Consulte `CpfEmitente` antes de acessar `CnpjEmitente` nesses casos.

Decimais são preservados sem arredondamento adicional, datas mantêm offsets e códigos textuais mantêm zeros à esquerda. Novos campos opcionais usam tipos nullable ou coleções vazias. Os escalares legados não nullable mantêm seus defaults por compatibilidade — por exemplo `TotalIcms` — e não substituem validação de presença pelo consumidor. Mensagens `CMsg`/`XMsg` do protocolo são listas, com associação pela posição.

`NFe.IsAutorizada` mantém o contrato legado de presença do protocolo. `IsAutorizacaoEfetiva` consulta seu status (100/150). Nenhum deles verifica criptograficamente a autorização.

Cancelamento 110111 é tipado. `IsEffective` exige retorno da mesma chave/tipo/sequência com status 135 ou 155; uma solicitação isolada ou status 136 não comprova cancelamento. Os demais eventos usam `EventoGenerico`, com árvore limitada de detalhes, nomes qualificados, atributos e repetições. Resumos SERPRO 610500–610615 sem chave aguardam o fim do `NFeLog` e só herdam a chave se houver exatamente uma nota. Essa pendência é limitada pelo orçamento do XML; não inventa sequência/identificador. Resumos sem sequência exigem critério próprio de idempotência; chave/tipo isolados não distinguem todas as vinculações.

## Cobertura verificável

`inventory.json` é calculado dos XSD fixados; `field-bindings.json` associa caminhos aos modelos/leitores/testes. Testes independentes verificam valores de cada folha, ordem canônica/reordenada, conversão atributo→elemento e ocorrências repetidas. A versão externa do protocolo é verificada pela API pública. As fixtures lexicais de cobertura incluem escolhas alternativas simultâneas para exercitar leitores isolados e **não são apresentadas como XMLs fiscalmente válidos**; uma fixture canônica tem validação XSD separada, somente nos testes.

Os [benchmarks e testes de carga](performance/nfe-importacao.md) registram throughput, alocações, retenção e condições das medições. A cobertura é vinculada à referência fixada; novas Notas Técnicas exigem atualizar XSD, inventário e testes. NF-e 3.10, emissão de XML, CT-e/MDF-e e validação criptográfica permanecem fora deste módulo.
