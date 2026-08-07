using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoM;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoM.Lote2;

public sealed class RegistroM360Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroM360(), "M360", "0:N");
    }

    [Fact]
    public void Parser_LeContaCentroDeCustoValorComSinalEIndicador()
    {
        var resultado = new ParserEcf().ParseLinha("|M360|2.02.03.04|CC0001|-250,75|D|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroM360>().Which;
        registro.CodCta.Should().Be("2.02.03.04");
        registro.CodCcus.Should().Be("CC0001");
        registro.VlCta.Should().Be(-250.75m);
        registro.IndVlCta.Should().Be(IndicadorDebitoCredito.Devedor);
    }

    [Fact]
    public void Parser_CentroDeCustoVazio_PreservaNulo()
    {
        var resultado = new ParserEcf().ParseLinha("|M360|2.02.03.04||250,75|C|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroM360>()
            .Which.CodCcus.Should().BeNull();
    }

    [Fact]
    public void Parser_ValorEIndicadorInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|M360|2.02.03.04||INVALIDO|X|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroM360>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain(["VL_CTA", "IND_VL_CTA"]);
    }
}
