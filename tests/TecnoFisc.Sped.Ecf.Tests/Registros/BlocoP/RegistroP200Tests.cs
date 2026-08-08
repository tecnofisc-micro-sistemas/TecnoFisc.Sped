using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoP;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoP;

public sealed class RegistroP200Tests
{
    [Fact]
    public void Registro_ConformeManifestoInclusiveAliasCodigo()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroP200(), "P200", "0:N");
    }

    [Fact]
    public void Parser_PreservaCodigoComZerosDescricaoEValorTextual()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|P200|0025|(-)DIVULGACAO ELEITORAL E PARTIDARIA GRATUITA|00010000,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroP200>().Which;
        registro.CampoCodigo.Should().Be("0025");
        registro.Descricao.Should().Be("(-)DIVULGACAO ELEITORAL E PARTIDARIA GRATUITA");
        registro.Valor.Should().Be("00010000,00");
    }

    [Fact]
    public void Parser_OpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha("|P200|0100|||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroP200>().Which;
        registro.Descricao.Should().BeNull();
        registro.Valor.Should().BeNull();
    }
}
