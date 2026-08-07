using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoK;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoK;

public sealed class RegistroK001Tests
{
    [Fact]
    public void Catalogo_ImplementaLoteCompletoDoBlocoK()
    {
        AssertRegistroEcf.CodesAreImplemented(
            "K001", "K030", "K155", "K156", "K355", "K356", "K915", "K935", "K990");
    }

    [Fact]
    public void Catalogo_DoBlocoK_TemSomenteOsNoveCodigosRevisados()
    {
        var codigos = new CatalogoSpedGerado().EnumerarRegistros()
            .Where(registro => registro.Bloco == "K")
            .Select(registro => registro.Codigo);

        codigos.Should().Equal(
            "K001", "K030", "K155", "K156", "K355", "K356", "K915", "K935", "K990");
    }

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroK001(), "K001", "1:1");
    }

    [Theory]
    [InlineData("0", IndicadorMovimentoBloco.ComDados)]
    [InlineData("1", IndicadorMovimentoBloco.SemDados)]
    public void Parser_LeIndicadorDeMovimento(string valor, IndicadorMovimentoBloco esperado)
    {
        var resultado = new ParserEcf().ParseLinha($"|K001|{valor}|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroK001>()
            .Which.IndDad.Should().Be(esperado);
    }

    [Fact]
    public void Parser_IndicadorForaDoDominio_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|K001|2|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroK001>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == "IND_DAD" && erro.ValorBruto == "2");
    }
}
