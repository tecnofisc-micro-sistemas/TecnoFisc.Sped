using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoK;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoK;

public sealed class RegistroK915Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroK915(), "K915", "1:N");
    }

    [Fact]
    public void Parser_LeDivergenciaPatrimonialComoDadosTipadosSemExecutarRegraFiscal()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|K915|T01|0001.01.01.001|000001|REGRA_COMPATIBILIDADE_K155_E155|" +
            "10,00|D|20,00|0,00|30,00|D|20,00|D|20,00|0,00|40,00|D|" +
            "JUSTIFICATIVA SINTETICA PARA DIVERGENCIA|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroK915>().Which;
        registro.PerApur.Should().Be("T01");
        registro.CodCta.Should().Be("0001.01.01.001");
        registro.CodCcus.Should().Be("000001");
        registro.IdRegra.Should().Be("REGRA_COMPATIBILIDADE_K155_E155");
        registro.VlSldIniEsp.Should().Be(10m);
        registro.IndVlSldIniEsp.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.VlDebEsp.Should().Be(20m);
        registro.VlCredEsp.Should().Be(0m);
        registro.VlSldFinEsp.Should().Be(30m);
        registro.IndVlSldFinEsp.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.SldIniPre.Should().Be(20m);
        registro.IndSldIniPre.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.VlDebPre.Should().Be(20m);
        registro.VlCredPre.Should().Be(0m);
        registro.SldFinPre.Should().Be(40m);
        registro.IndSldFinPre.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.Justificativa.Should().Be("JUSTIFICATIVA SINTETICA PARA DIVERGENCIA");
    }

    [Fact]
    public void Parser_ValoresCalculadosOpcionaisVazios_PreservamNulos()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|K915|A00|0001||CODIGO_DE_PROCESSO|||||||||||||JUSTIFICATIVA SINTETICA SUFICIENTE|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroK915>().Which;
        registro.CodCcus.Should().BeNull();
        registro.VlSldIniEsp.Should().BeNull();
        registro.IndVlSldIniEsp.Should().BeNull();
        registro.VlDebEsp.Should().BeNull();
        registro.VlCredEsp.Should().BeNull();
        registro.VlSldFinEsp.Should().BeNull();
        registro.IndVlSldFinEsp.Should().BeNull();
        registro.SldIniPre.Should().BeNull();
        registro.IndSldIniPre.Should().BeNull();
        registro.VlDebPre.Should().BeNull();
        registro.VlCredPre.Should().BeNull();
        registro.SldFinPre.Should().BeNull();
        registro.IndSldFinPre.Should().BeNull();
    }

    [Fact]
    public void Parser_ValorEIndicadorOpcionaisInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|K915|A00|0001||REGRA|INVALIDO|X|||||||||||JUSTIFICATIVA SINTETICA SUFICIENTE|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroK915>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain(["VL_SLD_INI_ESP", "IND_VL_SLD_INI_ESP"]);
    }
}
