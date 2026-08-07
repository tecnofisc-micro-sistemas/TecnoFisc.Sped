using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote1;

public sealed class RegistroX353Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX353(), "X353", "0:1");
    }

    [Fact]
    public void Parser_LeMontantesDeConsolidacaoESinaisDoResultadoProprio()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X353|100000,00|250000,00|50000,00|125000,00|-10000,00|-50000,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX353>().Which;
        registro.ResNegUtil.Should().Be(100000m);
        registro.ResNegUtilReal.Should().Be(250000m);
        registro.SaldoResNegNaoUtil.Should().Be(50000m);
        registro.SaldoResNegNaoUtilReal.Should().Be(125000m);
        registro.ResProp.Should().Be(-10000m);
        registro.ResPropReal.Should().Be(-50000m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_NaoExecutaRegraDeConsolidacao()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X353|999,00|888,00|777,00|666,00|555,00|444,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX353>().Which;
        registro.ResNegUtil.Should().Be(999m);
        registro.SaldoResNegNaoUtil.Should().Be(777m);
        registro.ResProp.Should().Be(555m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_DecimalInvalido_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X353|100,00|250,00|50,00|125,00|INVALIDO|50,00|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroX353>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == "RES_PROP" && erro.ValorBruto == "INVALIDO");
    }
}
