using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote1;

public sealed class RegistroX352Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX352(), "X352", "0:1");
    }

    [Fact]
    public void Parser_LeResultadosSinalizadosELucros()
    {
        var resultado = new ParserEcf().ParseLinha("|X352|-100000,00|-250000,00|5000,00|12500,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX352>().Which;
        registro.ResPer.Should().Be(-100000m);
        registro.ResPerReal.Should().Be(-250000m);
        registro.LucDisp.Should().Be(5000m);
        registro.LucDispReal.Should().Be(12500m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_DecimalInvalido_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|X352|INVALIDO|-250000,00|0,00|0,00|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroX352>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == nameof(RegistroX352.ResPer) && erro.ValorBruto == "INVALIDO");
    }
}
