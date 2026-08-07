using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote1;

public sealed class RegistroX355Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX355(), "X355", "0:1");
    }

    [Fact]
    public void Parser_LeRendasEPercentualComEscalasNormativas()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X355|100000,00|250000,00|1000000,00|2500000,00|900000,00|2250000,00|90,1234|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX355>().Which;
        registro.RendPassProp.Should().Be(100000m);
        registro.RendPassPropReal.Should().Be(250000m);
        registro.RendTotal.Should().Be(1000000m);
        registro.RendTotalReal.Should().Be(2500000m);
        registro.RendAtivProp.Should().Be(900000m);
        registro.RendAtivPropReal.Should().Be(2250000m);
        registro.Percentual.Should().Be(90.1234m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_NaoCalculaRendaAtivaOuPercentual()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X355|1,00|2,00|3,00|4,00|999,00|888,00|77,7777|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX355>().Which;
        registro.RendAtivProp.Should().Be(999m);
        registro.RendAtivPropReal.Should().Be(888m);
        registro.Percentual.Should().Be(77.7777m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_DecimalInvalido_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X355|1,00|2,00|3,00|4,00|5,00|6,00|INVALIDO|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroX355>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == "PERCENTUAL" && erro.ValorBruto == "INVALIDO");
    }
}
