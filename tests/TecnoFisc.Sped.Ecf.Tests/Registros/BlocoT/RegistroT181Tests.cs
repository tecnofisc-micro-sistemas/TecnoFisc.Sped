using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoT;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoT;

public sealed class RegistroT181Tests
{
    [Fact]
    public void Registro_ConformeManifestoInclusiveAliasCodigo()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroT181(), "T181", "0:N");
    }

    [Fact]
    public void Parser_PreservaCodigoDescricaoEValorNegativoComoDados()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|T181|0017|CSLL POSTERGADA DE PERIODOS ANTERIORES|-0,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroT181>().Which;
        registro.CampoCodigo.Should().Be("0017");
        registro.Descricao.Should().Be("CSLL POSTERGADA DE PERIODOS ANTERIORES");
        registro.Valor.Should().Be("-0,00");
    }

    [Fact]
    public void Parser_OpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha("|T181|0101|||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroT181>().Which;
        registro.Descricao.Should().BeNull();
        registro.Valor.Should().BeNull();
    }
}
