using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoE;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoE;

public sealed class RegistroE355Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroE355(), "E355", "0:N");
    }

    [Fact]
    public void Parser_LeSaldoAntesDoEncerramentoEPermiteCentroDeCustosVazio()
    {
        var resultado = new ParserEcf().ParseLinha("|E355|003.01||500,75|C|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroE355>().Which;
        registro.CodCta.Should().Be("003.01");
        registro.CodCcus.Should().BeNull();
        registro.VlSldFin.Should().Be(500.75m);
        registro.IndVlSldFin.Should().Be(IndicadorDebitoCredito.Credor);
    }
}
