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
