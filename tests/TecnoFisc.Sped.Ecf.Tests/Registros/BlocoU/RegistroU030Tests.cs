using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoU;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoU;

public sealed class RegistroU030Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroU030(), "U030", "0:13");
    }

    [Fact]
    public void Parser_LeDatasEPeriodoDeApuracao()
    {
        var resultado = new ParserEcf().ParseLinha("|U030|01042024|30062024|T02|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroU030>().Which;
        registro.DtIni.Should().Be(new DateOnly(2024, 4, 1));
        registro.DtFin.Should().Be(new DateOnly(2024, 6, 30));
        registro.PerApur.Should().Be("T02");
    }

    [Fact]
    public void Parser_DatasForaDoFormatoNormativo_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|U030|20240401|20240630|T02|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroU030>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([nameof(RegistroU030.DtIni), nameof(RegistroU030.DtFin)]);
    }

    [Theory]
    [InlineData("A00")]
    [InlineData("A12")]
    [InlineData("T01")]
    [InlineData("T04")]
    [InlineData("Z99")]
    public void Parser_PreservaPeriodoSemExecutarRegraFiscal(string periodo)
    {
        var resultado = new ParserEcf().ParseLinha($"|U030|01012025|31032025|{periodo}|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroU030>()
            .Which.PerApur.Should().Be(periodo);
    }
}
