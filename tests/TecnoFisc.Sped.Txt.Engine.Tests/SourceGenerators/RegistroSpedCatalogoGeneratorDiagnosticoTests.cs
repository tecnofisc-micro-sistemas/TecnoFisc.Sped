using static TecnoFisc.Sped.Txt.Engine.Tests.SourceGenerators.GeradorHarness;

namespace TecnoFisc.Sped.Txt.Engine.Tests.SourceGenerators;

/// <summary>
/// Achado 6 do PR 531: um único diagnóstico de campo (<c>TFSPED00x</c>) fazia o gerador retornar
/// antes de <c>AddSource</c>, suprimindo <c>CatalogoSpedGerado.g.cs</c> e
/// <c>RegistroSpedVisitor.g.cs</c> do assembly inteiro — o build real falhava com centenas de
/// <c>CS0246</c> (tipos gerados ausentes) que soterravam o único diagnóstico acionável. O
/// diagnóstico continua falhando o build (é o comportamento desejado); só a emissão do catálogo
/// não pode mais depender de "zero erros".
/// </summary>
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

    /// <summary>
    /// Achado do review da Task 8: o fallback de <c>TFSPED001</c> (nome CLR no lugar do alias
    /// inválido) precisa passar pela mesma deduplicação que qualquer outro nome de campo. Aqui
    /// <c>CodVer</c> tem alias inválido "COD VER" e cai para o nome CLR "CodVer"; <c>Outro</c>
    /// declara o alias explícito "CodVer" (válido, mas igual ao nome CLR do primeiro campo) — a
    /// colisão só existe por causa do fallback, então precisa disparar TFSPED002 além do
    /// TFSPED001 do alias inválido.
    /// </summary>
    private const string FonteComAliasInvalidoCujoFallbackColideComOutroAlias = """
        using TecnoFisc.Sped.Txt.Engine.Abstracoes;
        using TecnoFisc.Sped.Txt.Engine.Atributos;

        namespace Exemplo;

        [RegistroSped(Codigo = "B400", Nivel = 2, Bloco = "B")]
        public sealed partial class RegistroB400 : RegistroSped
        {
            public override string Codigo => "B400";

            [CampoSped(Ordem = 2, Nome = "COD VER")]
            public string? CodVer { get; set; }

            [CampoSped(Ordem = 3, Nome = "CodVer")]
            public string? Outro { get; set; }
        }
        """;

    private const string FonteComVigenciaForaDeOrdem = """
        using TecnoFisc.Sped.Txt.Engine.Abstracoes;
        using TecnoFisc.Sped.Txt.Engine.Atributos;

        namespace Exemplo;

        [RegistroSped(Codigo = "B300", Nivel = 2, Bloco = "B")]
        public sealed partial class RegistroB300 : RegistroSped
        {
            public override string Codigo => "B300";

            [CampoSped(Ordem = 2, DesdeVersao = 5, Nome = "MEIO")]
            public string? Meio { get; set; }

            [CampoSped(Ordem = 3, Nome = "FIM")]
            public string? Fim { get; set; }
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

    [Fact]
    public void AliasInvalidoComFallbackColidindoComOutroAlias_ReportaTFSPED001ETFSPED002()
    {
        var (gerado, diagnosticos) = ExecutarGeradorComDiagnosticos(FonteComAliasInvalidoCujoFallbackColideComOutroAlias);

        diagnosticos.Select(d => d.Id).Should().BeEquivalentTo(["TFSPED001", "TFSPED002"]);
        gerado.Should().Contain("class CatalogoSpedGerado");
        gerado.Should().Contain("IRegistroSpedVisitor");
    }

    /// <summary>
    /// TFSPED003 não tem fallback de nome — a vigência é a vigência que é. Diferente do teste de
    /// alias inválido, aqui a prova é só que o catálogo continua sendo emitido (não soterrado sob
    /// CS0246) mesmo com o diagnóstico de vigência fora de ordem.
    /// </summary>
    [Fact]
    public void VigenciaForaDeOrdem_ReportaUmDiagnosticoTFSPED003EAindaEmiteOCatalogo()
    {
        var (gerado, diagnosticos) = ExecutarGeradorComDiagnosticos(FonteComVigenciaForaDeOrdem);

        diagnosticos.Should().ContainSingle().Which.Id.Should().Be("TFSPED003");
        gerado.Should().Contain("class CatalogoSpedGerado");
        gerado.Should().Contain("IRegistroSpedVisitor");
        gerado.Should().Contain("\"MEIO\"");
        gerado.Should().Contain("\"FIM\"");
    }
}
