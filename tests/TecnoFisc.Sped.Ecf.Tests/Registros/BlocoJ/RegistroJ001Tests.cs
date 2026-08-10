using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoJ;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoJ;

public sealed class RegistroJ001Tests
{
    [Fact]
    public void Catalogo_ImplementaLoteCompletoDoBlocoJ()
    {
        AssertRegistroEcf.CodesAreImplemented(
            "J001", "J050", "J051", "J053", "J100", "J990");
    }

    [Fact]
    public void Catalogo_DoBlocoJ_TemSomenteOsSeisCodigosRevisados()
    {
        var codigos = new CatalogoSpedGerado().EnumerarRegistros()
            .Where(registro => registro.Bloco == "J")
            .Select(registro => registro.Codigo);

        codigos.Should().Equal("J001", "J050", "J051", "J053", "J100", "J990");
    }

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroJ001(), "J001", "1:1");
    }

    [Theory]
    [InlineData("0", IndicadorMovimentoBloco.ComDados)]
    [InlineData("1", IndicadorMovimentoBloco.SemDados)]
    public void Parser_LeIndicadorDeMovimento(string valor, IndicadorMovimentoBloco esperado)
    {
        var resultado = new ParserEcf().ParseLinha($"|J001|{valor}|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroJ001>()
            .Which.IndDad.Should().Be(esperado);
    }
}
