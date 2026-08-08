# Achados do review de follow-up do PR 531 — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Tratar os dez achados do review de follow-up do PR 531 e os três itens parked, de modo que a ECF leia os leiautes 8–12 e os futuros sem abortar e sem perder dado em silêncio.

**Architecture:** Um registro sem campos modelados é só um registro com zero `[CampoSped]` — é assim que os sete códigos removidos no leiaute 11 voltam ao catálogo sem exigir mecanismo de vigência novo. `Registro0000.VersaoLeiaute` passa a parsear `COD_VER` numericamente, de modo que o gate de vigência existente trate leiaute futuro igual a leiaute passado; fora da faixa conhecida (`IsLeiauteConhecido`) o leitor degrada para diagnóstico em vez de exceção. O que a linha traz e o modelo não representa passa a sair por `ColunasNaoModeladas`.

**Tech Stack:** .NET 10, C# file-scoped namespaces, xUnit + FluentAssertions, BenchmarkDotNet, source generator referenciado como analyzer.

## Global Constraints

- Solução: `TecnoFisc.Sped.slnx`. Build: `dotnet build TecnoFisc.Sped.slnx`. Testes: `dotnet test TecnoFisc.Sped.slnx`.
- Worktree de trabalho: `G:\repos\TecnoFisc.Sped\.worktrees\ecf-layout-12`, branch `feat/ecf-layout-12`.
- Spec: `docs/superpowers/specs/2026-08-08-pr531-achados-followup-design.md`.
- Zero dependências externas em runtime. Streams in, streams out.
- Projetos de leiaute nunca referenciam uns aos outros; o que é transversal vive em `TecnoFisc.Sped.Core` ou `TecnoFisc.Sped.Txt.Engine`.
- Duplicação é correta **no nível do registro** (CLAUDE.md hard rule 2): cada registro é uma classe própria por design, mesmo quando várias ficam idênticas. Não fatorar registros numa base comum nem gerar classes de registro por macro/template.
- Sem reflection em hot path de parsing.
- Nomenclatura: substantivos SPED em português; verbos, factories e predicados booleanos em inglês (`IsLeiauteConhecido`, `ShouldIgnore`, `Create`). Nunca misturar os dois idiomas dentro de um mesmo identificador, exceto no padrão `Is`+substantivo já consagrado no repo (`Cfop.IsEntrada`).
- Encoding dos `.txt` SPED: `Latin1`.
- Todo commit é Conventional Commit válido; prefixo em inglês, corpo em português.
- Um registro só pode ter campo versionado no fim (`ValidarVigenciaCrescente`).

---

## PR A — bloqueadores (dentro do PR #531)

### Task 1: RegistroX300 — o primeiro dos sete removidos

Esta task é também a verificação do risco 1 da spec: se o gerador ou o `CatalogoBuilder` recusarem uma classe `[RegistroSped]` sem nenhum `[CampoSped]`, descobrimos aqui, antes de escrever as outras seis.

**Files:**
- Create: `src/TecnoFisc.Sped.Ecf/Registros/BlocoX/RegistroX300.cs`
- Test: `tests/TecnoFisc.Sped.Ecf.Tests/Versionamento/RegistrosRemovidosLeiaute11Tests.cs`

**Interfaces:**
- Consumes: `RegistroSped`, `RegistroSpedAttribute`, `DescontinuadoAttribute`, `LayoutEcf`, `CatalogoSpedGerado`.
- Produces: `RegistroX300`, e o padrão de classe que as Tasks 2 repetem.

- [ ] **Step 1: Obter o `Nivel` dos sete registros**

Abrir `sped/guides/ecf-layout-10/Manual_ECF_Leiaute_10_21_12_2023.pdf` e localizar o bloco X (região contígua, perto do fim do manual — o PDF tem 672 páginas; a ordem dos blocos é 0, C, E, J, K, L, M, N, P, Q, T, U, V, W, X, Y, 9). Anotar o `Nivel` de X291, X300, X305, X310, X320, X325 e X330.

Se a busca não render em ~15 minutos, usar o fallback declarado na spec: inferir do vizinho já modelado. `RegistroX292` é `Nivel = 2` (`src/TecnoFisc.Sped.Ecf/Registros/BlocoX/RegistroX292.cs:7`), e X280 idem — então X291 e os X3xx recebem `Nivel = 2`. Ao usar o fallback, escrever no XML doc da classe: `Nivel inferido do vizinho X292; validar contra o manual do leiaute 10.`

- [ ] **Step 2: Escrever o teste que falha**

Criar `tests/TecnoFisc.Sped.Ecf.Tests/Versionamento/RegistrosRemovidosLeiaute11Tests.cs`:

```csharp
using System.Text;

using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Ecf.Tests.Versionamento;

public sealed class RegistrosRemovidosLeiaute11Tests
{
    [Fact]
    public void Catalogo_ConheceX300ComoDescontinuadoNoLeiaute11()
    {
        new CatalogoSpedGerado().TentarObter("X300", out var metadados).Should().BeTrue();

        metadados!.Bloco.Should().Be("X");
        metadados.DescontinuadoEm.Should().Be((int)LayoutEcf.V011);
        metadados.IntroduzidoEm.Should().Be(0);
        metadados.Campos.Should().BeEmpty();
    }

    [Fact]
    public async Task Leitura_DeLeiaute10ComX300_NaoAborta()
    {
        var registros = await ReadAsync(10, "|X300|000001|EXPORTACAO|1234,56|");

        registros.Should().ContainSingle(registro => registro.Codigo == "X300");
        registros.OfType<RegistroNaoReconhecido>().Should().BeEmpty();
    }

    internal static async Task<List<RegistroSped>> ReadAsync(int versao, string linha)
    {
        string arquivo =
            $"|0000|LECF|{versao:0000}|11111111000191|EMPRESA TESTE|0|0|||01012025|31122025|N||0||\r\n" +
            linha + "\r\n" +
            "|9999|3|\r\n";
        var registros = new List<RegistroSped>();
        await using var stream = new MemoryStream(Encoding.Latin1.GetBytes(arquivo));
        await foreach (var registro in new ParserEcf().ReadStreamingAsync(stream))
            registros.Add(registro);
        return registros;
    }
}
```

- [ ] **Step 3: Rodar o teste e confirmar que falha**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~RegistrosRemovidosLeiaute11Tests"`
Expected: FAIL — `TentarObter` devolve `false`, e a leitura lança `ErroLayoutSpedException: Código de registro desconhecido pelo catálogo.`

- [ ] **Step 4: Criar a classe**

`src/TecnoFisc.Sped.Ecf/Registros/BlocoX/RegistroX300.cs`, com o `Nivel` obtido no Step 1:

```csharp
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>
/// Registro X300 — existiu nos leiautes 8 a 10 e foi removido no leiaute 11.
/// Reconhecido pelo catálogo para que arquivos históricos sejam legíveis, mas sem campos
/// modelados: o conteúdo das colunas sai por <c>ColunasNaoModeladas</c>. Modelar os campos
/// exige extraí-los do manual do leiaute 10 e é evolução planejada, puramente aditiva.
/// </summary>
[RegistroSped(Codigo = "X300", Nivel = 2, Bloco = "X")]
[Descontinuado(EmVersao = (int)LayoutEcf.V011)]
public sealed partial class RegistroX300 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "X300";
}
```

- [ ] **Step 5: Rodar o teste e confirmar que passa**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~RegistrosRemovidosLeiaute11Tests"`
Expected: PASS.

Se falhar no **build** com diagnóstico do gerador exigindo ao menos um campo, o risco 1 da spec se materializou. Neste caso: declarar um único campo técnico posicional em vez de nenhum —

```csharp
    /// <summary>Primeira coluna do registro, preservada em bruto. Os demais campos não são modelados.</summary>
    [CampoSped(Ordem = 2, Nome = "CAMPO_02")]
    public string? Campo02 { get; set; }
```

— ajustar a expectativa `metadados.Campos.Should().BeEmpty()` para `.Should().HaveCount(1)`, e registrar o desvio num comentário na classe. Não inventar terceira solução sem consultar.

- [ ] **Step 6: Rodar a suíte inteira do ECF**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~TecnoFisc.Sped.Ecf.Tests"`
Expected: falham exatamente dois testes já conhecidos — `CatalogoAtual_NaoReintroduzRegistrosRemovidosNoLeiaute11` e os de contagem do manifesto. A Task 2 os corrige. Nenhuma outra falha.

- [ ] **Step 7: Commit**

```bash
git add src/TecnoFisc.Sped.Ecf/Registros/BlocoX/RegistroX300.cs tests/TecnoFisc.Sped.Ecf.Tests/Versionamento/RegistrosRemovidosLeiaute11Tests.cs
git commit -m "feat(ecf): reconhecer X300 removido no leiaute 11 sem modelar campos"
```

---

### Task 2: Os outros seis removidos e os testes que fixavam o oposto

**Files:**
- Create: `src/TecnoFisc.Sped.Ecf/Registros/BlocoX/RegistroX291.cs`, `RegistroX305.cs`, `RegistroX310.cs`, `RegistroX320.cs`, `RegistroX325.cs`, `RegistroX330.cs`
- Modify: `tests/TecnoFisc.Sped.Ecf.Tests/Versionamento/CompatibilidadeLayoutEcfTests.cs:165-174`
- Modify: `tests/TecnoFisc.Sped.Ecf.Tests/Manifesto/ManifestoCatalogoTests.cs`
- Test: `tests/TecnoFisc.Sped.Ecf.Tests/Versionamento/RegistrosRemovidosLeiaute11Tests.cs`

**Interfaces:**
- Consumes: o padrão de classe da Task 1.
- Produces: catálogo ECF com 187 registros; `RegistrosRemovidosLeiaute11Tests.CodigosRemovidos`.

- [ ] **Step 1: Escrever o teste que falha**

