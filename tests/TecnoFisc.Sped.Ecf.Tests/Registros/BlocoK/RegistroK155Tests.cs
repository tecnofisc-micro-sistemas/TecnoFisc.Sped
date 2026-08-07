using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoK;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoK;

public sealed class RegistroK155Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroK155(), "K155", "0:N");
    }

    [Fact]
    public void Parser_LeSaldosIndicadoresEPreservaCodigosComZeros()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|K155|0001.01.01.001|000001|0,00|D|7500,00|5000,00|2500,00|D|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroK155>().Which;
        registro.CodCta.Should().Be("0001.01.01.001");
        registro.CodCcus.Should().Be("000001");
        registro.VlSldIni.Should().Be(0m);
        registro.IndVlSldIni.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.VlDeb.Should().Be(7500m);
        registro.VlCred.Should().Be(5000m);
        registro.VlSldFin.Should().Be(2500m);
        registro.IndVlSldFin.Should().Be(IndicadorDebitoCredito.Devedor);
    }

    [Fact]
    public void Parser_CentroDeCustosOpcionalVazio_PreservaNulo()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|K155|0001.01.01.001||0,00|D|0,00|0,00|0,00|C|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroK155>()
            .Which.CodCcus.Should().BeNull();
    }

    [Fact]
    public void Parser_DecimalEIndicadorInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|K155|0001||INVALIDO|X|0,00|0,00|0,00|D|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroK155>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain(["VL_SLD_INI", "IND_VL_SLD_INI"]);
    }
}
