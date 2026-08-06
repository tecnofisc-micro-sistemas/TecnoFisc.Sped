using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoU;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoU;

public sealed class RegistroU150Tests
{
    [Fact]
    public void Registro_ConformeManifestoInclusiveAliasCodigo()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroU150(), "U150", "0:N");
    }

    [Fact]
    public void Parser_LeContaResultadoDecimalComSinalEEnums()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|U150|3.04.01.01|GANHOS NA ALIENACAO|A|4|04|3.04.01|-10000,25|D|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroU150>().Which;
        registro.CampoCodigo.Should().Be("3.04.01.01");
        registro.Descricao.Should().Be("GANHOS NA ALIENACAO");
        registro.Tipo.Should().Be(IndicadorTipoConta.Analitica);
        registro.Nivel.Should().Be(4);
        registro.CodNat.Should().Be("04");
        registro.CodCtaSup.Should().Be("3.04.01");
        registro.Valor.Should().Be(-10000.25m);
        registro.IndValor.Should().Be(IndicadorDebitoCredito.Devedor);
    }

    [Fact]
    public void Parser_OpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha("|U150|000001||S||||0,00|C|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroU150>().Which;
        registro.Descricao.Should().BeNull();
        registro.Nivel.Should().BeNull();
        registro.CodNat.Should().BeNull();
        registro.CodCtaSup.Should().BeNull();
    }

    [Fact]
    public void Parser_DecimalEEnumsInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|U150|000001||X|NIVEL|||INVALIDO|Y|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroU150>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([
                nameof(RegistroU150.Tipo),
                nameof(RegistroU150.Nivel),
                nameof(RegistroU150.Valor),
                nameof(RegistroU150.IndValor),
            ]);
    }
}
