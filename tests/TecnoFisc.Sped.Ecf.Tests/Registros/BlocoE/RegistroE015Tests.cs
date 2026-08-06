using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoE;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoE;

public sealed class RegistroE015Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroE015(), "E015", "0:N");
    }

    [Fact]
    public void Parser_LeContaMapeadaComCentroDeCustos()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|E015|001.01|CC001|CAIXA RECUPERADO|750,25|D|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroE015>().Which;
        registro.CodCta.Should().Be("001.01");
        registro.CodCcus.Should().Be("CC001");
        registro.DescCta.Should().Be("CAIXA RECUPERADO");
        registro.ValCta.Should().Be(750.25m);
        registro.IndValCta.Should().Be(IndicadorDebitoCredito.Devedor);
    }

    [Fact]
    public void Parser_CentroDeCustosVazio_PreservaNulo()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|E015|001.01||CAIXA RECUPERADO|1,00|C|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroE015>()
            .Which.CodCcus.Should().BeNull();
    }
}