Acrescentar a `RegistrosRemovidosLeiaute11Tests`:

```csharp
    internal static readonly string[] CodigosRemovidos =
        ["X291", "X300", "X305", "X310", "X320", "X325", "X330"];

    [Fact]
    public void Catalogo_ConheceOsSeteRemovidosENenhumTemCampoModelado()
    {
        var catalogo = new CatalogoSpedGerado();

        foreach (var codigo in CodigosRemovidos)
        {
            catalogo.TentarObter(codigo, out var metadados).Should().BeTrue($"{codigo} precisa ser reconhecido");
            metadados!.DescontinuadoEm.Should().Be((int)LayoutEcf.V011, $"{codigo} saiu no leiaute 11");
            metadados.Bloco.Should().Be("X");
        }
    }

    [Fact]
    public void Catalogo_TemOsCentoEOitentaDoLeiaute12MaisOsSeteRemovidos()
        => new CatalogoSpedGerado().EnumerarRegistros().Should().HaveCount(187);
```

- [ ] **Step 2: Rodar e confirmar que falha**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~RegistrosRemovidosLeiaute11Tests"`
Expected: FAIL — seis códigos ausentes; contagem 181.

- [ ] **Step 3: Criar as seis classes**

Uma por arquivo, idênticas à `RegistroX300` da Task 1 exceto pelo código e pelo `Nivel` obtido no manual. Exemplo para X291 (repetir o molde para X305, X310, X320, X325, X330):

```csharp
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>
/// Registro X291 — existiu nos leiautes 8 a 10 e foi removido no leiaute 11.
/// Reconhecido pelo catálogo para que arquivos históricos sejam legíveis, mas sem campos
/// modelados: o conteúdo das colunas sai por <c>ColunasNaoModeladas</c>.
/// </summary>
[RegistroSped(Codigo = "X291", Nivel = 2, Bloco = "X")]
[Descontinuado(EmVersao = (int)LayoutEcf.V011)]
public sealed partial class RegistroX291 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "X291";
}
```

- [ ] **Step 4: Inverter o teste que fixava a ausência**

Em `tests/TecnoFisc.Sped.Ecf.Tests/Versionamento/CompatibilidadeLayoutEcfTests.cs`, substituir o método das linhas 165-174 por:

```csharp
    [Fact]
    public void CatalogoAtual_ReconheceRegistrosRemovidosNoLeiaute11SemModelarCampos()
    {
        string[] removidos = ["X291", "X300", "X305", "X310", "X320", "X325", "X330"];
        var catalogo = new CatalogoSpedGerado();

        foreach (var codigo in removidos)
        {
            catalogo.TentarObter(codigo, out var metadados).Should().BeTrue();
            metadados!.DescontinuadoEm.Should().Be(11);
        }
    }
```

- [ ] **Step 5: Ajustar a comparação catálogo × manifesto**

O manifesto descreve o **leiaute 12**, onde esses sete não existem — ele continua com 180 registros. Em `tests/TecnoFisc.Sped.Ecf.Tests/Manifesto/ManifestoCatalogoTests.cs`, toda asserção que hoje exige igualdade entre o conjunto de códigos do catálogo e o do manifesto passa a excluir exatamente os sete, e nada além deles. Onde houver comparação de conjuntos, filtrar o catálogo antes:

```csharp
        var codigosCatalogo = catalogo.EnumerarRegistros()
            .Where(registro => registro.DescontinuadoEm == 0)
            .Select(registro => registro.Codigo);
```

E acrescentar uma asserção que trava o inverso, para que ninguém use `DescontinuadoEm` como porta dos fundos para divergir do manifesto:

```csharp
    [Fact]
    public void Catalogo_SoDivergeDoManifestoNosSeteRemovidosNoLeiaute11()
    {
        var manifesto = ManifestoEcf.Carregar().CodigosCanonicos.ToHashSet(StringComparer.Ordinal);
        var catalogo = new CatalogoSpedGerado().EnumerarRegistros()
            .Select(registro => registro.Codigo).ToHashSet(StringComparer.Ordinal);

        catalogo.Except(manifesto).Should().BeEquivalentTo(
            ["X291", "X300", "X305", "X310", "X320", "X325", "X330"]);
        manifesto.Except(catalogo).Should().BeEmpty();
    }
```

- [ ] **Step 6: Rodar a suíte do ECF inteira**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~TecnoFisc.Sped.Ecf.Tests"`
Expected: PASS, sem exceção.

- [ ] **Step 7: Commit**

```bash
git add src/TecnoFisc.Sped.Ecf/Registros/BlocoX tests/TecnoFisc.Sped.Ecf.Tests
git commit -m "feat(ecf): reconhecer os sete registros removidos no leiaute 11"
```

---

### Task 3: `VersaoLeiaute` parseia COD_VER numericamente

**Files:**
- Modify: `src/TecnoFisc.Sped.Ecf/Registros/Bloco0/Registro0000.cs:17-34`
- Test: `tests/TecnoFisc.Sped.Ecf.Tests/Registros/Bloco0/Registro0000Tests.cs`

**Interfaces:**
- Produces: `Registro0000.VersaoLeiaute` devolvendo o número declarado em `COD_VER`, e `0` apenas quando `COD_VER` não é numérico.

- [ ] **Step 1: Escrever o teste que falha**

Acrescentar a `Registro0000Tests`. Atenção: já existe um `[InlineData("0007", 0)]` no arquivo fixando o comportamento antigo — ele precisa sair junto, na Step 3.

```csharp
    [Theory]
    [InlineData("0008", 8)]
    [InlineData("0012", 12)]
    [InlineData("0013", 13)]
    [InlineData("0007", 7)]
    [InlineData("0100", 100)]
    public void VersaoLeiaute_ParseiaCodVerNumericamente(string codVer, int esperado)
        => new Registro0000 { CodVer = codVer }.VersaoLeiaute.Should().Be(esperado);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ABCD")]
    [InlineData("00 1")]
    public void VersaoLeiaute_EhZeroQuandoCodVerNaoEhNumerico(string? codVer)
        => new Registro0000 { CodVer = codVer }.VersaoLeiaute.Should().Be(0);
```

- [ ] **Step 2: Rodar e confirmar que falha**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~Registro0000Tests"`
Expected: FAIL nos casos `0013`, `0007` e `0100` (todos devolvem 0 hoje).

- [ ] **Step 3: Implementar**

Substituir o switch de `src/TecnoFisc.Sped.Ecf/Registros/Bloco0/Registro0000.cs:17-26` por:

```csharp
    /// <summary>
    /// Versão declarada em <see cref="CodVer"/>, convertida em número. Devolve o valor mesmo
    /// fora da faixa que a biblioteca conhece (<see cref="LayoutEcf"/>): descartar o número
    /// desligaria o gate de vigência e faria o arquivo ser lido como se fosse leiaute 12. Um
    /// leiaute fora da faixa é sinalizado por <see cref="IsLeiauteConhecido"/>, não por zero.
    /// Zero significa apenas <c>COD_VER</c> não numérico — arquivo inválido, não leiaute novo.
    /// </summary>
    public override int VersaoLeiaute =>
        int.TryParse(CodVer, System.Globalization.NumberStyles.None,
                     System.Globalization.CultureInfo.InvariantCulture, out int versao)
            ? versao
            : 0;
```

E atualizar o XML doc de `CodVer` (linha 32), que hoje diz "de `0008` a `0012`":

```csharp
    /// <summary>
    /// Código declarado da versão do leiaute. A biblioteca modela <c>0008</c> a <c>0012</c>;
    /// valores fora dessa faixa são lidos em modo tolerante (ver <see cref="IsLeiauteConhecido"/>).
    /// </summary>
```

Remover do arquivo de teste o `[InlineData("0007", 0)]` preexistente, que agora contradiz o comportamento correto.

- [ ] **Step 4: Rodar e confirmar que passa**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~Registro0000Tests"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/TecnoFisc.Sped.Ecf/Registros/Bloco0/Registro0000.cs tests/TecnoFisc.Sped.Ecf.Tests/Registros/Bloco0/Registro0000Tests.cs
git commit -m "fix(ecf): parsear COD_VER numericamente em vez de zerar fora de 8-12"
```

---

### Task 4: `IsLeiauteConhecido` e o aviso não fatal no 0000

**Files:**
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Abstracoes/RegistroSped.cs`
- Modify: `src/TecnoFisc.Sped.Ecf/Registros/Bloco0/Registro0000.cs`
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs:135-141`
- Test: `tests/TecnoFisc.Sped.Ecf.Tests/Versionamento/LeiauteForaDaFaixaTests.cs`

**Interfaces:**
- Produces: `RegistroSped.IsLeiauteConhecido` (virtual, default `true`); a variável local `leiauteConhecido` em `ReadStreamingAsync`, consumida pelas Tasks 5 e 6.

- [ ] **Step 1: Escrever o teste que falha**

Criar `tests/TecnoFisc.Sped.Ecf.Tests/Versionamento/LeiauteForaDaFaixaTests.cs`:

```csharp
using System.Text;

using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Ecf.Tests.Versionamento;

public sealed class LeiauteForaDaFaixaTests
{
    [Theory]
    [InlineData("0008", true)]
    [InlineData("0012", true)]
    [InlineData("0007", false)]
    [InlineData("0013", false)]
    [InlineData("ABCD", false)]
    public void IsLeiauteConhecido_RefleteAFaixaDoLayoutEcf(string codVer, bool esperado)
        => new Registro0000 { CodVer = codVer }.IsLeiauteConhecido.Should().Be(esperado);

    [Fact]
    public async Task Leitura_DeLeiauteForaDaFaixa_AvisaNoZeroZeroZeroZeroSemAbortar()
    {
        var registros = await ReadAsync(13, "|0001|0|");

        var zero = registros.OfType<Registro0000>().Single();
        zero.ErrosDeFormato.Should().ContainSingle()
            .Which.Mensagem.Should().Contain("fora da faixa");
        registros.Should().Contain(registro => registro.Codigo == "0001");
    }

