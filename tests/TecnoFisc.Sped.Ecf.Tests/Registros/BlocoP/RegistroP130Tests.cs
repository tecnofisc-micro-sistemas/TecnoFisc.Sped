using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoP;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoP;

public sealed class RegistroP130Tests
{
    [Fact]
    public void Registro_ConformeManifestoInclusiveAliasCodigo()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroP130(), "P130", "0:N");
    }

    [Fact]
    public void Parser_PreservaCodigoDescricaoEValorTextualComEscala()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|P130|0082|TOTAL DO LUCRO PRESUMIDO AJUSTADO|000100000,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroP130>().Which;
        registro.CampoCodigo.Should().Be("0082");
        registro.Descricao.Should().Be("TOTAL DO LUCRO PRESUMIDO AJUSTADO");
        registro.Valor.Should().Be("000100000,00");
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_OpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha("|P130|0082|||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroP130>().Which;
        registro.Descricao.Should().BeNull();
        registro.Valor.Should().BeNull();
    }
}
