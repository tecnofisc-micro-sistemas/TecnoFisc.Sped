using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.Bloco0;

public sealed class Registro0001Tests
{
    [Fact]
    public void Catalogo_ImplementaLoteCompletoDoBloco0()
    {
        AssertRegistroEcf.CodesAreImplemented(
            "0001", "0010", "0020", "0021", "0030", "0035", "0930", "0990");
    }

    [Fact]
    public void Catalogo_DoBloco0_TemSomenteOsNoveCodigosRevisados()
    {
        var codigos = new CatalogoSpedGerado().EnumerarRegistros()
            .Where(registro => registro.Bloco == "0")
            .Select(registro => registro.Codigo);

        codigos.Should().Equal("0000", "0001", "0010", "0020", "0021", "0030", "0035", "0930", "0990");
    }

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new Registro0001(), "0001", "1:1");
    }

    [Theory]
    [InlineData("0", IndicadorMovimentoBloco.ComDados)]
    [InlineData("1", IndicadorMovimentoBloco.SemDados)]
    public void Parser_LeIndicadorDeMovimento(string valor, IndicadorMovimentoBloco esperado)
    {
        var resultado = new ParserEcf().ParseLinha($"|0001|{valor}|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<Registro0001>()
            .Which.IndDad.Should().Be(esperado);
    }
}
