using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoT;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoT;

public sealed class RegistroT170Tests
{
    [Fact]
    public void Registro_ConformeManifestoInclusiveAliasCodigo()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroT170(), "T170", "0:N");
    }

    [Fact]
    public void Parser_PreservaCodigoDescricaoESinalPositivoComoDados()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|T170|0013|LUCROS DISPONIBILIZADOS NO EXTERIOR|+100000,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroT170>().Which;
        registro.CampoCodigo.Should().Be("0013");
        registro.Descricao.Should().Be("LUCROS DISPONIBILIZADOS NO EXTERIOR");
        registro.Valor.Should().Be("+100000,00");
    }

    [Fact]
    public void Parser_OpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha("|T170|0100|||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroT170>().Which;
        registro.Descricao.Should().BeNull();
        registro.Valor.Should().BeNull();
    }
}
