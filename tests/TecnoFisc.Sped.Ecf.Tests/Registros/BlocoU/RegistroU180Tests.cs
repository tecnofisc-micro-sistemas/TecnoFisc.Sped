using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoU;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoU;

public sealed class RegistroU180Tests
{
    [Fact]
    public void Registro_ConformeManifestoInclusiveAliasCodigo()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroU180(), "U180", "0:N");
    }

    [Theory]
    [InlineData("000012", "IMPOSTO DE RENDA A PAGAR", "10000,00")]
    [InlineData("000099", null, "12,3400%")]
    [InlineData("000100", "AJUSTE", "+100,0000")]
    public void Parser_PreservaCamposDaTabelaDinamicaSemInterpretacaoFiscal(
        string codigo,
        string? descricao,
        string valor)
    {
        var resultado = new ParserEcf().ParseLinha($"|U180|{codigo}|{descricao}|{valor}|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroU180>().Which;
        registro.CampoCodigo.Should().Be(codigo);
        registro.Descricao.Should().Be(descricao);
        registro.Valor.Should().Be(valor);
    }

    [Fact]
    public void Parser_OpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha("|U180|000001|||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroU180>().Which;
        registro.CampoCodigo.Should().Be("000001");
        registro.Descricao.Should().BeNull();
        registro.Valor.Should().BeNull();
    }
}
