using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoK;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoK;

public sealed class RegistroK030Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroK030(), "K030", "0:13");
    }

    [Fact]
    public void Parser_LeDatasEPeriodoComoDadoTipado()
    {
        var resultado = new ParserEcf().ParseLinha("|K030|01012025|31032025|T01|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroK030>().Which;
        registro.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2025, 3, 31));
        registro.PerApur.Should().Be("T01");
    }

    [Fact]
    public void Parser_DatasForaDoFormatoNormativo_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|K030|20250101|20250331|T01|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroK030>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([nameof(RegistroK030.DtIni), nameof(RegistroK030.DtFin)]);
    }

    [Fact]
    public void Parser_PeriodoNaoCatalogado_PreservaCodigoSemExecutarRegraFiscal()
    {
        var resultado = new ParserEcf().ParseLinha("|K030|01012025|31032025|Z99|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroK030>()
            .Which.PerApur.Should().Be("Z99");
    }
}
