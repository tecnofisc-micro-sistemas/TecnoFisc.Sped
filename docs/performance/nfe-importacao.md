# NF-e/NFC-e: cobertura e desempenho da importação

Medições locais em 02/10/2026, Windows 11 10.0.26300.9457, Intel Core i9-14900K (24 núcleos/32 processadores lógicos), .NET SDK 10.0.302/runtime 10.0.10, Release, BenchmarkDotNet 0.14.0. O BDN não identificou o nome do processador; o modelo foi consultado por `Win32_Processor`. Não há promessa de throughput em outra máquina, armazenamento ou corpus.

## Cobertura e verificação

- PL_010f_v1.04: 1.107 linhas de inventário, **1.093 caminhos fiscais únicos**, incluindo **922 folhas**. A diferença decorre de caminhos compartilhados entre alternativas de choice.
- 921 vetores lexicais executados em ordem canônica e reordenada/atributo como elemento; a versão externa do protocolo é coberta por teste da API pública.
- 168 testes de ligação dos grupos ao modelo pai e 26 testes de repetição. As variantes de choice são isoladas nos testes de ligação.
- Hashes dos arquivos XSD fixados, recomputação do inventário em CI, validação estrutural XSD de nota e solicitação de cancelamento nos testes. Assinaturas dessas fixtures são apenas estruturais, sem validade criptográfica.
- Solução: **7.919 aprovados**, zero falhas e quatro skips preexistentes de fixtures privadas ECF. Sete testes Python aprovados. Exemplo compilado e executado: duas notas e dois eventos, sem gravação em banco. A revisão e as correções finais estão registradas em [encerramento](../superpowers/plans/2026-10-02-nfe-nfce-encerramento.md).

## Parser: antes e depois

Mesma geração de XML, fora da medição; 1/50/500 itens ICMS60; memória de entrada já disponível. Baseline em `64c65f5` (parser parcial, apenas atualização de dependência de build em relação a `c894b80`). A medição inicial está em [nfe-baseline.md](nfe-baseline.md); a comparação abaixo usa a repetição com cinco warmups/cinco iterações, executada na mesma sessão.

Os resultados de desempenho correspondem a `10e56ce`, antes das correções finais de interpretação do ZIP, identidade CPF/evento e política dos envelopes. Essas correções foram verificadas por testes; os benchmarks não foram repetidos depois delas.

| Itens | Anterior, µs/nota | Completo, µs/nota | Variação de tempo | Alocação anterior | Alocação completa |
| ---: | ---: | ---: | ---: | ---: | ---: |
| 1 | 22,24 | 21,17 | −4,8% | 128,90 KiB | 118,99 KiB |
| 50 | 181,31 | 156,87 | −13,5% | 462,18 KiB | 297,97 KiB |
| 500 | 1.665,85 | 1.373,09 | −17,6% | 3.522,46 KiB | 1.941,04 KiB |

Os valores são do formato canônico. SERPRO no parser completo: 20,60 / 152,96 / 1.350,16 µs. A diferença de 1 item é pequena frente à variância; não deve ser tratada como ganho garantido. As tabelas completas com desvio, intervalo de erro e GC estão em [antes](nfe-parser-before.md) e [depois](nfe-parser-after.md).

As primeiras versões completas regrediram em CPU. O profiling motivou três mudanças: delimitação por profundidade no mesmo leitor, evitando `XmlSubtreeReader`; leitura de escalares em `ValueTask` sem as alocações intermediárias do leitor de conteúdo da BCL; e um ponto de espera compartilhado para os escalares comuns do produto. As proteções de bytes/profundidade/cancelamento continuam ativas. Uma tentativa adicional de contornar a delegação do wrapper foi descartada por não mostrar benefício. Houve também experimento com afinidade de CPU única, com forte alteração do comportamento de GC; seus números não são misturados à tabela acima.

O benchmark sintético não representa todas as combinações dos 922 campos. A expansão de grupos presentes aumenta naturalmente o trabalho e o tamanho do grafo. Não há validação XSD, assinatura ou banco dentro da medição.

## Importação de arquivos e ZIP

Corpus preparado fora da medição: 100 XMLs distintos em diretório ou 100 entradas em um ZIP. `OperationsPerInvoke=100`; tempos abaixo são por documento. Cada ZIP é sequencial, portanto aumentar `Parallelism` não paraleliza suas entradas. Vários arquivos ZIP podem ser processados simultaneamente pela API de arquivos.

