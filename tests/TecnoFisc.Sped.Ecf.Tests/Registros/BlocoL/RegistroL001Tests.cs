using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoL;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoL;

public sealed class RegistroL001Tests
{
    [Fact]
    public void Catalogo_ImplementaLoteCompletoDoBlocoL()
    {
        AssertRegistroEcf.CodesAreImplemented(
            "L001", "L030", "L100", "L200", "L210", "L300", "L990");
    }

    [Fact]
    public void Catalogo_DoBlocoL_TemSomenteOsSeteCodigosRevisados()
    {
        var codigos = new CatalogoSpedGerado().EnumerarRegistros()
            .Where(registro => registro.Bloco == "L")
            .Select(registro => registro.Codigo);

        codigos.Should().Equal("L001", "L030", "L100", "L200", "L210", "L300", "L990");
    }

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroL001(), "L001", "1:1");
    }

    [Theory]
    [InlineData("0", IndicadorMovimentoBloco.ComDados)]
    [InlineData("1", IndicadorMovimentoBloco.SemDados)]
    public void Parser_LeIndicadorDeMovimento(string valor, IndicadorMovimentoBloco esperado)
    {
        var resultado = new ParserEcf().ParseLinha($"|L001|{valor}|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroL001>()
            .Which.IndDad.Should().Be(esperado);
    }

    [Fact]
    public void Parser_IndicadorForaDoDominio_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|L001|2|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroL001>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == "IND_DAD" && erro.ValorBruto == "2");
    }
}
