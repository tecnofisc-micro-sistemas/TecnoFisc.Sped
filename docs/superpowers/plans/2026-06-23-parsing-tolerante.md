# Parsing Tolerante Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Permitir, de forma opt-in, que o parser .txt do TecnoFisc.Sped não aborte a linha/arquivo quando um campo falha a conversão (P1/P2), tolere registro desconhecido via sentinela (P3) e ofereça parse de linha isolada tolerante (P4).

**Architecture:** A mudança concentra-se em `LeitorSpedTxt` (motor .txt). Duas flags em `ReadingOptions` gateiam tolerância de campo e de layout; erros de campo acumulam numa lista lazy no `RegistroSped` base; registro desconhecido vira um `RegistroNaoReconhecido` pendurado como folha; `ParseLinha` reusa o mesmo núcleo de interpretação em modo forçado-leniente, devolvendo `ResultadoParse<RegistroSped>`. Tudo default-off reproduz o comportamento atual byte a byte.

**Tech Stack:** C# / .NET 10, xUnit + FluentAssertions, BenchmarkDotNet. `PipeReader` + `ReadOnlySpan<char>`. Encoding Latin1/Win-1252.

## Global Constraints

- **.NET 10**, file-scoped namespaces, sealed por padrão.
- **Sem dependências externas em runtime** (streams in/out).
- **Idioma (ARCHITECTURE §1.3):** EN para flags/verbos/capacidades (`LenientFieldParsing`, `LenientLayout`, `RegistrarErroDeFormato`, `ParseLinha`); PT para substantivos fiscais/domínio (`ErroFormato`, `ErrosDeFormato`, `ValorBruto`, `RegistroNaoReconhecido`).
- **Opt-in / backward-compatible:** defaults `false`. Nenhum teste existente muda de resultado.
- **Performance-first (hard rule §5):** lista de erros lazy (zero alocação no caminho feliz); o `when` do catch filtra exatamente `FormatException | ArgumentException | OverflowException`; mudança sensível exige benchmark de regressão (Task 6).
- **Não é validador fiscal (§2.3):** só sinaliza não-conformidade de formato.
- **Commits:** Conventional Commits em EN no título, corpo em PT. Branch de trabalho com commits granulares; merge para `dev` será Squash and Merge (não fazer o merge aqui).
- **`ReadOnlySpan<char>` não pode ser capturado em closure/async** — helpers que recebem o span são síncronos e locais.
- Build: `dotnet build TecnoFisc.Sped.slnx` · Test: `dotnet test TecnoFisc.Sped.slnx --filter "..."`.

---

## File Structure

**Modificados:**
- `src/TecnoFisc.Sped.Core/Erros/ErroFormato.cs` — adiciona `ValorBruto` (P2).
- `src/TecnoFisc.Sped.Txt.Engine/Parser/ReadingOptions.cs` — adiciona `LenientFieldParsing` (P1) e `LenientLayout` (P3).
- `src/TecnoFisc.Sped.Txt.Engine/Abstracoes/RegistroSped.cs` — lista lazy `_errosDeFormato` + `ErrosDeFormato` + `RegistrarErroDeFormato` (P1).
- `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs` — `InterpretarLinha` (helper `Definir`, branch de layout, flags como locais com override) + novo método público `ParseLinha` (P1/P3/P4).
- `src/TecnoFisc.Sped.EfdContribuicoes/Parser/ParserEfdContribuicoes.cs`, `src/TecnoFisc.Sped.EfdIcmsIpi/Parser/ParserEfdIcmsIpi.cs`, `src/TecnoFisc.Sped.Ecd/Parser/ParserEcd.cs` — delegação de `ParseLinha` (P4 propagação).

**Criados:**
- `src/TecnoFisc.Sped.Txt.Engine/Abstracoes/RegistroNaoReconhecido.cs` — sentinela de layout (P3).
- Arquivos de teste correspondentes (ver cada task).
- `benchmarks/.../LenientParsingBenchmark.cs` — guarda de regressão (Task 6).

---

## Task 1: P2 — `ErroFormato.ValorBruto`

**Files:**
- Modify: `src/TecnoFisc.Sped.Core/Erros/ErroFormato.cs`
- Test: `tests/TecnoFisc.Sped.Core.Tests/Erros/ErroFormatoValorBrutoTests.cs` (Create)

**Interfaces:**
- Produces: `ErroFormato.ValorBruto { get; init; }` (`string?`) — consumido por P1/P3/P4 e pelo FiscTax.

- [ ] **Step 1: Write the failing test**

Create `tests/TecnoFisc.Sped.Core.Tests/Erros/ErroFormatoValorBrutoTests.cs`:

```csharp
using TecnoFisc.Sped.Core.Erros;

namespace TecnoFisc.Sped.Core.Tests.Erros;

public sealed class ErroFormatoValorBrutoTests
{
    [Fact]
    public void ValorBruto_QuandoNaoInformado_EhNull()
    {
        var erro = new ErroFormato(10, "C100", "ChvNfe", "Valor inválido");

        erro.ValorBruto.Should().BeNull();
    }

    [Fact]
    public void ValorBruto_QuandoInformadoViaInit_Preserva()
    {
        var erro = new ErroFormato(10, "C100", "ChvNfe", "Valor inválido") { ValorBruto = "3225...6541" };

        erro.ValorBruto.Should().Be("3225...6541");
    }

    [Fact]
    public void ToString_PermaneceInalterado_IgnorandoValorBruto()
    {
        var erro = new ErroFormato(10, "C100", "ChvNfe", "Valor inválido") { ValorBruto = "X" };

        erro.ToString().Should().Be("Linha 10 (C100.ChvNfe): Valor inválido");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~ErroFormatoValorBrutoTests"`
