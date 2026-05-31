# Stage 12 Sniffer TXT Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implementar o sniffer TXT do Stage 12 em `TecnoFisc.Sped.Txt.Engine`, identificando EFD Contribuicoes, EFD ICMS-IPI e ECD pela primeira linha `|0000|...|`, com retorno de metadados e abertura replay-safe de parser via factories.

**Architecture:** `Txt.Engine` ganha tipos puros de identificacao (`ProjetoSped`, `MetadadosArquivoSped`, `SnifferSped`) e continua sem referenciar projetos de leiaute. `SnifferSped.IdentificarAsync` le a primeira linha nao vazia, restaura a posicao quando o stream e seekable, e classifica por discriminadores do `0000`; `AbrirParserAsync` recebe um mapa de factories `ProjetoSped -> ILeitorSped`, evitando dependencia circular porque os parsers especificos ja dependem de `Txt.Engine`.

**Tech Stack:** C# / .NET 10, `Stream`, `EncodingSped.Latin1`, xUnit v3, FluentAssertions. Sem dependencias externas.

**Spec:** `ARCHITECTURE.md` §12; nota de pendencia em `sped/STAGE_18_REORG.md` ("Stage 12 - sniffer TXT ainda nao existe").

---

## File Structure

- Create: `src/TecnoFisc.Sped.Txt.Engine/Parser/ProjetoSped.cs` - enum dos projetos TXT reconhecidos.
- Create: `src/TecnoFisc.Sped.Txt.Engine/Parser/MetadadosArquivoSped.cs` - record imutavel com projeto, versao, encoding e primeira linha.
- Create: `src/TecnoFisc.Sped.Txt.Engine/Parser/SnifferSped.cs` - sniffer TXT e abertura de parser por factory.
- Test: `tests/TecnoFisc.Sped.Core.Tests/Parser/SnifferSpedTests.cs` - testes no projeto existente que ja referencia `Txt.Engine`.
- Modify: `ARCHITECTURE.md` - documentar que `AbrirParserAsync` no engine usa factories registradas/injetadas, nao referencias diretas aos pacotes de leiaute.

---

## Task 0: Branch de trabalho

**Files:** nenhum.

- [ ] **Step 1: Criar branch curta a partir de `dev`**

```powershell
git switch dev
git pull --ff-only
git switch -c feat/stage-12-sniffer-txt
```

- [ ] **Step 2: Confirmar branch**

Run: `git branch --show-current`
Expected: `feat/stage-12-sniffer-txt`

---

## Task 1: Tipos de metadados do sniffer

**Files:**
- Create: `src/TecnoFisc.Sped.Txt.Engine/Parser/ProjetoSped.cs`
- Create: `src/TecnoFisc.Sped.Txt.Engine/Parser/MetadadosArquivoSped.cs`
- Test: `tests/TecnoFisc.Sped.Core.Tests/Parser/SnifferSpedTests.cs`

- [ ] **Step 1: Escrever o teste que falha para os tipos publicos**

Create `tests/TecnoFisc.Sped.Core.Tests/Parser/SnifferSpedTests.cs`:

```csharp
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Core.Tests.Parser;

public sealed class SnifferSpedTests
{
    [Fact]
    public void MetadadosArquivoSped_ArmazenaValores()
    {
        var metadados = new MetadadosArquivoSped(
            ProjetoSped.EfdContribuicoes,
            6,
            EncodingSped.Latin1,
            "|0000|006|0|||01022025|28022025|EMPRESA|11222333000181|MG|3126901||00|2|",
            "006");

        metadados.Projeto.Should().Be(ProjetoSped.EfdContribuicoes);
        metadados.VersaoLeiaute.Should().Be(6);
        metadados.EncodingDetectado.Should().BeSameAs(EncodingSped.Latin1);
        metadados.CodigoVersaoDeclarado.Should().Be("006");
    }
}
```

- [ ] **Step 2: Rodar o teste e confirmar que falha**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~SnifferSpedTests"`
Expected: FAIL de compilacao - `ProjetoSped` e `MetadadosArquivoSped` nao existem.

- [ ] **Step 3: Criar `ProjetoSped`**

Create `src/TecnoFisc.Sped.Txt.Engine/Parser/ProjetoSped.cs`:

