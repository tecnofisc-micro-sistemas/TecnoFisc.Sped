using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoJ;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoJ;

public sealed class RegistroJ100Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroJ100(), "J100", "0:N");
    }

    [Fact]
    public void Parser_LeDataEPreservaCodigoDoCentroDeCustos()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|J100|01012025|000001|CENTRO DE CUSTOS 000001|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroJ100>().Which;
        registro.DtAlt.Should().Be(new DateOnly(2025, 1, 1));
        registro.CodCcus.Should().Be("000001");
        registro.Ccus.Should().Be("CENTRO DE CUSTOS 000001");
    }
}
