using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoM;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoM.Lote1;

public sealed class RegistroM312Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroM312(), "M312", "0:N");
    }

    [Fact]
    public void Parser_PreservaNumeroDoLancamentoEcdComoIdentificadorTextual()
    {
        var resultado = new ParserEcf().ParseLinha("|M312|0000012345-A|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroM312>()
            .Which.NumLcto.Should().Be("0000012345-A");
    }
}
