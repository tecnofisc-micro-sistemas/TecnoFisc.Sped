using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoC;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoC;

public sealed class RegistroC157Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroC157(), "C157", "1:N");
    }

    [Fact]
    public void Parser_LeDecimalIndicadorOpcionalELinhaDaEcd()
    {
        var resultado = new ParserEcf().ParseLinha("|C157|OLD001||350,75|C|124|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroC157>().Which;
        registro.CodCcus.Should().BeNull();
        registro.VlSldFin.Should().Be(350.75m);
        registro.IndVlSldFin.Should().Be(IndicadorDebitoCredito.Credor);
        registro.LinhaEcd.Should().Be(124);
    }

    [Fact]
    public void Parser_IndicadorVazio_PreservaNulo()
    {
        var resultado = new ParserEcf().ParseLinha("|C157|OLD001||350,75||124|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroC157>()
            .Which.IndVlSldFin.Should().BeNull();
    }
}
