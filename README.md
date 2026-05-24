# TecnoFisc.Sped

Família de bibliotecas .NET para leitura, geração e manipulação tipada de arquivos
publicados pelos projetos do **SPED — Sistema Público de Escrituração Digital**
(Receita Federal do Brasil).

> Status atual: **0.4.0** publicado. Cobre EFD Contribuições V006 (leitura + geração,
> round-trip validado) e EFD ICMS-IPI baseline V015 + incrementos V016 → V020 (leiaute
> vigente em 2026, **modo read-only** — parser e modelo tipado, sem geração). Os números
> `006` (EFD Contribuições) e `015`–`020` (EFD ICMS-IPI) são o `COD_VER` do registro `0000`
> de cada leiaute (não devem ser confundidos com a versão do Guia Prático). Próximos
> passos rastreados no `ARCHITECTURE.md` (ECD, ECF e pacotes XML — todos planejados como
> read-only). Veja o `CHANGELOG.md` para detalhes.

## Visão geral

A biblioteca expõe registros fortemente tipados para cada projeto SPED, com leitura
e escrita simétricas (round-trip preservado), parser baseado em
`System.IO.Pipelines` para arquivos de múltiplos gigabytes e zero dependências
externas em tempo de execução.

Cada projeto SPED é distribuído como um pacote NuGet independente. Esta organização
mantém os layouts isolados — quando a Receita publica um novo layout, somente o
pacote afetado é versionado.

| Projeto SPED | Pacote NuGet | Status |
| --- | --- | --- |
| EFD Contribuições | `TecnoFisc.Sped.EfdContribuicoes` | **0.4.0** — leiaute V006 completo (leitura + geração) |
| EFD ICMS-IPI | `TecnoFisc.Sped.EfdIcmsIpi` | **0.4.0** — baseline V015 + incrementos V016 → V020 (vigente), **read-only** |
| ECD | `TecnoFisc.Sped.Ecd` | planejado (read-only) — baseline 2021 + incrementos até o leiaute vigente |
| ECF | `TecnoFisc.Sped.Ecf` | planejado (read-only) |
| NF-e | `TecnoFisc.Sped.NFe` | planejado (XML, read-only) |
| NFC-e | `TecnoFisc.Sped.NFCe` | planejado (XML, read-only) |
| CT-e | `TecnoFisc.Sped.CTe` | planejado (XML, read-only) |
| Metapacote agregador | `TecnoFisc.Sped` | planejado — referencia todos os leiautes acima em uma única dependência |

> **Modo de operação.** O único pacote com geração de arquivo confirmada é
> `TecnoFisc.Sped.EfdContribuicoes` (leitura + escrita, round-trip simétrico). Todos
> os demais — EFD ICMS-IPI, ECD, ECF, NF-e, NFC-e, CT-e — são planejados como
> **read-only** (parser + modelo tipado). Promoção para read+write em qualquer um
> deles depende de confirmação externa e entra como stage dedicada (`ARCHITECTURE.md` §2.5).

`TecnoFisc.Sped.Core` é a infraestrutura compartilhada (value objects fiscais, parser/gerador
genérico, abstrações de catálogo, identificador dinâmico de arquivos SPED) consumida por
todos os pacotes de leiaute. `TecnoFisc.Sped.Core.SourceGenerators` é o source generator que
produz, em tempo de compilação, o catálogo estático de registros — referenciado como
Analyzer pelos projetos de leiaute, não embarca no runtime do consumidor.

**Escopo definitivo.** A biblioteca cobre exatamente os sete leiautes listados na tabela
acima mais o metapacote agregador. Outros projetos SPED (eSocial, EFD-Reinf, NFS-e, MDF-e,
e-Financeira, DeRE, Central de Balanços) ficam **fora do escopo** e não serão
implementados — ver `ARCHITECTURE.md` §3 para a tabela autoritativa.

## Quickstart

### Instalação

```powershell
dotnet add package TecnoFisc.Sped.EfdContribuicoes
```

### Leitura buffered (modelo completo em memória)

```csharp
using TecnoFisc.Sped.EfdContribuicoes;
using TecnoFisc.Sped.EfdContribuicoes.Parser;

var parser = new ParserEfdContribuicoes();
await using var entrada = File.OpenRead("PISCOFINS-202401.txt");

ArquivoEfdContribuicoes arquivo = await parser.ReadAsync(entrada);

foreach (var registro in arquivo.Bloco0.EnumerarRegistros())
    Console.WriteLine(registro.Codigo);
```

### Leitura streaming (memory-bounded)