    [Fact]
    public async Task Leitura_DeLeiauteConhecido_NaoAvisa()
    {
        var registros = await ReadAsync(12, "|0001|0|");

        registros.OfType<Registro0000>().Single().ErrosDeFormato.Should().BeEmpty();
    }

    internal static async Task<List<RegistroSped>> ReadAsync(int versao, string linha)
    {
        string arquivo =
            $"|0000|LECF|{versao:0000}|11111111000191|EMPRESA TESTE|0|0|||01012025|31122025|N||0||\r\n" +
            linha + "\r\n" +
            "|9999|3|\r\n";
        var registros = new List<RegistroSped>();
        await using var stream = new MemoryStream(Encoding.Latin1.GetBytes(arquivo));
        await foreach (var registro in new ParserEcf().ReadStreamingAsync(stream))
            registros.Add(registro);
        return registros;
    }
}
```

- [ ] **Step 2: Rodar e confirmar que falha**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~LeiauteForaDaFaixaTests"`
Expected: FAIL — `IsLeiauteConhecido` não existe (erro de compilação).

- [ ] **Step 3: Acrescentar o membro na base**

Em `src/TecnoFisc.Sped.Txt.Engine/Abstracoes/RegistroSped.cs`, logo após `public virtual int VersaoLeiaute => 0;`:

```csharp
    /// <summary>
    /// Indica se a versão declarada por este registro pertence à faixa de leiautes que o módulo
    /// modela. Só o registro de abertura (<c>0000</c>) de cada módulo tem essa informação; os
    /// demais herdam <c>true</c>, que preserva o comportamento estrito. Quando <c>false</c>, o
    /// leitor degrada para diagnóstico em vez de exceção: um arquivo de leiaute que a biblioteca
    /// ainda não conhece deve ser legível, não fatal.
    /// </summary>
    public virtual bool IsLeiauteConhecido => true;
```

- [ ] **Step 4: Sobrescrever no `Registro0000` do ECF**

Em `src/TecnoFisc.Sped.Ecf/Registros/Bloco0/Registro0000.cs`, após `VersaoLeiaute`:

```csharp
    /// <inheritdoc />
    public override bool IsLeiauteConhecido =>
        VersaoLeiaute >= (int)LayoutEcf.V008 && VersaoLeiaute <= (int)LayoutEcf.V012;
```

- [ ] **Step 5: Emitir o aviso no leitor**

Em `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs`, declarar a variável junto das outras de estado (perto de `int versaoLeiaute = 0;`):

```csharp
        bool leiauteConhecido = true;
```

E substituir o bloco das linhas 138-140 por:

```csharp
                        // Captura a versão do leiaute assim que o Registro0000 é processado.
                        if (versaoLeiaute == 0 && registro.VersaoLeiaute > 0)
                        {
                            versaoLeiaute = registro.VersaoLeiaute;
                            leiauteConhecido = registro.IsLeiauteConhecido;
                            if (!leiauteConhecido)
                                registro.RegistrarErroDeFormato(new ErroFormato(
                                    linhaRegistro, registro.Codigo, "COD_VER",
                                    $"Leiaute {versaoLeiaute} está fora da faixa conhecida por esta " +
                                    "versão da biblioteca; a leitura segue em modo tolerante e campos " +
                                    "podem ter mudado de significado."));
                        }
```

- [ ] **Step 6: Rodar e confirmar que passa**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~LeiauteForaDaFaixaTests"`
Expected: PASS.

- [ ] **Step 7: Rodar a suíte inteira**

Run: `dotnet test TecnoFisc.Sped.slnx`
Expected: PASS. O default `true` preserva ECD, EFD Contribuições e EFD ICMS-IPI.

- [ ] **Step 8: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine/Abstracoes/RegistroSped.cs src/TecnoFisc.Sped.Ecf/Registros/Bloco0/Registro0000.cs src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs tests/TecnoFisc.Sped.Ecf.Tests/Versionamento/LeiauteForaDaFaixaTests.cs
git commit -m "feat(txt): sinalizar leiaute fora da faixa conhecida sem abortar a leitura"
```

---

### Task 5: Código desconhecido degrada para sentinela fora da faixa

**Files:**
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs:135`, `:485-490`, `:518-542`, `:580-591`
- Test: `tests/TecnoFisc.Sped.Ecf.Tests/Versionamento/LeiauteForaDaFaixaTests.cs`

**Interfaces:**
- Consumes: `leiauteConhecido` da Task 4.
- Produces: `ProcessarLinha(..., int versaoLeiaute, bool leiauteConhecido, MetadadosRegistro?)` e `InterpretarLinha(..., int versaoLeiaute, bool leiauteConhecido, ...)`.

- [ ] **Step 1: Escrever o teste que falha**

Acrescentar a `LeiauteForaDaFaixaTests`:

```csharp
    [Fact]
    public async Task Leitura_DeLeiaute13ComRegistroNovo_ViraSentinelaEmVezDeAbortar()
    {
        var registros = await ReadAsync(13, "|X999|conteudo novo|");

        var sentinela = registros.OfType<RegistroNaoReconhecido>().Should().ContainSingle().Subject;
        sentinela.Codigo.Should().Be("X999");
        sentinela.LinhaCrua.Should().Be("|X999|conteudo novo|");
    }

    [Fact]
    public async Task Leitura_DeLeiauteConhecidoComCodigoDesconhecido_ContinuaAbortando()
    {
        var act = async () => await ReadAsync(12, "|X999|conteudo novo|");

        await act.Should().ThrowAsync<ErroLayoutSpedException>();
    }
```

Acrescentar o `using TecnoFisc.Sped.Txt.Engine.Erros;` que o `ErroLayoutSpedException` exige (confirmar o namespace com `grep -rn "class ErroLayoutSpedException" src/`).

- [ ] **Step 2: Rodar e confirmar que falha**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~LeiauteForaDaFaixaTests"`
Expected: FAIL no primeiro teste — hoje lança `ErroLayoutSpedException` também no leiaute 13.

- [ ] **Step 3: Passar `leiauteConhecido` até `InterpretarLinha`**

Acrescentar o parâmetro em `ProcessarLinha` (linha 485) e em `InterpretarLinha` (linha 518), imediatamente depois de `int versaoLeiaute`, e repassar na chamada da linha 135:

```csharp
                    var registro = ProcessarLinha(in registroBytes, linhaRegistro, pilha,
                                                  versaoLeiaute, leiauteConhecido, metadados);
```

Na chamada interna de `ProcessarLinha` para `InterpretarLinha`, repassar o mesmo valor. Em `ParseLinha` (linha 188), passar `leiauteConhecido: true` — a Task 8 revisita esse ponto de entrada.

- [ ] **Step 4: Relaxar o abort**

Em `InterpretarLinha`, substituir a linha 542 e o `if` da linha 584:

```csharp
        // Fora da faixa de leiautes conhecida, um código que o catálogo não tem é evolução
        // esperada do leiaute, não corrupção: degrada para sentinela mesmo em modo estrito.
        bool lenienteLayout = (forcarLenienteLayout ?? _opcoes.LenientLayout) || !leiauteConhecido;
```

O `if (!lenienteLayout) throw ...` da linha 584 fica inalterado — passa a não disparar quando o leiaute é desconhecido.

- [ ] **Step 5: Rodar e confirmar que passa**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~LeiauteForaDaFaixaTests"`
Expected: PASS.

- [ ] **Step 6: Rodar a suíte inteira**

Run: `dotnet test TecnoFisc.Sped.slnx`
Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs tests/TecnoFisc.Sped.Ecf.Tests/Versionamento/LeiauteForaDaFaixaTests.cs
git commit -m "feat(txt): degradar codigo desconhecido para sentinela fora da faixa de leiautes"
```

---

### Task 6: Domínio de enum relaxa fora da faixa

**Files:**
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs:549-553`
- Test: `tests/TecnoFisc.Sped.Ecf.Tests/Parser/ValidacaoDominioEnumEcfTests.cs`

**Interfaces:**
- Consumes: `leiauteConhecido` da Task 5.

- [ ] **Step 1: Escrever o teste que falha**

Acrescentar a `ValidacaoDominioEnumEcfTests` (o helper `ReadAsync` de `LeiauteForaDaFaixaTests` é `internal static` e pode ser reusado dentro do mesmo assembly de teste):

```csharp
    [Fact]
    public async Task DominioDeEnum_ForaDaFaixaDeLeiautes_ViraDiagnosticoEmVezDeExcecao()
    {
        // IND_DAD = "Z" não pertence a IndicadorMovimentoBloco.
        var registros = await LeiauteForaDaFaixaTests.ReadAsync(13, "|0001|Z|");

        var zero0001 = registros.Single(registro => registro.Codigo == "0001");
        zero0001.ErrosDeFormato.Should().ContainSingle();
    }

    [Fact]
    public async Task DominioDeEnum_DentroDaFaixa_ContinuaSendoExcecao()
    {
        var act = async () => await LeiauteForaDaFaixaTests.ReadAsync(12, "|0001|Z|");

        await act.Should().ThrowAsync<ErroFormatoSpedException>();
    }
```

- [ ] **Step 2: Rodar e confirmar que falha**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~ValidacaoDominioEnumEcfTests"`
Expected: FAIL no primeiro — hoje lança nos dois leiautes.

- [ ] **Step 3: Implementar**

Em `InterpretarLinha`, dentro da função local `Definir` (linha 549), trocar a chamada da linha 553:

```csharp
                // Fora da faixa de leiautes conhecida, um valor fora do domínio pode ser
                // código novo da RFB: degrada para diagnóstico, mesma regra do código de
                // registro desconhecido. Dentro da faixa, é dado inválido e continua fatal.
                campo.Definidor(registro!, valor, _validarDominioDeEnum && leiauteConhecido);
```

