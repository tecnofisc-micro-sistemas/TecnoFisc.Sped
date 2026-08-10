using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoT;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoT;

public sealed class RegistroT120Tests
{
    [Fact]
    public void Registro_ConformeManifestoInclusiveAliasCodigo()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroT120(), "T120", "0:N");
    }

    [Fact]
    public void Parser_PreservaCodigoComZerosDescricaoEValorDecimalComoDados()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|T120|0026|BASE DE CALCULO|0001000000,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroT120>().Which;
        registro.CampoCodigo.Should().Be("0026");
        registro.Descricao.Should().Be("BASE DE CALCULO");
        registro.Valor.Should().Be("0001000000,00");
    }

    [Fact]
    public void Parser_OpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha("|T120|0100|||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroT120>().Which;
        registro.Descricao.Should().BeNull();
        registro.Valor.Should().BeNull();
    }
}
