using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoP;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoP;

public sealed class RegistroP150Tests
{
    [Fact]
    public void Registro_ConformeManifestoInclusiveAliasCodigo()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroP150(), "P150", "0:N");
    }

    [Fact]
    public void Parser_LeContaResultadoDecimalComSinalEEnums()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|P150|3.11.05.01.03.03|OUTRAS PARTICIPACOES|A|6|04|3.11.05.01.03|-10000,25|D|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroP150>().Which;
        registro.CampoCodigo.Should().Be("3.11.05.01.03.03");
        registro.Tipo.Should().Be(IndicadorTipoConta.Analitica);
        registro.Nivel.Should().Be(6);
        registro.CodNat.Should().Be("04");
        registro.CodCtaSup.Should().Be("3.11.05.01.03");
        registro.Valor.Should().Be(-10000.25m);
        registro.IndValor.Should().Be(IndicadorDebitoCredito.Devedor);
    }

    [Fact]
    public void Parser_OpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|P150|000001||S||||0,00|C|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroP150>().Which;
        registro.Descricao.Should().BeNull();
        registro.Nivel.Should().BeNull();
        registro.CodNat.Should().BeNull();
        registro.CodCtaSup.Should().BeNull();
    }

    [Fact]
    public void Parser_DecimalEEnumsInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|P150|000001||X||||INVALIDO|Y|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroP150>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([
                "TIPO",
                "VALOR",
                "IND_VALOR",
            ]);
    }
}
