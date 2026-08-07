# Correções do review do PR 531 — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Tratar os dez achados verificados do review do PR 531 sem regredir o comportamento padrão dos pacotes EFD Contribuições, EFD ICMS-IPI e ECD já publicados.

**Architecture:** Toda a rigidez nova introduzida pelo branch (validação de domínio de enum, vigência do leiaute) deixa de ser comportamento global e passa a ser opt-in por `ReadingOptions`, resolvida por leiaute — o `ParserEcf` liga, os demais parsers não. Onde o branch trocou um algoritmo auto-corretivo por um estatal, volta-se ao auto-corretivo. Onde o branch descarta dados, passa a existir sinal observável.

**Tech Stack:** .NET 10, C# com file-scoped namespaces, xUnit v3, FluentAssertions, BenchmarkDotNet, Roslyn Incremental Source Generators, Python + uv (`tools/ecf-layout`).

## Global Constraints

- Solução: `TecnoFisc.Sped.slnx`. Build: `dotnet build TecnoFisc.Sped.slnx -warnaserror`. Testes: `dotnet test TecnoFisc.Sped.slnx`.
- Branch de trabalho: `feat/ecf-layout-12`, no worktree `G:\repos\TecnoFisc.Sped\.worktrees\ecf-layout-12`. Todo comando roda a partir daí.
- **Nenhum comportamento padrão do TXT Engine muda para EFD Contribuições, EFD ICMS-IPI e ECD.** Único desvio autorizado: Task 11 (unificação de enums), que é breaking assumido.
- Sem dependências externas de runtime. Streams entram, streams saem.
- Substantivos do domínio SPED em português; verbos, factories e predicados booleanos em inglês (`Create`, `ReadAsync`, `IsEntrada`, `HasFilter`, `ShouldIgnore`). Os dois idiomas nunca se misturam dentro de um mesmo identificador.
- Toda flag nova de `MetadadosCampo`/`MetadadosRegistro` precisa existir em **três lugares**: o atributo, o `CatalogoBuilder` reflexivo e o `RegistroSpedCatalogoGenerator`. Divergência entre catálogo gerado e reflexivo é bug silencioso.
- Sem reflection em hot path de parsing. `Activator.CreateInstance` e `PropertyInfo.SetValue` por registro são proibidos.
- Classes `sealed` por padrão; `partial` nas que o source generator estende.
- Todo I/O `async` com `ConfigureAwait(false)`.
- Encoding dos `.txt` SPED: Latin1 / Windows-1252.
- Commits em Conventional Commits, prefixo em inglês imperativo minúsculo, corpo em português. Tipos aceitos: `feat`, `fix`, `perf`, `refactor`, `docs`, `test`, `build`, `ci`, `chore`, `revert`.
- Comentários de código: português para explicação fiscal/de formato, inglês para nota técnica.

## Estrutura de arquivos

**Modificados no TXT Engine (compartilhado):**

| Arquivo | Responsabilidade após o trabalho |
|---|---|
| `src/TecnoFisc.Sped.Txt.Engine/Parser/ReadingOptions.cs` | Passa a expor as duas flags de rigidez como `bool?` (`null` = decisão do leiaute) |
| `src/TecnoFisc.Sped.Txt.Engine/Catalogo/MetadadosCampo.cs` | Passa a carregar um segundo definidor opcional (estrito) e a escolher entre eles por parâmetro |
| `src/TecnoFisc.Sped.Txt.Engine/Catalogo/CatalogoBuilder.cs` | Produz os dois conversores de enum; o permissivo restaura o comportamento de `origin/dev` |
| `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs` | Resolve as flags uma vez, restaura o mapeamento posicional de campo e emite sentinela de vigência |
| `src/TecnoFisc.Sped.Txt.Engine.SourceGenerators/RegistroSpedCatalogoGenerator.cs` | Emite o setter estrito paralelo e para de suprimir o catálogo quando há diagnóstico |
| `src/TecnoFisc.Sped.Txt.Engine/Enums/IndicadorDebitoCredito.cs`, `IndicadorTipoConta.cs` | Passam a ser a definição canônica única (Task 11) |

**Modificados nos leiautes:**

| Arquivo | Responsabilidade após o trabalho |
|---|---|
| `src/TecnoFisc.Sped.Ecf/Parser/ParserEcf.cs` | Resolve `null` → `true` nas duas flags, preservando override explícito do chamador |
| `src/TecnoFisc.Sped.Ecf/ArquivoEcf.cs` | Expõe `RegistrosNaoReconhecidos` e roteia sentinelas para lá |
| `src/TecnoFisc.Sped.Ecd/ArquivoEcd.cs`, `src/TecnoFisc.Sped.EfdIcmsIpi/ArquivoEfdIcmsIpi.cs`, `src/TecnoFisc.Sped.EfdContribuicoes/ArquivoEfdContribuicoes.cs` | Idem |
| `src/TecnoFisc.Sped.Ecf/Registros/**/*.cs` (180 arquivos) | Todo `[CampoSped]` passa a declarar `Nome` normativo |

**Criados:**

| Arquivo | Responsabilidade |
|---|---|
| `tests/TecnoFisc.Sped.Txt.Engine.Tests/_Sintetico/RegistroEnumDominioSintetico.cs` | Registro sintético com enum numérico fechado, para exercitar as duas políticas |
| `tests/TecnoFisc.Sped.Txt.Engine.Tests/_Sintetico/RegistroVigenciaColunaSintetico.cs` | Registro sintético com campo barrado por vigência seguido de outros campos |
| `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/ValidacaoDominioEnumTests.cs` | Prova que a validação é opt-in e que o caminho desligado equivale a `origin/dev` |
| `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/VigenciaCampoPosicionalTests.cs` | Prova que coluna barrada não desloca as seguintes |
| `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/SentinelaVigenciaTests.cs` | Prova que todo registro descartado por vigência vira `RegistroNaoReconhecido` |
| `tests/TecnoFisc.Sped.Txt.Engine.Tests/Catalogo/OrdemCatalogoTests.cs` | Fixa a ordem canônica de enumeração no catálogo gerado |
| `benchmarks/TecnoFisc.Sped.Benchmarks/ParserVigenciaBenchmark.cs` | Mede o custo do gate de vigência e da validação de domínio |
| `tools/ecf-layout/src/ecf_layout/field_names.py` | Gera os aliases `Nome` a partir do manifesto |

---

### Task 1: Flags de rigidez viram `bool?` e o ECF resolve os defaults

Fecha o achado 2 (parte do opt-out) e prepara o terreno para o achado 1.

**Files:**
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Parser/ReadingOptions.cs`
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs:90`
- Modify: `src/TecnoFisc.Sped.Ecf/Parser/ParserEcf.cs:39-47`
- Test: `tests/TecnoFisc.Sped.Ecf.Tests/Parser/ParserEcfOpcoesTests.cs`

**Interfaces:**
- Produces: `ReadingOptions.RespeitarVigenciaDoLeiaute` e `ReadingOptions.ValidarDominioDeEnum`, ambos `bool?`. `LeitorSpedTxt` trata `null` como `false`. `ParserEcf` resolve `null` como `true` antes de construir o leitor.

- [ ] **Step 1: Escrever o teste que falha**

Criar `tests/TecnoFisc.Sped.Ecf.Tests/Parser/ParserEcfOpcoesTests.cs`:

```csharp
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Parser;

public sealed class ParserEcfOpcoesTests
{
    [Fact]
    public void SemOpcoes_LigaVigenciaEValidacaoDeDominio()
    {
        var resolvidas = ParserEcf.ResolverOpcoes(ReadingOptions.Default);

        resolvidas.RespeitarVigenciaDoLeiaute.Should().BeTrue();
        resolvidas.ValidarDominioDeEnum.Should().BeTrue();
    }

    [Fact]
    public void OverrideExplicito_VenceOPadraoDoLeiaute()
    {
        var resolvidas = ParserEcf.ResolverOpcoes(new ReadingOptions
        {
            RespeitarVigenciaDoLeiaute = false,
            ValidarDominioDeEnum = false,
        });

        resolvidas.RespeitarVigenciaDoLeiaute.Should().BeFalse();
        resolvidas.ValidarDominioDeEnum.Should().BeFalse();
    }

    [Fact]
    public void ResolverOpcoes_PreservaOsDemaisCamposDoChamador()
    {
        var origem = new ReadingOptions
        {
            LenientLayout = true,
            LenientFieldParsing = true,
            RegistrosIgnorados = new HashSet<string>(StringComparer.Ordinal) { "Y800" },
            BlocosIgnorados = new HashSet<string>(StringComparer.Ordinal) { "X" },
        };

        var resolvidas = ParserEcf.ResolverOpcoes(origem);

        resolvidas.LenientLayout.Should().BeTrue();
        resolvidas.LenientFieldParsing.Should().BeTrue();
        resolvidas.RegistrosIgnorados.Should().BeEquivalentTo(["Y800"]);
        resolvidas.BlocosIgnorados.Should().BeEquivalentTo(["X"]);
    }
}
```

- [ ] **Step 2: Rodar o teste e confirmar que falha**

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~ParserEcfOpcoesTests"
```

Esperado: erro de compilação — `ParserEcf.ResolverOpcoes` não existe e `ValidarDominioDeEnum` não existe.

- [ ] **Step 3: Trocar as duas propriedades para `bool?` em `ReadingOptions`**

Substituir a propriedade `RespeitarVigenciaDoLeiaute` existente por:

```csharp
    /// <summary>
    /// Quando <c>true</c>, omite registros anteriores a <c>IntroduzidoEm</c> e não atribui
    /// campos anteriores a <c>DesdeVersao</c>, usando a versão declarada pelo registro 0000.
    /// <c>null</c> (padrão) delega a decisão ao parser do leiaute: o ECF liga, os demais
    /// leiautes read-only mantêm o modelo informacional completo e não ligam.
    /// </summary>
    public bool? RespeitarVigenciaDoLeiaute { get; init; }

    /// <summary>
    /// Quando <c>true</c>, um código numérico fora do domínio declarado de um enum fechado
    /// (sem <c>[SpedValor]</c>) vira erro de campo em vez de cast permissivo. <c>null</c>
    /// (padrão) delega a decisão ao parser do leiaute: o ECF liga, os demais mantêm o cast
    /// permissivo — a Receita publica códigos novos entre versões do guia e um arquivo que
    /// hoje é lido não pode passar a falhar por atualização de pacote.
    /// </summary>
    public bool? ValidarDominioDeEnum { get; init; }
