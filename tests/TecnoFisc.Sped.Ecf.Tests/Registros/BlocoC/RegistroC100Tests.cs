using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoC;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoC;

public sealed class RegistroC100Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroC100(), "C100", "0:N");
    }

    [Fact]
    public void Parser_LeDataECentroDeCustos()
    {
        var resultado = new ParserEcf().ParseLinha("|C100|02012025|CC001|ADMINISTRATIVO|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroC100>().Which;
        registro.DtAlt.Should().Be(new DateOnly(2025, 1, 2));
        registro.CodCcus.Should().Be("CC001");
        registro.Ccus.Should().Be("ADMINISTRATIVO");
    }
}
