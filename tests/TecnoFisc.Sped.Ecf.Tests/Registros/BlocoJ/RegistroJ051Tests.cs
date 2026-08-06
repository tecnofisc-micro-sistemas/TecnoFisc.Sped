using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoJ;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoJ;

public sealed class RegistroJ051Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroJ051(), "J051", "0:N");
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("000001", "000001")]
    public void Parser_PreservaContaReferencialECentroDeCustosOpcional(
        string centroDeCustos,
        string? esperado)
    {
        var resultado = new ParserEcf().ParseLinha(
            $"|J051|{centroDeCustos}|1.01.01.01.01|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroJ051>().Which;
        registro.CodCcus.Should().Be(esperado);
        registro.CodCtaRef.Should().Be("1.01.01.01.01");
    }
}
