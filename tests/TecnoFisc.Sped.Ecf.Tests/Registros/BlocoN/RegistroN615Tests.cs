using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoN;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoN.Lote1;

public sealed class RegistroN615Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroN615(), "N615", "1:1");
    }

    [Fact]
    public void Parser_LeValoresDeclaradosSemExecutarCalculosTributarios()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|N615|2000,00|3,0000|-999,99|1,2500|7,50|-12,34|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroN615>().Which;
        registro.BaseCalc.Should().Be(2000m);
        registro.PerIncenFinor.Should().Be(3m);
        registro.VlLiqIncenFinor.Should().Be(-999.99m);
        registro.PerIncenFinam.Should().Be(1.25m);
        registro.VlLiqIncenFinam.Should().Be(7.50m);
        registro.VlTotal.Should().Be(-12.34m);
    }

    [Fact]
    public void Parser_DecimaisInvalidos_RegistramTodosOsErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|N615|INVALIDO|INVALIDO|INVALIDO|INVALIDO|INVALIDO|INVALIDO|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroN615>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([
                nameof(RegistroN615.BaseCalc),
                nameof(RegistroN615.PerIncenFinor),
                nameof(RegistroN615.VlLiqIncenFinor),
                nameof(RegistroN615.PerIncenFinam),
                nameof(RegistroN615.VlLiqIncenFinam),
                nameof(RegistroN615.VlTotal),
            ]);
    }
}
