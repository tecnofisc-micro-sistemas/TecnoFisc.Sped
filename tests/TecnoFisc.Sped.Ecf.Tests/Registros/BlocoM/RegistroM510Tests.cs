using System.Reflection;

using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoM;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoM.Lote2;

public sealed class RegistroM510Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroM510(), "M510", "0:N");
    }

    [Theory]
    [InlineData(nameof(RegistroM510.VlLctoParteB), "VL_LCTO_PARTEB")]
    [InlineData(nameof(RegistroM510.IndVlLctoParteB), "IND_VL_LCTO_PARTEB")]
    [InlineData(nameof(RegistroM510.IndSdFimLal), "IND_SD_FIM_LAL")]
    public void CamposQuebradosVisualmente_UsamNomeNormativoCompleto(
        string nomePropriedade,
        string nomeNormativo)
    {
        PropertyInfo propriedade = typeof(RegistroM510).GetProperty(nomePropriedade)!;
        CampoSpedAttribute campo = propriedade.GetCustomAttribute<CampoSpedAttribute>()!;

        campo.Nome.Should().Be(nomeNormativo);
    }

    [Fact]
    public void Parser_LeContaPadraoDescricaoTributoSaldosValoresEIndicadores()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|M510|001005|PROVISOES NAO DEDUTIVEIS|C|2000,00|D|500,00|C|-100,00|D|1400,00|D|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroM510>().Which;
        registro.CodPbRfb.Should().Be("001005");
        registro.DescricaoPbRfb.Should().Be("PROVISOES NAO DEDUTIVEIS");
        registro.CodTributo.Should().Be(IndicadorTributoContaParteB.Csll);
        registro.SdIniLal.Should().Be(2000m);
        registro.IndSdIniLal.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.VlLctoParteA.Should().Be(500m);
        registro.IndVlLctoParteA.Should().Be(IndicadorDebitoCredito.Credor);
        registro.VlLctoParteB.Should().Be(-100m);
        registro.IndVlLctoParteB.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.SdFimLal.Should().Be(1400m);
        registro.IndSdFimLal.Should().Be(IndicadorDebitoCredito.Devedor);
    }

    [Fact]
    public void Parser_TributoDecimalEIndicadoresInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|M510|001005|DESCRICAO|X|INVALIDO|X|0,00|X|0,00|X|0,00|X|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroM510>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([
                nameof(RegistroM510.CodTributo),
                nameof(RegistroM510.SdIniLal),
                nameof(RegistroM510.IndSdIniLal),
                nameof(RegistroM510.IndVlLctoParteA),
                "IND_VL_LCTO_PARTEB",
                "IND_SD_FIM_LAL",
            ]);
    }
}