```csharp
namespace TecnoFisc.Sped.Txt.Engine.Parser;

/// <summary>Projeto SPED textual identificado a partir do inicio do arquivo.</summary>
public enum ProjetoSped
{
    /// <summary>Nao foi possivel identificar o projeto.</summary>
    Desconhecido = 0,

    /// <summary>EFD Contribuicoes (PIS/COFINS).</summary>
    EfdContribuicoes,

    /// <summary>EFD ICMS-IPI.</summary>
    EfdIcmsIpi,

    /// <summary>ECD - Escrituracao Contabil Digital.</summary>
    Ecd,

    /// <summary>ECF - Escrituracao Contabil Fiscal; reservado para Stage 17.</summary>
    Ecf,
}
```

- [ ] **Step 4: Criar `MetadadosArquivoSped`**

Create `src/TecnoFisc.Sped.Txt.Engine/Parser/MetadadosArquivoSped.cs`:

```csharp
using System.Text;

namespace TecnoFisc.Sped.Txt.Engine.Parser;

/// <summary>
/// Resultado do sniffer TXT: identifica o projeto e a versao do leiaute declarada no inicio
/// do arquivo sem materializar registros.
/// </summary>
public sealed record MetadadosArquivoSped(
    ProjetoSped Projeto,
    int VersaoLeiaute,
    Encoding EncodingDetectado,
    string PrimeiraLinha,
    string? CodigoVersaoDeclarado);
```

- [ ] **Step 5: Rodar o teste e confirmar que passa**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~MetadadosArquivoSped_ArmazenaValores"`
Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add src/TecnoFisc.Sped.Txt.Engine/Parser/ProjetoSped.cs src/TecnoFisc.Sped.Txt.Engine/Parser/MetadadosArquivoSped.cs tests/TecnoFisc.Sped.Core.Tests/Parser/SnifferSpedTests.cs
git commit -m "feat(txt): metadados do sniffer SPED textual"
```

---

## Task 2: `SnifferSped.IdentificarAsync`

Implementa o sniffer puro. A classificacao inicial suportada:

- EFD Contribuicoes: `|0000|001..006|...|` com 15 pipes na linha do `0000`.
- EFD ICMS-IPI: `|0000|015..020|...|` com 16 pipes na linha do `0000`.
- ECD: `|0000|LECD|...|`; versao assumida `9` enquanto Stage 11 esta standby.
- Qualquer outro formato, linha vazia, EOF ou malformed: `ProjetoSped.Desconhecido`, `VersaoLeiaute = 0`.

**Files:**
- Modify: `tests/TecnoFisc.Sped.Core.Tests/Parser/SnifferSpedTests.cs`
- Create: `src/TecnoFisc.Sped.Txt.Engine/Parser/SnifferSped.cs`

- [ ] **Step 1: Escrever os testes que falham**

Append to `tests/TecnoFisc.Sped.Core.Tests/Parser/SnifferSpedTests.cs`:

```csharp
    [Fact]
    public async Task IdentificarAsync_EfdContribuicoesV006_RetornaMetadados()
    {
        await using var stream = Sped("|0000|006|0|||01022025|28022025|EMPRESA|11222333000181|MG|3126901||00|2|\r\n|0001|0|\r\n");

        var metadados = await SnifferSped.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.EfdContribuicoes);
        metadados.VersaoLeiaute.Should().Be(6);
        metadados.CodigoVersaoDeclarado.Should().Be("006");
        stream.Position.Should().Be(0, "o sniffer deve restaurar stream seekable para replay");
    }

    [Fact]
    public async Task IdentificarAsync_EfdIcmsIpiV015_RetornaMetadados()
    {
        await using var stream = Sped("|0000|015|1|01012021|31012021|EMPRESA|11222333000181||MG|123456789|3139409|||B|1|\n");

        var metadados = await SnifferSped.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.EfdIcmsIpi);
        metadados.VersaoLeiaute.Should().Be(15);
        metadados.CodigoVersaoDeclarado.Should().Be("015");
        stream.Position.Should().Be(0);
    }

    [Fact]
    public async Task IdentificarAsync_EcdLecd_RetornaLeiaute9()
    {
        await using var stream = Sped("|0000|LECD|01012023|31122023|EMPRESA|11222333000181|ES|\r\n|0001|0|\r\n");

        var metadados = await SnifferSped.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.Ecd);
        metadados.VersaoLeiaute.Should().Be(9);
        metadados.CodigoVersaoDeclarado.Should().Be("LECD");
        stream.Position.Should().Be(0);
    }

    [Fact]
    public async Task IdentificarAsync_IgnoraLinhasVaziasAntesDo0000()
    {
        await using var stream = Sped("\r\n\n|0000|LECD|01012023|31122023|EMPRESA|11222333000181|ES|\r\n");

        var metadados = await SnifferSped.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.Ecd);
        metadados.PrimeiraLinha.Should().StartWith("|0000|LECD|");
        stream.Position.Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("|9999|1|")]
    [InlineData("texto livre")]
    [InlineData("|0000|999|0|")]
    public async Task IdentificarAsync_EntradaDesconhecida_RetornaDesconhecido(string conteudo)
    {
        await using var stream = Sped(conteudo);

        var metadados = await SnifferSped.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.Desconhecido);
        metadados.VersaoLeiaute.Should().Be(0);
        stream.Position.Should().Be(0);
    }

    private static MemoryStream Sped(string conteudo)
        => new(EncodingSped.Latin1.GetBytes(conteudo), writable: false);
