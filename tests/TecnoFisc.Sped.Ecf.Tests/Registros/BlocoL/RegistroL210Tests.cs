using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoL;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoL;

public sealed class RegistroL210Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroL210(), "L210", "0:N");
    }

    [Theory]
    [InlineData("0092", "CONSTITUICAO DE PROVISOES", "00001000,00")]
    [InlineData("0007", "ROTULO DINAMICO", "R")]
    [InlineData("0010", "CALCULO NAO ALTERAVEL", "CNA")]
    public void Parser_PreservaLinhaDinamicaEMultiplasRepresentacoesDoValor(
        string codigo,
        string descricao,
        string valor)
    {
        var resultado = new ParserEcf().ParseLinha($"|L210|{codigo}|{descricao}|{valor}|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroL210>().Which;
        registro.CampoCodigo.Should().Be(codigo);
        registro.Descricao.Should().Be(descricao);
        registro.Valor.Should().Be(valor);
    }

    [Fact]
    public void Parser_DescricaoEValorOpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha("|L210|0001|||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroL210>().Which;
        registro.CampoCodigo.Should().Be("0001");
        registro.Descricao.Should().BeNull();
        registro.Valor.Should().BeNull();
    }
}
