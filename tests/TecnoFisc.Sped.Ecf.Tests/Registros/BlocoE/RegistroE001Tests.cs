using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoE;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoE;

public sealed class RegistroE001Tests
{
    [Fact]
    public void Catalogo_ImplementaLoteCompletoDoBlocoE()
    {
        AssertRegistroEcf.CodesAreImplemented(
            "E001", "E010", "E015", "E020", "E030", "E155", "E355", "E990");
    }

    [Fact]
    public void Catalogo_DoBlocoE_TemSomenteOsOitoCodigosRevisados()
    {
        var codigos = new CatalogoSpedGerado().EnumerarRegistros()
            .Where(registro => registro.Bloco == "E")
            .Select(registro => registro.Codigo);

        codigos.Should().Equal(
            "E001", "E010", "E015", "E020", "E030", "E155", "E355", "E990");
    }

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroE001(), "E001", "1:1");
    }

    [Theory]
    [InlineData("0", IndicadorMovimentoBloco.ComDados)]
    [InlineData("1", IndicadorMovimentoBloco.SemDados)]
    public void Parser_LeIndicadorDeMovimento(string valor, IndicadorMovimentoBloco esperado)
    {
        var resultado = new ParserEcf().ParseLinha($"|E001|{valor}|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroE001>()
            .Which.IndDad.Should().Be(esperado);
    }
}