```csharp
using TecnoFisc.Sped.EfdContribuicoes.Parser;

var parser = new ParserEfdContribuicoes();
await using var entrada = File.OpenRead("arquivo-grande.txt");

await foreach (var registro in parser.ReadStreamingAsync(entrada))
{
    // Um registro por vez. Memória usada não cresce com o tamanho do arquivo.
}
```

### Persistir em banco com `OfType<T>()` + `Batch(n)`

`TecnoFisc.Sped.Core.Streaming` traz dois helpers que removem o boilerplate comum
de ingestão SPED → banco. `OfType<T>` filtra o stream pelo tipo concreto sem cast
manual; `Batch(n)` agrupa em lotes para bulk-insert.

```csharp
using TecnoFisc.Sped.Core.Streaming;
using TecnoFisc.Sped.EfdContribuicoes.Parser;
using TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

var parser = new ParserEfdContribuicoes();
await using var entrada = File.OpenRead("PISCOFINS-202401.txt");

await foreach (var lote in parser.ReadStreamingAsync(entrada)
                                 .OfType<RegistroC100>()
                                 .Batch(1000))
{
    // lote é IReadOnlyList<RegistroC100> com até 1.000 registros tipados.
    // Use com Dapper, EF Core AddRangeAsync, SqlBulkCopy, etc.
    await conexao.BulkInsertAsync(lote);
}
```

Memória continua bounded — `OfType` e `Batch` não bufferizam o arquivo inteiro;
apenas o lote corrente fica em memória. Pattern matching do `OfType` é resolvido
em compile-time (zero reflection, zero boxing).

### IDs surrogate para FK com `WithContext()`

Para modelos relacionais é comum precisar de PK/FK consistentes ao inserir o
registro pai antes do filho. `WithContext()` enriquece o stream com um
`ContextoPersistencia` contendo o ID surrogate do registro atual e o ID do
pai já emitido.

```csharp
using TecnoFisc.Sped.Core.Streaming;
using TecnoFisc.Sped.EfdContribuicoes.Parser;
using TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

await foreach (var (registro, ctx) in parser.ReadStreamingAsync(stream).WithContext())
{
    switch (registro)
    {
        case RegistroC100 c100:
            await conexao.ExecuteAsync(
                "INSERT INTO docs (id, num_doc, vl_doc) VALUES (@id, @n, @v)",
                new { id = ctx.IdRegistroAtual, n = c100.NumDoc, v = c100.VlDoc });
            break;
        case RegistroC170 c170:
            await conexao.ExecuteAsync(
                "INSERT INTO itens (id, doc_id, num_item) VALUES (@id, @doc, @n)",
                new { id = ctx.IdRegistroAtual, doc = ctx.IdPai, n = c170.NumItem });
            break;
    }
}
```

`ctx.IdPai` é `null` para registros raiz (`0000`, `9990`, `9999`). Para retomar
import multi-arquivo sem colidir com IDs já persistidos, use o overload
`WithContext(startAt: <ultimo-id-do-arquivo-anterior + 1>)`.

### Geração

```csharp
using TecnoFisc.Sped.EfdContribuicoes;
using TecnoFisc.Sped.EfdContribuicoes.Gerador;

var gerador = new GeradorEfdContribuicoes();
await using var saida = File.Create("saida.txt");

await gerador.WriteAsync(saida, arquivo);
```

O gerador injeta automaticamente os totalizadores `X990` (encerramento por bloco)
e `9999` (contagem global) — basta entregar a árvore de registros.

## Princípios

- **Auto-contido.** Sem banco de dados, sem arquivos de configuração externos,
  sem chamadas de rede. Streams entram, streams saem.
- **Independência de formato.** Projetos específicos nunca dependem uns dos outros —
  registros que parecem iguais (ex.: `RegistroC100` na EFD Contribuições e na
  ICMS-IPI) são classes distintas, propositalmente duplicadas.
- **Performance em primeiro lugar.** `PipeReader`, `ReadOnlySpan<byte>`,
  `Utf8Parser.TryParse` e catálogos gerados em tempo de compilação. Sem reflexão
  no caminho quente.
- **Tipagem forte de ponta a ponta.** Consumidores nunca lidam com `string` ou
  `string[]` — recebem `Cnpj`, `Cfop`, `DateOnly`, `decimal`, enums.
- **Round-trip simétrico onde há geração.** Nos pacotes read+write (hoje apenas
  `EfdContribuicoes`), ler → gerar → ler precisa devolver o mesmo arquivo (modulo
  normalizações deliberadas). Invariante coberta por testes. Nos pacotes read-only
  (EFD ICMS-IPI; ECD/ECF/NFe/NFCe/CTe planejados), a invariante é apenas leitura
  estável: a mesma entrada sempre produz o mesmo modelo tipado.

## Arquivos assinados pelo PVA

