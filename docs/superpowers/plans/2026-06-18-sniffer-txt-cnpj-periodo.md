# Sniffer TXT Fiscal Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Criar um novo sniffer TXT opt-in que retorna CNPJ e periodo fiscal, preservando o `SnifferSped` atual como identificador leve de tipo/versao.

**Architecture:** `SnifferSped` e `MetadadosArquivoSped` permanecem sem alteracao de API e comportamento. O novo `SnifferSpedFiscal` chama `SnifferSped.IdentificarAsync` para obter projeto/versao e reaproveita a `PrimeiraLinha` ja lida para extrair `Cnpj`, `DataInicial` e `DataFinal` conforme o leiaute identificado; o consumidor escolhe entre sniff leve (`SnifferSped`) e sniff fiscal (`SnifferSpedFiscal`).

**Tech Stack:** C# / .NET 10, `DateOnly`, `Cnpj`, `ParseadoresPrimitivos`, xUnit v3, FluentAssertions.

---

## File Structure

- Create: `src/TecnoFisc.Sped.Txt.Engine/Parser/MetadadosFiscaisArquivoSped.cs` - resultado do novo sniff fiscal, contendo a identificacao leve e os dados fiscais opcionais.
- Create: `src/TecnoFisc.Sped.Txt.Engine/Parser/SnifferSpedFiscal.cs` - novo sniffer opt-in que extrai CNPJ e periodo sem alterar o sniffer atual.
- Create: `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/SnifferSpedFiscalTests.cs` - cobertura do novo comportamento.
- Modify: `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/SnifferSpedTests.cs` - adicionar teste de guarda garantindo que o sniffer atual continua sem dados fiscais.
- Optional Modify: `CHANGELOG.md` ou `ARCHITECTURE.md` - registrar a nova API se houver secao de release/doc atual sendo mantida.

## Task 1: Guard rail do sniffer atual

**Files:**
- Modify: `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/SnifferSpedTests.cs`

- [ ] **Step 1: Adicionar teste que documenta a API leve existente**

Append to `SnifferSpedTests`:

```csharp
[Fact]
public async Task IdentificarAsync_MantemContratoLeveSemMetadadosFiscais()
{
    await using var stream = Sped("|0000|006|0|||01022025|28022025|EMPRESA|11222333000181|MG|3126901||00|2|\r\n");

    var metadados = await SnifferSped.IdentificarAsync(stream, TestContext.Current.CancellationToken);

    metadados.Projeto.Should().Be(ProjetoSped.EfdContribuicoes);
    metadados.VersaoLeiaute.Should().Be(6);
    metadados.GetType().GetProperty("Cnpj").Should().BeNull();
    metadados.GetType().GetProperty("DataInicial").Should().BeNull();
    metadados.GetType().GetProperty("DataFinal").Should().BeNull();
}
```

- [ ] **Step 2: Rodar teste de guarda**

Run:

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~IdentificarAsync_MantemContratoLeveSemMetadadosFiscais"
```

Expected: PASS, confirmando que a API atual continua leve antes da nova funcionalidade.

## Task 2: Tipos publicos do sniff fiscal

**Files:**
- Create: `src/TecnoFisc.Sped.Txt.Engine/Parser/MetadadosFiscaisArquivoSped.cs`
- Create: `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/SnifferSpedFiscalTests.cs`

- [ ] **Step 1: Escrever teste que falha para o novo resultado**

Create `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/SnifferSpedFiscalTests.cs`:

```csharp
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Parser;

public sealed class SnifferSpedFiscalTests
{
    [Fact]
    public void MetadadosFiscaisArquivoSped_ArmazenaIdentificacaoECamposFiscais()
    {
        var identificacao = new MetadadosArquivoSped(
            ProjetoSped.EfdContribuicoes,
            6,
            EncodingSped.Latin1,
            "|0000|006|0|||01022025|28022025|EMPRESA|11222333000181|MG|3126901||00|2|",
            "006");

        var metadados = new MetadadosFiscaisArquivoSped(
            identificacao,
            Cnpj.Create("11222333000181"),
            new DateOnly(2025, 2, 1),
            new DateOnly(2025, 2, 28));

        metadados.Identificacao.Should().BeSameAs(identificacao);
        metadados.Projeto.Should().Be(ProjetoSped.EfdContribuicoes);
        metadados.VersaoLeiaute.Should().Be(6);
        metadados.Cnpj.Should().Be(Cnpj.Create("11222333000181"));
        metadados.DataInicial.Should().Be(new DateOnly(2025, 2, 1));
        metadados.DataFinal.Should().Be(new DateOnly(2025, 2, 28));
    }
}
```

- [ ] **Step 2: Rodar e confirmar falha de compilacao**

Run:

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~MetadadosFiscaisArquivoSped_ArmazenaIdentificacaoECamposFiscais"
```