```

- [ ] **Step 4: Ajustar o call site em `LeitorSpedTxt`**

No campo de instância, logo após a atribuição de `_opcoes` no construtor, adicionar dois campos resolvidos uma única vez (evita `Nullable<bool>` no hot path):

```csharp
    private readonly bool _respeitarVigencia;
    private readonly bool _validarDominioDeEnum;
```

e no corpo do construtor:

```csharp
        _respeitarVigencia = opcoes.RespeitarVigenciaDoLeiaute ?? false;
        _validarDominioDeEnum = opcoes.ValidarDominioDeEnum ?? false;
```

Trocar a condição em `ReadStreamingAsync` (linha ~90) de `_opcoes.RespeitarVigenciaDoLeiaute &&` para `_respeitarVigencia &&`, e a função local `CampoAtivo` (final de `InterpretarLinha`) de `!_opcoes.RespeitarVigenciaDoLeiaute` para `!_respeitarVigencia`.

- [ ] **Step 5: Substituir `ComVigenciaDoLeiaute` por `ResolverOpcoes` em `ParserEcf`**

```csharp
    /// <summary>
    /// Resolve as opções do chamador contra os padrões do leiaute ECF: vigência e validação
    /// de domínio ligadas quando o chamador não se pronunciou; override explícito sempre vence.
    /// </summary>
    internal static ReadingOptions ResolverOpcoes(ReadingOptions opcoes)
        => new()
        {
            RegistrosIgnorados = opcoes.RegistrosIgnorados,
            BlocosIgnorados = opcoes.BlocosIgnorados,
            LenientFieldParsing = opcoes.LenientFieldParsing,
            LenientLayout = opcoes.LenientLayout,
            RespeitarVigenciaDoLeiaute = opcoes.RespeitarVigenciaDoLeiaute ?? true,
            ValidarDominioDeEnum = opcoes.ValidarDominioDeEnum ?? true,
        };
```

e no construtor trocar `ComVigenciaDoLeiaute(opcoes)` por `ResolverOpcoes(opcoes)`. Remover `ComVigenciaDoLeiaute`.

Expor `internal` para o assembly de teste: conferir se `src/TecnoFisc.Sped.Ecf/TecnoFisc.Sped.Ecf.csproj` já tem `InternalsVisibleTo` para `TecnoFisc.Sped.Ecf.Tests`; se não tiver, adicionar:

```xml
  <ItemGroup>
    <InternalsVisibleTo Include="TecnoFisc.Sped.Ecf.Tests" />
  </ItemGroup>
```

- [ ] **Step 6: Rodar os testes**

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~ParserEcfOpcoesTests"
dotnet build TecnoFisc.Sped.slnx -warnaserror
```

Esperado: 3 testes passando, build sem warning.

- [ ] **Step 7: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine/Parser/ReadingOptions.cs src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs src/TecnoFisc.Sped.Ecf/Parser/ParserEcf.cs src/TecnoFisc.Sped.Ecf/TecnoFisc.Sped.Ecf.csproj tests/TecnoFisc.Sped.Ecf.Tests/Parser/ParserEcfOpcoesTests.cs
git commit -m "fix(txt): tornar rigidez de leitura opt-in por leiaute

As flags RespeitarVigenciaDoLeiaute e ValidarDominioDeEnum viram bool?:
null significa decisao do leiaute. O ParserEcf resolve as duas para true
e preserva override explicito do chamador; os demais parsers mantem o
comportamento informacional."
```

---

### Task 2: Conversor de enum permissivo e estrito no catálogo reflexivo

Fecha os achados 1 e 7 no caminho `CatalogoBuilder`.

**Files:**
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Catalogo/MetadadosCampo.cs`
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Catalogo/CatalogoBuilder.cs:464-478`
- Create: `tests/TecnoFisc.Sped.Txt.Engine.Tests/_Sintetico/RegistroEnumDominioSintetico.cs`
- Create: `tests/TecnoFisc.Sped.Txt.Engine.Tests/Catalogo/ConversorEnumDominioTests.cs`

**Interfaces:**
- Consumes: `ReadingOptions.ValidarDominioDeEnum` (Task 1).
- Produces: `MetadadosCampo.Definidor(RegistroSped registro, ReadOnlySpan<char> valor, bool validarDominio)`. A sobrecarga de dois argumentos permanece e equivale a `validarDominio: false`. O construtor de `MetadadosCampo` ganha o parâmetro opcional final `Action<RegistroSped, ReadOnlySpan<char>>? definidorEstrito = null`.

- [ ] **Step 1: Criar o registro sintético**

`tests/TecnoFisc.Sped.Txt.Engine.Tests/_Sintetico/RegistroEnumDominioSintetico.cs`:

```csharp
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

/// <summary>Enum numérico fechado, sem [SpedValor] — o caso do achado 1.</summary>
public enum TipoItemSintetico
{
    Mercadoria = 0,
    Servico = 1,
}

/// <summary>Enum com nomes de membro, para provar o parsing por nome do caminho permissivo.</summary>
public enum SituacaoSintetica
{
    S = 0,
    N = 1,
}

[Flags]
public enum MarcadoresSinteticos
{
    Nenhum = 0,
    Primeiro = 1,
    Segundo = 2,
}

[RegistroSped(Codigo = "A200", Nivel = 2, Bloco = "A")]
public sealed partial class RegistroEnumDominioSintetico : RegistroSped
{
    public override string Codigo => "A200";

    [CampoSped(Ordem = 2, Tamanho = 2, Nome = "TIPO_ITEM")]
    public TipoItemSintetico TipoItem { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 1, Nome = "SITUACAO")]
    public SituacaoSintetica Situacao { get; set; }

    [CampoSped(Ordem = 4, Tamanho = 3, Nome = "MARCADORES")]
    public MarcadoresSinteticos Marcadores { get; set; }
}
```

- [ ] **Step 2: Escrever o teste que falha**

`tests/TecnoFisc.Sped.Txt.Engine.Tests/Catalogo/ConversorEnumDominioTests.cs`:

```csharp
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Catalogo;

public sealed class ConversorEnumDominioTests
{
    private static MetadadosCampo Campo(string nome)
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(typeof(RegistroEnumDominioSintetico).Assembly);
        catalogo.TentarObter("A200", out var metadados).Should().BeTrue();
        return metadados!.Campos.Single(c => c.Nome == nome);
    }

    [Fact]
    public void SemValidacao_CodigoForaDoDominioEhAceitoComoCast()
    {
        var campo = Campo("TIPO_ITEM");
        var registro = new RegistroEnumDominioSintetico();

        campo.Definidor(registro, "12", validarDominio: false);

        ((int)registro.TipoItem).Should().Be(12);
    }

    [Fact]
    public void ComValidacao_CodigoForaDoDominioLancaFormatException()
    {
        var campo = Campo("TIPO_ITEM");
        var registro = new RegistroEnumDominioSintetico();

        var acao = () => campo.Definidor(registro, "12", validarDominio: true);

        acao.Should().Throw<FormatException>();
    }

    [Fact]
    public void ComValidacao_CodigoDentroDoDominioEhAceito()
    {
        var campo = Campo("TIPO_ITEM");
        var registro = new RegistroEnumDominioSintetico();

        campo.Definidor(registro, "1", validarDominio: true);

        registro.TipoItem.Should().Be(TipoItemSintetico.Servico);
    }

    [Fact]
    public void SemValidacao_NomeDeMembroContinuaSendoAceito()
    {
        var campo = Campo("SITUACAO");
        var registro = new RegistroEnumDominioSintetico();

        campo.Definidor(registro, "N", validarDominio: false);

        registro.Situacao.Should().Be(SituacaoSintetica.N);
    }

    [Fact]
    public void EnumFlags_NaoEhValidadoNemComValidacaoLigada()
    {
        var campo = Campo("MARCADORES");
        var registro = new RegistroEnumDominioSintetico();

        campo.Definidor(registro, "3", validarDominio: true);

        registro.Marcadores.Should().Be(MarcadoresSinteticos.Primeiro | MarcadoresSinteticos.Segundo);
    }

    [Fact]
    public void SobrecargaDeDoisArgumentos_EquivaleAoCaminhoPermissivo()
    {
        var campo = Campo("TIPO_ITEM");
        var registro = new RegistroEnumDominioSintetico();

        campo.Definidor(registro, "12");

        ((int)registro.TipoItem).Should().Be(12);
    }
}
```

- [ ] **Step 3: Rodar e confirmar a falha**

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~ConversorEnumDominioTests"
```

Esperado: erro de compilação — a sobrecarga de três argumentos de `Definidor` não existe.

- [ ] **Step 4: Estender `MetadadosCampo`**

Adicionar o campo privado, ao lado de `_definidor`:

```csharp
    private readonly Action<RegistroSped, ReadOnlySpan<char>>? _definidorEstrito;
```

Adicionar o parâmetro opcional ao final da lista do construtor (depois de `bool campoArquivo = false`):

```csharp
        Action<RegistroSped, ReadOnlySpan<char>>? definidorEstrito = null)
```

e no corpo:

```csharp
        _definidorEstrito = definidorEstrito;
```

Substituir o método `Definidor` de dois argumentos por:

```csharp
    /// <summary>
    /// Aplica o valor textual ao registro. Recebe o conteúdo do campo entre pipes (sem
    /// delimitadores). Vazio é interpretado conforme a nullabilidade do tipo de destino.
    /// Equivale a <c>Definidor(registro, valor, validarDominio: false)</c>.
    /// </summary>
    public void Definidor(RegistroSped registro, ReadOnlySpan<char> valor)
        => _definidor(registro, valor);

    /// <summary>
    /// Aplica o valor textual ao registro. Quando <paramref name="validarDominio"/> é
    /// <c>true</c> e o campo é um enum fechado, um código fora do domínio declarado vira
    /// <see cref="FormatException"/> em vez de cast permissivo.
    /// </summary>
    public void Definidor(RegistroSped registro, ReadOnlySpan<char> valor, bool validarDominio)
    {
        if (validarDominio && _definidorEstrito is not null)
            _definidorEstrito(registro, valor);
        else
            _definidor(registro, valor);
    }
```