Arquivos emitidos pelo PVA da Receita Federal trazem um bloco de assinatura
digital PKCS#7 anexado após o registro `|9999|`. O parser encerra o consumo
no `|9999|` e descarta silenciosamente todo o conteúdo posterior — não é
necessário pré-processar o arquivo para remover a assinatura. A geração não
re-anexa nenhuma assinatura: a saída contém apenas a porção textual de
registros. Quem precisar reassinar deve fazê-lo fora da biblioteca, com um
provedor PKCS#7/CMS dedicado.

## Requisitos

- .NET SDK **10.0** (preview) ou superior
- Windows, Linux ou macOS

## Build local

```powershell
dotnet build  TecnoFisc.Sped.slnx
dotnet test   TecnoFisc.Sped.slnx
dotnet pack   TecnoFisc.Sped.slnx -c Release
```

Filtrar testes:

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~Cnpj"
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~Cnpj.ValidaDigito"
```

Rodar benchmarks:

```powershell
dotnet run -c Release --project benchmarks/TecnoFisc.Sped.Benchmarks -- --filter "*StreamingVsBufferedBenchmark*"
dotnet run -c Release --project benchmarks/TecnoFisc.Sped.Benchmarks -- --filter "*ParserCatalogoBenchmark*"
dotnet run -c Release --project benchmarks/TecnoFisc.Sped.Benchmarks -- --probe peak
```

## Estrutura do repositório

```text
TecnoFisc.Sped/
├── src/
│   ├── TecnoFisc.Sped.Core/                  # Value objects fiscais + infra compartilhada + sniffer
│   ├── TecnoFisc.Sped.Core.SourceGenerators/ # Source generator do catálogo (analyzer)
│   ├── TecnoFisc.Sped.EfdContribuicoes/      # Leiaute EFD Contribuições V006
│   └── TecnoFisc.Sped.EfdIcmsIpi/            # Leiaute EFD ICMS-IPI baseline V015 + V016-V020 (read-only)
│   # Stages futuros (planejados): Ecd, Ecf, NFe, NFCe, CTe + metapacote TecnoFisc.Sped
├── tests/
│   ├── TecnoFisc.Sped.Core.Tests/
│   ├── TecnoFisc.Sped.EfdContribuicoes.Tests/
│   └── TecnoFisc.Sped.EfdIcmsIpi.Tests/
├── benchmarks/
│   └── TecnoFisc.Sped.Benchmarks/            # BenchmarkDotNet (.NET 10)
├── sped/
│   ├── STAGE_4_REGISTROS.md                  # Decomposição do Stage 4 (EFD Contribuições)
│   ├── STAGE_8_EFD_ICMS_IPI_V015.md          # Decomposição do Stage 8 (EFD ICMS-IPI V015)
│   └── guides/                               # PDFs oficiais Receita Federal (gitignored)
├── ARCHITECTURE.md                           # Documento mestre (inglês, para LLMs)
├── CHANGELOG.md                              # Notas de release por pacote
└── CLAUDE.md                                 # Instruções para Claude Code
```

## Convenções

- **Português** para **substantivos** do domínio SPED: classes de registro
  (`Registro0000`, `RegistroC100`), value objects fiscais (`Cnpj`, `Cfop`, `Ncm`),
  enums fiscais (`IndicadorOperacao`, `ModeloDocumento`), campos (`IndOper`, `VlDoc`),
  tipos top-level (`ArquivoEfdContribuicoes`, `BlocoC`).
- **Inglês** para **verbos**, factories estáticos e predicados booleanos:
  `Cnpj.Create(...)`, `parser.ReadAsync(...)`, `parser.ReadStreamingAsync(...)`,
  `gerador.WriteAsync(...)`, `cfop.IsEntrada`, `inscricao.IsIsento`,
  `CodigosUf.IsValid(uf)`.
- **Inglês** também para infraestrutura técnica universal: `Parser`, `Generator`,
  `Reader`, `Writer`, `Builder`, tipos da BCL, palavras-chave de C#.
- Encoding dos `.txt` SPED: **Latin1 / Windows-1252**. UTF-8 apenas para os
  pacotes XML (família NF-e).
- Commits seguem [Conventional Commits](https://www.conventionalcommits.org/)
  (prefixo em inglês: `feat:`, `fix:`, `refactor:`...) com corpo em português.

Detalhes completos em `ARCHITECTURE.md` §1.3 e §13.

## Licença

[MIT](LICENSE) — © 2026 TecnoFisc Micro Sistemas.

## Contribuição

Repositório em fase inicial e mantido internamente. Issues e pull requests externos
ainda não são aceitos. Para colaboradores internos, leia primeiro
`ARCHITECTURE.md` (documento mestre) e `CLAUDE.md`.