- [ ] **Step 4: Rodar e confirmar que passa**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~ValidacaoDominioEnumEcfTests"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs tests/TecnoFisc.Sped.Ecf.Tests/Parser/ValidacaoDominioEnumEcfTests.cs
git commit -m "feat(txt): relaxar validacao de dominio de enum fora da faixa de leiautes"
```

---

### Task 7: Cobertura de domínio de enum nos campos reais

**Files:**
- Modify: `tests/TecnoFisc.Sped.Ecf.Tests/Parser/ValidacaoDominioEnumEcfTests.cs:1-20`

**Interfaces:**
- Consumes: `LeiauteForaDaFaixaTests.ReadAsync`.

- [ ] **Step 1: Remover o remark falso**

No topo de `ValidacaoDominioEnumEcfTests.cs`, o remark afirma que todos os enums de `TecnoFisc.Sped.Ecf.Enums` carregam `[SpedValor]` e que portanto nenhum registro real é elegível ao setter estrito. É falso: `IndicadorMovimentoBloco` e `CodigoNaturezaContaContabil` vivem em `TecnoFisc.Sped.Txt.Engine.Enums`, não têm `[SpedValor]`, e são usados por 19 registros `*001` (`IND_DAD`) e por C050/J050 (`COD_NAT`). Substituir por:

```csharp
/// <summary>
/// Validação de domínio de enum no ECF. Os enums elegíveis ao setter estrito não vivem em
/// <c>TecnoFisc.Sped.Ecf.Enums</c> e sim em <c>TecnoFisc.Sped.Txt.Engine.Enums</c>:
/// <c>IndicadorMovimentoBloco</c> (campo <c>IND_DAD</c> dos 19 registros de abertura de bloco) e
/// <c>CodigoNaturezaContaContabil</c> (campo <c>COD_NAT</c> de C050 e J050). Como o ECF usa
/// <c>ValidarDominioDeEnum = true</c> por padrão, esses campos são caminho de produção.
/// </summary>
```

- [ ] **Step 2: Escrever os testes sobre campos reais**

```csharp
    [Fact]
    public async Task IndDad_ForaDoDominio_AbortaNoLeiauteConhecido()
    {
        var act = async () => await LeiauteForaDaFaixaTests.ReadAsync(12, "|0001|Z|");

        await act.Should().ThrowAsync<ErroFormatoSpedException>();
    }

    [Fact]
    public async Task IndDad_DentroDoDominio_EhLido()
    {
        var registros = await LeiauteForaDaFaixaTests.ReadAsync(12, "|0001|0|");

        registros.Should().Contain(registro => registro.Codigo == "0001");
    }

    [Fact]
    public async Task CodNat_ForaDoDominio_AbortaNoLeiauteConhecido()
    {
        // C050 traz COD_NAT; "99" não pertence a CodigoNaturezaContaContabil.
        var act = async () => await LeiauteForaDaFaixaTests.ReadAsync(
            12, "|C001|0|\r\n|C050|01012025|1|99|S|1|CTA|CONTA TESTE|");

        await act.Should().ThrowAsync<ErroFormatoSpedException>();
    }
```

Se a montagem da linha C050 divergir do leiaute (ordem ou quantidade de colunas), conferir os `[CampoSped]` de `src/TecnoFisc.Sped.Ecf/Registros/BlocoC/RegistroC050.cs` e ajustar a linha — o que o teste precisa provar é o `COD_NAT` fora do domínio, não a linha completa.

- [ ] **Step 3: Rodar**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~ValidacaoDominioEnumEcfTests"`
Expected: PASS. Se algum falhar, é achado novo — parar e reportar, não ajustar a asserção para passar.

- [ ] **Step 4: Commit**

```bash
git add tests/TecnoFisc.Sped.Ecf.Tests/Parser/ValidacaoDominioEnumEcfTests.cs
git commit -m "test(ecf): cobrir dominio de enum em IND_DAD e COD_NAT"
```

---

### Task 8: `ParseLinha` deixa de divergir de `ReadAsync`

**Files:**
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs:169-199`
- Modify: `src/TecnoFisc.Sped.Ecf/Parser/ParserEcf.cs:64`
- Test: `tests/TecnoFisc.Sped.Ecf.Tests/Parser/ParserEcfParseLinhaTests.cs`

**Interfaces:**
- Produces: `LeitorSpedTxt.ParseLinha(ReadOnlySpan<char> linha, long numeroLinha = 0, int versaoLeiaute = 0)`; `ParserEcf.ParseLinha(ReadOnlySpan<char> linha, LayoutEcf leiaute, long numeroLinha = 0)`.

- [ ] **Step 1: Escrever o teste que falha**

Criar `tests/TecnoFisc.Sped.Ecf.Tests/Parser/ParserEcfParseLinhaTests.cs`:

```csharp
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;

namespace TecnoFisc.Sped.Ecf.Tests.Parser;

public sealed class ParserEcfParseLinhaTests
{
    private static string Linha0020ComTrintaEDuasColunas()
    {
        var valores = new List<string> { "1", "1" };
        valores.AddRange(Enumerable.Repeat("N", 27));
        valores.Add("S");
        valores.Add("CEBAS-TESTE");
        return "|0020|" + string.Join('|', valores) + "|";
    }

    [Fact]
    public void ParseLinha_ComLeiaute9_NaoPreencheCampoIntroduzidoNo12()
    {
        var resultado = new ParserEcf().ParseLinha(Linha0020ComTrintaEDuasColunas(), LayoutEcf.V009);

        resultado.Sucesso.Should().BeTrue();
        ((Registro0020)resultado.Valor!).Cebas.Should().BeNull();
    }

    [Fact]
    public void ParseLinha_ComLeiaute12_PreencheCampoIntroduzidoNo12()
    {
        var resultado = new ParserEcf().ParseLinha(Linha0020ComTrintaEDuasColunas(), LayoutEcf.V012);

        ((Registro0020)resultado.Valor!).Cebas.Should().Be("CEBAS-TESTE");
    }
}
```

Conferir o nome da propriedade de sucesso de `ResultadoParse<T>` (`grep -n "public.*bool" src/TecnoFisc.Sped.Txt.Engine/Parser/ResultadoParse.cs`) e ajustar `Sucesso` se o nome real for outro.

- [ ] **Step 2: Rodar e confirmar que falha**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~ParserEcfParseLinhaTests"`
Expected: FAIL — a sobrecarga com `LayoutEcf` não existe.

- [ ] **Step 3: Acrescentar o parâmetro no engine**

Em `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs`, trocar a assinatura da linha 176 e a chamada da linha 188:

```csharp
    public ResultadoParse<RegistroSped> ParseLinha(
        ReadOnlySpan<char> linha, long numeroLinha = 0, int versaoLeiaute = 0)
```

```csharp
        var registro = InterpretarLinha(linha, numeroLinha, pilha, versaoLeiaute,
            leiauteConhecido: true, metadadosResolvido: null,
            forcarLenienteCampo: true, forcarLenienteLayout: true);
```

E completar o XML doc das linhas 169-175 com:

```csharp
    /// <para>
    /// <paramref name="versaoLeiaute"/> controla a vigência sintática exatamente como em
    /// <c>ReadStreamingAsync</c>. O default <c>0</c> significa "sem vigência" — todos os campos
    /// do catálogo são aceitos, inclusive os introduzidos em versões posteriores. Informe a
    /// versão para que a validação linha a linha concorde com a leitura do arquivo inteiro.
    /// </para>
```

- [ ] **Step 4: Expor a sobrecarga tipada no ECF**

Em `src/TecnoFisc.Sped.Ecf/Parser/ParserEcf.cs`, manter o método existente e acrescentar:

```csharp
    /// <summary>
    /// Interpreta uma única linha ECF sem construir a hierarquia, aplicando a vigência do
    /// <paramref name="leiaute"/> informado — o mesmo critério que <c>ReadStreamingAsync</c>
    /// aplica a partir do <c>COD_VER</c> do arquivo. A sobrecarga sem <c>leiaute</c> não aplica
    /// vigência nenhuma.
    /// </summary>
    public ResultadoParse<RegistroSped> ParseLinha(
        ReadOnlySpan<char> linha, LayoutEcf leiaute, long numeroLinha = 0)
        => _leitor.ParseLinha(linha, numeroLinha, (int)leiaute);
```

E completar o XML doc da sobrecarga existente (linha 64) dizendo que ela não aplica vigência.

- [ ] **Step 5: Rodar e confirmar que passa**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~ParserEcfParseLinhaTests"`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs src/TecnoFisc.Sped.Ecf/Parser/ParserEcf.cs tests/TecnoFisc.Sped.Ecf.Tests/Parser/ParserEcfParseLinhaTests.cs
git commit -m "feat(txt): aplicar vigencia em ParseLinha via versao informada"
```

---

### Task 9: Remover os aliases do 0020

**Files:**
- Modify: `src/TecnoFisc.Sped.Ecf/Registros/Bloco0/Registro0020.cs:137-152`
- Modify: `tests/TecnoFisc.Sped.Ecf.Tests/Versionamento/CompatibilidadeLayoutEcfTests.cs:128-132`

**Interfaces:**
- Produces: `Registro0020.IndicadorPosicao31` como única porta da posição 31. A Task 14 devolve os aliases, guardados.

- [ ] **Step 1: Ajustar o teste existente**

Em `CompatibilidadeLayoutEcfTests.cs`, remover as duas linhas 130-131 (`registro.IndPrTransf.Should()...` e `registro.PossuiCebras.Should()...`), que passam a não compilar.

- [ ] **Step 2: Remover os aliases e corrigir o rótulo**

Em `src/TecnoFisc.Sped.Ecf/Registros/Bloco0/Registro0020.cs`, apagar as propriedades `IndPrTransf` e `PossuiCebras` e substituir o campo posicional por:

```csharp
    /// <summary>
    /// Valor posicional do campo 31, cuja semântica depende do <c>COD_VER</c> do arquivo:
    /// <c>IND_PR_TRANSF</c> (opção pelas novas regras de preços de transferência) nos leiautes
    /// 10 e 11, e <c>POSSUI_CEBRAS</c> (posse de certificado Cebas) no leiaute 12. Não há como o
    /// registro saber qual das duas é a sua sem a versão do arquivo — consulte o <c>COD_VER</c>
    /// do <c>0000</c> antes de interpretar este valor.
    /// </summary>
    [CampoSped(Ordem = 31, Tamanho = 1, Obrigatorio = true, DesdeVersao = (int)LayoutEcf.V010,
               Nome = "IND_PR_TRANSF/POSSUI_CEBRAS")]
    public IndicadorSimNao IndicadorPosicao31 { get; set; }