- [ ] **Step 5: Restaurar o conversor permissivo e adicionar o estrito no `CatalogoBuilder`**

Substituir o bloco `if (porValorSped.Count == 0) { ... }` de `ConstruirConversorEnum` por uma versão parametrizada. O método passa a receber a política:

```csharp
    private static Func<string, object> ConstruirConversorEnum(
        Type alvo,
        CampoSpedAttribute atributo,
        bool validarDominio)
    {
        // ... o cálculo de porValorSped permanece inalterado ...

        if (porValorSped.Count == 0)
        {
            if (!validarDominio)
                return s => Enum.Parse(alvo, s, ignoreCase: false);

            bool ehFlags = alvo.IsDefined(typeof(FlagsAttribute), inherit: false);
            Func<string, object> conversorSubjacente =
                ConstruirConversorIntegral(Enum.GetUnderlyingType(alvo));
            return s =>
            {
                object valor = Enum.ToObject(alvo, conversorSubjacente(s));
                if (!ehFlags && !Enum.IsDefined(alvo, valor))
                    throw new FormatException($"Valor '{s}' não é válido para {alvo.Name}.");
                return valor;
            };
        }

        return s => porValorSped.TryGetValue(s, out var valor)
            ? valor
            : throw new FormatException($"Valor '{s}' não é válido para {alvo.Name}.");
    }
```

`ConstruirConversorIntegral` permanece como está no branch — é usado apenas pelo caminho estrito.

No ponto de `ConstruirCampos` onde o definidor é composto, construir os dois. Localizar a montagem do `MetadadosCampo` (a chamada `lista.Add((atributo.Ordem, new MetadadosCampo(...)))`) e, imediatamente antes dela, produzir o definidor estrito apenas quando o campo for enum fechado sem `[SpedValor]`:

```csharp
            Action<RegistroSped, ReadOnlySpan<char>>? definidorEstrito =
                PrecisaDefinidorEstrito(propriedade.PropertyType, atributo)
                    ? ComporDefinidor(propriedade, atributo, validarDominio: true)
                    : null;
```

onde `ComporDefinidor` é o método já existente que produz o `Action<RegistroSped, ReadOnlySpan<char>>` (renomear/parametrizar o caminho atual para aceitar `bool validarDominio` e repassá-lo a `ConstruirConversorEnum`), e:

```csharp
    /// <summary>
    /// Só enum fechado sem <c>[SpedValor]</c> tem política de domínio: os enums textuais já
    /// rejeitam token desconhecido, e os demais tipos não têm domínio a validar.
    /// </summary>
    private static bool PrecisaDefinidorEstrito(Type tipo, CampoSpedAttribute atributo)
    {
        Type alvo = Nullable.GetUnderlyingType(tipo) ?? tipo;
        if (!alvo.IsEnum)
            return false;

        return !alvo.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Any(campo => campo.IsDefined(typeof(SpedValorAttribute), inherit: false));
    }
```

Passar `definidorEstrito` como último argumento do construtor de `MetadadosCampo`.

- [ ] **Step 6: Rodar os testes**

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~ConversorEnumDominioTests"
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~CatalogoBuilder"
```

Esperado: 6 testes novos passando, suíte existente de `CatalogoBuilder` verde.

- [ ] **Step 7: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine/Catalogo/MetadadosCampo.cs src/TecnoFisc.Sped.Txt.Engine/Catalogo/CatalogoBuilder.cs tests/TecnoFisc.Sped.Txt.Engine.Tests/_Sintetico/RegistroEnumDominioSintetico.cs tests/TecnoFisc.Sped.Txt.Engine.Tests/Catalogo/ConversorEnumDominioTests.cs
git commit -m "fix(txt): restaurar conversor de enum permissivo no catalogo reflexivo

O caminho padrao volta a aceitar codigo fora do dominio e nome de membro,
como em origin/dev. A validacao por Enum.IsDefined passa a viver em um
segundo definidor, escolhido em tempo de leitura pela politica do leiaute."
```

---

### Task 3: Setter estrito paralelo no source generator

Fecha o achado 1 no caminho gerado. Sem esta task o catálogo gerado e o reflexivo divergem — exatamente a classe de bug que a regra dos três lugares existe para prevenir.

**Files:**
- Modify: `src/TecnoFisc.Sped.Txt.Engine.SourceGenerators/RegistroSpedCatalogoGenerator.cs` — `EmitirSetterEnum` (~linha 718), `EmitirHelperSet` (~linha 589) e `EmitirCampo` (~linha 564)
- Test: `tests/TecnoFisc.Sped.Txt.Engine.Tests/SourceGenerators/RegistroSpedCatalogoGeneratorDominioEnumTests.cs`

**Interfaces:**
- Consumes: o parâmetro opcional `definidorEstrito` do construtor de `MetadadosCampo` (Task 2).
- Produces: para todo campo de enum fechado sem `[SpedValor]`, o gerador emite dois métodos — `Set_<Codigo>_<NomeClr>` (permissivo, já existente) e `Set_<Codigo>_<NomeClr>_Estrito` (novo) — e passa o segundo ao `MetadadosCampo` pelo argumento nomeado `definidorEstrito:`.

Nomenclatura já usada pelo gerador, a respeitar: `InfoCampo.Nome` é o nome da propriedade CLR (usado nos nomes dos helpers) e `InfoCampo.FieldName` é o nome SPED (usado no `MetadadosCampo`). A categoria de enum é `CategoriaCampo.Enum`.

- [ ] **Step 1: Escrever o teste que falha**

Espelhar o estilo do arquivo existente `tests/TecnoFisc.Sped.Txt.Engine.Tests/SourceGenerators/RegistroSpedCatalogoGeneratorNomeCampoTests.cs` (mesmo harness de compilação Roslyn). Criar `RegistroSpedCatalogoGeneratorDominioEnumTests.cs`:

```csharp
namespace TecnoFisc.Sped.Txt.Engine.Tests.SourceGenerators;

public sealed class RegistroSpedCatalogoGeneratorDominioEnumTests
{
    private const string Fonte = """
        using TecnoFisc.Sped.Txt.Engine.Abstracoes;
        using TecnoFisc.Sped.Txt.Engine.Atributos;

        namespace Exemplo;

        public enum TipoItemGerado { Mercadoria = 0, Servico = 1 }

        [RegistroSped(Codigo = "B100", Nivel = 2, Bloco = "B")]
        public sealed partial class RegistroB100 : RegistroSped
        {
            public override string Codigo => "B100";

            [CampoSped(Ordem = 2, Tamanho = 2, Nome = "TIPO_ITEM")]
            public TipoItemGerado TipoItem { get; set; }
        }
        """;

    [Fact]
    public void EnumFechado_GeraSetterPermissivoESetterEstrito()
    {
        string gerado = ExecutarGerador(Fonte);

        gerado.Should().Contain("Set_RegistroB100_TipoItem_Estrito");
        gerado.Should().Contain("Enum.IsDefined(convertido)");
    }

    [Fact]
    public void SetterPermissivo_NaoValidaODominio()
    {
        string gerado = ExecutarGerador(Fonte);

        int inicio = gerado.IndexOf("Set_RegistroB100_TipoItem(", StringComparison.Ordinal);
        int fim = gerado.IndexOf("Set_RegistroB100_TipoItem_Estrito(", StringComparison.Ordinal);
        inicio.Should().BeGreaterThan(-1);
        fim.Should().BeGreaterThan(inicio);
        gerado[inicio..fim].Should().NotContain("Enum.IsDefined");
    }

    [Fact]
    public void MetadadosCampo_RecebeODefinidorEstritoComoUltimoArgumento()
    {
        string gerado = ExecutarGerador(Fonte);

        gerado.Should().Contain("Set_RegistroB100_TipoItem_Estrito)");
    }
}
```

`ExecutarGerador` é o helper já existente no projeto de teste do gerador; reutilizá-lo com o mesmo nome e assinatura do arquivo `RegistroSpedCatalogoGeneratorNomeCampoTests.cs`. Se ele estiver privado naquele arquivo, extraí-lo para `tests/TecnoFisc.Sped.Txt.Engine.Tests/SourceGenerators/GeradorHarness.cs` como `internal static class GeradorHarness` com `internal static string ExecutarGerador(string fonte)` e atualizar o chamador antigo.

- [ ] **Step 2: Rodar e confirmar a falha**

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~RegistroSpedCatalogoGeneratorDominioEnumTests"
```

Esperado: FAIL — `Set_RegistroB100_TipoItem_Estrito` não aparece no código gerado.

- [ ] **Step 3: Separar a emissão do corpo estrito**

Em `EmitirSetterEnum`, remover o bloco `if (!c.EnumFlags) { ... Enum.IsDefined ... }` — o setter padrão volta ao cast direto:

```csharp
        sb.Append("        ").Append(under).Append(" bruto = ").Append(under)
            .AppendLine(".Parse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture);");
        sb.Append("        alvo.").Append(c.Nome).Append(" = (").Append(c.TipoFq).AppendLine(")bruto;");
