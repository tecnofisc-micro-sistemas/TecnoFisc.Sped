using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoM;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoM.Lote2;

public sealed class RegistroM362Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroM362(), "M362", "0:N");
    }

    [Fact]
    public void Parser_PreservaNumeroDoLancamentoEcdComoIdentificadorLossless()
    {
        var resultado = new ParserEcf().ParseLinha("|M362|0000005678-A/2025|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroM362>()
            .Which.NumLcto.Should().Be("0000005678-A/2025");
    }
}
