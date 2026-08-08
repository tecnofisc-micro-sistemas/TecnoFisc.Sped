using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoW;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoW;

public sealed class RegistroW200Tests
{
    private const string LinhaCompleta =
        "|W200|DE|100|500|200|700|300|1200|-50|-250|30|150|40|200|" +
        "1000|5000|800|4000|2500|12500|321|";

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroW200(), "W200", "0:N");
    }

    [Fact]
    public void Parser_LeJurisdicaoMontantesInteirosESinalizados()
    {
        var resultado = new ParserEcf().ParseLinha(LinhaCompleta);

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroW200>().Which;
        registro.Jurisdicao.Should().Be("DE");
        registro.VlRecNaoRelEst.Should().Be(100m);
        registro.VlRecNaoRel.Should().Be(500m);
        registro.VlRecRelEst.Should().Be(200m);
        registro.VlRecRel.Should().Be(700m);
        registro.VlRecTotalEst.Should().Be(300m);
        registro.VlRecTotal.Should().Be(1200m);
        registro.VlLucPrejAntesIrEst.Should().Be(-50m);
        registro.VlLucPrejAntesIr.Should().Be(-250m);
        registro.VlIrPagoEst.Should().Be(30m);
        registro.VlIrPago.Should().Be(150m);
        registro.VlIrDevidoEst.Should().Be(40m);
        registro.VlIrDevido.Should().Be(200m);
        registro.VlCapSocEst.Should().Be(1000m);
        registro.VlCapSoc.Should().Be(5000m);
        registro.VlLucAcumEst.Should().Be(800m);
        registro.VlLucAcum.Should().Be(4000m);
        registro.VlAtivTangEst.Should().Be(2500m);
        registro.VlAtivTang.Should().Be(12500m);
        registro.NumEmp.Should().Be(321);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_OpcionaisVaziosEPaisSemResidencia_SaoPreservados()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|W200|X5||600||800||1400||300||100||120||4000||3500||7500|98|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroW200>().Which;
        registro.Jurisdicao.Should().Be("X5");
        registro.VlRecNaoRelEst.Should().BeNull();
        registro.VlLucPrejAntesIrEst.Should().BeNull();
        registro.VlRecNaoRel.Should().Be(600m);
        registro.VlLucPrejAntesIr.Should().Be(300m);
        registro.NumEmp.Should().Be(98);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Theory]
    [InlineData("INVALIDO", "321", "VL_REC_NAO_REL")]
    [InlineData("500", "3,5", "NUM_EMP")]
    public void Parser_FormatoNumericoInvalido_RegistraErro(
        string receita,
        string empregados,
        string campo)
    {
        string linha =
            $"|W200|DE||{receita}||700||1200||-250||150||200||5000||4000||12500|{empregados}|";

        var resultado = new ParserEcf().ParseLinha(linha);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroW200>()
            .Which.ErrosDeFormato.Should().Contain(erro => erro.Campo == campo);
    }
}
