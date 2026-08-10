using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoT;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoT;

public sealed class RegistroT030Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroT030(), "T030", "0:4");
    }

    [Fact]
    public void Parser_LeDatasEPeriodoDeApuracao()
    {
        var resultado = new ParserEcf().ParseLinha("|T030|01042024|30062024|T02|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroT030>().Which;
        registro.DtIni.Should().Be(new DateOnly(2024, 4, 1));
        registro.DtFin.Should().Be(new DateOnly(2024, 6, 30));
        registro.PerApur.Should().Be("T02");
    }

    [Fact]
    public void Parser_DatasForaDoFormatoNormativo_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|T030|20240401|20240630|T02|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroT030>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain(["DT_INI", "DT_FIN"]);
    }

    [Theory]
    [InlineData("T01")]
    [InlineData("T02")]
    [InlineData("T03")]
    [InlineData("T04")]
    [InlineData("Z99")]
    public void Parser_PreservaPeriodoSemExecutarRegraFiscal(string periodo)
    {
        var resultado = new ParserEcf().ParseLinha($"|T030|01012025|31032025|{periodo}|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroT030>()
            .Which.PerApur.Should().Be(periodo);
    }
}