Expected: FAIL porque `MetadadosFiscaisArquivoSped` ainda nao existe.

- [ ] **Step 3: Criar o record fiscal**

Create `src/TecnoFisc.Sped.Txt.Engine/Parser/MetadadosFiscaisArquivoSped.cs`:

```csharp
using System.Text;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.Txt.Engine.Parser;

/// <summary>
/// Resultado do sniff fiscal TXT. Inclui a identificacao leve produzida por
/// <see cref="SnifferSped"/> e, quando disponiveis no Registro 0000, CNPJ e periodo.
/// </summary>
public sealed record MetadadosFiscaisArquivoSped(
    MetadadosArquivoSped Identificacao,
    Cnpj? Cnpj,
    DateOnly? DataInicial,
    DateOnly? DataFinal)
{
    public ProjetoSped Projeto => Identificacao.Projeto;
    public int VersaoLeiaute => Identificacao.VersaoLeiaute;
    public Encoding EncodingDetectado => Identificacao.EncodingDetectado;
    public string PrimeiraLinha => Identificacao.PrimeiraLinha;
    public string? CodigoVersaoDeclarado => Identificacao.CodigoVersaoDeclarado;
}
```

- [ ] **Step 4: Rodar e confirmar que passa**

Run:

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~MetadadosFiscaisArquivoSped_ArmazenaIdentificacaoECamposFiscais"
```

Expected: PASS.

## Task 3: Novo sniffer fiscal opt-in

**Files:**
- Create: `src/TecnoFisc.Sped.Txt.Engine/Parser/SnifferSpedFiscal.cs`
- Modify: `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/SnifferSpedFiscalTests.cs`

- [ ] **Step 1: Adicionar testes que falham para EFD Contribuicoes, EFD ICMS-IPI e ECD**

Append to `SnifferSpedFiscalTests`:

```csharp
[Fact]
public async Task IdentificarAsync_EfdContribuicoes_RetornaIdentificacaoCnpjEPeriodo()
{
    await using var stream = Sped("|0000|006|0|||01022025|28022025|EMPRESA|11222333000181|MG|3126901||00|2|\r\n|0001|0|\r\n");

    var metadados = await SnifferSpedFiscal.IdentificarAsync(stream, TestContext.Current.CancellationToken);

    metadados.Projeto.Should().Be(ProjetoSped.EfdContribuicoes);
    metadados.VersaoLeiaute.Should().Be(6);
    metadados.CodigoVersaoDeclarado.Should().Be("006");
    metadados.Cnpj.Should().Be(Cnpj.Create("11222333000181"));
    metadados.DataInicial.Should().Be(new DateOnly(2025, 2, 1));
    metadados.DataFinal.Should().Be(new DateOnly(2025, 2, 28));
    stream.Position.Should().Be(0);
}

[Fact]
public async Task IdentificarAsync_EfdIcmsIpi_RetornaIdentificacaoCnpjEPeriodo()
{
    await using var stream = Sped("|0000|015|1|01012021|31012021|EMPRESA|11222333000181||MG|123456789|3139409|||B|1|\n");

    var metadados = await SnifferSpedFiscal.IdentificarAsync(stream, TestContext.Current.CancellationToken);

    metadados.Projeto.Should().Be(ProjetoSped.EfdIcmsIpi);
    metadados.VersaoLeiaute.Should().Be(15);
    metadados.CodigoVersaoDeclarado.Should().Be("015");
    metadados.Cnpj.Should().Be(Cnpj.Create("11222333000181"));
    metadados.DataInicial.Should().Be(new DateOnly(2021, 1, 1));
    metadados.DataFinal.Should().Be(new DateOnly(2021, 1, 31));
    stream.Position.Should().Be(0);
}

