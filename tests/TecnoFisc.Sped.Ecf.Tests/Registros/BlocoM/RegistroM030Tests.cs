using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoM;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoM.Lote1;

public sealed class RegistroM030Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroM030(), "M030", "0:13");
    }

    [Fact]
    public void Parser_LeDatasEPeriodoDeApuracaoSemNormalizarCodigo()
    {
        var resultado = new ParserEcf().ParseLinha("|M030|01012025|31032025|T01|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroM030>().Which;
        registro.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2025, 3, 31));
        registro.PerApur.Should().Be("T01");
    }

    [Fact]
    public void Parser_DatasInvalidas_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|M030|20250101|20250331|A00|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroM030>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain(["DT_INI", "DT_FIN"]);
    }
}
