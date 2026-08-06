using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote1;

public sealed class RegistroX350Tests
{
    private const string LinhaCompleta =
        "|X350|1000,00|600,00|400,00|100,00|0,00|40,00|10,00|450,00|-50,00|0,00|0,00|400,00|-25,00|100,00|275,00|";

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX350(), "X350", "0:1");
    }

    [Fact]
    public void Parser_LeMontantesComEscalaDoisESinaisPermitidos()
    {
        var resultado = new ParserEcf().ParseLinha(LinhaCompleta);

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX350>().Which;
        registro.RecLiq.Should().Be(1000m);
        registro.Custos.Should().Be(600m);
        registro.LucBruto.Should().Be(400m);
        registro.LucOper.Should().Be(450m);
        registro.RecPartic.Should().Be(-50m);
        registro.LucLiqAntIr.Should().Be(400m);
        registro.LucArbAntIr.Should().Be(-25m);
        registro.ImpDev.Should().Be(100m);
        registro.LucLiq.Should().Be(275m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_NaoCalculaResultadosOuConsolidacao()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X350|1,00|2,00|999,00|3,00|4,00|5,00|6,00|-777,00|-8,00|9,00|10,00|123,00|-11,00|12,00|456,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX350>().Which;
        registro.LucBruto.Should().Be(999m);
        registro.LucOper.Should().Be(-777m);
        registro.LucLiqAntIr.Should().Be(123m);
        registro.LucLiq.Should().Be(456m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_DecimalInvalido_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            LinhaCompleta.Replace("1000,00", "INVALIDO", StringComparison.Ordinal));

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroX350>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == nameof(RegistroX350.RecLiq) && erro.ValorBruto == "INVALIDO");
    }
}
