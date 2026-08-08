using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoK;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoK;

public sealed class RegistroK356Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroK356(), "K356", "1:N");
    }

    [Fact]
    public void Parser_LeMapeamentoReferencialDoResultado()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|K356|3.01.01.01.01.01|5000,00|C|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroK356>().Which;
        registro.CodCtaRef.Should().Be("3.01.01.01.01.01");
        registro.VlSldFin.Should().Be(5000m);
        registro.IndVlSldFin.Should().Be(IndicadorDebitoCredito.Credor);
    }
}