[Fact]
public async Task IdentificarAsync_Ecd_RetornaIdentificacaoCnpjEPeriodo()
{
    await using var stream = Sped("|0000|LECD|01012023|31122023|EMPRESA|11222333000181|ES|\r\n|0001|0|\r\n");

    var metadados = await SnifferSpedFiscal.IdentificarAsync(stream, TestContext.Current.CancellationToken);

    metadados.Projeto.Should().Be(ProjetoSped.Ecd);
    metadados.VersaoLeiaute.Should().Be(9);
    metadados.CodigoVersaoDeclarado.Should().Be("LECD");
    metadados.Cnpj.Should().Be(Cnpj.Create("11222333000181"));
    metadados.DataInicial.Should().Be(new DateOnly(2023, 1, 1));
    metadados.DataFinal.Should().Be(new DateOnly(2023, 12, 31));
    stream.Position.Should().Be(0);
}

private static MemoryStream Sped(string conteudo)
    => new(EncodingSped.Latin1.GetBytes(conteudo), writable: false);
```

- [ ] **Step 2: Rodar e confirmar falha de compilacao**

Run:

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~SnifferSpedFiscalTests"
```

Expected: FAIL porque `SnifferSpedFiscal` ainda nao existe.

- [ ] **Step 3: Criar `SnifferSpedFiscal`**

Create `src/TecnoFisc.Sped.Txt.Engine/Parser/SnifferSpedFiscal.cs`:

```csharp
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.Txt.Engine.Parser;

/// <summary>
/// Sniffer TXT fiscal opt-in. Use <see cref="SnifferSped"/> quando bastar identificar
/// projeto/versao; use este tipo quando tambem precisar de CNPJ e periodo.
/// </summary>
public static class SnifferSpedFiscal
{
    public static async ValueTask<MetadadosFiscaisArquivoSped> IdentificarAsync(
        Stream entrada,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);

        var identificacao = await SnifferSped.IdentificarAsync(entrada, cancelamento)
            .ConfigureAwait(false);

        var campos = identificacao.PrimeiraLinha.Split('|');
        var (indiceDataInicial, indiceDataFinal, indiceCnpj) = ObterIndices(identificacao.Projeto);

        return new MetadadosFiscaisArquivoSped(
            identificacao,
            ExtrairCnpj(campos, indiceCnpj),
            ExtrairData(campos, indiceDataInicial),
            ExtrairData(campos, indiceDataFinal));
    }

    private static (int DataInicial, int DataFinal, int Cnpj) ObterIndices(ProjetoSped projeto)
        => projeto switch
        {
            ProjetoSped.EfdContribuicoes => (6, 7, 9),
            ProjetoSped.EfdIcmsIpi => (4, 5, 7),
            ProjetoSped.Ecd => (3, 4, 6),
            _ => (-1, -1, -1),
        };

    private static Cnpj? ExtrairCnpj(string[] campos, int indice)
        => TryGetCampo(campos, indice) is string valor
            && Cnpj.TentarCriar(valor.AsSpan(), out var cnpj)
                ? cnpj
                : null;

    private static DateOnly? ExtrairData(string[] campos, int indice)
        => TryGetCampo(campos, indice) is string valor
            && ParseadoresPrimitivos.TentarDataComFormato(valor.AsSpan(), "ddMMyyyy", out var data)
                ? data
                : null;

    private static string? TryGetCampo(string[] campos, int indice)
        => indice >= 0 && indice < campos.Length && campos[indice].Length > 0
            ? campos[indice]
            : null;
}
```

- [ ] **Step 4: Rodar e confirmar que passa**

