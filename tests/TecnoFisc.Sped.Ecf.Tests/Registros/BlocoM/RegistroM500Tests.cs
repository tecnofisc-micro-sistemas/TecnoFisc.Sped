using System.Reflection;

using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoM;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoM.Lote2;

public sealed class RegistroM500Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroM500(), "M500", "0:N");
    }

    [Theory]
    [InlineData(nameof(RegistroM500.IndSdIniLal), "IND_SD_INI_LAL")]
    [InlineData(nameof(RegistroM500.VlLctoParteB), "VL_LCTO_PARTEB")]
    [InlineData(nameof(RegistroM500.IndVlLctoParteB), "IND_VL_LCTO_PARTEB")]
    [InlineData(nameof(RegistroM500.IndSdFimLal), "IND_SD_FIM_LAL")]
    public void CamposQuebradosVisualmente_UsamNomeNormativoCompleto(
        string nomePropriedade,
        string nomeNormativo)
    {
        PropertyInfo propriedade = typeof(RegistroM500).GetProperty(nomePropriedade)!;
        CampoSpedAttribute campo = propriedade.GetCustomAttribute<CampoSpedAttribute>()!;

        campo.Nome.Should().Be(nomeNormativo);
    }

    [Fact]
    public void Parser_LeContaTributoSaldosValoresComSinalEIndicadores()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|M500|000123|I|1000,00|C|-500,25|D|100,10|C|399,65|C|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroM500>().Which;
        registro.CodCtaB.Should().Be("000123");
        registro.CodTributo.Should().Be(IndicadorTributoContaParteB.Irpj);
        registro.SdIniLal.Should().Be(1000m);
        registro.IndSdIniLal.Should().Be(IndicadorDebitoCredito.Credor);
        registro.VlLctoParteA.Should().Be(-500.25m);
        registro.IndVlLctoParteA.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.VlLctoParteB.Should().Be(100.10m);
        registro.IndVlLctoParteB.Should().Be(IndicadorDebitoCredito.Credor);
        registro.SdFimLal.Should().Be(399.65m);
        registro.IndSdFimLal.Should().Be(IndicadorDebitoCredito.Credor);
    }

    [Fact]
    public void Parser_TributoDecimalEIndicadoresInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|M500|000123|X|INVALIDO|X|0,00|X|0,00|X|0,00|X|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroM500>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([
                "COD_TRIBUTO",
                "SD_INI_LAL",
                "IND_SD_INI_LAL",
                "IND_VL_LCTO_PARTE_A",
                "IND_VL_LCTO_PARTEB",
                "IND_SD_FIM_LAL",
            ]);
    }
}