```

- [ ] **Step 2: Rodar e confirmar a falha**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~SnifferSpedTests"`
Expected: FAIL de compilacao - `SnifferSped` nao existe.

- [ ] **Step 3: Implementar `SnifferSped.IdentificarAsync`**

Create `src/TecnoFisc.Sped.Txt.Engine/Parser/SnifferSped.cs`:

```csharp
using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Txt.Engine.Parser;

/// <summary>
/// Sniffer do mundo SPED-TXT. Identifica o projeto a partir da primeira linha nao vazia
/// <c>|0000|...|</c>, sem materializar registros.
/// </summary>
public static class SnifferSped
{
    public static async ValueTask<MetadadosArquivoSped> IdentificarAsync(
        Stream entrada,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);

        long? posicaoInicial = entrada.CanSeek ? entrada.Position : null;
        try
        {
            string? primeiraLinha = await ReadPrimeiraLinhaNaoVaziaAsync(entrada, cancelamento)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(primeiraLinha))
                return Desconhecido(string.Empty, null);

            return Classificar(primeiraLinha);
        }
        finally
        {
            if (posicaoInicial is long posicao)
                entrada.Position = posicao;
        }
    }

    public static async ValueTask<ILeitorSped> AbrirParserAsync(
        Stream entrada,
        IReadOnlyDictionary<ProjetoSped, Func<ILeitorSped>> fabricas,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        ArgumentNullException.ThrowIfNull(fabricas);

        if (!entrada.CanSeek)
            throw new NotSupportedException("AbrirParserAsync requer stream seekable para reposicionar antes da leitura.");

        long origem = entrada.Position;
        var metadados = await IdentificarAsync(entrada, cancelamento).ConfigureAwait(false);
        entrada.Position = origem;

        if (!fabricas.TryGetValue(metadados.Projeto, out var fabrica))
            throw new NotSupportedException($"Nenhum parser registrado para o projeto SPED '{metadados.Projeto}'.");

        return fabrica();
    }

    private static MetadadosArquivoSped Classificar(string linha)
    {
        if (!linha.StartsWith("|0000|", StringComparison.Ordinal))
            return Desconhecido(linha, null);

        var campos = linha.Split('|');
        if (campos.Length < 4 || campos[1] != "0000")
            return Desconhecido(linha, null);

        string discriminador = campos[2];
        if (discriminador == "LECD")
            return new MetadadosArquivoSped(ProjetoSped.Ecd, 9, EncodingSped.Latin1, linha, discriminador);

        if (!int.TryParse(discriminador, out int versao))
            return Desconhecido(linha, discriminador);

        int pipes = linha.Count(c => c == '|');
        return discriminador switch
        {
            "001" or "002" or "003" or "004" or "005" or "006" when pipes == 15
                => new MetadadosArquivoSped(ProjetoSped.EfdContribuicoes, versao, EncodingSped.Latin1, linha, discriminador),

            "015" or "016" or "017" or "018" or "019" or "020" when pipes == 16
                => new MetadadosArquivoSped(ProjetoSped.EfdIcmsIpi, versao, EncodingSped.Latin1, linha, discriminador),

            _ => Desconhecido(linha, discriminador),
        };
    }

    private static MetadadosArquivoSped Desconhecido(string primeiraLinha, string? codigoVersaoDeclarado)
        => new(ProjetoSped.Desconhecido, 0, EncodingSped.Latin1, primeiraLinha, codigoVersaoDeclarado);

    private static async ValueTask<string?> ReadPrimeiraLinhaNaoVaziaAsync(
        Stream entrada,
        CancellationToken cancelamento)
    {
        using var linha = new MemoryStream();
        var buffer = new byte[1];

        while (await entrada.ReadAsync(buffer, cancelamento).ConfigureAwait(false) == 1)
        {
            byte b = buffer[0];
            if (b == EncodingSped.LfAscii)
            {
                string texto = DecodificarLinha(linha);
                if (texto.Length > 0)
                    return texto;

                linha.SetLength(0);
                continue;
            }

            linha.WriteByte(b);
        }

        if (linha.Length == 0)
            return null;

        string ultima = DecodificarLinha(linha);
        return ultima.Length == 0 ? null : ultima;
    }

    private static string DecodificarLinha(MemoryStream linha)
    {
        var bytes = linha.ToArray();
        int length = bytes.Length;
        if (length > 0 && bytes[length - 1] == EncodingSped.CrAscii)
            length--;

        return length == 0 ? string.Empty : EncodingSped.Latin1.GetString(bytes, 0, length);
    }
}
```