```

- [ ] **Step 3: Rodar a suíte do ECF**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~TecnoFisc.Sped.Ecf.Tests"`
Expected: PASS. Se o manifesto comparar `Nome` de campo, o novo rótulo pode quebrar `ManifestoCatalogoTests` — nesse caso atualizar o `layout-12-manifest.json` para o mesmo texto, porque o manifesto descreve o leiaute 12 mas o catálogo é único.

- [ ] **Step 4: Commit**

```bash
git add src/TecnoFisc.Sped.Ecf/Registros/Bloco0/Registro0020.cs tests/TecnoFisc.Sped.Ecf.Tests/Versionamento/CompatibilidadeLayoutEcfTests.cs
git commit -m "fix(ecf)!: remover aliases do campo 31 do 0020 que trocavam de semantica"
```

---

### Task 10: Mensagem de `ValidarVigenciaCrescente`

**Files:**
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Catalogo/CatalogoBuilder.cs:291-304`
- Test: `tests/TecnoFisc.Sped.Txt.Engine.Tests/Catalogo/CatalogoBuilderTests.cs`

- [ ] **Step 1: Escrever o teste que falha**

A mensagem atual não diz a **posição** do campo, que é o dado necessário para corrigir. Acrescentar aos testes do `CatalogoBuilder` (localizar a classe de teste existente com `grep -rln "ValidarVigenciaCrescente\|DesdeVersao precisa ser" tests/`):

```csharp
    private sealed class RegistroComVigenciaDecrescente : RegistroSped
    {
        public override string Codigo => "TST1";

        [CampoSped(Ordem = 2, DesdeVersao = 12, Nome = "TARDIO")]
        public string? Tardio { get; set; }

        [CampoSped(Ordem = 3, Nome = "SEMPRE")]
        public string? Sempre { get; set; }
    }

    [Fact]
    public void ValidarVigenciaCrescente_NomeiaRegistroCampoEPosicao()
    {
        var act = () => CatalogoBuilder.BuildFromAssembly(typeof(RegistroComVigenciaDecrescente).Assembly);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*SEMPRE*").And.Message.Should().Contain("posição 3");
    }
```

Se a classe de teste do `CatalogoBuilder` não existir, criá-la em `tests/TecnoFisc.Sped.Txt.Engine.Tests/Catalogo/CatalogoBuilderTests.cs` com os `using` de `TecnoFisc.Sped.Txt.Engine.Abstracoes`, `.Atributos` e `.Catalogo`. Atenção: `BuildFromAssembly` varre o assembly inteiro, então a classe de fixture quebraria os outros testes do mesmo assembly — declará-la aninhada e privada, e confirmar que o builder ignora tipos aninhados privados; se não ignorar, mover a fixture para um assembly de teste dedicado ou usar um overload que receba os tipos.

- [ ] **Step 2: Rodar e confirmar que falha**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~CatalogoBuilderTests"`
Expected: FAIL — a mensagem não contém a posição.

- [ ] **Step 3: Enriquecer a mensagem**

Em `src/TecnoFisc.Sped.Txt.Engine/Catalogo/CatalogoBuilder.cs:291-304`, passar a ordem e usá-la:

```csharp
    private static void ValidarVigenciaCrescente(Type tipo, List<(int Ordem, MetadadosCampo Campo)> lista)
    {
        for (int i = 1; i < lista.Count; i++)
        {
            var anterior = lista[i - 1].Campo;
            var atual = lista[i].Campo;
            if (atual.DesdeVersao < anterior.DesdeVersao)
                throw new InvalidOperationException(
                    $"Campo {tipo.FullName}.{atual.Nome} na posição {lista[i].Ordem} " +
                    $"(DesdeVersao={atual.DesdeVersao}) vem depois de {anterior.Nome} na posição " +
                    $"{lista[i - 1].Ordem} (DesdeVersao={anterior.DesdeVersao}); DesdeVersao precisa " +
                    "ser não-decrescente ao longo da posição dos campos — campo versionado só pode " +
                    "ficar no fim do registro, senão o mapeamento posicional do leitor desalinha " +
                    "silenciosamente. Mova o campo para o fim do registro ou remova DesdeVersao.");
        }
    }
```

- [ ] **Step 4: Rodar e confirmar que passa**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~CatalogoBuilderTests"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine/Catalogo/CatalogoBuilder.cs tests/TecnoFisc.Sped.Txt.Engine.Tests/Catalogo/CatalogoBuilderTests.cs
git commit -m "fix(txt): nomear posicao do campo no erro de vigencia decrescente"
```

---

### Task 11: Documentação — parar de prometer o que não se entrega

**Files:**
- Modify: `README.md` (linhas 10, 50, 96, 326)
- Modify: `ARCHITECTURE.md` (linhas 102, 218, 778)
- Modify: `CHANGELOG.md` (linha 13 e seção de breaking changes)
- Modify: `src/TecnoFisc.Sped.Ecf/TecnoFisc.Sped.Ecf.csproj:4`
- Modify: `sped/STAGE_17_ECF_BASELINE.md:3`

- [ ] **Step 1: Reescrever a promessa nos cinco arquivos**

A formulação verdadeira, a ser adaptada ao tom de cada arquivo (README em português para usuário final, ARCHITECTURE em inglês):

> Modelo tipado único do leiaute 12, com leitura dos leiautes 8 a 12. Os registros e as colunas que existiam nos leiautes 8 a 10 e saíram no 11 são **reconhecidos, não tipados**: o leitor não aborta e o conteúdo fica acessível em bruto. Leiautes fora da faixa 8–12 são lidos em modo tolerante, com aviso no `0000`.

Trocar toda ocorrência de "leiautes 8 a 12 completos" / "complete layouts 8 through 12" por essa formulação. Em `ARCHITECTURE.md:778`, atualizar também a contagem: o catálogo passa a reconhecer **187** registros — os 180 do leiaute 12 mais os 7 descontinuados.

- [ ] **Step 2: Registrar os breaking changes no CHANGELOG**

Acrescentar, na seção da versão não publicada:

```markdown
#### Breaking changes

- `CatalogoBuilder.BuildFromAssembly` agora lança `InvalidOperationException` quando um
  registro tem `DesdeVersao` num campo que não está no fim do layout. Antes a anotação era
  puramente informacional no caminho reflexivo. Não há opt-out: mova o campo para o fim do
  registro ou remova `DesdeVersao`.
- `Registro0020.IndPrTransf` e `Registro0020.PossuiCebras` foram removidos. A posição 31 é
  lida por `IndicadorPosicao31`; a semântica depende do `COD_VER` do arquivo.
- `Registro0000.VersaoLeiaute` passa a devolver o número declarado em `COD_VER` mesmo fora
  de 8–12. Antes devolvia `0`, o que desligava a vigência silenciosamente.
```

- [ ] **Step 3: Verificar que nada ficou para trás**

Run: `grep -rn "8 a 12\|8 through 12\|8–12\|8-12" README.md ARCHITECTURE.md CHANGELOG.md sped/STAGE_17_ECF_BASELINE.md src/TecnoFisc.Sped.Ecf/TecnoFisc.Sped.Ecf.csproj`
Expected: toda ocorrência restante já qualificada pela ressalva.

- [ ] **Step 4: Commit**

```bash
git add README.md ARCHITECTURE.md CHANGELOG.md sped/STAGE_17_ECF_BASELINE.md src/TecnoFisc.Sped.Ecf/TecnoFisc.Sped.Ecf.csproj
git commit -m "docs: qualificar o alcance real da leitura dos leiautes ECF 8 a 12"
```

- [ ] **Step 5: Atualizar o título do PR #531**

O título atual (`feat(ecf)!: add complete read-only layouts 8 through 12`) afirma "complete". Mantendo tipo, escopo e `!`:

```bash
gh pr edit 531 --title "feat(ecf)!: add read-only ECF layout 12 model with layouts 8-12 reading"
```

- [ ] **Step 6: Rodar a suíte inteira antes de fechar o PR A**

Run: `dotnet test TecnoFisc.Sped.slnx`
Expected: PASS, sem exceção. Este é o gate do PR A.

---

## PR B — contrato de diagnóstico

Abrir a partir de `dev`, depois do merge do #531. Título: `feat(txt)!: expor colunas nao modeladas e discriminar sentinelas`.

### Task 12: `ColunasNaoModeladas`

**Files:**
- Create: `src/TecnoFisc.Sped.Txt.Engine/Abstracoes/ColunaNaoModelada.cs`
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Abstracoes/RegistroSped.cs`
- Test: `tests/TecnoFisc.Sped.Txt.Engine.Tests/Abstracoes/RegistroSpedTests.cs`

**Interfaces:**
- Produces: `ColunaNaoModelada`, `MotivoColunaNaoModelada`, `RegistroSped.ColunasNaoModeladas`, `RegistroSped.RegistrarColunaNaoModelada` (internal).

