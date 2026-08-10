using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoN;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoN.Lote1;

public sealed class RegistroN630Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroN630(), "N630", "0:N");
    }

    [Fact]
    public void Parser_PreservaLinhaIrpjComoDadosDaTabelaDinamica()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|N630|0021|IMPOSTO DE RENDA MENSAL PAGO POR ESTIMATIVA|-10000,00|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroN630>().Which;
        registro.CampoCodigo.Should().Be("0021");
        registro.Descricao.Should().Be("IMPOSTO DE RENDA MENSAL PAGO POR ESTIMATIVA");
        registro.Valor.Should().Be("-10000,00");
    }

    [Fact]
    public void Parser_ValorTextualNaoNumerico_PreservaSemErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|N630|R001|ROTULO|NAO CALCULADO|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroN630>().Which;
        registro.Valor.Should().Be("NAO CALCULADO");
        registro.ErrosDeFormato.Should().BeEmpty();
    }
}
