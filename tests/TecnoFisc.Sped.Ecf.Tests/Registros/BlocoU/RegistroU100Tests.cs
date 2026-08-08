using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoU;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoU;

public sealed class RegistroU100Tests
{
    [Fact]
    public void Registro_ConformeManifestoInclusiveAliasCodigo()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroU100(), "U100", "0:N");
    }

    [Fact]
    public void Parser_LeContaSaldosEEnumsEPreservaSaldoFinalTipoC()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|U100|2.03.02.04.01|SUPERAVIT ACUMULADO|A|5|03|2.03.02.04|10000,00|C|5000,25|15000,75|00020000,50|C|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroU100>().Which;
        registro.CampoCodigo.Should().Be("2.03.02.04.01");
        registro.Descricao.Should().Be("SUPERAVIT ACUMULADO");
        registro.Tipo.Should().Be(IndicadorTipoConta.Analitica);
        registro.Nivel.Should().Be(5);
        registro.CodNat.Should().Be("03");
        registro.CodCtaSup.Should().Be("2.03.02.04");
        registro.ValCtaRefIni.Should().Be(10000m);
        registro.IndValCtaRefIni.Should().Be(IndicadorDebitoCredito.Credor);
        registro.ValCtaRefDeb.Should().Be(5000.25m);
        registro.ValCtaRefCred.Should().Be(15000.75m);
        registro.ValCtaRefFin.Should().Be("00020000,50");
        registro.IndValCtaRefFin.Should().Be(IndicadorDebitoCredito.Credor);
    }

    [Fact]
    public void Parser_OpcionaisVaziosEPreservacaoLossless_NaoCalculamSaldo()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|U100|000001||S||||0,00|D|0,00|0,00|CALCULADO|D|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroU100>().Which;
        registro.Descricao.Should().BeNull();
        registro.Nivel.Should().BeNull();
        registro.CodNat.Should().BeNull();
        registro.CodCtaSup.Should().BeNull();
        registro.ValCtaRefFin.Should().Be("CALCULADO");
    }

    [Fact]
    public void Parser_ValoresNivelEEnumsInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|U100|000001||X|NIVEL|||INVALIDO|Y|DEBITO|0,00|0,00|Z|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroU100>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([
                "TIPO",
                "NIVEL",
                "VAL_CTA_REF_INI",
                "IND_VAL_CTA_REF_INI",
                "VAL_CTA_REF_DEB",
                "IND_VAL_CTA_REF_FIN",
            ]);
    }
}