Expected: FAIL — compilação falha (`ValorBruto` não existe).

- [ ] **Step 3: Add the property**

Edit `src/TecnoFisc.Sped.Core/Erros/ErroFormato.cs`, dentro do corpo do record (antes do `ToString()`):

```csharp
public sealed record ErroFormato(
    long Linha,
    string? CodigoRegistro,
    string? Campo,
    string Mensagem)
{
    /// <summary>Texto cru do campo que falhou a conversão (preservado para o consumidor). Null quando
    /// o erro não está associado a um valor de campo específico (ex.: linha sem '|').</summary>
    public string? ValorBruto { get; init; }

    public override string ToString()
        => CodigoRegistro is null
            ? $"Linha {Linha}: {Mensagem}"
            : Campo is null
                ? $"Linha {Linha} ({CodigoRegistro}): {Mensagem}"
                : $"Linha {Linha} ({CodigoRegistro}.{Campo}): {Mensagem}";
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~ErroFormatoValorBrutoTests"`
Expected: PASS (3 testes).

- [ ] **Step 5: Commit**

```bash
git add src/TecnoFisc.Sped.Core/Erros/ErroFormato.cs tests/TecnoFisc.Sped.Core.Tests/Erros/ErroFormatoValorBrutoTests.cs
git commit -m "feat(core): adicionar ErroFormato.ValorBruto

Propriedade init nullable que preserva o texto cru do campo que falhou
a conversao, para o consumidor materializar apontamento. Aditiva; nao
quebra o construtor posicional nem o ToString."
```

---

## Task 2: P1 — modo leniente de campo

**Files:**
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Parser/ReadingOptions.cs`
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Abstracoes/RegistroSped.cs`
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs` (`InterpretarLinha`, ≈456-524)
- Test: `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/LeitorSpedTxtLenienteTests.cs` (Create)

**Interfaces:**
- Consumes: `ErroFormato.ValorBruto` (Task 1).
- Produces:
  - `ReadingOptions.LenientFieldParsing { get; init; }` (`bool`, default `false`).
  - `RegistroSped.ErrosDeFormato` (`IReadOnlyList<ErroFormato>`) e `internal void RegistrarErroDeFormato(ErroFormato)`.

- [ ] **Step 1: Write the failing tests**

Create `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/LeitorSpedTxtLenienteTests.cs`:

```csharp
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Core.Erros;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Parser;

public sealed class LeitorSpedTxtLenienteTests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000Sintetico).Assembly);

    private static MemoryStream FluxoSped(string conteudo)
        => new(EncodingSped.Latin1.GetBytes(conteudo));

    private static async Task<List<RegistroSped>> LerAsync(string conteudo, ReadingOptions opcoes)
    {
        var leitor = new LeitorSpedTxt(_catalogo, opcoes);
        var resultado = new List<RegistroSped>();
        await foreach (var r in leitor.ReadStreamingAsync(FluxoSped(conteudo)))
            resultado.Add(r);
        return resultado;
    }

    private static ReadingOptions Leniente => new() { LenientFieldParsing = true };

    [Fact]
    public async Task Estrito_CampoNumericoMalformado_ContinuaLancando()
    {
        // Comportamento atual preservado: default (estrito) lança no primeiro erro de campo.
        var act = async () => await LerAsync("|C001|abc|\r\n", ReadingOptions.Default);

        await act.Should().ThrowAsync<ErroFormatoSpedException>();
    }

    [Fact]
    public async Task Leniente_CampoMalformado_EmiteRegistroComCampoNoDefaultEAcumulaErro()
    {
        var registros = await LerAsync(
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|C001|abc|\r\n" +
            "|9999|3|\r\n", Leniente);

        registros.Select(r => r.Codigo).Should().Equal(["0000", "C001", "9999"]);

        var c001 = registros.OfType<RegistroC001Sintetico>().Single();
        c001.IndMov.Should().Be(default);                       // campo falho permanece no default
        c001.ErrosDeFormato.Should().HaveCount(1);
        c001.ErrosDeFormato[0].Campo.Should().Be("IndMov");
        c001.ErrosDeFormato[0].CodigoRegistro.Should().Be("C001");
        c001.ErrosDeFormato[0].ValorBruto.Should().Be("abc");
    }

    [Fact]
    public async Task Leniente_DoisCamposRuinsNaMesmaLinha_AcumulaAmbosSemAbortar()
    {
        // C100: |IND_OPER|COD_PART|VL_DOC|CFOP| — COD_PART e VL_DOC malformados.
        var registros = await LerAsync(
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|C100|0|abc|xyz|5102|\r\n" +
            "|9999|3|\r\n", Leniente);

        var c100 = registros.OfType<RegistroC100Sintetico>().Single();
        c100.ErrosDeFormato.Should().HaveCount(2);
        c100.ErrosDeFormato.Select(e => e.Campo).Should().Contain(["CodPart", "VlDoc"]);
    }

    [Fact]
    public async Task Leniente_CaminhoFeliz_NaoAlocaListaDeErros()
    {
        var registros = await LerAsync(
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|9999|2|\r\n", Leniente);

        registros.Should().OnlyContain(r => r.ErrosDeFormato.Count == 0);
        // instância vazia compartilhada (lista lazy não inicializada)
        registros[0].ErrosDeFormato.Should().BeSameAs(registros[1].ErrosDeFormato);
    }

    [Fact]
    public async Task Leniente_UmaLinhaRuim_NaoDerrubaAsDemais()
    {
        var registros = await LerAsync(
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|C001|abc|\r\n" +
            "|C100|0|123|1500,75|5102|\r\n" +
            "|9999|4|\r\n", Leniente);

        registros.Select(r => r.Codigo).Should().Equal(["0000", "C001", "C100", "9999"]);
        registros.OfType<RegistroC100Sintetico>().Single().CodPart.Should().Be(123);
    }
}
```

> Nota: o `ErrosDeFormato.Should().BeSameAs(...)` valida a instância vazia compartilhada (proxy de "zero alocação"). Se a grafia exata dos nomes `IndMov`/`CodPart`/`VlDoc` divergir nos sintéticos, ajustar pela definição real em `tests/.../_Sintetico/`.

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~LeitorSpedTxtLenienteTests"`
Expected: FAIL — `LenientFieldParsing` e `ErrosDeFormato` não existem (erro de compilação).