- [ ] **Step 1: Escrever o teste que falha**

```csharp
    [Fact]
    public void ColunasNaoModeladas_EhVaziaPorPadrao()
        => new RegistroDeTeste().ColunasNaoModeladas.Should().BeEmpty();

    [Fact]
    public void RegistrarColunaNaoModelada_AcumulaNaOrdemDeChegada()
    {
        var registro = new RegistroDeTeste();

        registro.RegistrarColunaNaoModelada(
            new ColunaNaoModelada(5, "BENEFICIARIO", MotivoColunaNaoModelada.AlemDoModelo));
        registro.RegistrarColunaNaoModelada(
            new ColunaNaoModelada(6, "1234,56", MotivoColunaNaoModelada.PosteriorAVersaoDeclarada));

        registro.ColunasNaoModeladas.Select(coluna => coluna.Posicao).Should().Equal(5, 6);
    }
```

Usar como fixture uma subclasse mínima local: `private sealed class RegistroDeTeste : RegistroSped { public override string Codigo => "TST2"; }`.

- [ ] **Step 2: Rodar e confirmar que falha**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~RegistroSpedTests"`
Expected: FAIL — os tipos não existem.

- [ ] **Step 3: Criar os tipos**

`src/TecnoFisc.Sped.Txt.Engine/Abstracoes/ColunaNaoModelada.cs`:

```csharp
namespace TecnoFisc.Sped.Txt.Engine.Abstracoes;

/// <summary>Por que uma coluna presente na linha não virou propriedade do registro.</summary>
public enum MotivoColunaNaoModelada
{
    /// <summary>A coluna vem depois do último campo declarado no catálogo.</summary>
    AlemDoModelo = 0,

    /// <summary>
    /// O campo existe no catálogo mas foi introduzido em versão posterior à declarada no
    /// <c>0000</c>, então não vigorava no arquivo lido.
    /// </summary>
    PosteriorAVersaoDeclarada = 1,
}

/// <summary>
/// Coluna presente na linha SPED que o modelo tipado não representa. Preserva o valor em bruto
/// para que nenhum dado do arquivo se perca em silêncio.
/// </summary>
/// <param name="Posicao">Posição na nomenclatura do Guia Prático: 1 = REG, 2..N = campos.</param>
/// <param name="Valor">Conteúdo da coluna, verbatim.</param>
/// <param name="Motivo">Por que a coluna não foi materializada.</param>
public readonly record struct ColunaNaoModelada(
    int Posicao, string Valor, MotivoColunaNaoModelada Motivo);
```

Em `RegistroSped.cs`, espelhando o padrão de `_errosDeFormato`:

```csharp
    private List<ColunaNaoModelada>? _colunasNaoModeladas;

    /// <summary>
    /// Colunas presentes na linha que o modelo tipado não representa — coluna além do último
    /// campo declarado, ou campo cuja vigência é posterior ao <c>COD_VER</c> do arquivo. Vazia
    /// no caso comum. O valor fica em bruto: nada do arquivo se perde em silêncio.
    /// </summary>
    public IReadOnlyList<ColunaNaoModelada> ColunasNaoModeladas
        => _colunasNaoModeladas ?? (IReadOnlyList<ColunaNaoModelada>)[];

    internal void RegistrarColunaNaoModelada(ColunaNaoModelada coluna)
        => (_colunasNaoModeladas ??= []).Add(coluna);
```

Tornar o membro interno visível ao assembly de teste conferindo se já existe `InternalsVisibleTo` para `TecnoFisc.Sped.Txt.Engine.Tests` (`grep -rn "InternalsVisibleTo" src/TecnoFisc.Sped.Txt.Engine/`); se não existir, testar via `LeitorSpedTxt` na Task 13 e reduzir esta task ao primeiro teste.

- [ ] **Step 4: Rodar e confirmar que passa**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~RegistroSpedTests"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine/Abstracoes tests/TecnoFisc.Sped.Txt.Engine.Tests/Abstracoes
git commit -m "feat(txt): adicionar ColunasNaoModeladas ao registro base"
```

---

### Task 13: Captura no leitor

**Files:**
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs:597-638`
- Test: `tests/TecnoFisc.Sped.Ecf.Tests/Parser/ColunasNaoModeladasTests.cs`

**Interfaces:**
- Consumes: `RegistroSped.RegistrarColunaNaoModelada`, `MotivoColunaNaoModelada`.

- [ ] **Step 1: Escrever o teste que falha**

```csharp
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Versionamento;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Ecf.Tests.Parser;

public sealed class ColunasNaoModeladasTests
{
    [Fact]
    public async Task X450DeLeiaute10_PreservaAsColunasDeDetalheEmBruto()
    {
        var registros = await LeiauteForaDaFaixaTests.ReadAsync(
            10, "|X450|249|11222333000181|BENEFICIARIO|1234,56|01|N|");

        var x450 = registros.OfType<RegistroX450>().Single();
        x450.Pais.Should().Be("249");
        x450.ColunasNaoModeladas.Select(coluna => (coluna.Posicao, coluna.Valor)).Should().Equal(
            (3, "11222333000181"), (4, "BENEFICIARIO"), (5, "1234,56"), (6, "01"), (7, "N"));
        x450.ColunasNaoModeladas.Should().OnlyContain(
            coluna => coluna.Motivo == MotivoColunaNaoModelada.AlemDoModelo);
    }

    [Fact]
    public async Task Campo0020PosteriorAoLeiauteDeclarado_ViraColunaNaoModelada()
    {
        var valores = new List<string> { "1", "1" };
        valores.AddRange(Enumerable.Repeat("N", 27));
        valores.Add("S");
        valores.Add("CEBAS-TESTE");
        string linha = "|0020|" + string.Join('|', valores) + "|";

        var registros = await LeiauteForaDaFaixaTests.ReadAsync(9, linha);

        var registro0020 = registros.Single(item => item.Codigo == "0020");
        registro0020.ColunasNaoModeladas.Should().BeEquivalentTo([
            new ColunaNaoModelada(31, "S", MotivoColunaNaoModelada.PosteriorAVersaoDeclarada),
            new ColunaNaoModelada(32, "CEBAS-TESTE", MotivoColunaNaoModelada.PosteriorAVersaoDeclarada),
        ]);
    }

    [Fact]
    public async Task LinhaSemExcedente_NaoAlocaNada()
    {
        var registros = await LeiauteForaDaFaixaTests.ReadAsync(12, "|0001|0|");

        registros.Should().OnlyContain(registro => registro.ColunasNaoModeladas.Count == 0);
    }
}
```

- [ ] **Step 2: Rodar e confirmar que falha**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~ColunasNaoModeladasTests"`
Expected: FAIL — as colunas são descartadas.

- [ ] **Step 3: Implementar o `else`**

Em `InterpretarLinha`, substituir o comentário das linhas 636-637 por um `else` real:

```csharp
                    Definir(campo, fatia);
                }
                else
                {
                    // Nada do arquivo se perde em silêncio: a coluna existe na linha mas não tem
                    // propriedade que a receba, seja porque vem depois do último campo declarado
                    // (leiaute mais novo, ou registro removido cujo modelo não tem campos), seja
                    // porque o campo só vigora a partir de versão posterior à declarada no 0000.
                    var motivo = indice < metadados.Campos.Count
                        ? MotivoColunaNaoModelada.PosteriorAVersaoDeclarada
                        : MotivoColunaNaoModelada.AlemDoModelo;
                    registro.RegistrarColunaNaoModelada(
                        new ColunaNaoModelada(posicaoCampo, fatia.ToString(), motivo));
                }
```

- [ ] **Step 4: Rodar e confirmar que passa**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~ColunasNaoModeladasTests"`
Expected: PASS.

- [ ] **Step 5: Rodar a suíte inteira**

Run: `dotnet test TecnoFisc.Sped.slnx`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs tests/TecnoFisc.Sped.Ecf.Tests/Parser/ColunasNaoModeladasTests.cs
git commit -m "feat(txt): capturar colunas nao modeladas em vez de descarta-las"
```

---

### Task 14: Benchmark do caminho feliz

**Files:**
- Modify: `benchmarks/TecnoFisc.Sped.Benchmarks/ParserVigenciaBenchmark.cs`

Regra 5: o caminho é sensível a performance. A tese é que o custo é zero quando não há excedente, porque a condição já era avaliada.

- [ ] **Step 1: Acrescentar os dois casos**

Em `benchmarks/TecnoFisc.Sped.Benchmarks/ParserVigenciaBenchmark.cs`, seguindo o padrão de `[GlobalSetup]`/`[Benchmark]` já usado no arquivo:

```csharp
    private byte[] _semExcedente = null!;
    private byte[] _comExcedente = null!;

    [GlobalSetup(Targets = [nameof(LeituraSemColunaExcedente), nameof(LeituraComColunaExcedente)])]
    public void PrepararColunasNaoModeladas()
    {
        const string cabecalho =
            "|0000|LECF|0012|11111111000191|EMPRESA TESTE|0|0|||01012025|31122025|N||0||\r\n";
        var semExcedente = new StringBuilder(cabecalho);
        var comExcedente = new StringBuilder(cabecalho);
        for (int i = 0; i < 10_000; i++)
        {
            semExcedente.Append("|X450|249|\r\n");
            comExcedente.Append("|X450|249|EXCEDENTE_A|EXCEDENTE_B|\r\n");
        }
        semExcedente.Append("|9999|10002|\r\n");
        comExcedente.Append("|9999|10002|\r\n");
        _semExcedente = Encoding.Latin1.GetBytes(semExcedente.ToString());
        _comExcedente = Encoding.Latin1.GetBytes(comExcedente.ToString());
    }

    [Benchmark(Baseline = true)]
    public async Task<int> LeituraSemColunaExcedente() => await ContarAsync(_semExcedente);

    [Benchmark]
    public async Task<int> LeituraComColunaExcedente() => await ContarAsync(_comExcedente);

    private static async Task<int> ContarAsync(byte[] conteudo)
    {
        int total = 0;
        await using var stream = new MemoryStream(conteudo);
        await foreach (var _ in new ParserEcf().ReadStreamingAsync(stream))
            total++;
        return total;
    }
