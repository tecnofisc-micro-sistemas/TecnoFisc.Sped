using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoM;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoM.Lote2;

public sealed class RegistroM410Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroM410(), "M410", "0:N");
    }

    [Theory]
    [InlineData("CR", IndicadorLancamentoParteB.Credito)]
    [InlineData("DB", IndicadorLancamentoParteB.Debito)]
    [InlineData("PF", IndicadorLancamentoParteB.PrejuizoFiscal)]
    [InlineData("BC", IndicadorLancamentoParteB.BaseCalculoNegativa)]
    public void Parser_LeDominioCompletoDoIndicadorDeLancamento(
        string valor,
        IndicadorLancamentoParteB esperado)
    {
        var resultado = new ParserEcf().ParseLinha(
            $"|M410|000123|I|-125,50|{valor}||HISTORICO|N|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroM410>().Which;
        registro.CodCtaB.Should().Be("000123");
        registro.CodTributo.Should().Be(IndicadorTributoContaParteB.Irpj);
        registro.ValLanLalbPb.Should().Be(-125.50m);
        registro.IndValLanLalbPb.Should().Be(esperado);
        registro.CodCtaBCtp.Should().BeNull();
        registro.HistLanLalb.Should().Be("HISTORICO");
        registro.IndLanAnt.Should().Be(IndicadorSimNao.Nao);
    }

    [Fact]
    public void Parser_LeContrapartidaEIndicadorDeLancamentoAnterior()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|M410|000123|C|125,50|CR|000456|TRANSFERENCIA ENTRE CONTAS|S|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroM410>().Which;
        registro.CodTributo.Should().Be(IndicadorTributoContaParteB.Csll);
        registro.CodCtaBCtp.Should().Be("000456");
        registro.IndLanAnt.Should().Be(IndicadorSimNao.Sim);
    }

    [Fact]
    public void Parser_DominiosEValorInvalidos_RegistramTodosOsErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|M410||X|INVALIDO|ZZ||HISTORICO|X|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroM410>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([
                nameof(RegistroM410.CodTributo),
                nameof(RegistroM410.ValLanLalbPb),
                nameof(RegistroM410.IndValLanLalbPb),
                nameof(RegistroM410.IndLanAnt),
            ]);
    }
}
