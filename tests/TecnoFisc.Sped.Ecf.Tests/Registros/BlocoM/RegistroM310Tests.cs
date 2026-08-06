using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoM;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoM.Lote1;

public sealed class RegistroM310Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroM310(), "M310", "0:N");
    }

    [Fact]
    public void Parser_LeContaCentroDeCustoOpcionalValorEIndicador()
    {
        var resultado = new ParserEcf().ParseLinha("|M310|01.01.01.01||1000,00|C|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroM310>().Which;
        registro.CodCta.Should().Be("01.01.01.01");
        registro.CodCcus.Should().BeNull();
        registro.VlCta.Should().Be(1000m);
        registro.IndVlCta.Should().Be(IndicadorDebitoCredito.Credor);
    }

    [Fact]
    public void Parser_ValorEIndicadorInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|M310|01|CC01|INVALIDO|X|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroM310>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([nameof(RegistroM310.VlCta), nameof(RegistroM310.IndVlCta)]);
    }
}
