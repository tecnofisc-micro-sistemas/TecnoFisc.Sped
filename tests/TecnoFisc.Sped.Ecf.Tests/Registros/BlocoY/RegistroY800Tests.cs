using TecnoFisc.Sped.Core.Erros;
using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.BlocoY;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoY.Lote2;

public sealed class RegistroY800Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistroY800()
    {
        AssertRegistroEcf.CodesAreImplemented("Y800");
    }

    [Fact]
    public void Registro_MultilinhaConformeManifestoETerminadorDeArquivo()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroY800(), "Y800", "0:N");
        var metadados = new CatalogoSpedGerado().EnumerarRegistros()
            .Single(registro => registro.Codigo == "Y800");

        metadados.TokenFimArquivo.Should().Be("Y800FIM");
        metadados.Campos.Single(campo => campo.Ordem == 5).CampoArquivo.Should().BeTrue();
    }

    [Theory]
    [InlineData("001", TipoDocumentoY800.MemoriaCalculoIncorporacao)]
    [InlineData("002", TipoDocumentoY800.LaudoAvaliacaoValorJusto)]
    [InlineData("003", TipoDocumentoY800.Outros)]
    public void Parser_LeDominioCompletoDeTipoDocumento(string token, TipoDocumentoY800 esperado)
    {
        var registro = new ParserEcf().ParseLinha(
            $"|Y800|{token}|DESCRICAO||RTF|Y800FIM|").Valor
            .Should().BeOfType<RegistroY800>().Which;

        registro.TipoDoc.Should().Be(esperado);
    }

    [Fact]
    public async Task ReadWriterReparse_RtfComQuebrasEPipes_PreservaConteudoEParaNoTerminador()
    {
        const string arqRtf = "{\\rtf1\\ansi linha A|com pipe\r\nlinha B|tambem|com pipes}";
        const string texto =
            "|0000|LECF|0012|11111111000191|EMPRESA TESTE|0|0|||01012025|31122025|N||0||\r\n" +
            "|Y001|0|\r\n" +
            "|Y800|001|MEMORIA||{\\rtf1\\ansi linha A|com pipe\r\n" +
            "linha B|tambem|com pipes}|Y800FIM|\r\n" +
            "|Y990|3|\r\n";
        await using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(texto), writable: false);
        var parser = new ParserEcf();

        var arquivo = await parser.ReadAsync(entrada, TestContext.Current.CancellationToken);
        var y800 = arquivo.BlocoY.Registros.OfType<RegistroY800>().Should().ContainSingle().Which;
        y800.ArqRtf.Should().Be(arqRtf);
        y800.IndFimRtf.Should().Be("Y800FIM");
        arquivo.BlocoY.Registros[^1].Should().BeOfType<RegistroY990>();

        await using var saida = new MemoryStream();
        await new EscritorSpedTxt(new CatalogoSpedGerado()).WriteAsync(
            saida, arquivo.EnumerarRegistros(), TestContext.Current.CancellationToken);
        EncodingSped.Latin1.GetString(saida.ToArray()).Should().Be(texto);

        saida.Position = 0;
        var relido = await parser.ReadAsync(saida, TestContext.Current.CancellationToken);
        relido.BlocoY.Registros.OfType<RegistroY800>().Single().ArqRtf.Should().Be(arqRtf);
        relido.BlocoY.Registros.Select(registro => registro.Codigo).Should().Equal("Y001", "Y800", "Y990");
    }

    [Fact]
    public async Task ReadAsync_RtfSemTerminadorAteEof_FalhaFechado()
    {
        const string texto = "|Y800|001|MEMORIA||{\\rtf1\r\nCONTEUDO SEM TERMINADOR\r\n";
        await using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(texto), writable: false);

        var act = async () => await new ParserEcf().ReadAsync(
            entrada, TestContext.Current.CancellationToken);

        var assercao = await act.Should().ThrowAsync<ErroFormatoSpedException>();
        assercao.Which.Erro.CodigoRegistro.Should().Be("Y800");
    }
}
