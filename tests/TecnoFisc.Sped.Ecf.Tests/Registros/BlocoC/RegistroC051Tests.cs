using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoC;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoC;

public sealed class RegistroC051Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroC051(), "C051", "0:N");
    }

    [Fact]
    public void Parser_PreservaCentroDeCustosOpcionalECodigoReferencial()
    {
        var resultado = new ParserEcf().ParseLinha("|C051||01.01|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroC051>().Which;
        registro.CodCcus.Should().BeNull();
        registro.CodCtaRef.Should().Be("01.01");
    }
}