```

Se o arquivo já tiver um `[GlobalSetup]` sem `Targets`, acrescentar `Targets` a ele também — sem isso o BenchmarkDotNet roda todo setup para todo benchmark e a medição fica poluída.

- [ ] **Step 2: Rodar**

Run: `dotnet run -c Release --project benchmarks/TecnoFisc.Sped.Benchmarks --filter "*Vigencia*"`
Expected: o caso sem excedente dentro do ruído do baseline. Se houver regressão fora do ruído, parar e reportar — não seguir para a Task 15.

- [ ] **Step 3: Commit**

```bash
git add benchmarks/TecnoFisc.Sped.Benchmarks/ParserVigenciaBenchmark.cs
git commit -m "test(bench): medir custo da captura de colunas nao modeladas"
```

---

### Task 15: Discriminador de sentinela

**Files:**
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Abstracoes/RegistroNaoReconhecido.cs`
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs:123-131`, `:582-590`
- Modify: `src/TecnoFisc.Sped.Ecf/ArquivoEcf.cs:41-46` e as três classes `Arquivo*` equivalentes
- Test: `tests/TecnoFisc.Sped.Ecf.Tests/Parser/MotivoNaoReconhecimentoTests.cs`

**Interfaces:**
- Produces: `MotivoNaoReconhecimento`, `RegistroNaoReconhecido.Motivo`, e o construtor passa a exigir o motivo.

- [ ] **Step 1: Escrever o teste que falha**

```csharp
    [Fact]
    public async Task CodigoDesconhecido_EVigencia_SaoDistinguiveisSemLerAMensagem()
    {
        var porVigencia = await LeiauteForaDaFaixaTests.ReadAsync(9, "|Y730|1|");
        var porCodigo = await LeiauteForaDaFaixaTests.ReadAsync(13, "|X999|x|");

        porVigencia.OfType<RegistroNaoReconhecido>().Single().Motivo
            .Should().Be(MotivoNaoReconhecimento.PosteriorAVersaoDeclarada);
        porCodigo.OfType<RegistroNaoReconhecido>().Single().Motivo
            .Should().Be(MotivoNaoReconhecimento.CodigoDesconhecido);
    }
```

- [ ] **Step 2: Rodar e confirmar que falha**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~MotivoNaoReconhecimentoTests"`
Expected: FAIL — o tipo não existe.

- [ ] **Step 3: Implementar**

Em `RegistroNaoReconhecido.cs`, acrescentar o enum e a propriedade, e exigir o motivo no construtor:

```csharp
/// <summary>Origem de um <see cref="RegistroNaoReconhecido"/>.</summary>
public enum MotivoNaoReconhecimento
{
    /// <summary>Código de registro que o catálogo não conhece.</summary>
    CodigoDesconhecido = 0,

    /// <summary>
    /// Registro conhecido pelo catálogo, descartado por ter sido introduzido em versão
    /// posterior à declarada no <c>0000</c>.
    /// </summary>
    PosteriorAVersaoDeclarada = 1,
}
```

```csharp
    public RegistroNaoReconhecido(
        string codigo, string linhaCrua, ErroLayout erro, MotivoNaoReconhecimento motivo)
```

```csharp
    /// <summary>
    /// Origem desta sentinela. Prefira este discriminador a inspecionar
    /// <see cref="Erro"/>: a mensagem é texto livre e pode ser reescrita.
    /// </summary>
    public MotivoNaoReconhecimento Motivo { get; }
```

Atualizar o XML doc da classe (linhas 5-18) e o de `Codigo` (33-38), que hoje mandam o consumidor usar `Erro` para distinguir as origens.

Nos dois pontos de construção em `LeitorSpedTxt`, passar o motivo: `MotivoNaoReconhecimento.PosteriorAVersaoDeclarada` na linha 123 e `MotivoNaoReconhecimento.CodigoDesconhecido` na linha 588.

Atualizar o XML doc de `RegistrosNaoReconhecidos` nas quatro classes `Arquivo*` para apontar o `Motivo` como forma de separar as origens.

- [ ] **Step 4: Rodar e confirmar que passa**

Run: `dotnet test TecnoFisc.Sped.slnx`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine src/TecnoFisc.Sped.Ecf src/TecnoFisc.Sped.Ecd src/TecnoFisc.Sped.EfdContribuicoes src/TecnoFisc.Sped.EfdIcmsIpi tests
git commit -m "feat(txt)!: discriminar a origem de RegistroNaoReconhecido"
```

---

### Task 16: `VersaoDoArquivo` e o retorno dos aliases

**Files:**
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Abstracoes/RegistroSped.cs`
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs:644-649`
- Modify: `src/TecnoFisc.Sped.Ecf/Registros/Bloco0/Registro0020.cs`
- Test: `tests/TecnoFisc.Sped.Ecf.Tests/Registros/Bloco0/Registro0020Tests.cs`

**Interfaces:**
- Produces: `RegistroSped.VersaoDoArquivo`; `Registro0020.IndPrTransf` e `Registro0020.PossuiCebras` como `IndicadorSimNao?`.

- [ ] **Step 1: Escrever o teste que falha**

```csharp
    [Theory]
    [InlineData(10)]
    [InlineData(11)]
    public async Task IndPrTransf_RespondeSomenteNosLeiautes10E11(int versao)
    {
        var registro = await Ler0020(versao);

        registro.IndPrTransf.Should().Be(registro.IndicadorPosicao31);
        registro.PossuiCebras.Should().BeNull();
    }

    [Fact]
    public async Task PossuiCebras_RespondeSomenteNoLeiaute12()
    {
        var registro = await Ler0020(12);

        registro.PossuiCebras.Should().Be(registro.IndicadorPosicao31);
        registro.IndPrTransf.Should().BeNull();
    }

    [Fact]
    public async Task VersaoDoArquivo_EhPropagadaParaTodoRegistro()
    {
        var registros = await LeiauteForaDaFaixaTests.ReadAsync(11, "|0001|0|");

        registros.Should().OnlyContain(registro => registro.VersaoDoArquivo == 11);
    }

    private static async Task<Registro0020> Ler0020(int versao)
    {
        var valores = new List<string> { "1", "1" };
        valores.AddRange(Enumerable.Repeat("N", 27));
        valores.Add("S");
        string linha = "|0020|" + string.Join('|', valores) + "|";
        return (await LeiauteForaDaFaixaTests.ReadAsync(versao, linha)).OfType<Registro0020>().Single();
    }
```

- [ ] **Step 2: Rodar e confirmar que falha**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~Registro0020Tests"`
Expected: FAIL — nem `VersaoDoArquivo` nem os aliases existem.

- [ ] **Step 3: Acrescentar `VersaoDoArquivo`**

Em `RegistroSped.cs`:

```csharp
    /// <summary>
    /// Versão do leiaute declarada no <c>0000</c> do arquivo em que este registro foi lido, ou
    /// <c>0</c> quando o registro não veio de uma leitura de arquivo. Distinto de
    /// <see cref="VersaoLeiaute"/>, que é a versão que o próprio registro declara e só o
    /// <c>0000</c> conhece.
    /// </summary>
    public int VersaoDoArquivo { get; internal set; }
```

Em `LeitorSpedTxt.InterpretarLinha`, antes de empilhar (linha 647):

```csharp
        registro.VersaoDoArquivo = versaoLeiaute;
```

- [ ] **Step 4: Devolver os aliases guardados**

Em `Registro0020.cs`, depois de `IndicadorPosicao31`:

```csharp
    /// <summary>
    /// Semântica do campo 31 nos leiautes 10 e 11: opção pelas novas regras de preços de
    /// transferência. <c>null</c> em qualquer outro leiaute, onde a posição significa outra
    /// coisa — ver <see cref="IndicadorPosicao31"/>.
    /// </summary>
    public IndicadorSimNao? IndPrTransf => VersaoDoArquivo is 10 or 11 ? IndicadorPosicao31 : null;

    /// <summary>
    /// Semântica do campo 31 no leiaute 12: posse de certificado Cebas. <c>null</c> em qualquer
    /// outro leiaute — ver <see cref="IndicadorPosicao31"/>.
    /// </summary>
    public IndicadorSimNao? PossuiCebras => VersaoDoArquivo >= 12 ? IndicadorPosicao31 : null;
```

- [ ] **Step 5: Rodar e confirmar que passa**

Run: `dotnet test TecnoFisc.Sped.slnx`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine src/TecnoFisc.Sped.Ecf tests/TecnoFisc.Sped.Ecf.Tests
git commit -m "feat(txt): propagar a versao do arquivo para cada registro lido"
```

---

## PR C — limpeza

Abrir a partir de `dev`. Título: `refactor(txt): unificar o modelo raiz dos quatro leiautes`.

### Task 17: `ArquivoSpedBase`

**Files:**
- Create: `src/TecnoFisc.Sped.Txt.Engine/Abstracoes/ArquivoSpedBase.cs`
- Modify: `src/TecnoFisc.Sped.Ecf/ArquivoEcf.cs`, `src/TecnoFisc.Sped.Ecd/ArquivoEcd.cs`, `src/TecnoFisc.Sped.EfdContribuicoes/ArquivoEfdContribuicoes.cs`, `src/TecnoFisc.Sped.EfdIcmsIpi/ArquivoEfdIcmsIpi.cs`

**Critério de aceite:** suíte dos quatro pacotes verde **sem nenhuma alteração de teste**. Se um teste precisar mudar, o comportamento mudou e o refactor está errado — parar e reportar.

- [ ] **Step 1: Rodar a suíte e guardar o baseline**

Run: `dotnet test TecnoFisc.Sped.slnx`
Expected: PASS. Anotar a contagem de testes.

- [ ] **Step 2: Criar a base**

`src/TecnoFisc.Sped.Txt.Engine/Abstracoes/ArquivoSpedBase.cs`, genérica no tipo de bloco para preservar os acessores tipados de cada leiaute:

```csharp
namespace TecnoFisc.Sped.Txt.Engine.Abstracoes;

