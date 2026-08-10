using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoU;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoU;

public sealed class RegistroU182Tests
{
    [Fact]
    public void Registro_ConformeManifestoInclusiveAliasCodigo()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroU182(), "U182", "1:N");
    }

    [Theory]
    [InlineData("000001", "BASE DE CALCULO DA CSLL", "10000,00")]
    [InlineData("000099", null, "9,0000%")]
    [InlineData("000100", "AJUSTE", "VALOR-TABELA")]
    public void Parser_PreservaCodigoDeSeisPosicoesEDadosDinamicos(
        string codigo,
        string? descricao,
        string valor)
    {
        var resultado = new ParserEcf().ParseLinha($"|U182|{codigo}|{descricao}|{valor}|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroU182>().Which;
        registro.CampoCodigo.Should().Be(codigo);
        registro.Descricao.Should().Be(descricao);
        registro.Valor.Should().Be(valor);
    }

    [Fact]
    public void Parser_OpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha("|U182|000001|||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroU182>().Which;
        registro.CampoCodigo.Should().Be("000001");
        registro.Descricao.Should().BeNull();
        registro.Valor.Should().BeNull();
    }
}