```

Adicionar o método irmão:

```csharp
    /// <summary>
    /// Emite o corpo do setter estrito de enum fechado: mesma conversão do permissivo, mais a
    /// checagem de domínio. Só é chamado quando <see cref="PrecisaSetterEstrito"/> for true.
    /// </summary>
    private static void EmitirSetterEnumEstrito(StringBuilder sb, InfoCampo c)
    {
        string under = c.UnderlyingPrimitivo ?? "int";
        if (c.Nullable)
        {
            sb.Append("        if (valor.IsEmpty) { alvo.").Append(c.Nome).AppendLine(" = null; return; }");
        }
        else
        {
            sb.AppendLine("        if (valor.IsEmpty)");
            sb.AppendLine("        {");
            sb.Append("            alvo.").Append(c.Nome).AppendLine(" = default;");
            sb.AppendLine("            return;");
            sb.AppendLine("        }");
        }
        sb.Append("        ").Append(under).Append(" bruto = ").Append(under)
            .AppendLine(".Parse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture);");
        sb.Append("        ").Append(c.TipoFq).Append(" convertido = (").Append(c.TipoFq).AppendLine(")bruto;");
        sb.AppendLine("        if (!Enum.IsDefined(convertido))");
        sb.Append("            throw new FormatException(\"Valor '\" + valor.ToString() + \"' não é válido para ")
            .Append(EscaparLiteral(c.TipoNome)).AppendLine(".\");");
        sb.Append("        alvo.").Append(c.Nome).AppendLine(" = convertido;");
    }

    /// <summary>
    /// Só enum fechado sem <c>[SpedValor]</c> e sem <c>[Flags]</c> tem domínio a validar.
    /// </summary>
    private static bool PrecisaSetterEstrito(InfoCampo c)
        => c.Categoria == CategoriaCampo.Enum && !c.EnumFlags && c.EnumValoresSped.IsDefaultOrEmpty;
```

- [ ] **Step 4: Emitir o método estrito e ligá-lo ao `MetadadosCampo`**

Em `EmitirHelperSet`, ao final do método, emitir o helper irmão quando aplicável:

```csharp
        if (PrecisaSetterEstrito(c))
        {
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.Append("    private static void Set_").Append(reg.Codigo).Append('_').Append(c.Nome)
                .AppendLine("_Estrito(RegistroSped registro, ReadOnlySpan<char> valor)");
            sb.AppendLine("    {");
            sb.Append("        var alvo = (").Append(reg.TipoFullyQualified).AppendLine(")registro;");
            EmitirSetterEnumEstrito(sb, c);
        }
```

Posicionar esse bloco imediatamente antes do fechamento `sb.AppendLine("    }");` final de `EmitirHelperSet`, de modo que o primeiro `AppendLine("    }")` acima feche o helper permissivo e o fechamento original feche o estrito.

Em `EmitirCampo`, acrescentar o argumento nomeado logo depois dos três opcionais existentes (`desdeVersao`, `capturaTudo`, `campoArquivo`) e antes do `sb.Append(')')`:

```csharp
        if (PrecisaSetterEstrito(campo))
        {
            sb.Append(", definidorEstrito: Set_").Append(reg.Codigo).Append('_')
                .Append(campo.Nome).Append("_Estrito");
        }
```

O argumento é nomeado, então não depende de os opcionais anteriores terem sido emitidos — o gerador já os emite condicionalmente por nome.

- [ ] **Step 5: Rodar os testes**

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~RegistroSpedCatalogoGenerator"
dotnet build TecnoFisc.Sped.slnx -warnaserror
```

Esperado: 3 testes novos passando e a suíte de gerador existente verde.

- [ ] **Step 6: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine.SourceGenerators/RegistroSpedCatalogoGenerator.cs tests/TecnoFisc.Sped.Txt.Engine.Tests/SourceGenerators/
git commit -m "fix(txt): emitir setter estrito de enum em paralelo ao permissivo

O catalogo gerado passa a acompanhar o reflexivo: setter padrao volta ao
cast direto e a checagem de dominio vive em um setter irmao, escolhido em
tempo de leitura."
```

---

### Task 4: Ligar a política de domínio ao leitor e provar o comportamento por leiaute

Fecha o critério de aceitação 3 do spec.

**Files:**
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs:522` (a função local `Definir`)
- Create: `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/ValidacaoDominioEnumTests.cs`
- Test: `tests/TecnoFisc.Sped.Ecf.Tests/Parser/ValidacaoDominioEnumEcfTests.cs`

**Interfaces:**
- Consumes: `MetadadosCampo.Definidor(registro, valor, validarDominio)` (Task 2), `_validarDominioDeEnum` (Task 1).

- [ ] **Step 1: Escrever os testes que falham**

`tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/ValidacaoDominioEnumTests.cs`:

```csharp
using TecnoFisc.Sped.Core.Erros;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Parser;

public sealed class ValidacaoDominioEnumTests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroEnumDominioSintetico).Assembly);

    [Fact]
    public void OpcoesPadrao_LeemCodigoForaDoDominioSemErro()
    {
        var resultado = new LeitorSpedTxt(_catalogo).ParseLinha("|A200|12|S|0|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroEnumDominioSintetico>().Which;
        ((int)registro.TipoItem).Should().Be(12);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void ComValidacaoLigada_CodigoForaDoDominioAbortaALeitura()
    {
        var opcoes = new ReadingOptions { ValidarDominioDeEnum = true };
        var leitor = new LeitorSpedTxt(_catalogo, opcoes);

        var acao = () => leitor.ParseLinha("|A200|12|S|0|");

        acao.Should().Throw<ErroFormatoSpedException>();
    }

    [Fact]
    public void ComValidacaoLigadaELeniente_AcumulaErroEContinua()
    {
        var opcoes = new ReadingOptions { ValidarDominioDeEnum = true, LenientFieldParsing = true };

        var resultado = new LeitorSpedTxt(_catalogo, opcoes).ParseLinha("|A200|12|S|0|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.ErrosDeFormato.Should().ContainSingle()
            .Which.Campo.Should().Be("TIPO_ITEM");
    }
}
```

`tests/TecnoFisc.Sped.Ecf.Tests/Parser/ValidacaoDominioEnumEcfTests.cs`:

```csharp
using TecnoFisc.Sped.Core.Erros;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Parser;

public sealed class ValidacaoDominioEnumEcfTests
{
    [Fact]
    public void ParserEcfPadrao_RejeitaCodigoDeEnumForaDoDominio()
    {
        var acao = () => new ParserEcf().ParseLinha("|M500|CTA1|9|0,00|D|0,00|D|0,00|D|0,00|D|");

        acao.Should().Throw<ErroFormatoSpedException>();
    }

    [Fact]
    public void ParserEcfComValidacaoDesligada_AceitaCodigoForaDoDominio()
    {
        var parser = new ParserEcf(new ReadingOptions { ValidarDominioDeEnum = false });

        var resultado = parser.ParseLinha("|M500|CTA1|9|0,00|D|0,00|D|0,00|D|0,00|D|");

        resultado.Sucesso.Should().BeTrue();
    }
}
```

Antes de rodar, conferir no `RegistroM500` qual campo é enum numérico fechado; se `CodTributo` (`IndicadorTributoContaParteB`) usar `[SpedValor]`, escolher outro registro ECF cujo enum seja numérico fechado — usar `git grep -l "public enum" src/TecnoFisc.Sped.Ecf/Enums` e escolher um sem `[SpedValor]`, ajustando a linha SPED de exemplo ao layout desse registro.

- [ ] **Step 2: Rodar e confirmar a falha**

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~ValidacaoDominioEnum"
```

Esperado: FAIL nos casos que exigem a validação ligada — o leitor ainda ignora a flag.

- [ ] **Step 3: Repassar a flag no leitor**

Em `InterpretarLinha`, dentro da função local `Definir`, trocar

```csharp
                campo.Definidor(registro!, valor);
```

por

```csharp
                campo.Definidor(registro!, valor, _validarDominioDeEnum);
```

- [ ] **Step 4: Rodar os testes**

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~ValidacaoDominioEnum"
dotnet test TecnoFisc.Sped.slnx
```

Esperado: 5 testes novos passando e a suíte inteira verde — em especial as suítes de EFD Contribuições, EFD ICMS-IPI e ECD, que provam que nada mudou para elas.

- [ ] **Step 5: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/ValidacaoDominioEnumTests.cs tests/TecnoFisc.Sped.Ecf.Tests/Parser/ValidacaoDominioEnumEcfTests.cs
git commit -m "fix(txt): aplicar politica de dominio de enum conforme o leiaute

O leitor passa a escolher o definidor pela flag resolvida. EFD
Contribuicoes, EFD ICMS-IPI e ECD seguem lendo codigos fora do dominio
como antes; o ECF rejeita."
```

---

### Task 5: Restaurar o mapeamento posicional de campo

Fecha o achado 4.

**Files:**
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs:571-612`
- Create: `tests/TecnoFisc.Sped.Txt.Engine.Tests/_Sintetico/RegistroVigenciaColunaSintetico.cs`
- Create: `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/VigenciaCampoPosicionalTests.cs`

**Interfaces:**
- Consumes: `_respeitarVigencia` (Task 1).

- [ ] **Step 1: Criar o registro sintético**

`tests/TecnoFisc.Sped.Txt.Engine.Tests/_Sintetico/RegistroVigenciaColunaSintetico.cs`:

```csharp
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

/// <summary>
/// Campo 3 só existe a partir da versão 12; campos 2 e 4 existem sempre. Um arquivo que
/// declara versão anterior mas traz a coluna 3 preenchida não pode deslocar a coluna 4.
/// </summary>
[RegistroSped(Codigo = "A300", Nivel = 2, Bloco = "A")]
public sealed partial class RegistroVigenciaColunaSintetico : RegistroSped
{
    public override string Codigo => "A300";

    [CampoSped(Ordem = 2, Nome = "ANTES")]
    public string? Antes { get; set; }

    [CampoSped(Ordem = 3, Nome = "NOVO", DesdeVersao = 12)]
    public string? Novo { get; set; }

    [CampoSped(Ordem = 4, Nome = "DEPOIS")]
    public string? Depois { get; set; }
}
```

- [ ] **Step 2: Escrever o teste que falha**

`tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/VigenciaCampoPosicionalTests.cs`:

```csharp
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Parser;

public sealed class VigenciaCampoPosicionalTests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroVigenciaColunaSintetico).Assembly);

    private static RegistroVigenciaColunaSintetico Ler(int versao)
    {
        var leitor = new LeitorSpedTxt(_catalogo, new ReadingOptions { RespeitarVigenciaDoLeiaute = true });
        return (RegistroVigenciaColunaSintetico)LeitorSpedTxtTestHelper
            .ParseLinhaComVersao(leitor, "|A300|a|n|d|", versao)!;
    }

    [Fact]
    public void ColunaBarradaPresenteNoArquivo_NaoDeslocaAsSeguintes()
    {
        var registro = Ler(versao: 10);

        registro.Antes.Should().Be("a");
        registro.Novo.Should().BeNull();
        registro.Depois.Should().Be("d");
    }

    [Fact]
    public void VersaoNoLimite_AtribuiOCampoNovo()
    {
        var registro = Ler(versao: 12);

        registro.Antes.Should().Be("a");
        registro.Novo.Should().Be("n");
        registro.Depois.Should().Be("d");
    }
}
```