- [ ] **Step 4: Rodar e confirmar que passa**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~SnifferSpedTests"`
Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add src/TecnoFisc.Sped.Txt.Engine/Parser/SnifferSped.cs tests/TecnoFisc.Sped.Core.Tests/Parser/SnifferSpedTests.cs
git commit -m "feat(txt): sniffer SPED textual por Registro 0000"
```

---

## Task 3: `AbrirParserAsync` com factories

Valida a parte replay-safe sem acoplar `Txt.Engine` aos pacotes de leiaute. O teste usa parsers fake para provar que o sniffer seleciona a factory correta e restaura o stream.

**Files:**
- Modify: `tests/TecnoFisc.Sped.Core.Tests/Parser/SnifferSpedTests.cs`

- [ ] **Step 1: Escrever os testes que falham/validam a API**

Append to `tests/TecnoFisc.Sped.Core.Tests/Parser/SnifferSpedTests.cs`:

```csharp
    [Fact]
    public async Task AbrirParserAsync_UsaFactoryDoProjetoIdentificado_EReposicionaStream()
    {
        await using var stream = Sped("|0000|LECD|01012023|31122023|EMPRESA|11222333000181|ES|\r\n");
        var parserEcd = new LeitorFake();
        var fabricas = new Dictionary<ProjetoSped, Func<ILeitorSped>>
        {
            [ProjetoSped.Ecd] = () => parserEcd,
        };

        var parser = await SnifferSped.AbrirParserAsync(stream, fabricas, TestContext.Current.CancellationToken);

        parser.Should().BeSameAs(parserEcd);
        stream.Position.Should().Be(0);
    }

    [Fact]
    public async Task AbrirParserAsync_SemFactoryDoProjeto_LancaNotSupportedException()
    {
        await using var stream = Sped("|0000|LECD|01012023|31122023|EMPRESA|11222333000181|ES|\r\n");
        var fabricas = new Dictionary<ProjetoSped, Func<ILeitorSped>>();

        Func<Task> act = async () => _ = await SnifferSped.AbrirParserAsync(
            stream,
            fabricas,
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*Ecd*");
        stream.Position.Should().Be(0);
    }

    private sealed class LeitorFake : ILeitorSped
    {
        public async IAsyncEnumerable<RegistroSped> ReadStreamingAsync(
            Stream entrada,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancelamento = default)
        {
            await Task.CompletedTask;
            yield break;
        }
    }
```

Add these `using` lines at the top of the same file:

```csharp
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
```

- [ ] **Step 2: Rodar e confirmar que passa**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~AbrirParserAsync"`
Expected: PASS.

- [ ] **Step 3: Commit**

```powershell
git add tests/TecnoFisc.Sped.Core.Tests/Parser/SnifferSpedTests.cs
git commit -m "test(txt): cobertura de abertura replay-safe por factory"
```

---

## Task 4: Documentar a decisao de factories no Stage 12

**Files:**
- Modify: `ARCHITECTURE.md`

- [ ] **Step 1: Atualizar a descricao de `AbrirParserAsync` em §12**

In `ARCHITECTURE.md`, under `Stage 12 — Identificadores dinamicos de documento`, replace the current bullet:

```markdown
- `SnifferSped.AbrirParserAsync(Stream)` devolve o `ILeitorSped` do leiaute identificado, stream reposicionado na origem (replay-safe). Delega para `ParserEfdContribuicoes`, `ParserEfdIcmsIpi`, `ParserEcd`, `ParserEcf`.
```

with:

```markdown
- `SnifferSped.AbrirParserAsync(Stream, IReadOnlyDictionary<ProjetoSped, Func<ILeitorSped>>)` devolve o `ILeitorSped` do leiaute identificado, stream reposicionado na origem (replay-safe). O `Txt.Engine` nao referencia projetos de leiaute para nao inverter a direcao das dependencias; quem monta o ponto de entrada registra factories para `ParserEfdContribuicoes`, `ParserEfdIcmsIpi`, `ParserEcd` e, no futuro, `ParserEcf`.
```

- [ ] **Step 2: Rodar grep de sanidade**

Run: `rg -n "SnifferSped\\.AbrirParserAsync" ARCHITECTURE.md docs sped`
Expected: a assinatura documentada com o mapa de factories aparece em `ARCHITECTURE.md`; nenhuma instrucao restante exige referencia direta de `Txt.Engine` aos pacotes de leiaute.

- [ ] **Step 3: Commit**

```powershell
git add ARCHITECTURE.md
git commit -m "docs: registrar factories para abertura de parser no sniffer TXT"
```

---

## Task 5: Verificacao final e tracking

**Files:**
- Optional Modify: `sped/STAGE_18_REORG.md`

- [ ] **Step 1: Rodar build e testes relevantes**

Run: `dotnet build TecnoFisc.Sped.slnx`
Expected: 0 errors, 0 warnings.

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~SnifferSpedTests"`
Expected: todos os testes de `SnifferSpedTests` passam.

Run: `dotnet test TecnoFisc.Sped.slnx`
Expected: suite completa passa.

- [ ] **Step 2: Marcar nota de reorganizacao como resolvida**

If `sped/STAGE_18_REORG.md` still contains this pending item:

```markdown
- [ ] Stage 12 — sniffer TXT (`IdentificadorArquivoSped`/`SnifferSped`) ainda nao existe (so o XML foi feito na Stage 14).
```

replace it with:

```markdown
- [x] Stage 12 — sniffer TXT (`SnifferSped`) implementado em `TecnoFisc.Sped.Txt.Engine`; XML ja existia em `TecnoFisc.Sped.Xml.Engine`.
```

- [ ] **Step 3: Commit do tracking, se houve alteracao**

```powershell
git add sped/STAGE_18_REORG.md
git commit -m "docs: marcar sniffer TXT do Stage 12 como implementado"
```

Skip this commit if the item is absent or already checked.

---

## Self-Review

- Spec coverage: cobre `IdentificarAsync`, metadados com projeto/versao/encoding, EFD Contribuicoes, EFD ICMS-IPI, ECD, entrada desconhecida, stream replay-safe e abertura de parser. ECF fica reservado porque Stage 17 ainda nao existe.
- Dependency check: `Txt.Engine` nao referencia projetos especificos, evitando ciclo (`Efd*`/`Ecd` -> `Txt.Engine` ja existe).
- Placeholder scan: nenhum passo depende de conteudo a preencher depois; toda edicao de codigo tem arquivo e conteudo.
- Type consistency: `ProjetoSped`, `MetadadosArquivoSped`, `SnifferSped`, `IdentificarAsync` e `AbrirParserAsync` usam os mesmos nomes em testes, implementacao e docs.
