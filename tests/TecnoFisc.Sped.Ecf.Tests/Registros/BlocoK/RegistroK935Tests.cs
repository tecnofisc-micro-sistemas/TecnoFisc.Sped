using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoK;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoK;

public sealed class RegistroK935Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroK935(), "K935", "1:N");
    }

    [Fact]
    public void Parser_LeDivergenciaDeResultadoEPreservaCodigoDaRegra()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|K935|T01|0004.01.01.001||REGRA_COMPATIBILIDADE_K355_E355|" +
            "30,00|D|40,00|D|JUSTIFICATIVA SINTETICA PARA DIVERGENCIA|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroK935>().Which;
        registro.PerApur.Should().Be("T01");
        registro.CodCta.Should().Be("0004.01.01.001");
        registro.CodCcus.Should().BeNull();
        registro.IdRegra.Should().Be("REGRA_COMPATIBILIDADE_K355_E355");
        registro.VlSldFinEsp.Should().Be(30m);
        registro.IndVlSldFinEsp.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.SldFinPre.Should().Be(40m);
        registro.IndSldFinPre.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.Justificativa.Should().Be("JUSTIFICATIVA SINTETICA PARA DIVERGENCIA");
    }

    [Fact]
    public void Parser_ValoresCalculadosOpcionaisVazios_PreservamNulos()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|K935|A00|0004||CODIGO_DE_PROCESSO|||||JUSTIFICATIVA SINTETICA SUFICIENTE|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroK935>().Which;
        registro.CodCcus.Should().BeNull();
        registro.VlSldFinEsp.Should().BeNull();
        registro.IndVlSldFinEsp.Should().BeNull();
        registro.SldFinPre.Should().BeNull();
        registro.IndSldFinPre.Should().BeNull();
    }
}
