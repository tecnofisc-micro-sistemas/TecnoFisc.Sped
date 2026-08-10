using System.Reflection;

using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoY;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoY.Lote1;

public sealed class RegistroY520Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistroY520()
    {
        AssertRegistroEcf.CodesAreImplemented("Y520");
    }

    [Fact]
    public void DominiosFechados_CoincidemComTabelasCompletasDoManual()
    {
        ValoresSped<TipoOperacaoExterior>().Should().Equal("R", "P");
        ValoresSped<FormaPagamentoRecebimentoExterior>().Should()
            .Equal("1", "2", "3", "4", "5", "6");
    }

    [Fact]
    public void Parser_LeDominiosCodigosLosslessEValor()
    {
        var resultado = new ParserEcf().ParseLinha("|Y520|R|001|1|00500|100000,25|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroY520>().Which;
        registro.TipExt.Should().Be(TipoOperacaoExterior.RendimentosRecebidos);
        registro.Pais.Should().Be("001");
        registro.Forma.Should().Be(FormaPagamentoRecebimentoExterior.OperacaoCambio);
        registro.NatOper.Should().Be("00500");
        registro.VlPeriodo.Should().Be(100000.25m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_DominiosEValorInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|Y520|X|076|9|10500|INVALIDO|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroY520>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([
                "TIP_EXT",
                "FORMA",
                "VL_PERIODO",
            ]);
    }


    private static string[] ValoresSped<TEnum>() where TEnum : struct, Enum
        => typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(campo => campo.GetCustomAttribute<SpedValorAttribute>()!.Valor)
            .ToArray();
}
