using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoT;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoT;

public sealed class RegistroT150Tests
{
    [Fact]
    public void Registro_ConformeManifestoInclusiveAliasCodigo()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroT150(), "T150", "0:N");
    }

    [Theory]
    [InlineData("-100000,00")]
    [InlineData("12,3400%")]
    [InlineData("VALOR-TABELA")]
    public void Parser_PreservaSinaisPercentuaisEConteudoDinamicoSemCalculo(string valor)
    {
        var resultado = new ParserEcf().ParseLinha($"|T150|0016|IMPOSTO DE RENDA A PAGAR|{valor}|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroT150>()
            .Which.Valor.Should().Be(valor);
    }

    [Fact]
    public void Parser_OpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha("|T150|0100|||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroT150>().Which;
        registro.CampoCodigo.Should().Be("0100");
        registro.Descricao.Should().BeNull();
        registro.Valor.Should().BeNull();
    }
}
