using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoC;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoC;

public sealed class RegistroC350Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroC350(), "C350", "0:N");
    }

    [Fact]
    public void Parser_LeDataDaApuracao()
    {
        var resultado = new ParserEcf().ParseLinha("|C350|31122025|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroC350>()
            .Which.DtRes.Should().Be(new DateOnly(2025, 12, 31));
    }
}
