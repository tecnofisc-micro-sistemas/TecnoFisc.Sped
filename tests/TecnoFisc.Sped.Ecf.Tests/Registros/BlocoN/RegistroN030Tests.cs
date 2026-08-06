using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoN;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoN.Lote1;

public sealed class RegistroN030Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroN030(), "N030", "0:13");
    }

    [Fact]
    public void Parser_LeDatasEPeriodoDeApuracaoSemNormalizarCodigo()
    {
        var resultado = new ParserEcf().ParseLinha("|N030|01012025|31032025|T01|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroN030>().Which;
        registro.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2025, 3, 31));
        registro.PerApur.Should().Be("T01");
    }

    [Fact]
    public void Parser_DatasInvalidas_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|N030|20250101|20250331|A00|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroN030>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([nameof(RegistroN030.DtIni), nameof(RegistroN030.DtFin)]);
    }
}