- [ ] **Step 3: Add the flag**

Edit `src/TecnoFisc.Sped.Txt.Engine/Parser/ReadingOptions.cs`, antes do `HasFilter`:

```csharp
    /// <summary>
    /// Quando <c>true</c>, uma falha de conversão de campo (FormatException/ArgumentException/
    /// OverflowException no Definidor) NÃO aborta a leitura: o campo fica no default, o erro é
    /// acumulado em <see cref="Abstracoes.RegistroSped.ErrosDeFormato"/> e o parsing continua.
    /// Padrão: <c>false</c> (lança ErroFormatoSpedException no primeiro erro de campo).
    /// Não afeta erros de LAYOUT (registro desconhecido) — ver <see cref="LenientLayout"/>.
    /// </summary>
    public bool LenientFieldParsing { get; init; } = false;
```

- [ ] **Step 4: Add accumulation to the base record**

Edit `src/TecnoFisc.Sped.Txt.Engine/Abstracoes/RegistroSped.cs`. Adicione o `using` no topo e os membros dentro da classe:

```csharp
using TecnoFisc.Sped.Core.Erros;
```

Dentro de `RegistroSped`, após o campo `_filhos`:

```csharp
    private List<ErroFormato>? _errosDeFormato;

    /// <summary>
    /// Erros de conversão de campo capturados em modo leniente (ver
    /// <see cref="Parser.ReadingOptions.LenientFieldParsing"/> e <see cref="Parser.LeitorSpedTxt.ParseLinha"/>).
    /// Vazia quando o registro foi lido sem problemas ou em modo estrito. O campo correspondente
    /// a cada erro permanece no valor default.
    /// </summary>
    public IReadOnlyList<ErroFormato> ErrosDeFormato => _errosDeFormato ?? (IReadOnlyList<ErroFormato>)[];

    internal void RegistrarErroDeFormato(ErroFormato erro) => (_errosDeFormato ??= []).Add(erro);
```

> `(IReadOnlyList<ErroFormato>)[]` devolve a mesma instância de array vazio compartilhada do runtime — zero alocação no caminho feliz.

- [ ] **Step 5: Branch the catch in `InterpretarLinha`**

Edit `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs`. No início de `InterpretarLinha` (após `var conteudo = linha[1..^1];`, ≈linha 448), adicione o local de tolerância e um helper síncrono que decide lançar ou acumular:

```csharp
        bool lenienteCampo = _opcoes.LenientFieldParsing;

        // Aplica um campo; em modo leniente, captura a falha de conversão, acumula no registro
        // (campo permanece no default) e segue. Em modo estrito, mantém o comportamento atual.
        void Definir(MetadadosCampo campo, ReadOnlySpan<char> valor)
        {
            try
            {
                campo.Definidor(registro!, valor);
            }
            catch (Exception ex) when (ex is FormatException or ArgumentException or OverflowException)
            {
                var erro = new ErroFormato(numeroLinha, metadados!.Codigo, campo.Nome, ex.Message)
                {
                    ValorBruto = valor.ToString()
                };
                if (!lenienteCampo)
                    throw new ErroFormatoSpedException(erro, ex);
                registro!.RegistrarErroDeFormato(erro);
            }
        }
```

Substitua os **três** sítios de `Definidor` no bloco `else if (metadados is not null && registro is not null)` (≈476-517) e **remova o `try/catch` externo** que os envolvia (≈481-516), pois a captura passa a viver dentro de `Definir`:

```csharp
            else if (metadados is not null && registro is not null)
            {
                int indice = posicaoCampo - 2;
                if (indice < metadados.Campos.Count)
                {
                    var campo = metadados.Campos[indice];
                    if (campo.CapturaTudo)
                    {
                        Definir(campo, conteudo[inicioCampo..]);
                        break;
                    }
                    if (campo.CampoArquivo)
                    {
                        var resto = conteudo[inicioCampo..];
                        int idxSep = resto.LastIndexOf('|');
                        if (idxSep < 0)
                        {
                            Definir(campo, resto);
                            break;
                        }
                        Definir(campo, resto[..idxSep]);
                        if (indice + 1 < metadados.Campos.Count)
                            Definir(metadados.Campos[indice + 1], resto[(idxSep + 1)..]);
                        break;
                    }
                    Definir(campo, fatia);
                }
            }
```

> O helper `Definir` é uma função local síncrona, então recebe o `ReadOnlySpan<char>` por parâmetro sem captura ilegal. `registro!`/`metadados!` são não-nulos neste branch.

- [ ] **Step 6: Run tests to verify they pass**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~LeitorSpedTxtLenienteTests"`
Expected: PASS (5 testes).

- [ ] **Step 7: Run the full Txt.Engine suite to confirm no regression**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~Txt.Engine.Tests"`
Expected: PASS — incluindo os testes estritos existentes (`...QuandoCampoNumericoMalformado_LancaErroFormato` etc.) inalterados.

- [ ] **Step 8: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine/Parser/ReadingOptions.cs src/TecnoFisc.Sped.Txt.Engine/Abstracoes/RegistroSped.cs src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/LeitorSpedTxtLenienteTests.cs
git commit -m "feat(txt): modo leniente de campo opt-in

