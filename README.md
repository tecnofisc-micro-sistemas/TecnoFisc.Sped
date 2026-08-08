# TecnoFisc.Sped

Família de bibliotecas .NET para leitura, geração e manipulação tipada de arquivos
publicados pelos projetos do **SPED — Sistema Público de Escrituração Digital**
(Receita Federal do Brasil).

> Status atual: **0.9.0** publicado. Cobre EFD Contribuições V006 (leitura + geração,
> round-trip validado), EFD ICMS-IPI baseline V015 + incrementos V016 → V020 (leiaute
> vigente em 2026, **read-only**) e **ECD** leiaute 9 (**read-only**). Na árvore de
> desenvolvimento ainda não publicada, a **ECF** entrega um modelo tipado único do
> leiaute 12 com leitura dos leiautes 8 a 12 — os registros e as colunas exclusivos dos
> leiautes 8 a 10 são **reconhecidos, não tipados** (o leitor não aborta, mas o conteúdo
> deles não é materializado) —, também em modo **read-only** (parser e modelo tipado, sem
> geração). Os números
> `006` (EFD Contribuições) e `015`–`020` (EFD ICMS-IPI) são o `COD_VER` do registro `0000`
> de cada leiaute (não devem ser confundidos com a versão do Guia Prático); a ECD informa a
> versão do leiaute em `I010.COD_VER_LC`, não no `0000`. A `0.7.x` é **breaking** em relação à
> `0.6.0`: reorganiza
> o antigo `Core` monolítico em `Core` universal + engines `Txt.Engine`/`Xml.Engine` (Stage 18),
> adiciona os guarda-chuvas `TecnoFisc.Sped.Txt` e `TecnoFisc.Sped` (Stage 13) e os sniffers
> de identificação de documento por mundo (Stage 12). Estreia ainda, em **preview**, o pacote
> XML `TecnoFisc.Sped.NFeNFCe` — cobertura atual limitada a **NF-e modelo 55 parcial** (NFC-e 65,
> eventos e os grupos transp/cobr/pag/protNFe ainda em desenvolvimento). Próximos passos
> rastreados no `ARCHITECTURE.md`; o CT-e permanece planejado como read-only. A `0.8.0`
> adiciona o sniffer fiscal TXT opt-in para extrair CNPJ e período do registro `0000`. A `0.9.0`
> adiciona **parsing tolerante opt-in** no leitor TXT (`ReadingOptions.LenientFieldParsing` e
> `LenientLayout`, mais `LeitorSpedTxt.ParseLinha`): campo falho ou registro desconhecido deixam
> de abortar a leitura quando o consumidor optar por isso. Na árvore de desenvolvimento ainda não
> publicada, a ECF introduz mais duas flags `bool?` em `ReadingOptions`:
> `RespeitarVigenciaDoLeiaute` (omite registro/campo anterior à vigência declarada no `0000`) e
> `ValidarDominioDeEnum` (rejeita código fora do domínio de um enum fechado em vez de aceitar por
> cast permissivo). `null` delega a decisão ao parser do leiaute — a ECF liga as duas, os leiautes
> já publicados mantêm o comportamento atual. Na mesma árvore, `LenientFieldParsing` e
> `LenientLayout` viram **catraca de mão única**: sob arquivo cujo leiaute está fora da faixa
> conhecida pelo módulo, o leitor força as duas para `true` e não há como pedir fail-fast.
> Veja o `CHANGELOG.md` para detalhes.

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
| EFD Contribuições | `TecnoFisc.Sped.EfdContribuicoes` | **0.9.0** — leiaute V006 completo (leitura + geração) |
| EFD ICMS-IPI | `TecnoFisc.Sped.EfdIcmsIpi` | **0.9.0** — baseline V015 + incrementos V016 → V020 (vigente), **read-only** |
| ECD | `TecnoFisc.Sped.Ecd` | **0.9.0** — baseline leiaute 9 completo (vigente), **read-only** |
| NF-e / NFC-e | `TecnoFisc.Sped.NFeNFCe` | **0.9.0 preview** — só NF-e modelo 55 parcial (XML, read-only); NFC-e 65 e eventos em desenvolvimento |
| CT-e | `TecnoFisc.Sped.CTe` | planejado (XML, read-only) |
| ECF | `TecnoFisc.Sped.Ecf` | **Não publicado** — modelo tipado do leiaute 12 com leitura dos leiautes 8 → 12 (registros e colunas exclusivos dos leiautes 8 a 10 são reconhecidos, não tipados), **read-only** |
| Guarda-chuva TXT | `TecnoFisc.Sped.Txt` | **Não publicado** — agrega EFD Contribuições, EFD ICMS-IPI, ECD e ECF |
| Guarda-chuva geral | `TecnoFisc.Sped` | **0.9.0** — agrega `TecnoFisc.Sped.Txt`; passará a agregar XML após CT-e |
| Guarda-chuva XML | `TecnoFisc.Sped.Xml` | planejado após CT-e — agregará NFe/NFC-e e CT-e |