`ParseLinha` público não recebe versão (passa `versaoLeiaute: 0`). Criar o helper `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/LeitorSpedTxtTestHelper.cs` que alcança `InterpretarLinha` com uma versão explícita. Se `InternalsVisibleTo` para `TecnoFisc.Sped.Txt.Engine.Tests` ainda não existir em `src/TecnoFisc.Sped.Txt.Engine/TecnoFisc.Sped.Txt.Engine.csproj`, adicionar; e marcar `InterpretarLinha` como `internal` em vez de `private`. Alternativa aceitável, se a exposição incomodar: montar um arquivo SPED completo em memória com um `0000` sintético que declare a versão e usar `ReadStreamingAsync`. Escolher a alternativa que não amplie a superfície pública.

- [ ] **Step 3: Rodar e confirmar a falha**

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~VigenciaCampoPosicionalTests"
```

Esperado: FAIL em `ColunaBarradaPresenteNoArquivo_NaoDeslocaAsSeguintes` — o cursor atual grava `"n"` em `Depois` e deixa a coluna `"d"` sem destino.

- [ ] **Step 4: Restaurar o mapeamento posicional**

No bloco `else if (metadados is not null && registro is not null)`, remover os dois laços `while` do cursor e a variável `indiceCampoMetadados` (declarada no início do método). A guarda de vigência entra na própria condição de entrada, de modo que a coluna é consumida normalmente e apenas o valor é descartado — o alinhamento das colunas seguintes fica preservado:

```csharp
                // A coluna é sempre consumida pela posição; um campo que ainda não vigorava
                // na versão declarada no 0000 simplesmente não recebe o valor.
                int indice = posicaoCampo - 2;
                if (indice < metadados.Campos.Count && CampoAtivo(metadados.Campos[indice], versaoLeiaute))
                {
                    var campo = metadados.Campos[indice];
                    // O corpo existente (CapturaTudo, CampoArquivo, Definir) permanece inalterado,
                    // exceto pelo ramo de campo-arquivo tratado no trecho abaixo.
                }
```

No ramo de campo-arquivo, restaurar também o acesso ao campo seguinte por posição:

```csharp
                        Definir(campo, resto[..idxSep]);
                        if (indice + 1 < metadados.Campos.Count &&
                            CampoAtivo(metadados.Campos[indice + 1], versaoLeiaute))
                        {
                            Definir(metadados.Campos[indice + 1], resto[(idxSep + 1)..]);
                        }
                        break;
```

- [ ] **Step 5: Rodar os testes**

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~VigenciaCampoPosicionalTests"
dotnet test TecnoFisc.Sped.slnx
```

Esperado: 2 testes novos passando; suíte de aceitação do ECF (leiautes 8 a 12) continua verde.

- [ ] **Step 6: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs tests/TecnoFisc.Sped.Txt.Engine.Tests/
git commit -m "fix(txt): voltar ao mapeamento posicional de campo sob vigencia

O cursor sequencial assumia que campo barrado por vigencia estava ausente
do arquivo. Nenhum dos 180 registros ECF tem campo versionado fora do fim,
entao o cursor nao comprava nada e deslocava as colunas seguintes quando a
coluna existia fisicamente."
```

---

### Task 6: Descarte por vigência emite sentinela

Fecha o achado 2.

**Files:**
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs:90-93`
- Create: `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/SentinelaVigenciaTests.cs`

**Interfaces:**
- Produces: quando `RespeitarVigenciaDoLeiaute` está ligado, cada linha descartada por vigência é emitida no stream como `RegistroNaoReconhecido`, com `Erro.Mensagem` iniciando por `"Registro posterior à versão declarada no 0000"`.

- [ ] **Step 1: Escrever o teste que falha**

`tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/SentinelaVigenciaTests.cs`. Montar um arquivo em memória com um `0000` sintético declarando versão antiga, um registro cujo `IntroduzidoEm` seja posterior e um filho desse registro:

```csharp
using System.Text;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Parser;

public sealed class SentinelaVigenciaTests
{
    private static async Task<List<RegistroSped>> LerAsync(string conteudo)
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(typeof(RegistroVigenciaColunaSintetico).Assembly);
        var leitor = new LeitorSpedTxt(catalogo, new ReadingOptions { RespeitarVigenciaDoLeiaute = true });
        using var stream = new MemoryStream(EncodingSped.Latin1.GetBytes(conteudo));

        var lidos = new List<RegistroSped>();
        await foreach (var registro in leitor.ReadStreamingAsync(stream).ConfigureAwait(false))
            lidos.Add(registro);
        return lidos;
    }

    [Fact]
    public async Task RegistroPosteriorAVersaoDeclarada_ViraSentinelaEmVezDeSumir()
    {
        var lidos = await LerAsync(ArquivoSinteticoComRegistroFuturo);

        var sentinela = lidos.OfType<RegistroNaoReconhecido>().Should().ContainSingle().Which;
        sentinela.Codigo.Should().Be("A400");
        sentinela.Erro.Mensagem.Should().StartWith("Registro posterior à versão declarada no 0000");
        sentinela.LinhaCrua.Should().Contain("|A400|");
    }

    [Fact]
    public async Task SubarvoreCortada_TambemViraSentinela()
    {
        var lidos = await LerAsync(ArquivoSinteticoComRegistroFuturoEFilho);

        lidos.OfType<RegistroNaoReconhecido>().Select(r => r.Codigo)
            .Should().BeEquivalentTo(["A400", "A410"]);
    }
}
```

Criar em `_Sintetico` os registros `A400` (com `IntroduzidoEm = 12` no `[RegistroSped]`) e `A410` (nível maior, filho de `A400`), além do `0000` sintético que o assembly de teste já usa para declarar `VersaoLeiaute` — reutilizar o existente se houver; se não houver, criar `Registro0000Sintetico` com `CodVer` mapeado para `VersaoLeiaute`.

As constantes `ArquivoSinteticoComRegistroFuturo` e `ArquivoSinteticoComRegistroFuturoEFilho` são strings literais com as linhas SPED, terminando em `\r\n`, declarando versão `10` no `0000`.

- [ ] **Step 2: Rodar e confirmar a falha**

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~SentinelaVigenciaTests"
```

Esperado: FAIL — nenhuma sentinela é emitida, os registros simplesmente somem.

- [ ] **Step 3: Emitir a sentinela**

Substituir o bloco de descarte silencioso em `ReadStreamingAsync`:

```csharp
                    if (_respeitarVigencia && versaoLeiaute > 0 &&
                        ShouldIgnoreByVersion(metadados, versaoLeiaute, ref nivelCorteVigencia))
                    {
                        // Descarte por vigência nunca é silencioso: o consumidor recebe a linha
                        // crua e o motivo, e decide se filtra ou se trata como erro.
                        string linhaCrua = registroBytes.IsSingleSegment
                            ? EncodingSped.Latin1.GetString(registroBytes.FirstSpan)
                            : EncodingSped.Latin1.GetString(registroBytes.ToArray());
                        string codigo = metadados?.Codigo ?? string.Empty;
                        yield return new RegistroNaoReconhecido(
                            codigo,
                            linhaCrua,
                            new ErroLayout(
                                linhaRegistro,
                                codigo,
                                $"Registro posterior à versão declarada no 0000 ({versaoLeiaute})."));
                        continue;
                    }
```

Conferir o `using` de `TecnoFisc.Sped.Core.Erros` no arquivo; adicionar se faltar.

- [ ] **Step 4: Rodar os testes**

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~SentinelaVigenciaTests"
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~Ecf"
```

Esperado: 2 testes novos passando. **A suíte de aceitação do ECF pode quebrar** se alguma fixture tiver registro barrado por vigência que antes sumia — nesse caso o teste de aceitação precisa passar a contar/filtrar as sentinelas, não a ignorá-las. Ajustar as asserções de aceitação para filtrar `RegistroNaoReconhecido` explicitamente e registrar quantas apareceram.

- [ ] **Step 5: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs tests/
git commit -m "fix(txt): sinalizar registro descartado por vigencia

O descarte deixa de ser mudo: cada linha barrada vira RegistroNaoReconhecido
com a linha crua e o motivo, incluindo a subarvore cortada junto."
```

---

### Task 7: `LenientLayout` deixa de estourar em bloco desconhecido

Fecha o achado 3, nos quatro leiautes.

**Files:**
- Modify: `src/TecnoFisc.Sped.Ecf/ArquivoEcf.cs:60-72`
- Modify: `src/TecnoFisc.Sped.Ecd/ArquivoEcd.cs`
- Modify: `src/TecnoFisc.Sped.EfdIcmsIpi/ArquivoEfdIcmsIpi.cs`
- Modify: `src/TecnoFisc.Sped.EfdContribuicoes/ArquivoEfdContribuicoes.cs`
- Test: `tests/TecnoFisc.Sped.Ecf.Tests/ArquivoEcfLenientTests.cs` e um teste equivalente em cada um dos outros três projetos de teste

**Interfaces:**
- Produces: `IReadOnlyList<RegistroNaoReconhecido> RegistrosNaoReconhecidos { get; }` em cada uma das quatro classes `Arquivo*`. `Adicionar` roteia `RegistroNaoReconhecido` para essa coleção e **continua lançando** `InvalidOperationException` para registro tipado cujo bloco não exista.

- [ ] **Step 1: Escrever o teste que falha (ECF)**

`tests/TecnoFisc.Sped.Ecf.Tests/ArquivoEcfLenientTests.cs`:

```csharp
using System.Text;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests;

public sealed class ArquivoEcfLenientTests
{
    private const string ArquivoComLinhaEstranha =
        "|0000|LECF|0012|...|\r\n|1010|linha de bloco inexistente|\r\n|9999|3|\r\n";

