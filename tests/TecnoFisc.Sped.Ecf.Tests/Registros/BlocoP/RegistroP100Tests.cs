using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoP;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoP;

public sealed class RegistroP100Tests
{
    [Fact]
    public void Registro_ConformeManifestoInclusiveAliasCodigo()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroP100(), "P100", "0:N");
    }

    [Fact]
    public void Parser_LeContaSaldosEEnumsEPreservaSaldoFinalTipoC()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|P100|02.03.04.01.99|CONTA PATRIMONIAL|A|5|03|02.03.04.01|10000,00|C|5000,25|15000,75|00020000,50|C|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroP100>().Which;
        registro.CampoCodigo.Should().Be("02.03.04.01.99");
        registro.Descricao.Should().Be("CONTA PATRIMONIAL");
        registro.Tipo.Should().Be(IndicadorTipoConta.Analitica);
        registro.Nivel.Should().Be(5);
        registro.CodNat.Should().Be("03");
        registro.CodCtaSup.Should().Be("02.03.04.01");
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
            "|P100|000001||S||||0,00|D|0,00|0,00|CALCULADO|D|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroP100>().Which;
        registro.Descricao.Should().BeNull();
        registro.Nivel.Should().BeNull();
        registro.CodNat.Should().BeNull();
        registro.CodCtaSup.Should().BeNull();
        registro.ValCtaRefFin.Should().Be("CALCULADO");
    }

    [Fact]
    public void Parser_ValorENivelInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|P100|000001||S|NIVEL|||INVALIDO|D|0,00|0,00|0,00|D|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroP100>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([nameof(RegistroP100.Nivel), nameof(RegistroP100.ValCtaRefIni)]);
    }
}
