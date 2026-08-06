using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote2;

public sealed class RegistroX371Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX371(), "X371", "0:N");
    }

    [Theory]
    [InlineData("D", IndicadorDebitoCredito.Devedor)]
    [InlineData("C", IndicadorDebitoCredito.Credor)]
    public void Parser_LeContaCentroOpcionalSaldoEIndicador(string valorIndicador, IndicadorDebitoCredito indicador)
    {
        var resultado = new ParserEcf().ParseLinha(
            $"|X371|2328.2.0001||25000,00|{valorIndicador}|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX371>().Which;
        registro.CodCta.Should().Be("2328.2.0001");
        registro.CodCcus.Should().BeNull();
        registro.Valor.Should().Be(25000m);
        registro.IndValor.Should().Be(indicador);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_IndicadorInvalido_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|X371|2328.2.0001|CC-01|25,00|X|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroX371>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == nameof(RegistroX371.IndValor) && erro.ValorBruto == "X");
    }
}
