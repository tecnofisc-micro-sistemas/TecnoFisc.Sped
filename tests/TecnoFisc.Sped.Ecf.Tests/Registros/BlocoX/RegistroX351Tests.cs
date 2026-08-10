using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote1;

public sealed class RegistroX351Tests
{
    private const string LinhaCompleta =
        "|X351|-100,00|-250,00|-10,00|-25,00|5,00|12,50|90,00|225,00|50,00|125,00|20,00|50,00|5,00|12,50|7,00|";

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX351(), "X351", "0:1");
    }

    [Fact]
    public void Parser_LeResultadosSinalizadosETributosComEscalaDois()
    {
        var resultado = new ParserEcf().ParseLinha(LinhaCompleta);

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX351>().Which;
        registro.ResInvPer.Should().Be(-100m);
        registro.ResInvPerReal.Should().Be(-250m);
        registro.ResIsenPetrPer.Should().Be(-10m);
        registro.ResIsenPetrPerReal.Should().Be(-25m);
        registro.ResNegAcum.Should().Be(5m);
        registro.ResPosTrib.Should().Be(90m);
        registro.ImpLucr.Should().Be(50m);
        registro.ImpRetBr.Should().Be(7m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_NaoRecalculaResultadoTributavel()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X351|1,00|2,00|3,00|4,00|5,00|6,00|999,00|888,00|7,00|8,00|9,00|10,00|11,00|12,00|13,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX351>().Which;
        registro.ResPosTrib.Should().Be(999m);
        registro.ResPosTribReal.Should().Be(888m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_DecimalInvalido_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            LinhaCompleta.Replace("|7,00|", "|INVALIDO|", StringComparison.Ordinal));

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroX351>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == "IMP_RET_BR" && erro.ValorBruto == "INVALIDO");
    }
}
