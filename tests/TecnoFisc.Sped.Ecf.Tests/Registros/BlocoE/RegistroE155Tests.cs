using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoE;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoE;

public sealed class RegistroE155Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroE155(), "E155", "1:N");
    }

    [Fact]
    public void Parser_LeSaldosCalculadosDaEcdComoDadosTipados()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|E155|001.01|CC001|100,25|D|200,50|50,25|250,50|C|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroE155>().Which;
        registro.CodCta.Should().Be("001.01");
        registro.CodCcus.Should().Be("CC001");
        registro.VlSldIni.Should().Be(100.25m);
        registro.IndVlSldIni.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.VlDeb.Should().Be(200.50m);
        registro.VlCred.Should().Be(50.25m);
        registro.VlSldFin.Should().Be(250.50m);
        registro.IndVlSldFin.Should().Be(IndicadorDebitoCredito.Credor);
    }
}
