using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoC;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoC;

public sealed class RegistroC050Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroC050(), "C050", "0:N");
    }

    [Fact]
    public void Parser_LeDataNaturezaTipoNivelEPreservaCodigos()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|C050|01012025|01|A|4|001.01|001|CAIXA|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroC050>().Which;
        registro.DtAlt.Should().Be(new DateOnly(2025, 1, 1));
        registro.CodNat.Should().Be(CodigoNaturezaContaContabil.ContasDeAtivo);
        registro.IndCta.Should().Be(IndicadorTipoConta.Analitica);
        registro.Nível.Should().Be(4);
        registro.CodCta.Should().Be("001.01");
        registro.CodCtaSup.Should().Be("001");
    }
}
