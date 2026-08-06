using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoJ;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoJ;

public sealed class RegistroJ050Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroJ050(), "J050", "0:N");
    }

    [Fact]
    public void Parser_LeDataTipoNivelEPreservaCodigosDeConta()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|J050|01012025|01|A|4|0001.0001|0001|CAIXA ANALITICA|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroJ050>().Which;
        registro.DtAlt.Should().Be(new DateOnly(2025, 1, 1));
        registro.CodNat.Should().Be(CodigoNaturezaContaContabil.ContasDeAtivo);
        registro.IndCta.Should().Be(IndicadorTipoConta.Analitica);
        registro.Nível.Should().Be(4);
        registro.CodCta.Should().Be("0001.0001");
        registro.CodCtaSup.Should().Be("0001");
        registro.Cta.Should().Be("CAIXA ANALITICA");
    }

    [Theory]
    [InlineData("01", CodigoNaturezaContaContabil.ContasDeAtivo)]
    [InlineData("02", CodigoNaturezaContaContabil.ContasDePassivo)]
    [InlineData("03", CodigoNaturezaContaContabil.PatrimonioLiquido)]
    [InlineData("04", CodigoNaturezaContaContabil.ContasDeResultado)]
    [InlineData("05", CodigoNaturezaContaContabil.ContasDeCompensacao)]
    [InlineData("09", CodigoNaturezaContaContabil.Outras)]
    public void Parser_ReusaDominioNormativoDeNaturezaDaConta(
        string valor,
        CodigoNaturezaContaContabil esperado)
    {
        var resultado = new ParserEcf().ParseLinha(
            $"|J050|01012025|{valor}|S|1|0001||CONTA|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroJ050>()
            .Which.CodNat.Should().Be(esperado);
    }
}