> **Modo de operação.** O único pacote com geração de arquivo confirmada é
> `TecnoFisc.Sped.EfdContribuicoes` (leitura + escrita, round-trip simétrico). Todos
> os demais — EFD ICMS-IPI, ECD e ECF (implementados), NFeNFCe (preview, NF-e 55 parcial)
> e CT-e (planejado) — são **read-only** (parser + modelo tipado). Promoção para
> read+write em qualquer um deles depende de confirmação externa e entra como stage
> dedicada (`ARCHITECTURE.md` §2.5).

`TecnoFisc.Sped.Core` contém os primitivos fiscais universais. A maquinaria textual vive em
`TecnoFisc.Sped.Txt.Engine`; a maquinaria XML vive em `TecnoFisc.Sped.Xml.Engine`. Para
consumidores que querem todos os leiautes textuais em uma única dependência, use
`TecnoFisc.Sped.Txt`. O pacote `TecnoFisc.Sped` agrega os guarda-chuvas disponíveis; hoje ele
puxa o mundo TXT e será ampliado com `TecnoFisc.Sped.Xml` depois do CT-e.

**Escopo definitivo.** A biblioteca cobre os leiautes listados na tabela acima e os
guarda-chuvas explicitamente marcados como disponíveis/planejados. Outros projetos SPED (eSocial, EFD-Reinf, NFS-e, MDF-e,
e-Financeira, DeRE, Central de Balanços) ficam **fora do escopo** e não serão
implementados — ver `ARCHITECTURE.md` §3 para a tabela autoritativa.

## Quickstart

### Instalação

```powershell
dotnet add package TecnoFisc.Sped.EfdContribuicoes
```

Para ler ECF diretamente, instale o pacote específico (ou use o guarda-chuva
`TecnoFisc.Sped.Txt`):

```powershell
dotnet add package TecnoFisc.Sped.Ecf
```

```csharp
using TecnoFisc.Sped.Ecf.Parser;

var parser = new ParserEcf();
await using var entrada = File.OpenRead("escrituracao.ecf");

await foreach (var registro in parser.ReadStreamingAsync(entrada))
{
    // Leiautes 8, 9, 10, 11 e 12 usam o mesmo modelo tipado do leiaute 12.
}
```

> **Alcance da leitura dos leiautes 8 a 12.** Existe **um único modelo tipado**, o do
> leiaute 12. Os registros e as colunas que existiam nos leiautes 8 a 10 e saíram no 11
> são **reconhecidos, não tipados**: o catálogo os conhece (por isso a leitura de um
> arquivo histórico não aborta nem degrada a linha para `RegistroNaoReconhecido`), mas o
> conteúdo das colunas **não é materializado** nesta versão — os sete registros
> descontinuados (`X291`, `X300`, `X305`, `X310`, `X320`, `X325`, `X330`) chegam ao
> consumidor sem nenhuma propriedade preenchida — eles já declaram `[Descontinuado]`; o
> que falta é declarar as colunas com `[CampoSped]`. Modelar esses campos é evolução
> planejada e puramente aditiva: quem lê hoje continua lendo igual quando ela chegar. Um arquivo de leiaute **fora da faixa 8–12** (menor que
> 8 ou maior que 12) também é lido, em **modo tolerante**: o `0000` recebe um aviso não
> fatal em `ErrosDeFormato`, código de registro desconhecido vira `RegistroNaoReconhecido`
> em vez de abortar e falha de conversão de campo vira diagnóstico em vez de exceção.
> Dentro da faixa 8–12 o rigor permanece o mesmo de sempre.

O parser valida formato, tipos e hierarquia sintática. Obrigatoriedade tributária
condicional, regras `REGRA_*`, cruzamentos, reconciliações e cálculos de IRPJ/CSLL
continuam sob responsabilidade do consumidor e do PGE.

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

`TecnoFisc.Sped.Txt.Engine.Streaming` traz dois helpers que removem o boilerplate comum
de ingestão SPED → banco. `OfType<T>` filtra o stream pelo tipo concreto sem cast
manual; `Batch(n)` agrupa em lotes para bulk-insert.

