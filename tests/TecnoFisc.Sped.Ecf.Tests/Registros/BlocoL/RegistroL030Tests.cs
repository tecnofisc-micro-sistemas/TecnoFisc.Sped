using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoL;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoL;

public sealed class RegistroL030Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroL030(), "L030", "0:13");
    }

    [Fact]
    public void Parser_LeDatasEPeriodoComoDadosTipados()
    {
        var resultado = new ParserEcf().ParseLinha("|L030|01012025|31032025|T01|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroL030>().Which;
        registro.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2025, 3, 31));
        registro.PerApur.Should().Be("T01");
    }

    [Fact]
    public void Parser_DatasForaDoFormatoNormativo_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|L030|20250101|20250331|T01|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroL030>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([nameof(RegistroL030.DtIni), nameof(RegistroL030.DtFin)]);
    }

    [Theory]
    [InlineData("A00")]
    [InlineData("A01")]
    [InlineData("T04")]
    [InlineData("Z99")]
    public void Parser_PreservaCodigoDoPeriodoSemExecutarRegraFiscal(string periodo)
    {
        var resultado = new ParserEcf().ParseLinha($"|L030|01012025|31032025|{periodo}|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroL030>()
            .Which.PerApur.Should().Be(periodo);
    }
}
