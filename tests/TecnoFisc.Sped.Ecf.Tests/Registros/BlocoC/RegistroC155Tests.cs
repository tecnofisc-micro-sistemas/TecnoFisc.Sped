using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoC;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoC;

public sealed class RegistroC155Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroC155(), "C155", "1:N");
    }

    [Fact]
    public void Parser_LeDecimaisIndicadoresELinhaDaEcd()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|C155|001.01|CC001|1000,25|D|200,50|50,25|1150,50|C|123|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroC155>().Which;
        registro.VlSldIni.Should().Be(1000.25m);
        registro.IndVlSldIni.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.VlDeb.Should().Be(200.50m);
        registro.VlCred.Should().Be(50.25m);
        registro.VlSldFin.Should().Be(1150.50m);
        registro.IndVlSldFin.Should().Be(IndicadorDebitoCredito.Credor);
        registro.LinhaEcd.Should().Be(123);
    }

    [Fact]
    public void Parser_DecimalInvalido_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|C155|001.01||INVALIDO|D|0,00|0,00|0,00|D|1|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroC155>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == "VlSldIni" && erro.ValorBruto == "INVALIDO");
    }
}