    [Fact]
    public async Task LenientLayout_ColetaBlocoDesconhecidoSemLancar()
    {
        var parser = new ParserEcf(new ReadingOptions { LenientLayout = true });
        using var stream = new MemoryStream(EncodingSped.Latin1.GetBytes(ArquivoComLinhaEstranha));

        var arquivo = await parser.ReadAsync(stream);

        arquivo.RegistrosNaoReconhecidos.Should().ContainSingle()
            .Which.Codigo.Should().Be("1010");
    }

    [Fact]
    public void Adicionar_RegistroTipadoDeBlocoInexistente_ContinuaLancando()
    {
        var arquivo = new ArquivoEcf();

        var acao = () => arquivo.Adicionar(new RegistroBlocoInexistenteSintetico());

        acao.Should().Throw<InvalidOperationException>();
    }
}
```

Ajustar a linha do `0000` para o layout real do registro `0000` da ECF (15 campos) — copiar de uma fixture existente em `tests/TecnoFisc.Sped.Ecf.Tests` e trocar apenas o necessário. `RegistroBlocoInexistenteSintetico` é um `RegistroSped` de teste cujo `Codigo` retorna `"Z999"`.

- [ ] **Step 2: Rodar e confirmar a falha**

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~ArquivoEcfLenientTests"
```

Esperado: erro de compilação (`RegistrosNaoReconhecidos` não existe) e, uma vez declarado, FAIL por `InvalidOperationException` na leitura tolerante.

- [ ] **Step 3: Implementar em `ArquivoEcf`**

Adicionar o campo e a propriedade:

```csharp
    private readonly List<RegistroNaoReconhecido> _naoReconhecidos = [];

    /// <summary>
    /// Registros que o leitor não conseguiu classificar — código desconhecido pelo catálogo ou
    /// descartado por vigência. Só é populado sob <c>LenientLayout</c> ou vigência ligada; sob
    /// leitura estrita o parser já teria abortado antes.
    /// </summary>
    public IReadOnlyList<RegistroNaoReconhecido> RegistrosNaoReconhecidos => _naoReconhecidos;
```

e no início de `Adicionar`, logo após o `ArgumentNullException.ThrowIfNull`:

```csharp
        if (registro is RegistroNaoReconhecido naoReconhecido)
        {
            _naoReconhecidos.Add(naoReconhecido);
            return;
        }
```

O restante do método — incluindo os dois `throw` — permanece como está: registro tipado de bloco inexistente é erro de programação e deve continuar falhando alto.

- [ ] **Step 4: Replicar nos outros três leiautes**

Aplicar exatamente as mesmas três edições em `ArquivoEcd`, `ArquivoEfdIcmsIpi` e `ArquivoEfdContribuicoes`, trocando apenas o nome do leiaute na documentação XML quando houver menção. Criar em cada projeto de teste o par de testes equivalente ao do Step 1, adaptando a linha do `0000` ao layout daquele formato.

- [ ] **Step 5: Rodar os testes**

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~Lenient"
dotnet build TecnoFisc.Sped.slnx -warnaserror
```

Esperado: 8 testes novos passando (2 por leiaute).

- [ ] **Step 6: Commit**

```bash
git add src/TecnoFisc.Sped.Ecf/ArquivoEcf.cs src/TecnoFisc.Sped.Ecd/ArquivoEcd.cs src/TecnoFisc.Sped.EfdIcmsIpi/ArquivoEfdIcmsIpi.cs src/TecnoFisc.Sped.EfdContribuicoes/ArquivoEfdContribuicoes.cs tests/
git commit -m "fix(txt): coletar registro nao reconhecido em vez de lancar

Sob LenientLayout o leitor ja emitia a sentinela, mas Arquivo*.Adicionar
estourava ao rotear pelo primeiro caractere do codigo, anulando a leitura
tolerante nos quatro leiautes. O throw permanece para registro tipado de
bloco inexistente, que e erro de programacao."
```

---

### Task 8: Gerador emite o catálogo mesmo com diagnóstico

Fecha o achado 6.

**Files:**
- Modify: `src/TecnoFisc.Sped.Txt.Engine.SourceGenerators/RegistroSpedCatalogoGenerator.cs:60-92`
- Test: `tests/TecnoFisc.Sped.Txt.Engine.Tests/SourceGenerators/RegistroSpedCatalogoGeneratorDiagnosticoTests.cs`

**Interfaces:**
- Produces: com um alias inválido presente, o gerador reporta exatamente um `TFSPED001` e **ainda assim** emite `CatalogoSpedGerado.g.cs` e `RegistroSpedVisitor.g.cs`. O campo com alias inválido entra no catálogo com o nome CLR da propriedade.

- [ ] **Step 1: Escrever o teste que falha**

```csharp
namespace TecnoFisc.Sped.Txt.Engine.Tests.SourceGenerators;

public sealed class RegistroSpedCatalogoGeneratorDiagnosticoTests
{
    private const string FonteComAliasInvalido = """
        using TecnoFisc.Sped.Txt.Engine.Abstracoes;
        using TecnoFisc.Sped.Txt.Engine.Atributos;

        namespace Exemplo;

        [RegistroSped(Codigo = "B200", Nivel = 2, Bloco = "B")]
        public sealed partial class RegistroB200 : RegistroSped
        {
            public override string Codigo => "B200";

            [CampoSped(Ordem = 2, Nome = "COD VER")]
            public string? CodVer { get; set; }
        }
        """;

    [Fact]
    public void AliasInvalido_ReportaUmDiagnosticoEAindaEmiteOCatalogo()
    {
        var (gerado, diagnosticos) = ExecutarGeradorComDiagnosticos(FonteComAliasInvalido);

        diagnosticos.Should().ContainSingle().Which.Id.Should().Be("TFSPED001");
        gerado.Should().Contain("class CatalogoSpedGerado");
        gerado.Should().Contain("IRegistroSpedVisitor");
    }

    [Fact]
    public void AliasInvalido_CampoEntraNoCatalogoComONomeClr()
    {
        var (gerado, _) = ExecutarGeradorComDiagnosticos(FonteComAliasInvalido);

        gerado.Should().Contain("\"CodVer\"");
        gerado.Should().NotContain("COD VER");
    }
}
```

`ExecutarGeradorComDiagnosticos` é a variante do helper de Task 3 que devolve também os diagnósticos. Se ainda não existir, adicionar ao `GeradorHarness` como `internal static (string Gerado, ImmutableArray<Diagnostic> Diagnosticos) ExecutarGeradorComDiagnosticos(string fonte)`.

- [ ] **Step 2: Rodar e confirmar a falha**

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~RegistroSpedCatalogoGeneratorDiagnosticoTests"
```

Esperado: FAIL — `gerado` vem vazio porque o gerador retorna antes de `AddSource`.

- [ ] **Step 3: Remover o early-return**

Em `RegisterSourceOutput`, apagar as duas linhas

```csharp
            if (hasErrors)
                return;
```

e a variável `hasErrors` (com a atribuição `hasErrors = true;` dentro do laço de diagnósticos), já que não é mais lida. Os `spc.ReportDiagnostic` permanecem — o build continua falhando pelo diagnóstico, que é o comportamento desejado.

Conferir se o caminho de nome de campo já cai no nome CLR quando o alias é inválido; se o `InfoCampo` guardar o alias cru, ajustar a coleta para que campo com `FieldErrorKind.InvalidName` use `property.Name` no `Nome` emitido.

- [ ] **Step 4: Rodar os testes**

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~RegistroSpedCatalogoGenerator"
```

Esperado: 2 testes novos passando, suíte de gerador verde.

- [ ] **Step 5: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine.SourceGenerators/RegistroSpedCatalogoGenerator.cs tests/TecnoFisc.Sped.Txt.Engine.Tests/SourceGenerators/
git commit -m "fix(txt): emitir catalogo mesmo com diagnostico de campo

Um unico TFSPED001 suprimia CatalogoSpedGerado e IRegistroSpedVisitor do
assembly inteiro, e o build falhava com centenas de CS0246 que escondiam a
causa. O diagnostico segue falhando o build, agora sozinho na lista."
```

---

### Task 9: Ordem de enumeração do catálogo vira contrato

Fecha o achado 8.

**Files:**
- Create: `tests/TecnoFisc.Sped.Txt.Engine.Tests/Catalogo/OrdemCatalogoTests.cs`
- Create: `tests/TecnoFisc.Sped.Ecf.Tests/Catalogo/OrdemCatalogoEcfTests.cs`
- Create: `tests/TecnoFisc.Sped.Ecd.Tests/Catalogo/OrdemCatalogoEcdTests.cs`
- Create: `tests/TecnoFisc.Sped.EfdIcmsIpi.Tests/Catalogo/OrdemCatalogoEfdIcmsIpiTests.cs`
- Create: `tests/TecnoFisc.Sped.EfdContribuicoes.Tests/Catalogo/OrdemCatalogoEfdContribuicoesTests.cs`

**Interfaces:**
- Produces: teste que fixa a ordem canônica de `CatalogoSpedGerado.EnumerarRegistros()` — bloco `0` primeiro, depois blocos alfabéticos em ordem ordinal, depois blocos `1`–`8`, e `9` por último; dentro de cada bloco, código em ordem ordinal.

- [ ] **Step 1: Escrever o teste (um por módulo)**

Modelo, a replicar nos quatro módulos trocando o namespace e o tipo do catálogo:

```csharp
using TecnoFisc.Sped.Ecf.Generated;

namespace TecnoFisc.Sped.Ecf.Tests.Catalogo;

public sealed class OrdemCatalogoEcfTests
{
    private static int CategoriaOrdemBloco(string bloco) => bloco switch
    {
        "0" => 0,
        "9" => 3,
        _ when bloco.Length == 1 && bloco[0] >= 'A' && bloco[0] <= 'Z' => 1,
        _ when bloco.Length == 1 && bloco[0] >= '1' && bloco[0] <= '8' => 2,
        _ => 4,
    };

    [Fact]
    public void EnumerarRegistros_SegueAOrdemCanonicaDeBloco()
    {
        var registros = new CatalogoSpedGerado().EnumerarRegistros().ToList();

        var esperado = registros
            .OrderBy(r => CategoriaOrdemBloco(r.Bloco))
            .ThenBy(r => r.Bloco, StringComparer.Ordinal)
            .ThenBy(r => r.Codigo, StringComparer.Ordinal)
            .Select(r => r.Codigo)
            .ToList();

        registros.Select(r => r.Codigo).Should().Equal(esperado);
    }

    [Fact]
    public void EnumerarRegistros_ComecaNoBloco0ETerminaNoBloco9()
    {
        var registros = new CatalogoSpedGerado().EnumerarRegistros().ToList();

        registros[0].Bloco.Should().Be("0");
        registros[^1].Bloco.Should().Be("9");
    }
}
```

