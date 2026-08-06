using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoP;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoP;

public sealed class RegistroP030Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroP030(), "P030", "0:5");
    }

    [Fact]
    public void Parser_LeDatasEPeriodoDeApuracao()
    {
        var resultado = new ParserEcf().ParseLinha("|P030|01012025|31032025|T01|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroP030>().Which;
        registro.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2025, 3, 31));
        registro.PerApur.Should().Be("T01");
    }

    [Fact]
    public void Parser_DatasForaDoFormatoNormativo_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|P030|20250101|20250331|T01|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroP030>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([nameof(RegistroP030.DtIni), nameof(RegistroP030.DtFin)]);
    }

    [Theory]
    [InlineData("A00")]
    [InlineData("T01")]
    [InlineData("T04")]
    [InlineData("Z99")]
    public void Parser_PreservaPeriodoSemExecutarRegraFiscal(string periodo)
    {
        var resultado = new ParserEcf().ParseLinha($"|P030|01012025|31032025|{periodo}|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroP030>()
            .Which.PerApur.Should().Be(periodo);
    }
}