| Itens | Diretório P=1, µs | Diretório P=4, µs | ZIP, µs (P=1) | Diretório P=4, notas/s |
| ---: | ---: | ---: | ---: | ---: |
| 1 | 208,34 | 63,50 | 46,60 | 15.748 |
| 50 | 428,06 | 130,52 | 294,98 | 7.662 |
| 500 | 1.668,37 | 615,59 | 2.731,85 | 1.624 |

[Resultados completos](nfe-import-benchmark.md), incluindo P=4 para ZIP. São execuções curtas com arquivos em cache; os valores não estimam latência de rede ou importação em banco. Os relatórios de alocação assíncrona do BDN não substituem a medição de retenção do processo abaixo.

## Carga, backpressure e retenção

Probe com capacidade 64 e um XML de 1.782 bytes/um item. A fonte repete **o mesmo caminho** de forma lazy, abrindo um novo `FileStream` e materializando novo grafo a cada documento. Isso isola retenção e filas; não simula o custo de descobrir 100 mil caminhos distintos. O benchmark de diretório acima usa arquivos distintos.

Consumidor lento: pausa de 10 ms a cada 100 resultados (`--delay-every 100`), suficiente para exercer contrapressão em rajadas. Não foi simulada uma transação real de banco. Heap/working set são amostrados a cada 25 ms, logo picos muito curtos podem escapar. Heap retido é medido após coleta completa; throughput exclui a coleta final.

| Documentos | P | Consumidor | Notas/s | Heap após GC, KiB | Pico heap, MiB | Pico working set, MiB |
| ---: | ---: | --- | ---: | ---: | ---: | ---: |
| 10.000 | 1 | imediato | 3.884 | 361,8 | 7,49 | 51,62 |
| 10.000 | 1 | pausas | 2.779 | 369,2 | 9,50 | 66,26 |
| 10.000 | 4 | imediato | 11.679 | 366,7 | 18,28 | 62,68 |
| 10.000 | 4 | pausas | 6.096 | 365,7 | 16,13 | 61,23 |
| 100.000 | 1 | imediato | 2.956 | 359,7 | 9,66 | 67,07 |
| 100.000 | 1 | pausas | 2.353 | 360,5 | 12,27 | 69,38 |
| 100.000 | 4 | imediato | 6.115 | 391,6 | 21,23 | 79,13 |
| 100.000 | 4 | pausas | 3.606 | 388,4 | 17,50 | 72,27 |

[JSONL bruto](nfe-heap.jsonl) inclui bytes alocados, contagens de GC e tempo. A retenção não cresceu proporcionalmente ao número de documentos. Alocação cumulativa e número de coletas crescem com o trabalho: bounded streaming não significa ausência de alocações. Throughput caiu nos ensaios mais longos; use o resultado de 100 mil ao comparar carga sustentada neste ambiente.

Para ZIP, a memória do diretório central cresce com entradas e é limitada por orçamento; não é constante. Spool não posicionável usa disco, limitado por `MaxZipSpoolBytes`. Os testes verificam limites exatos/excedidos, ZIP64, contagem adulterada, CRC incorreto e remoção de temporários após sucesso, falha, cancelamento e interrupção. Este probe não é um ensaio de ZIP gigante nem mede seu consumo de disco.

## Reprodução

```powershell
dotnet run -c Release --project benchmarks/TecnoFisc.Sped.Benchmarks -- --filter '*ParserNFeBenchmark*' --job Short --warmupCount 5 --iterationCount 5
dotnet run -c Release --project benchmarks/TecnoFisc.Sped.Benchmarks -- --filter '*ParserNFeImportBenchmark*' --job Short
dotnet run -c Release --project benchmarks/TecnoFisc.Sped.Benchmarks -- --probe nfe-import --count 100000 --parallelism 4 --buffer 64 --delay-ms 10 --delay-every 100
```

Execute o primeiro comando também em checkout isolado de `64c65f5` para reproduzir o baseline. Compare a mesma máquina/runtime/configuração e evite trabalho concorrente. Para dimensionar FiscTax, repita com seu corpus anonimizado, tamanho de notas, ZIPs e latência efetiva do destino; ajuste paralelismo e capacidade ao orçamento de memória disponível.
