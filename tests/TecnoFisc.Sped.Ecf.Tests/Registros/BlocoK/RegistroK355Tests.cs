using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoK;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoK;

public sealed class RegistroK355Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroK355(), "K355", "0:N");
    }

    [Fact]
    public void Parser_LeSaldoResultadoEPreservaCentroDeCustosVazio()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|K355|0004.01.01.001||5000,00|C|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroK355>().Which;
        registro.CodCta.Should().Be("0004.01.01.001");
        registro.CodCcus.Should().BeNull();
        registro.VlSldFin.Should().Be(5000m);
        registro.IndVlSldFin.Should().Be(IndicadorDebitoCredito.Credor);
    }
}
