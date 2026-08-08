using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoE;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoE;

public sealed class RegistroE030Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroE030(), "E030", "0:13");
    }

    [Fact]
    public void Parser_LeDatasEPeriodoComoDadoTipado()
    {
        var resultado = new ParserEcf().ParseLinha("|E030|01012025|31012025|A01|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroE030>().Which;
        registro.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2025, 1, 31));
        registro.PerApur.Should().Be("A01");
    }

    [Fact]
    public void Parser_PeriodoNaoCatalogado_PreservaCodigoSemExecutarRegraFiscal()
    {
        var resultado = new ParserEcf().ParseLinha("|E030|01012025|31012025|Z99|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroE030>()
            .Which.PerApur.Should().Be("Z99");
    }
}
