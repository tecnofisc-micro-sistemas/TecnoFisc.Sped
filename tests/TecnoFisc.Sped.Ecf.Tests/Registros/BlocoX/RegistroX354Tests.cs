using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote1;

public sealed class RegistroX354Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX354(), "X354", "0:1");
    }

    [Fact]
    public void Parser_LePrejuizosESaldoComEscalaDois()
    {
        var resultado = new ParserEcf().ParseLinha("|X354|100000,00|250000,00|50000,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX354>().Which;
        registro.ResNegAnt.Should().Be(100000m);
        registro.ResNegAntReal.Should().Be(250000m);
        registro.SaldoNegAcum.Should().Be(50000m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_DecimalInvalido_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|X354|100000,00|INVALIDO|50000,00|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroX354>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == "RES_NEG_ANT_REAL" && erro.ValorBruto == "INVALIDO");
    }
}
