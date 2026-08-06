using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoK;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoK;

public sealed class RegistroK156Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroK156(), "K156", "1:N");
    }

    [Fact]
    public void Parser_LeMapeamentoReferencialComSaldosFortementeTipados()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|K156|1.01.01.01.01|0,00|D|7500,00|5000,00|2500,00|D|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroK156>().Which;
        registro.CodCtaRef.Should().Be("1.01.01.01.01");
        registro.VlSldIni.Should().Be(0m);
        registro.IndVlSldIni.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.VlDeb.Should().Be(7500m);
        registro.VlCred.Should().Be(5000m);
        registro.VlSldFin.Should().Be(2500m);
        registro.IndVlSldFin.Should().Be(IndicadorDebitoCredito.Devedor);
    }
}