ReadingOptions.LenientFieldParsing acumula falhas de conversao de campo
em RegistroSped.ErrosDeFormato (lista lazy, zero alocacao no caminho
feliz) e continua o parsing em vez de lancar. Default false reproduz o
comportamento atual."
```

---

## Task 3: P3 — tolerância a registro desconhecido (sentinela)

**Files:**
- Create: `src/TecnoFisc.Sped.Txt.Engine/Abstracoes/RegistroNaoReconhecido.cs`
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Parser/ReadingOptions.cs`
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs` (`InterpretarLinha`, branch de código desconhecido ≈463-474)
- Test: `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/LeitorSpedTxtLayoutLenienteTests.cs` (Create)

**Interfaces:**
- Consumes: `RegistroSped.AdicionarFilho` (internal, já existente), `PilhaHierarquica.Topo` (existente).
- Produces:
  - `RegistroNaoReconhecido : RegistroSped` com `string LinhaCrua` e `ErroLayout Erro`; `Codigo` = código cru lido.
  - `ReadingOptions.LenientLayout { get; init; }` (`bool`, default `false`).

- [ ] **Step 1: Write the failing tests**

Create `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/LeitorSpedTxtLayoutLenienteTests.cs`:

```csharp
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Core.Erros;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Parser;

public sealed class LeitorSpedTxtLayoutLenienteTests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000Sintetico).Assembly);

    private static MemoryStream FluxoSped(string conteudo)
        => new(EncodingSped.Latin1.GetBytes(conteudo));

    private static async Task<List<RegistroSped>> LerAsync(string conteudo, ReadingOptions opcoes)
    {
        var leitor = new LeitorSpedTxt(_catalogo, opcoes);
        var resultado = new List<RegistroSped>();
        await foreach (var r in leitor.ReadStreamingAsync(FluxoSped(conteudo)))
            resultado.Add(r);
        return resultado;
    }

    private static ReadingOptions LenienteLayout => new() { LenientLayout = true };

    [Fact]
    public async Task Estrito_CodigoDesconhecido_ContinuaLancandoErroLayout()
    {
        var act = async () => await LerAsync("|XXXX|1|\r\n", ReadingOptions.Default);

        var assercao = await act.Should().ThrowAsync<ErroLayoutSpedException>();
        assercao.Which.Erro.CodigoRegistro.Should().Be("XXXX");
    }

    [Fact]
    public async Task Leniente_CodigoDesconhecido_EmiteSentinelaECarregaLinhaCrua()
    {
        var registros = await LerAsync(
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|XXXX|foo|bar|\r\n" +
            "|9999|3|\r\n", LenienteLayout);

        registros.Select(r => r.Codigo).Should().Equal(["0000", "XXXX", "9999"]);

        var sentinela = registros.OfType<RegistroNaoReconhecido>().Single();
        sentinela.Codigo.Should().Be("XXXX");
        sentinela.LinhaCrua.Should().Be("|XXXX|foo|bar|");
        sentinela.Erro.CodigoRegistro.Should().Be("XXXX");
    }

    [Fact]
    public async Task Leniente_SentinelaEhFolha_NaoViraPaiDosSeguintes()
    {
        // O registro conhecido seguinte (C001) deve ancorar no pai real (0000), nao no sentinela.
        var registros = await LerAsync(
            "|0000|006|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|XXXX|foo|\r\n" +
            "|C001|0|\r\n" +
            "|9999|4|\r\n", LenienteLayout);

        var r0000 = registros.OfType<Registro0000Sintetico>().Single();
        var sentinela = registros.OfType<RegistroNaoReconhecido>().Single();
        var c001 = registros.OfType<RegistroC001Sintetico>().Single();

        sentinela.Pai.Should().BeSameAs(r0000);     // pendurado como folha no topo vigente
        sentinela.Filhos.Should().BeEmpty();        // nunca recebe filhos
        c001.Pai.Should().BeSameAs(r0000);          // sentinela nao perturbou a hierarquia
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~LeitorSpedTxtLayoutLenienteTests"`
Expected: FAIL — `RegistroNaoReconhecido` e `LenientLayout` não existem.

- [ ] **Step 3: Create the sentinel record**

Create `src/TecnoFisc.Sped.Txt.Engine/Abstracoes/RegistroNaoReconhecido.cs`:

```csharp
using TecnoFisc.Sped.Core.Erros;

namespace TecnoFisc.Sped.Txt.Engine.Abstracoes;

/// <summary>
/// Registro emitido pelo leitor em modo <see cref="Parser.ReadingOptions.LenientLayout"/> quando o
/// código de registro é desconhecido pelo catálogo. Preserva a linha crua completa e o
/// <see cref="ErroLayout"/> correspondente para o consumidor diagnosticar sem abortar o arquivo.
/// É sempre folha na hierarquia (nunca recebe filhos).
/// </summary>
public sealed class RegistroNaoReconhecido : RegistroSped
{
    public RegistroNaoReconhecido(string codigo, string linhaCrua, ErroLayout erro)
    {
        ArgumentNullException.ThrowIfNull(codigo);
        ArgumentNullException.ThrowIfNull(linhaCrua);
        ArgumentNullException.ThrowIfNull(erro);
        _codigo = codigo;
        LinhaCrua = linhaCrua;
        Erro = erro;
    }

    private readonly string _codigo;

    /// <summary>Código cru lido na posição 1 da linha (desconhecido pelo catálogo).</summary>
    public override string Codigo => _codigo;

    /// <summary>Linha SPED crua completa (com pipes), preservada verbatim.</summary>
    public string LinhaCrua { get; }

    /// <summary>Diagnóstico de layout associado.</summary>
    public ErroLayout Erro { get; }
}
```

- [ ] **Step 4: Add the `LenientLayout` flag**

Edit `src/TecnoFisc.Sped.Txt.Engine/Parser/ReadingOptions.cs`, após `LenientFieldParsing`:

```csharp
    /// <summary>
    /// Quando <c>true</c>, um código de registro desconhecido pelo catálogo NÃO aborta a leitura:
    /// o leitor emite um <see cref="Abstracoes.RegistroNaoReconhecido"/> (linha crua + erro) como
    /// folha na hierarquia e continua. Padrão: <c>false</c> (lança ErroLayoutSpedException,
    /// comportamento atual). Independente de <see cref="LenientFieldParsing"/>.
    /// </summary>
    public bool LenientLayout { get; init; } = false;
```

- [ ] **Step 5: Branch the unknown-code path in `InterpretarLinha`**

Edit `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs`. Adicione o local de tolerância de layout junto ao `lenienteCampo` (≈após `var conteudo = ...`):

```csharp
        bool lenienteLayout = _opcoes.LenientLayout;
```

Substitua o `throw new ErroLayoutSpedException(...)` do branch `posicaoCampo == 1` (≈466-469) por:

```csharp
                metadados = metadadosResolvido;
                if (metadados is null && !_catalogo.TentarObter(fatia, out metadados))
                {
                    var erroLayout = new ErroLayout(numeroLinha, fatia.ToString(),
                        "Código de registro desconhecido pelo catálogo.");
                    if (!lenienteLayout)
                        throw new ErroLayoutSpedException(erroLayout);

                    // Sentinela: pendura como folha no topo atual (sem empilhar, nunca vira pai).
                    var sentinela = new RegistroNaoReconhecido(fatia.ToString(), linha.ToString(), erroLayout);
                    pilha.Topo?.AdicionarFilho(sentinela);
                    return sentinela;
                }
```

> `return sentinela;` sai de `InterpretarLinha` sem atingir o `pilha.Empilhar(registro, metadados.Nivel)` final — o sentinela não entra na pilha, então não vira pai. `pilha.Topo?.AdicionarFilho` só vincula se há um pai vigente (folha pendurada). O `yield return registro` em `ReadStreamingAsync` emite o sentinela normalmente; seu `Codigo` (cru) nunca é `"9999"`, então não dispara encerramento.

- [ ] **Step 6: Run tests to verify they pass**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~LeitorSpedTxtLayoutLenienteTests"`
Expected: PASS (3 testes).

- [ ] **Step 7: Confirm the existing strict layout test still passes**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~LeitorSpedTxtTests.LerStreamingAsync_QuandoCodigoDesconhecido_LancaErroLayout"`
Expected: PASS (inalterado).

- [ ] **Step 8: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine/Abstracoes/RegistroNaoReconhecido.cs src/TecnoFisc.Sped.Txt.Engine/Parser/ReadingOptions.cs src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/LeitorSpedTxtLayoutLenienteTests.cs
git commit -m "feat(txt): tolerar registro desconhecido via sentinela

ReadingOptions.LenientLayout emite RegistroNaoReconhecido (linha crua +
ErroLayout) como folha no topo da pilha quando o codigo e desconhecido,
em vez de lancar ErroLayoutSpedException. Sentinela nunca vira pai.
Default false reproduz o comportamento atual."
```

---

## Task 4: P4 — `ParseLinha` (linha isolada, sempre tolerante)

**Files:**
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs` (`InterpretarLinha` ganha overrides opcionais; novo método público `ParseLinha`)
- Test: `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/LeitorSpedTxtParseLinhaTests.cs` (Create)

**Interfaces:**
- Consumes: `InterpretarLinha` (privado), `ResultadoParse<RegistroSped>` (Core), `RegistroNaoReconhecido` (Task 3).
- Produces: `public ResultadoParse<RegistroSped> ParseLinha(ReadOnlySpan<char> linha, long numeroLinha = 0)`.

- [ ] **Step 1: Write the failing tests**

Create `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/LeitorSpedTxtParseLinhaTests.cs`:

```csharp
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Core.Erros;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Parser;

public sealed class LeitorSpedTxtParseLinhaTests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000Sintetico).Assembly);

    private static LeitorSpedTxt Leitor => new(_catalogo);   // opções default: ParseLinha é leniente por natureza

    [Fact]
    public void ParseLinha_LinhaLimpa_RetornaOkSemErros()
    {
        var resultado = Leitor.ParseLinha("|C100|0|123|1500,75|5102|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroC100Sintetico>();
        resultado.Valor.ErrosDeFormato.Should().BeEmpty();
        resultado.Valor.Pai.Should().BeNull();              // sem hierarquia
    }

    [Fact]
    public void ParseLinha_CampoRuim_RetornaSucessoComErroNoRegistro()
    {
        var resultado = Leitor.ParseLinha("|C100|0|abc|1500,75|5102|");

        resultado.Sucesso.Should().BeTrue();                // produziu registro
        resultado.Valor.ErrosDeFormato.Should().HaveCount(1);
        resultado.Valor.ErrosDeFormato[0].Campo.Should().Be("CodPart");
        resultado.Valor.ErrosDeFormato[0].ValorBruto.Should().Be("abc");
    }

    [Fact]
    public void ParseLinha_CodigoDesconhecido_RetornaFalha()
    {
        var resultado = Leitor.ParseLinha("|XXXX|foo|");

        resultado.Falha.Should().BeTrue();
        resultado.Erros.Should().ContainSingle();
        resultado.Erros[0].CodigoRegistro.Should().Be("XXXX");
    }

    [Fact]
    public void ParseLinha_LinhaSemPipes_RetornaFalhaComValorBruto()
    {
        var resultado = Leitor.ParseLinha("C100;0;123");

        resultado.Falha.Should().BeTrue();
        resultado.Erros[0].ValorBruto.Should().Be("C100;0;123");
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~LeitorSpedTxtParseLinhaTests"`
Expected: FAIL — `ParseLinha` não existe.

- [ ] **Step 3: Allow forced-lenient overrides in `InterpretarLinha`**

Edit `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs`. Acrescente dois parâmetros opcionais ao final da assinatura de `InterpretarLinha` (≈429-434):

```csharp
    private RegistroSped? InterpretarLinha(
        ReadOnlySpan<char> linha,
        long numeroLinha,
        PilhaHierarquica pilha,
        int versaoLeiaute,
        MetadadosRegistro? metadadosResolvido,
        bool? forcarLenienteCampo = null,
        bool? forcarLenienteLayout = null)
```

E troque a inicialização dos dois locais (introduzidos em Task 2/Task 3) para honrar o override:

```csharp
        bool lenienteCampo = forcarLenienteCampo ?? _opcoes.LenientFieldParsing;
        bool lenienteLayout = forcarLenienteLayout ?? _opcoes.LenientLayout;
```

> `ReadStreamingAsync` chama `InterpretarLinha` sem os novos argumentos → comportamento idêntico (lê de `_opcoes`).

- [ ] **Step 4: Add the public `ParseLinha`**

Edit `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs`. Adicione o método público (após `ReadStreamingAsync`, antes dos helpers privados). Garanta o `using TecnoFisc.Sped.Core.Erros;` (já presente):

```csharp
    /// <summary>
    /// Parseia uma única linha SPED canônica (<c>|REG|...|</c>) isoladamente, sem hierarquia nem
    /// streaming. Nunca lança por erro de campo: o registro (em <see cref="ResultadoParse{T}.Valor"/>)
    /// traz os campos conversíveis preenchidos e os que falharam no valor default, com os erros em
    /// <see cref="Abstracoes.RegistroSped.ErrosDeFormato"/>. Devolve falha apenas quando nenhum
    /// registro pôde ser produzido (linha sem '|' nas pontas ou código desconhecido pelo catálogo).
    /// </summary>
    public ResultadoParse<RegistroSped> ParseLinha(ReadOnlySpan<char> linha, long numeroLinha = 0)
    {
        if (linha.IsEmpty || linha[0] != '|' || linha[^1] != '|')
            return ResultadoParse<RegistroSped>.Falhar(
                new ErroFormato(numeroLinha, null, null, "Linha SPED deve iniciar e terminar com '|'.")
                {
                    ValorBruto = linha.IsEmpty ? null : linha.ToString()
                });

        var pilha = new PilhaHierarquica();   // descartável: ParseLinha não constrói hierarquia
        var registro = InterpretarLinha(linha, numeroLinha, pilha, versaoLeiaute: 0,
            metadadosResolvido: null, forcarLenienteCampo: true, forcarLenienteLayout: true);

        if (registro is RegistroNaoReconhecido sentinela)
            return ResultadoParse<RegistroSped>.Falhar(
                new ErroFormato(numeroLinha, sentinela.Codigo, null, sentinela.Erro.Mensagem));

        return registro is null
            ? ResultadoParse<RegistroSped>.Falhar(
                new ErroFormato(numeroLinha, null, null, "Linha não produziu registro."))
            : ResultadoParse<RegistroSped>.Ok(registro);
    }
```

> Forçando `lenienteLayout: true`, código desconhecido volta como sentinela (sem exceção) e é convertido em `Falha`. Forçando `lenienteCampo: true`, erros de campo nunca lançam e ficam em `registro.ErrosDeFormato`, com `Sucesso == true`. A `pilha` descartável deixa `registro.Pai == null` (topo vazio).

- [ ] **Step 5: Run tests to verify they pass**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~LeitorSpedTxtParseLinhaTests"`
Expected: PASS (4 testes).

- [ ] **Step 6: Confirm streaming unchanged**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~Txt.Engine.Tests"`
Expected: PASS (toda a suíte do engine, incluindo lenient/layout das tasks anteriores).

- [ ] **Step 7: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/LeitorSpedTxtParseLinhaTests.cs
git commit -m "feat(txt): ParseLinha para parse tolerante de linha isolada

Novo metodo publico em LeitorSpedTxt que parseia uma unica linha sem
hierarquia nem streaming, sempre leniente. Ok(registro) com erros de
campo em ErrosDeFormato; Falha apenas quando nenhum registro sai
(linha sem pipes ou codigo desconhecido). Reusa InterpretarLinha via
overrides forcados de leniencia."
```

---

## Task 5: Integração end-to-end + propagação de `ParseLinha`

**Files:**
- Modify: `src/TecnoFisc.Sped.EfdContribuicoes/Parser/ParserEfdContribuicoes.cs`
- Modify: `src/TecnoFisc.Sped.EfdIcmsIpi/Parser/ParserEfdIcmsIpi.cs`
- Modify: `src/TecnoFisc.Sped.Ecd/Parser/ParserEcd.cs`
- Test: `tests/TecnoFisc.Sped.EfdContribuicoes.Tests/Parser/ParserEfdContribuicoesLenienteTests.cs` (Create)

**Interfaces:**
- Consumes: `LeitorSpedTxt.ParseLinha` (Task 4), `ReadingOptions.LenientFieldParsing`/`LenientLayout` (já fluem pelo ctor existente `Parser(..., ReadingOptions)`).
- Produces: `public ResultadoParse<RegistroSped> ParseLinha(ReadOnlySpan<char> linha, long numeroLinha = 0)` em cada parser de formato (delegação ao `_leitor`).

> O streaming já propaga as flags (os 3 parsers repassam `ReadingOptions` ao `LeitorSpedTxt` no ctor). Esta task adiciona a delegação de `ParseLinha` e prova o caso real do FiscTax (C100.ChvNfe com DV inválido) ponta a ponta.

- [ ] **Step 1: Write the failing test (caso real do FiscTax)**

Create `tests/TecnoFisc.Sped.EfdContribuicoes.Tests/Parser/ParserEfdContribuicoesLenienteTests.cs`:

```csharp
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdContribuicoes.Parser;
using TecnoFisc.Sped.EfdContribuicoes.Registros.BlocoC;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Parser;

public sealed class ParserEfdContribuicoesLenienteTests
{
    private static MemoryStream Fluxo(string conteudo)
        => new(EncodingSped.Latin1.GetBytes(conteudo));

    // C100 real com ChvNfe de 44 dígitos mas DV/cUF/CNPJ inválido (dado sujo de terceiros).
    private const string ChaveInvalida = "32251100011998216756550252411200013640116541";

    [Fact]
    public async Task ReadStreamingAsync_Leniente_ChvNfeInvalida_NaoAbortaEAcumulaErro()
    {
        // Monte uma linha C100 minimamente válida com a ChvNfe na posição correta do leiaute V006.
        // Ajuste os demais campos conforme RegistroC100 (ver definição do registro); o ponto do
        // teste é que ChvNfe inválida NAO derruba o arquivo em modo leniente.
        var sped =
            "|0000|006|0|01012025|31012025|EMPRESA|11222333000181|UF|0000000|0|0|\r\n" +
            "|C001|0|\r\n" +
            $"|C100|0|1|FORN|55|00|1|001|123|{ChaveInvalida}|01012025|01012025|1500,75|...|\r\n" +
            "|C990|3|\r\n" +
            "|9999|5|\r\n";

        var parser = new ParserEfdContribuicoes(new ReadingOptions { LenientFieldParsing = true });
        var registros = new List<RegistroSped>();
        await foreach (var r in parser.ReadStreamingAsync(Fluxo(sped)))
            registros.Add(r);

        var c100 = registros.OfType<RegistroC100>().Single();
        c100.ChvNfe.Should().BeNull();                      // campo nullable permanece no default
        c100.ErrosDeFormato.Should().ContainSingle(e => e.Campo == "ChvNfe");
        c100.ErrosDeFormato.Single(e => e.Campo == "ChvNfe").ValorBruto.Should().Be(ChaveInvalida);
    }

    [Fact]
    public void ParseLinha_DelegaAoLeitor_RetornaResultadoTolerante()
    {
        var parser = new ParserEfdContribuicoes();

        var resultado = parser.ParseLinha($"|C100|0|1|FORN|55|00|1|001|123|{ChaveInvalida}|01012025|01012025|1500,75|...|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.ErrosDeFormato.Should().ContainSingle(e => e.Campo == "ChvNfe");
    }
}
```

> **Importante para o implementador:** a ordem/contagem exata de campos do `C100` deve casar com `src/TecnoFisc.Sped.EfdContribuicoes/Registros/BlocoC/RegistroC100.cs`. Abra esse arquivo, posicione `ChvNfe` corretamente e preencha os demais campos com valores válidos (o teste não depende dos outros valores, só de a linha ser estruturalmente válida e ChvNfe ser a única falha). Se preferir, reduza a linha aos campos até `ChvNfe` (campos finais ausentes são ignorados pelo leitor — layouts podem omitir colunas à direita).

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~ParserEfdContribuicoesLenienteTests"`
Expected: FAIL — `parser.ParseLinha` não existe (e/ou a linha precisa de ajuste de campos).

- [ ] **Step 3: Delegate `ParseLinha` in `ParserEfdContribuicoes`**

Edit `src/TecnoFisc.Sped.EfdContribuicoes/Parser/ParserEfdContribuicoes.cs`. Adicione, após `ReadStreamingAsync` (≈54-55). Garanta os `using` de `TecnoFisc.Sped.Core.Erros` e `TecnoFisc.Sped.Txt.Engine.Abstracoes`:

```csharp
    /// <summary>
    /// Parseia uma única linha SPED isoladamente, tolerante por natureza (ver
    /// <see cref="LeitorSpedTxt.ParseLinha"/>). Não constrói hierarquia.
    /// </summary>
    public ResultadoParse<RegistroSped> ParseLinha(ReadOnlySpan<char> linha, long numeroLinha = 0)
        => _leitor.ParseLinha(linha, numeroLinha);
```

- [ ] **Step 4: Mirror the delegation in the other two .txt format parsers**

Edit `src/TecnoFisc.Sped.EfdIcmsIpi/Parser/ParserEfdIcmsIpi.cs` e `src/TecnoFisc.Sped.Ecd/Parser/ParserEcd.cs`, adicionando o mesmo método `ParseLinha` delegando ao respectivo campo leitor (confira o nome do campo — `_leitor`). Use o mesmo corpo do Step 3.

> Se a grafia do campo interno divergir (ex.: `_leitorSped`), ajuste para o nome real em cada arquivo. Não altere assinaturas existentes de `ReadStreamingAsync`/`ReadAsync`.

- [ ] **Step 5: Run the test to verify it passes**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~ParserEfdContribuicoesLenienteTests"`
Expected: PASS (2 testes). Se falhar por estrutura da linha C100, ajuste os campos conforme `RegistroC100.cs` e re-rode.

- [ ] **Step 6: Run the full suite to confirm no regression**

Run: `dotnet test TecnoFisc.Sped.slnx`
Expected: PASS — toda a solução. Nenhum teste pré-existente muda de resultado (defaults off).

- [ ] **Step 7: Commit**

```bash
git add src/TecnoFisc.Sped.EfdContribuicoes/Parser/ParserEfdContribuicoes.cs src/TecnoFisc.Sped.EfdIcmsIpi/Parser/ParserEfdIcmsIpi.cs src/TecnoFisc.Sped.Ecd/Parser/ParserEcd.cs tests/TecnoFisc.Sped.EfdContribuicoes.Tests/Parser/ParserEfdContribuicoesLenienteTests.cs
git commit -m "feat: expor ParseLinha nos parsers de formato + e2e leniente

Parsers EFD Contribuicoes/ICMS-IPI/ECD delegam ParseLinha ao
LeitorSpedTxt. Teste ponta a ponta cobre o caso real do FiscTax:
C100.ChvNfe com DV invalido nao derruba o arquivo em modo leniente
(campo fica null, erro acumulado com ValorBruto)."
```

---

## Task 6: Performance — guarda de regressão no caminho estrito

**Files:**
- Create: `benchmarks/TecnoFisc.Sped.Benchmarks/LenientParsingBenchmark.cs`

**Interfaces:**
- Consumes: `ParserEfdContribuicoes`, `ReadingOptions`.

> Objetivo: provar que o caminho estrito (default, sem flags) não regrediu e medir o overhead do modo leniente no caminho feliz (deve ser ~zero — lista lazy não aloca). Cumpre a hard rule §5 (mudança sensível a performance exige benchmark).

- [ ] **Step 1: Add the benchmark**

Create `benchmarks/TecnoFisc.Sped.Benchmarks/LenientParsingBenchmark.cs` (siga o padrão dos benchmarks existentes — ver `ParserCatalogoBenchmark.cs` para setup de stream/catálogo):

```csharp
using BenchmarkDotNet.Attributes;

using TecnoFisc.Sped.EfdContribuicoes.Parser;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Benchmarks;

[MemoryDiagnoser]
public class LenientParsingBenchmark
{
    private byte[] _arquivo = null!;

    [GlobalSetup]
    public void Setup()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("|0000|006|0|01012025|31012025|EMPRESA|11222333000181|UF|0000000|0|0|");
        sb.AppendLine("|C001|0|");
        for (int i = 1; i <= 2000; i++)
            sb.AppendLine($"|C100|0|1|FORN|55|00|1|001|{i}|||01012025|01012025|1500,75|...|");
        sb.AppendLine("|C990|2002|");
        sb.AppendLine("|9999|2004|");
        _arquivo = EncodingSped.Latin1.GetBytes(sb.ToString());
    }

    [Benchmark(Baseline = true)]
    public async Task<int> Estrito() => await Contar(new ReadingOptions());

    [Benchmark]
    public async Task<int> Leniente_CaminhoFeliz() => await Contar(new ReadingOptions { LenientFieldParsing = true });

    private async Task<int> Contar(ReadingOptions opcoes)
    {
        var parser = new ParserEfdContribuicoes(opcoes);
        int n = 0;
        await foreach (var _ in parser.ReadStreamingAsync(new MemoryStream(_arquivo)))
            n++;
        return n;
    }
}
```

> Ajuste a estrutura da linha C100 conforme `RegistroC100.cs` (mesmo cuidado da Task 5). O ponto é parsear linhas limpas; nenhuma deve falhar.

- [ ] **Step 2: Build the benchmarks project in Release**

Run: `dotnet build benchmarks/TecnoFisc.Sped.Benchmarks -c Release`
Expected: build OK (sem erros).

- [ ] **Step 3: Run the benchmark**

Run: `dotnet run -c Release --project benchmarks/TecnoFisc.Sped.Benchmarks -- --filter "*LenientParsingBenchmark*"`
Expected: `Leniente_CaminhoFeliz` com `Allocated` igual ao `Estrito` (lista lazy não inicializa no caminho feliz) e tempo dentro do ruído (~baseline). Registre o resultado no PR.

- [ ] **Step 4: Commit**

```bash
git add benchmarks/TecnoFisc.Sped.Benchmarks/LenientParsingBenchmark.cs
git commit -m "perf(bench): guarda de regressao do parsing leniente

Compara caminho estrito (baseline) vs leniente no caminho feliz;
confirma overhead ~zero (lista de erros lazy nao aloca)."
```

---

## Self-Review

**1. Spec coverage:**
- §3 P1 (flag + acúmulo + catch) → Task 2 ✓
- §4 P2 (`ValorBruto`) → Task 1 ✓
- §5 P3 (sentinela + `LenientLayout` + folha) → Task 3 ✓
- §6 P4 (`ParseLinha`, semântica Sucesso/Falha) → Task 4 ✓
- §7 propagação (parsers repassam options; `ParseLinha` delegado) → Task 5 ✓ (streaming já propaga via ctor existente)
- §8 testes (6 cenários P1/P2 + P3 + P4) → Tasks 2–5 ✓
- §9 performance (lazy, benchmark) → Task 6 ✓
- §10 conformidade (idioma, opt-in, não-validador) → Global Constraints + defaults off ✓
- §11 fora de escopo (escrita/gerador) → não há task (correto) ✓

**2. Placeholder scan:** sem TBD/TODO. As duas notas "ajuste conforme `RegistroC100.cs`" são instruções concretas de verificação contra o registro real (a estrutura exata do C100 não é citável sem abrir o arquivo do leiaute), não placeholders de design.

**3. Type consistency:** `LenientFieldParsing`/`LenientLayout`, `ErrosDeFormato`/`RegistrarErroDeFormato`, `ValorBruto`, `RegistroNaoReconhecido(codigo, linhaCrua, erro)` com `LinhaCrua`/`Erro`/`Codigo`, `ParseLinha(ReadOnlySpan<char>, long)` → `ResultadoParse<RegistroSped>` — consistentes entre Tasks 1→6.