Conferir o namespace `Generated` de cada módulo antes de escrever (`git grep -l "namespace .*Generated" src/`).

- [ ] **Step 2: Rodar os testes**

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~OrdemCatalogo"
```

Esperado: PASS nos quatro — a ordenação já está implementada; estes testes a fixam como contrato. Se algum falhar, o `OrdenarRegistros` não está sendo aplicado àquele módulo e isso precisa ser investigado antes de seguir.

- [ ] **Step 3: Commit**

```bash
git add tests/
git commit -m "test(txt): fixar a ordem canonica de enumeracao do catalogo

A ordenacao por bloco introduzida neste PR muda a ordem observavel de
EnumerarRegistros nos quatro modulos. O contrato passa a ter teste em cada
um, em vez de existir apenas como efeito colateral do gerador."
```

---

### Task 10: Nome normativo em todos os campos ECF

Fecha o achado 5.

**Files:**
- Create: `tools/ecf-layout/src/ecf_layout/field_names.py`
- Create: `tools/ecf-layout/tests/test_field_names.py`
- Modify: `src/TecnoFisc.Sped.Ecf/Registros/**/*.cs` (180 arquivos, edição gerada)
- Modify: `tests/TecnoFisc.Sped.Ecf.Tests/Manifesto/AssertRegistroEcf.cs:315-322`

**Interfaces:**
- Consumes: `sped/ecf/layout-12-manifest.json`, campo `fields[].name` de cada registro.
- Produces: todo `[CampoSped]` dos 180 registros ECF declara `Nome = "<NOME_NORMATIVO>"`. `AssertRegistroEcf` compara nome de campo por igualdade ordinal exata, sem canonicalizar.

- [ ] **Step 1: Endurecer o harness primeiro (o teste que falha)**

Em `AssertRegistroEcf`, na comparação de nome de campo, trocar

```csharp
            CanonicalFieldName(esperado.Name),
            CanonicalFieldName(atual.Nome));
```

por

```csharp
            esperado.Name,
            atual.Nome);
```

`CanonicalFieldName` permanece no arquivo — continua sendo usado na detecção do par NIF/CNPJ (linhas ~360-369).

- [ ] **Step 2: Rodar e confirmar a falha**

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~Manifesto"
```

Esperado: FAIL em massa — a maioria dos campos reporta o nome CLR e o manifesto espera o normativo.

- [ ] **Step 3: Escrever o gerador de aliases**

`tools/ecf-layout/src/ecf_layout/field_names.py`:

```python
"""Aplica o nome normativo do manifesto como alias `Nome` em cada [CampoSped]."""

from __future__ import annotations

import json
import re
from pathlib import Path

_CAMPO = re.compile(
    r"\[CampoSped\((?P<args>[^\]]*?)\)\]\s*\n(?P<indent>\s*)public\s",
    re.MULTILINE,
)
_ORDEM = re.compile(r"\bOrdem\s*=\s*(?P<ordem>\d+)")
_NOME = re.compile(r",?\s*Nome\s*=\s*\"[^\"]*\"")


def nomes_por_ordem(manifesto: Path) -> dict[str, dict[int, str]]:
    """Mapeia código de registro -> {número do campo: nome normativo}."""
    dados = json.loads(manifesto.read_text(encoding="utf-8"))
    return {
        registro["code"]: {
            campo["number"]: campo["name"]
            for campo in registro["fields"]
            if campo["number"] != 1  # REG não recebe atributo
        }
        for registro in dados
    }


def aplicar(fonte: str, nomes: dict[int, str]) -> str:
    """Reescreve os atributos [CampoSped] da fonte com o alias normativo."""

    def substituir(match: re.Match[str]) -> str:
        args = match.group("args")
        ordem_match = _ORDEM.search(args)
        if ordem_match is None:
            return match.group(0)
        ordem = int(ordem_match.group("ordem"))
        nome = nomes.get(ordem)
        if nome is None:
            return match.group(0)
        limpo = _NOME.sub("", args).strip().rstrip(",")
        return f'[CampoSped({limpo}, Nome = "{nome}")]\n{match.group("indent")}public '

    return _CAMPO.sub(substituir, fonte)
```

- [ ] **Step 4: Escrever o teste do gerador**

`tools/ecf-layout/tests/test_field_names.py`:

```python
from ecf_layout.field_names import aplicar


def test_adiciona_alias_quando_nao_existe():
    fonte = (
        "    [CampoSped(Ordem = 4, Tamanho = 19, Decimais = 2, Obrigatorio = true)]\n"
        "    public decimal SdIniLal { get; set; }\n"
    )
    assert 'Nome = "SD_INI_LAL"' in aplicar(fonte, {4: "SD_INI_LAL"})


def test_substitui_alias_existente():
    fonte = (
        '    [CampoSped(Ordem = 5, Tamanho = 1, Nome = "ERRADO")]\n'
        "    public int Campo { get; set; }\n"
    )
    resultado = aplicar(fonte, {5: "IND_SD_INI_LAL"})
    assert 'Nome = "IND_SD_INI_LAL"' in resultado
    assert "ERRADO" not in resultado


def test_preserva_campo_ausente_do_manifesto():
    fonte = "    [CampoSped(Ordem = 9)]\n    public int Campo { get; set; }\n"
    assert aplicar(fonte, {}) == fonte
```

- [ ] **Step 5: Rodar o teste do gerador**

```powershell
uv run --project tools/ecf-layout pytest tools/ecf-layout/tests/test_field_names.py -q
```

Esperado: 3 testes passando.

- [ ] **Step 6: Aplicar aos 180 registros**

Adicionar um subcomando ao `tools/ecf-layout/src/ecf_layout/cli.py` seguindo o padrão dos subcomandos existentes, chamado `field-names`, que percorre `src/TecnoFisc.Sped.Ecf/Registros/**/*.cs`, casa o código do registro pelo nome do arquivo (`RegistroM500.cs` → `M500`) e reescreve o arquivo com `aplicar`. Depois:

```powershell
uv run --project tools/ecf-layout python -m ecf_layout field-names
dotnet build TecnoFisc.Sped.slnx -warnaserror
```

Esperado: build limpo. Se o gerador reportar `TFSPED001` para algum nome, o manifesto tem um nome com caractere fora do permitido — corrigir o manifesto na origem, não o alias.

- [ ] **Step 7: Rodar a suíte completa**

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~Ecf"
dotnet test TecnoFisc.Sped.slnx
```

Esperado: conformidade de manifesto verde nos 180 registros, com comparação exata.

- [ ] **Step 8: Commit**

```bash
git add tools/ecf-layout src/TecnoFisc.Sped.Ecf/Registros tests/TecnoFisc.Sped.Ecf.Tests/Manifesto/AssertRegistroEcf.cs
git commit -m "fix(ecf): expor o nome normativo em todos os campos do catalogo

O alias Nome estava aplicado a 54 dos 180 registros de forma irregular, e o
harness nao detectava porque canonicalizava antes de comparar. Os aliases
passam a ser gerados do manifesto e a comparacao passa a ser exata."
```

---

### Task 11: Unificar os enums contábeis duplicados (breaking)

Fecha o achado 10. **Esta é a única mudança breaking do PR.**

**Files:**
- Create: `src/TecnoFisc.Sped.Txt.Engine/Enums/IndicadorDebitoCredito.cs`
- Create: `src/TecnoFisc.Sped.Txt.Engine/Enums/IndicadorTipoConta.cs`
- Delete: `src/TecnoFisc.Sped.Ecd/Enums/IndicadorDebitoCredito.cs`, `src/TecnoFisc.Sped.Ecd/Enums/IndicadorTipoConta.cs`
- Delete: `src/TecnoFisc.Sped.Ecf/Enums/IndicadorDebitoCredito.cs`, `src/TecnoFisc.Sped.Ecf/Enums/IndicadorTipoConta.cs`
- Modify: 14 registros ECD, 31 registros ECF e os 50 arquivos de teste que os referenciam (lista completa via `git grep -ln "IndicadorTipoConta\|IndicadorDebitoCredito" -- src tests`)

**Interfaces:**
- Produces: `TecnoFisc.Sped.Txt.Engine.Enums.IndicadorDebitoCredito` e `TecnoFisc.Sped.Txt.Engine.Enums.IndicadorTipoConta` como definição única. Os namespaces `TecnoFisc.Sped.Ecd.Enums` e `TecnoFisc.Sped.Ecf.Enums` deixam de conter esses dois tipos.

- [ ] **Step 1: Confirmar que as duas cópias são idênticas**

```powershell
git diff --no-index src/TecnoFisc.Sped.Ecd/Enums/IndicadorDebitoCredito.cs src/TecnoFisc.Sped.Ecf/Enums/IndicadorDebitoCredito.cs
git diff --no-index src/TecnoFisc.Sped.Ecd/Enums/IndicadorTipoConta.cs src/TecnoFisc.Sped.Ecf/Enums/IndicadorTipoConta.cs
```

Esperado: diferença apenas na linha `namespace`. Se houver divergência de membro ou de `[SpedValor]`, **parar e reportar** — o drift já aconteceu e a decisão de qual versão é canônica precisa ser tomada antes de unificar.

- [ ] **Step 2: Criar os tipos canônicos**

`src/TecnoFisc.Sped.Txt.Engine/Enums/IndicadorDebitoCredito.cs`:

```csharp
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Txt.Engine.Enums;

