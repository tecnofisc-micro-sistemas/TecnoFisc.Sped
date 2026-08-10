using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoC;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoC;

public sealed class RegistroC355Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroC355(), "C355", "0:N");
    }

    [Fact]
    public void Parser_LeDecimalIndicadorELinhaDaEcdComCentroOpcional()
    {
        var resultado = new ParserEcf().ParseLinha("|C355|003.01||2500,00|C|125|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroC355>().Which;
        registro.CodCcus.Should().BeNull();
        registro.VlCta.Should().Be(2500.00m);
        registro.IndVlCta.Should().Be(IndicadorDebitoCredito.Credor);
        registro.LinhaEcd.Should().Be(125);
    }
}
