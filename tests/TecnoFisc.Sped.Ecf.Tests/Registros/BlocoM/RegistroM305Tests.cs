using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoM;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoM.Lote1;

public sealed class RegistroM305Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroM305(), "M305", "0:N");
    }

    [Fact]
    public void Parser_LeContaParteBValorDecimalEIndicador()
    {
        var resultado = new ParserEcf().ParseLinha("|M305|000123|2000,25|D|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroM305>().Which;
        registro.CodCtaB.Should().Be("000123");
        registro.VlCta.Should().Be(2000.25m);
        registro.IndVlCta.Should().Be(IndicadorDebitoCredito.Devedor);
    }

    [Fact]
    public void Parser_ValorEIndicadorInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|M305|000123|INVALIDO|X|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroM305>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain(["VL_CTA", "IND_VL_CTA"]);
    }
}