/// <summary>
/// Indicador da situação de um saldo contábil. Compartilhado por ECD e ECF: mesma semântica,
/// mesmos tokens SPED. Não é regido pelo Ato COTEPE — é convenção contábil transversal.
/// </summary>
public enum IndicadorDebitoCredito
{
    /// <summary>D - saldo devedor.</summary>
    [SpedValor("D")]
    Devedor = 0,

    /// <summary>C - saldo credor.</summary>
    [SpedValor("C")]
    Credor = 1,
}
```

`IndicadorTipoConta.cs`: copiar o conteúdo verbatim de `src/TecnoFisc.Sped.Ecd/Enums/IndicadorTipoConta.cs`, trocando apenas o `namespace` para `TecnoFisc.Sped.Txt.Engine.Enums` e acrescentando à documentação XML a nota de que é compartilhado por ECD e ECF.

- [ ] **Step 3: Migrar os consumidores**

Apagar os quatro arquivos duplicados e ajustar os `using` de cada consumidor. Nos registros ECD e ECF os arquivos já importam `TecnoFisc.Sped.Ecd.Enums` / `TecnoFisc.Sped.Ecf.Enums` para outros tipos, então basta **acrescentar** `using TecnoFisc.Sped.Txt.Engine.Enums;` — não remover o using existente.

```powershell
git rm src/TecnoFisc.Sped.Ecd/Enums/IndicadorDebitoCredito.cs src/TecnoFisc.Sped.Ecd/Enums/IndicadorTipoConta.cs src/TecnoFisc.Sped.Ecf/Enums/IndicadorDebitoCredito.cs src/TecnoFisc.Sped.Ecf/Enums/IndicadorTipoConta.cs
dotnet build TecnoFisc.Sped.slnx -warnaserror
```

O build aponta cada arquivo que precisa do `using` novo. Corrigir um a um até o build limpar. Se algum arquivo passar a ter ambiguidade (`using` de dois namespaces com o mesmo tipo), é sinal de cópia remanescente — investigar antes de qualificar o tipo por extenso.

- [ ] **Step 4: Rodar a suíte completa**

```powershell
dotnet build TecnoFisc.Sped.slnx -warnaserror
dotnet test TecnoFisc.Sped.slnx
```

Esperado: build limpo e suíte verde. Nenhuma mudança de comportamento — só de namespace.

- [ ] **Step 5: Commit**

```bash
git add -A src tests
git commit -m "feat(txt)!: unificar enums contabeis compartilhados por ECD e ECF

IndicadorDebitoCredito e IndicadorTipoConta eram copias byte a byte em
TecnoFisc.Sped.Ecd.Enums e TecnoFisc.Sped.Ecf.Enums. Passam a viver em
TecnoFisc.Sped.Txt.Engine.Enums, ao lado de IndicadorSimNao e
CodigoNaturezaContaContabil.

BREAKING CHANGE: TecnoFisc.Sped.Ecd.Enums.IndicadorDebitoCredito e
TecnoFisc.Sped.Ecd.Enums.IndicadorTipoConta mudaram de namespace para
TecnoFisc.Sped.Txt.Engine.Enums. Consumidores precisam trocar
'using TecnoFisc.Sped.Ecd.Enums;' por
'using TecnoFisc.Sped.Txt.Engine.Enums;' nos pontos que usam esses tipos."
```

---

### Task 12: Benchmarks das mudanças em hot path

Fecha o achado 9 e a regra 5 do `CLAUDE.md`.

**Files:**
- Create: `benchmarks/TecnoFisc.Sped.Benchmarks/ParserVigenciaBenchmark.cs`
- Modify: `benchmarks/TecnoFisc.Sped.Benchmarks/ParserCatalogoBenchmark.cs`

**Interfaces:**
- Consumes: `ReadingOptions.RespeitarVigenciaDoLeiaute`, `ReadingOptions.ValidarDominioDeEnum`.

- [ ] **Step 1: Escrever o benchmark de vigência**

`benchmarks/TecnoFisc.Sped.Benchmarks/ParserVigenciaBenchmark.cs`, espelhando a estrutura de `LenientParsingBenchmark.cs` (mesmos atributos `[MemoryDiagnoser]`, mesma montagem de stream em memória):

```csharp
using BenchmarkDotNet.Attributes;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Benchmarks;

[MemoryDiagnoser]
public class ParserVigenciaBenchmark
{
    private byte[] _arquivo = null!;

    [GlobalSetup]
    public void Setup() => _arquivo = MontarArquivoEcf(registros: 10_000);

    [Benchmark(Baseline = true)]
    public async Task<int> SemVigencia()
        => await ContarAsync(new ReadingOptions { RespeitarVigenciaDoLeiaute = false });

    [Benchmark]
    public async Task<int> ComVigencia()
        => await ContarAsync(new ReadingOptions { RespeitarVigenciaDoLeiaute = true });
}
```

`MontarArquivoEcf` e `ContarAsync` seguem o padrão dos helpers privados já usados em `LenientParsingBenchmark.cs` — reaproveitar a forma existente em vez de inventar outra.

- [ ] **Step 2: Estender o benchmark de catálogo**

Adicionar a `ParserCatalogoBenchmark` um par de métodos `ComValidacaoDeDominio` / `SemValidacaoDeDominio` sobre o mesmo arquivo de entrada, com `[Benchmark(Baseline = true)]` no caminho desligado.

- [ ] **Step 3: Rodar os benchmarks**

```powershell
dotnet run -c Release --project benchmarks/TecnoFisc.Sped.Benchmarks -- --filter "*ParserVigenciaBenchmark*"
dotnet run -c Release --project benchmarks/TecnoFisc.Sped.Benchmarks -- --filter "*ParserCatalogoBenchmark*"
```

Esperado: o caminho desligado dentro do ruído do baseline (ratio ≤ 1,02). Se `SemVigencia` regredir de forma mensurável em relação a `origin/dev`, **parar e reportar** — a regra 5 diz que regressão bloqueia merge.

- [ ] **Step 4: Commit**

```bash
git add benchmarks/
git commit -m "test(txt): cobrir vigencia e validacao de dominio com benchmark

Regra 5 do CLAUDE.md: mudanca em hot path exige BenchmarkDotNet. Cobre o
gate por registro, o CampoAtivo por campo e o custo do caminho desligado."
```

---

### Task 13: Documentação e metadados de release

Fecha os itens de comunicação do spec.

**Files:**
- Modify: `CHANGELOG.md`
- Modify: `README.md`
- Modify: descrição e título do PR 531 (via `gh`)

- [ ] **Step 1: Atualizar o `CHANGELOG.md`**

Adicionar na seção não lançada, sob `TecnoFisc.Sped.Txt.Engine`:

```markdown
### Alterado

- `ReadingOptions.RespeitarVigenciaDoLeiaute` e a nova `ReadingOptions.ValidarDominioDeEnum` são `bool?`: `null` delega a decisão ao parser do leiaute. O ECF liga as duas; EFD Contribuições, EFD ICMS-IPI e ECD mantêm o comportamento anterior.
- `Arquivo*.Adicionar` passa a coletar `RegistroNaoReconhecido` em `RegistrosNaoReconhecidos` em vez de lançar, nos quatro leiautes. Registro tipado de bloco inexistente continua lançando.
- A ordem de enumeração de `CatalogoSpedGerado.EnumerarRegistros()` passa a ser a ordem canônica de bloco (`0`, blocos alfabéticos, blocos `1`–`8`, `9`) em todos os módulos. Quem dependia da ordem puramente lexicográfica do código precisa reordenar.

### Corrigido

- Registro descartado por vigência do leiaute deixa de sumir em silêncio: passa a ser emitido como `RegistroNaoReconhecido` com a linha crua e o motivo.
- Campo barrado por vigência não desloca mais as colunas seguintes do registro.
- Um diagnóstico `TFSPED001`/`TFSPED002` deixa de suprimir a emissão do catálogo, o que soterrava a causa sob uma cascata de `CS0246`.

### Quebrado

- `TecnoFisc.Sped.Ecd.Enums.IndicadorDebitoCredito` e `TecnoFisc.Sped.Ecd.Enums.IndicadorTipoConta` mudaram para `TecnoFisc.Sped.Txt.Engine.Enums`. Troque o `using` nos pontos que usam esses tipos.
```

- [ ] **Step 2: Documentar as flags no `README.md`**

Na seção de opções de leitura, acrescentar as duas flags com uma frase cada, no mesmo formato das existentes.

- [ ] **Step 3: Atualizar título e corpo do PR**

```powershell
gh pr edit 531 --title "feat(ecf)!: add complete read-only layouts 8 through 12"
```

Acrescentar ao corpo do PR uma seção `## Breaking changes` com o texto do rodapé `BREAKING CHANGE:` de Task 11, e uma seção `## Correções do review` listando os dez achados e a task que fechou cada um.

- [ ] **Step 4: Verificação final**

```powershell
dotnet build TecnoFisc.Sped.slnx -warnaserror
dotnet test TecnoFisc.Sped.slnx
dotnet pack TecnoFisc.Sped.slnx -c Release --no-restore
uv run --project tools/ecf-layout pytest tools/ecf-layout/tests -q
```

Esperado: tudo verde. Registrar os números no corpo do PR, no mesmo formato da seção `## Validação` já existente.

- [ ] **Step 5: Commit**

```bash
git add CHANGELOG.md README.md
git commit -m "docs: registrar as mudancas de comportamento do TXT Engine

Documenta as duas flags novas de ReadingOptions, a mudanca de ordem de
enumeracao do catalogo e a quebra de namespace dos enums contabeis."
```

---

## Mapa achado → task

| Achado | Task |
|---|---|
| 1 — `Enum.IsDefined` quebra leitura publicada | 1, 2, 3, 4 |
| 2 — vigência forçada e descarte silencioso | 1, 6 |
| 3 — `Adicionar` anula `LenientLayout` | 7 |
| 4 — cursor desloca colunas | 5 |
| 5 — nome de campo misturado | 10 |
| 6 — early-return do gerador | 8 |
| 7 — remoção de `Enum.Parse` | 2 |
| 8 — ordem do catálogo | 9, 13 |
| 9 — hot path sem benchmark | 12 |
| 10 — enums duplicados | 11 |