Run:

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~SnifferSpedFiscalTests"
```

Expected: PASS.

## Task 4: Casos negativos e decisao do consumidor

**Files:**
- Modify: `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/SnifferSpedFiscalTests.cs`

- [ ] **Step 1: Adicionar teste para desconhecido**

Append:

```csharp
[Fact]
public async Task IdentificarAsync_EntradaDesconhecida_PreservaIdentificacaoENaoRetornaDadosFiscais()
{
    await using var stream = Sped("|0000|999|0|01012025|31012025|EMPRESA|11222333000181|\r\n");

    var metadados = await SnifferSpedFiscal.IdentificarAsync(stream, TestContext.Current.CancellationToken);

    metadados.Projeto.Should().Be(ProjetoSped.Desconhecido);
    metadados.VersaoLeiaute.Should().Be(0);
    metadados.Cnpj.Should().BeNull();
    metadados.DataInicial.Should().BeNull();
    metadados.DataFinal.Should().BeNull();
    stream.Position.Should().Be(0);
}
```

- [ ] **Step 2: Adicionar teste para campos fiscais invalidos**

Append:

```csharp
[Fact]
public async Task IdentificarAsync_CamposFiscaisInvalidos_NaoFalhaENaoRetornaCamposInvalidos()
{
    await using var stream = Sped("|0000|006|0|||99022025|31022025|EMPRESA|00000000000000|MG|3126901||00|2|\r\n");

    var metadados = await SnifferSpedFiscal.IdentificarAsync(stream, TestContext.Current.CancellationToken);

    metadados.Projeto.Should().Be(ProjetoSped.EfdContribuicoes);
    metadados.VersaoLeiaute.Should().Be(6);
    metadados.Cnpj.Should().BeNull();
    metadados.DataInicial.Should().BeNull();
    metadados.DataFinal.Should().BeNull();
    stream.Position.Should().Be(0);
}
```

- [ ] **Step 3: Rodar os testes fiscais**

Run:

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~SnifferSpedFiscalTests"
```

Expected: PASS.

## Task 5: Documentacao e verificacao final

**Files:**
- Optional Modify: `CHANGELOG.md`
- Optional Modify: `ARCHITECTURE.md`

- [ ] **Step 1: Atualizar documentacao da decisao de duas APIs, se houver secao ativa**

If a new release section exists in `CHANGELOG.md`, add:

```markdown
- Novo `SnifferSpedFiscal.IdentificarAsync(Stream)`: sniff TXT opt-in que reaproveita `SnifferSped` e retorna `MetadadosFiscaisArquivoSped` com CNPJ, data inicial e data final quando esses campos estao disponiveis no Registro `0000`. O `SnifferSped` existente permanece como identificador leve de projeto/versao.
```

If `ARCHITECTURE.md` Stage 12 is being maintained for API details, keep the existing `SnifferSped` bullet unchanged and add:

```markdown
- `SnifferSpedFiscal.IdentificarAsync(Stream)` e a opcao detalhada para consumidores que precisam, alem de projeto/versao, dos metadados fiscais do Registro `0000` (`Cnpj`, `DataInicial`, `DataFinal`). Ele preserva o contrato leve de `SnifferSped`.
```

- [ ] **Step 2: Rodar testes de ambos os sniffers**

Run:

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~SnifferSpedTests|FullyQualifiedName~SnifferSpedFiscalTests"
```

Expected: PASS para os testes do sniffer atual e do novo sniffer fiscal.

- [ ] **Step 3: Rodar testes relevantes do engine TXT**

Run:

```powershell
dotnet test tests/TecnoFisc.Sped.Txt.Engine.Tests/TecnoFisc.Sped.Txt.Engine.Tests.csproj
```

Expected: PASS.

- [ ] **Step 4: Rodar build da solucao**

Run:

```powershell
dotnet build TecnoFisc.Sped.slnx
```

Expected: 0 errors.

## Self-Review

- Spec coverage: o plano preserva o sniffer atual para identificacao leve e cria um novo sniffer fiscal para CNPJ e periodo.
- Compatibility check: `MetadadosArquivoSped`, `SnifferSped.IdentificarAsync` e `SnifferSped.AbrirParserAsync` nao mudam de assinatura nem de comportamento.
- Consumer choice: a API fica explicita: `SnifferSped` para projeto/versao, `SnifferSpedFiscal` para projeto/versao/CNPJ/periodo.
- Failure behavior: entradas desconhecidas ou campos fiscais malformados retornam identificacao quando possivel e deixam CNPJ/periodo nulos.
- Dependency check: `Txt.Engine` ja referencia `Core`, portanto usar `Cnpj` no novo tipo fiscal nao cria ciclo.
