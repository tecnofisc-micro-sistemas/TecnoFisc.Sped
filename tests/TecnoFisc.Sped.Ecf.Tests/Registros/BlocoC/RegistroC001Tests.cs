using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoC;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoC;

public sealed class RegistroC001Tests
{
    [Fact]
    public void Catalogo_ImplementaLoteCompletoDoBlocoC()
    {
        AssertRegistroEcf.CodesAreImplemented(
            "C001", "C040", "C050", "C051", "C053", "C100",
            "C150", "C155", "C157", "C350", "C355", "C990");
    }

    [Fact]
    public void Catalogo_DoBlocoC_TemSomenteOsDozeCodigosRevisados()
    {
        var codigos = new CatalogoSpedGerado().EnumerarRegistros()
            .Where(registro => registro.Bloco == "C")
            .Select(registro => registro.Codigo);

        codigos.Should().Equal(
            "C001", "C040", "C050", "C051", "C053", "C100",
            "C150", "C155", "C157", "C350", "C355", "C990");
    }

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroC001(), "C001", "1:1");
    }

    [Theory]
    [InlineData("0", IndicadorMovimentoBloco.ComDados)]
    [InlineData("1", IndicadorMovimentoBloco.SemDados)]
    public void Parser_LeIndicadorDeMovimento(string valor, IndicadorMovimentoBloco esperado)
    {
        var resultado = new ParserEcf().ParseLinha($"|C001|{valor}|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroC001>()
            .Which.IndDad.Should().Be(esperado);
    }
}