/// <summary>
/// Modelo raiz comum aos leiautes: agrupa registros em blocos na ordem canônica, roteia
/// <see cref="RegistroNaoReconhecido"/> para uma coleção à parte e enumera blocos e registros.
/// Cada leiaute concreto fornece sua ordem de blocos e sua fábrica de bloco.
/// </summary>
/// <typeparam name="TBloco">Tipo de bloco do leiaute.</typeparam>
public abstract class ArquivoSpedBase<TBloco> : IArquivoSped
    where TBloco : IBlocoSped
{
    private readonly string[] _ordemBlocos;
    private readonly Dictionary<string, TBloco> _blocos;
    private readonly List<RegistroNaoReconhecido> _naoReconhecidos = [];

    /// <param name="ordemBlocos">Identificadores dos blocos na ordem canônica do leiaute.</param>
    /// <param name="criarBloco">Fábrica do bloco, chamada uma vez por identificador.</param>
    protected ArquivoSpedBase(string[] ordemBlocos, Func<string, TBloco> criarBloco)
    {
        ArgumentNullException.ThrowIfNull(ordemBlocos);
        ArgumentNullException.ThrowIfNull(criarBloco);
        _ordemBlocos = ordemBlocos;
        _blocos = new Dictionary<string, TBloco>(ordemBlocos.Length, StringComparer.Ordinal);
        foreach (var id in ordemBlocos)
            _blocos.Add(id, criarBloco(id));
    }

    /// <summary>Nome do leiaute, usado nas mensagens de erro de roteamento.</summary>
    protected abstract string NomeDoLeiaute { get; }

    /// <summary>Acesso ao bloco pelo identificador. Lança se o bloco não existir no leiaute.</summary>
    protected TBloco Bloco(string id) => _blocos[id];

    /// <inheritdoc cref="ArquivoSpedBase{TBloco}" />
    public IReadOnlyList<RegistroNaoReconhecido> RegistrosNaoReconhecidos => _naoReconhecidos;

    /// <inheritdoc />
    public IEnumerable<IBlocoSped> EnumerarBlocos()
    {
        foreach (var id in _ordemBlocos)
            yield return _blocos[id];
    }

    /// <summary>Enumera todos os registros na ordem canônica dos blocos.</summary>
    public IEnumerable<RegistroSped> EnumerarRegistros()
    {
        foreach (var id in _ordemBlocos)
            foreach (var registro in _blocos[id].EnumerarRegistros())
                yield return registro;
    }

    /// <summary>
    /// Adiciona um registro ao bloco correspondente à primeira posição do código.
    /// <see cref="RegistroNaoReconhecido"/> desvia para <see cref="RegistrosNaoReconhecidos"/>
    /// em vez de ser roteado por código — nunca lança.
    /// </summary>
    public void Adicionar(RegistroSped registro)
    {
        ArgumentNullException.ThrowIfNull(registro);

        if (registro is RegistroNaoReconhecido naoReconhecido)
        {
            _naoReconhecidos.Add(naoReconhecido);
            return;
        }

        var codigo = registro.Codigo;
        if (string.IsNullOrEmpty(codigo))
            throw new ArgumentException("Registro com código vazio não pode ser adicionado.", nameof(registro));

        var idBloco = char.ToUpperInvariant(codigo[0]).ToString();
        if (!_blocos.TryGetValue(idBloco, out var bloco))
            throw new InvalidOperationException(
                $"Código '{codigo}' não pertence a um bloco conhecido do leiaute {NomeDoLeiaute}.");

        bloco.Adicionar(registro);
    }

    /// <summary>Consome o fluxo do parser preenchendo este arquivo.</summary>
    protected async Task PreencherAsync(
        IAsyncEnumerable<RegistroSped> registros, CancellationToken cancelamento)
    {
        ArgumentNullException.ThrowIfNull(registros);
        await foreach (var registro in registros.WithCancellation(cancelamento).ConfigureAwait(false))
            Adicionar(registro);
    }
}
```

Conferir se `IBlocoSped` declara `Adicionar` e `EnumerarRegistros`; se não declarar, acrescentar a restrição necessária ou declarar os membros na interface.

- [ ] **Step 3: Migrar `ArquivoEcf`**

```csharp
public sealed class ArquivoEcf : ArquivoSpedBase<BlocoEcf>
{
    private static readonly string[] _ordemBlocos =
        ["0", "C", "E", "J", "K", "L", "M", "N", "P", "Q", "T", "U", "V", "W", "X", "Y", "9"];

    public ArquivoEcf() : base(_ordemBlocos, id => new BlocoEcf(id)) { }

    /// <inheritdoc />
    protected override string NomeDoLeiaute => "ECF";

    public BlocoEcf Bloco0 => Bloco("0");
    // ... um acessor por bloco, idêntico ao que existe hoje, trocando _blocos["X"] por Bloco("X")

    /// <summary>Constrói o arquivo a partir do fluxo do parser.</summary>
    public static async Task<ArquivoEcf> LoadAsync(
        IAsyncEnumerable<RegistroSped> registros, CancellationToken cancelamento = default)
    {
        var arquivo = new ArquivoEcf();
        await arquivo.PreencherAsync(registros, cancelamento).ConfigureAwait(false);
        return arquivo;
    }
}
```

- [ ] **Step 4: Rodar só a suíte do ECF**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~TecnoFisc.Sped.Ecf.Tests"`
Expected: PASS sem alterar teste algum.

- [ ] **Step 5: Migrar os outros três, um por vez, rodando a suíte do pacote entre cada um**

Run após cada migração: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~TecnoFisc.Sped.<Pacote>.Tests"`
Expected: PASS sem alterar teste algum. Se um deles exigir mudança de teste, reverter aquela migração e reportar.

- [ ] **Step 6: Rodar a suíte inteira e comparar com o baseline**

Run: `dotnet test TecnoFisc.Sped.slnx`
Expected: PASS, mesma contagem de testes do Step 1.

- [ ] **Step 7: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine/Abstracoes/ArquivoSpedBase.cs src/TecnoFisc.Sped.Ecf/ArquivoEcf.cs src/TecnoFisc.Sped.Ecd/ArquivoEcd.cs src/TecnoFisc.Sped.EfdContribuicoes/ArquivoEfdContribuicoes.cs src/TecnoFisc.Sped.EfdIcmsIpi/ArquivoEfdIcmsIpi.cs
git commit -m "refactor(txt): unificar o modelo raiz dos quatro leiautes em ArquivoSpedBase"
```

---

### Task 18: Os dois parked restantes

**Files:**
- Test: `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/LeitorSpedTxtFiltroTests.cs`
- Modify: `CHANGELOG.md`

- [ ] **Step 1: Teste com `RegistrosIgnorados` sobre fixture com filho**

Defense-in-depth do achado A do review anterior: o corte de subárvore do filtro precisa engolir os filhos do registro ignorado, e precisa ser pelo menos tão amplo quanto o corte de vigência que ele preempta.

```csharp
    [Fact]
    public async Task RegistrosIgnorados_DescartaOFilhoJuntoComOPai()
    {
        var opcoes = new ReadingOptions { RegistrosIgnorados = ["X280"] };
        var registros = await LerAsync(opcoes,
            "|X280|pai|\r\n" +
            "|X292|filho|\r\n" +
            "|X350|irmao|\r\n");

        registros.Select(registro => registro.Codigo).Should().NotContain(["X280", "X292"]);
        registros.Select(registro => registro.Codigo).Should().Contain("X350");
    }
```

Adaptar códigos e helper ao que o assembly de teste do engine já usa (`grep -n "RegistrosIgnorados" tests/TecnoFisc.Sped.Txt.Engine.Tests/ -r`), mantendo a forma: pai ignorado, filho de nível maior, irmão de nível igual ou menor que sobrevive.

- [ ] **Step 2: Rodar**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~LeitorSpedTxtFiltroTests"`
Expected: PASS na primeira execução — o caminho já foi provado seguro analiticamente; o teste existe para travar o comportamento, não para descobrir defeito. Se falhar, é achado novo: parar e reportar.

- [ ] **Step 3: Uniformizar a ordem das subseções do CHANGELOG**

Fixar a mesma ordem de subseções em todas as entradas por pacote (por exemplo `Adicionado`, `Alterado`, `Corrigido`, `Removido`, `Breaking changes`) e reordenar as existentes.

- [ ] **Step 4: Commit**

```bash
git add tests/TecnoFisc.Sped.Txt.Engine.Tests CHANGELOG.md
git commit -m "test(txt): travar o corte de subarvore de RegistrosIgnorados"
```

---

## Verificação final antes da publicação

- [ ] `dotnet build TecnoFisc.Sped.slnx` — 0 erros, 0 avisos
- [ ] `dotnet test TecnoFisc.Sped.slnx` — tudo verde
- [ ] `dotnet run -c Release --project benchmarks/TecnoFisc.Sped.Benchmarks --filter "*Vigencia*"` — sem regressão
- [ ] Os três PRs mergeados em `dev`, cada um com título em Conventional Commit
- [ ] `CHANGELOG.md` com a seção de breaking changes completa: `CatalogoBuilder.BuildFromAssembly`, aliases do `0020`, `Registro0000.VersaoLeiaute`, construtor de `RegistroNaoReconhecido`, e a herança de `ArquivoSpedBase`