```csharp
using TecnoFisc.Sped.Txt.Engine.Streaming;
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
using TecnoFisc.Sped.Txt.Engine.Streaming;
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

### Visitor dispatcher tipado (source-generated)

Para processar muitos tipos de registro sem escrever um `switch` de 200+ casos,
implemente a interface `IRegistroSpedVisitor` (emitida pelo source generator
no namespace `<Projeto>.Generated`). Cada overload `VisitAsync(TipoConcreto)`
tem implementação default vazia — sobrescreva só o que importa. O despacho via
`DispatchAsync()` é resolvido em compile-time (zero reflection).

```csharp
using TecnoFisc.Sped.EfdContribuicoes.Generated;

public sealed class GravadorBanco : IRegistroSpedVisitor
{
    public ValueTask VisitAsync(Registro0000 r, CancellationToken ct) { /* INSERT escrituracoes */ return default; }
    public ValueTask VisitAsync(RegistroC100 r, CancellationToken ct) { /* INSERT docs */ return default; }
    public ValueTask VisitAsync(RegistroC170 r, CancellationToken ct) { /* INSERT itens */ return default; }
    // Demais registros: default vazio, ignorados sem nada a fazer.
}

await parser.ReadStreamingAsync(stream).DispatchAsync(new GravadorBanco());
```

Registros de outros assemblies (caso o consumidor componha streams de
projetos diferentes) caem no `VisitUnknownAsync(RegistroSped)`.

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
  (EFD ICMS-IPI, ECD e ECF implementados; NFeNFCe em preview; CT-e planejado), a invariante é apenas leitura
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

## Publicação NuGet

A publicação é **contínua e automática**, orquestrada por
[semantic-release](https://semantic-release.gitbook.io/) no workflow `Release`. **Não há
bump manual de versão nem PR de release**: o merge em `main` (via o PR `dev → main`) dispara
o pipeline, que computa a próxima versão a partir dos **Conventional Commits** desde o último
release, injeta essa versão no empacotamento (`dotnet pack -p:Version=X.Y.Z`), publica os
`.nupkg` no nuget.org, cria a tag `vX.Y.Z` e gera a GitHub Release — tudo numa única execução,
sem intervenção humana.

A versão é **computada**, não editada: `Directory.Build.props` carrega só o placeholder
`0.0.0-dev` para builds locais. A regra de bump segue o SemVer a partir do prefixo do commit:
`fix:` → patch, `feat:` → minor, `feat!:`/`BREAKING CHANGE:` → major. Commits `chore:`/`docs:`
não geram release.

> O `CHANGELOG.md` permanece **curado à mão** para a narrativa por pacote, mas não dirige mais
> o release — as Release Notes do GitHub são geradas automaticamente a partir dos commits.

Para validar sem publicar (calcular a versão e as notas sem empacotar/publicar), execute
manualmente o workflow `Release` com `dry_run=true`.

## Estrutura do repositório

```text
TecnoFisc.Sped/
├── src/
│   ├── TecnoFisc.Sped/                       # Guarda-chuva geral (Txt agora; Xml após CT-e)
│   ├── TecnoFisc.Sped.Txt/                   # Guarda-chuva textual (EFD Contribuições, EFD ICMS-IPI, ECD, ECF)
│   ├── TecnoFisc.Sped.Core/                  # Value objects fiscais universais
│   ├── TecnoFisc.Sped.Txt.Engine/            # Motor .txt + catálogo + parser/gerador + sniffer TXT
│   ├── TecnoFisc.Sped.Txt.Engine.SourceGenerators/ # Source generator do catálogo TXT (analyzer)
│   ├── TecnoFisc.Sped.Xml.Engine/            # Motor XML + IDocumentoFiscalXml + sniffer XML
│   ├── TecnoFisc.Sped.EfdContribuicoes/      # Leiaute EFD Contribuições V006
│   ├── TecnoFisc.Sped.EfdIcmsIpi/            # Leiaute EFD ICMS-IPI baseline V015 + V016-V020 (read-only)
│   ├── TecnoFisc.Sped.Ecd/                   # Leiaute ECD 9 (Sped Contábil, read-only)
│   ├── TecnoFisc.Sped.Ecf/                   # ECF: modelo do leiaute 12, leitura 8–12 (read-only)
│   └── TecnoFisc.Sped.NFeNFCe/               # NF-e/NFC-e 4.00 (XML, read-only)
│   # Stages futuros (planejados): CTe, TecnoFisc.Sped.Xml
├── tests/
│   ├── TecnoFisc.Sped.Core.Tests/
│   ├── TecnoFisc.Sped.EfdContribuicoes.Tests/
│   ├── TecnoFisc.Sped.EfdIcmsIpi.Tests/
│   ├── TecnoFisc.Sped.Ecd.Tests/
│   └── TecnoFisc.Sped.NFeNFCe.Tests/
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
