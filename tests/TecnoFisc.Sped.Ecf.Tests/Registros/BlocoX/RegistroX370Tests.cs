using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote2;

public sealed class RegistroX370Tests
{
    [Fact]
    public void Registro_ConformeManifestoCompletoComVinteCampos()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX370(), "X370", "0:N");
    }

    [Fact]
    public void Parser_SemAjustes_PreservaDominiosLexicaisEOptionaisVazios()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X370|E00001|01|ENTIDADE 01|001|11212121||TRANSACAO CONTROLADA|100000,00|N||||PIC||N|N|N|N||");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX370>().Which;
        registro.Identificador.Should().Be("E00001");
        registro.TipoTransacao.Should().Be("01");
        registro.Pais.Should().Be("001");
        registro.CodNcm.Should().Be("11212121");
        registro.TipoDemais.Should().BeNull();
        registro.VlTransacao.Should().Be(100000m);
        registro.IndAjustes.Should().Be(IndicadorSimNao.Nao);
        registro.VlEspontaneo.Should().BeNull();
        registro.VlCompensatorio.Should().BeNull();
        registro.TipAjCompensatorio.Should().BeNull();
        registro.Metodo.Should().Be("PIC");
        registro.Descricao.Should().BeNull();
        registro.CompIntencional.Should().Be(IndicadorSimNao.Nao);
        registro.Sinergia.Should().Be(IndicadorSimNao.Nao);
        registro.IndTransCombinadas.Should().Be(IndicadorSimNao.Nao);
        registro.IndDadosMultip.Should().Be(IndicadorSimNao.Nao);
        registro.IndSimplific.Should().BeNull();
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_ComAjustes_PreservaMontantesMetodosECodigosSemCalculos()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X370|00A9Z9|03|ENTIDADE EXTERIOR|076||101|SERVICOS|1234,56|S|100,00|25,50|01|MLT|JUSTIFICATIVA|S|N|S|N|S|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX370>().Which;
        registro.TipoTransacao.Should().Be("03");
        registro.TipoDemais.Should().Be("101");
        registro.VlTransacao.Should().Be(1234.56m);
        registro.IndAjustes.Should().Be(IndicadorSimNao.Sim);
        registro.VlEspontaneo.Should().Be(100m);
        registro.VlCompensatorio.Should().Be(25.50m);
        registro.TipAjCompensatorio.Should().Be("01");
        registro.Metodo.Should().Be("MLT");
        registro.CompIntencional.Should().Be(IndicadorSimNao.Sim);
        registro.IndSimplific.Should().Be(IndicadorSimNao.Sim);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_FormatosInvalidos_IdentificaMontanteEIndicadoresSemValidarCalculos()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X370|E00001|99|ENTIDADE|001|||DESCRICAO|INVALIDO|X||||DOMINIO-LIVRE||?|N|S|N||");

        resultado.Sucesso.Should().BeTrue();
        var erros = resultado.Valor.Should().BeOfType<RegistroX370>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo);
        erros.Should().Contain([
            nameof(RegistroX370.VlTransacao),
            nameof(RegistroX370.IndAjustes),
            nameof(RegistroX370.CompIntencional),
        ]);
    }
}
