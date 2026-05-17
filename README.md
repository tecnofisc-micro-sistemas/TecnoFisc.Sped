# TecnoFisc.Sped

Família de bibliotecas .NET para leitura, geração e manipulação tipada de arquivos
publicados pelos projetos do **SPED — Sistema Público de Escrituração Digital**
(Receita Federal do Brasil).

> Status atual: **0.2.0** publicado. No ramo de desenvolvimento, EFD ICMS-IPI
> baseline V306 já possui os registros tipados e a API pública de parser/gerador;
> a validação round-trip com fixture real anonimizada ainda é etapa pendente antes
> da release 0.3.0. Veja o `CHANGELOG.md` para detalhes.

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
| EFD Contribuições | `TecnoFisc.Sped.EfdContribuicoes` | **0.2.0** — layout V006 completo |
| EFD ICMS-IPI | `TecnoFisc.Sped.EfdIcmsIpi` | **não publicado** — registros V306 completos; parser/gerador implementados; fixture real pendente |
| NF-e / NFC-e / CT-e / MDF-e | `TecnoFisc.Sped.NFe`, etc. | planejado |
| eSocial / EFD-Reinf / ECD / ECF | pacotes próprios | planejado |

`TecnoFisc.Sped.Core` é a infraestrutura compartilhada (value objects fiscais, parser/gerador
genérico, abstrações de catálogo) consumida por todos os pacotes de leiaute.
`TecnoFisc.Sped.Core.SourceGenerators` é o source generator que produz, em tempo de
compilação, o catálogo estático de registros — referenciado como Analyzer pelos
projetos de leiaute, não embarca no runtime do consumidor.

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

ArquivoEfdContribuicoes arquivo = await parser.LerAsync(entrada);

foreach (var registro in arquivo.Bloco0.EnumerarRegistros())
    Console.WriteLine(registro.Codigo);
```

### Leitura streaming (memory-bounded)

```csharp
using TecnoFisc.Sped.EfdContribuicoes.Parser;

var parser = new ParserEfdContribuicoes();
await using var entrada = File.OpenRead("arquivo-grande.txt");

await foreach (var registro in parser.LerStreamingAsync(entrada))
{
    // Um registro por vez. Memória usada não cresce com o tamanho do arquivo.
}
```

### Geração

```csharp
using TecnoFisc.Sped.EfdContribuicoes;
using TecnoFisc.Sped.EfdContribuicoes.Gerador;

var gerador = new GeradorEfdContribuicoes();
await using var saida = File.Create("saida.txt");

await gerador.EscreverAsync(saida, arquivo);
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
- **Round-trip simétrico.** Ler → gerar → ler precisa devolver o mesmo arquivo
  (modulo normalizações deliberadas). Invariante coberta por testes.

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
│   ├── TecnoFisc.Sped.Core/                  # Value objects fiscais + infra compartilhada
│   ├── TecnoFisc.Sped.Core.SourceGenerators/ # Source generator do catálogo (analyzer)
│   ├── TecnoFisc.Sped.EfdContribuicoes/      # Layout EFD Contribuições V006
│   └── TecnoFisc.Sped.EfdIcmsIpi/            # Layout EFD ICMS-IPI baseline V306
├── tests/
│   ├── TecnoFisc.Sped.Core.Tests/
│   ├── TecnoFisc.Sped.EfdContribuicoes.Tests/
│   └── TecnoFisc.Sped.EfdIcmsIpi.Tests/
├── benchmarks/
│   └── TecnoFisc.Sped.Benchmarks/            # BenchmarkDotNet (.NET 10)
├── sped/
│   ├── STAGE_4_REGISTROS.md                  # Decomposição do Stage 4 em sub-stages
│   └── guides/                               # PDFs oficiais Receita Federal (gitignored)
│       └── Guia_Pratico_EFD_Contribuicoes_*.pdf
├── ARCHITECTURE.md                           # Documento mestre (inglês, para LLMs)
├── CHANGELOG.md                              # Notas de release por pacote
└── CLAUDE.md                                 # Instruções para Claude Code
```

## Convenções

- **Português** para conceitos SPED: classes de registro (`Registro0000`,
  `RegistroC100`), value objects fiscais (`Cnpj`, `Cfop`, `Ncm`),
  campos (`IndOper`, `VlDoc`), métodos de domínio (`LerArquivo`, `EscreverArquivo`).
- **Inglês** para infraestrutura técnica universal: `Parser`, `Generator`,
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
