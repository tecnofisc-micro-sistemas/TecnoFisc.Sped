using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoP;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoP;

public sealed class RegistroP300Tests
{
    [Fact]
    public void Registro_ConformeManifestoInclusiveAliasCodigo()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroP300(), "P300", "0:N");
    }

    [Fact]
    public void Parser_PreservaValorComSinalEZerosComoDadoTextual()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|P300|0017|IMPOSTO DE RENDA POSTERGADO|-00010000,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroP300>().Which;
        registro.CampoCodigo.Should().Be("0017");
        registro.Valor.Should().Be("-00010000,00");
    }
}
