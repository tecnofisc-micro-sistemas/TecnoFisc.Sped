using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoL;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoL;

public sealed class RegistroL300Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroL300(), "L300", "0:N");
    }

    [Theory]
    [InlineData("10000,50", 10000.50)]
    [InlineData("-25,75", -25.75)]
    [InlineData("0,00", 0)]
    public void Parser_LeSaldoComEscalaESinal(
        string valor,
        decimal esperado)
    {
        var resultado = new ParserEcf().ParseLinha(
            $"|L300|03.11.05.01.03.03|RESULTADO|A|6|04|03.11.05.01.03|{valor}|D|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroL300>().Which;
        registro.CampoCodigo.Should().Be("03.11.05.01.03.03");
        registro.Tipo.Should().Be(IndicadorTipoConta.Analitica);
        registro.Nivel.Should().Be(6);
        registro.CodNat.Should().Be("04");
        registro.CodCtaSup.Should().Be("03.11.05.01.03");
        registro.Valor.Should().Be(esperado);
        registro.IndValor.Should().Be(IndicadorDebitoCredito.Devedor);
    }

    [Fact]
    public void Parser_CamposOpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|L300|000001||S||||0,00|C|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroL300>().Which;
        registro.Descricao.Should().BeNull();
        registro.Nivel.Should().BeNull();
        registro.CodNat.Should().BeNull();
        registro.CodCtaSup.Should().BeNull();
    }

    [Fact]
    public void Parser_SaldoEIndicadorInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|L300|000001||S||||INVALIDO|X|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroL300>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([nameof(RegistroL300.Valor), nameof(RegistroL300.IndValor)]);
    }
}
