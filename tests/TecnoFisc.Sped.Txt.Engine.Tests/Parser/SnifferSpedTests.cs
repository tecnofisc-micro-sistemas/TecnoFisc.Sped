using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Parser;

public sealed class SnifferSpedTests
{
    private const string LinhaEcfCompleta =
        "|0000|LECF|0011|11111111000191|EMPRESA TESTE|0|0|||01012025|31122025|N||0||";

    [Fact]
    public void MetadadosArquivoSped_ArmazenaValores()
    {
        var metadados = new MetadadosArquivoSped(
            ProjetoSped.EfdContribuicoes,
            6,
            EncodingSped.Latin1,
            "|0000|006|0|||01022025|28022025|EMPRESA|11222333000181|MG|3126901||00|2|",
            "006");

        metadados.Projeto.Should().Be(ProjetoSped.EfdContribuicoes);
        metadados.VersaoLeiaute.Should().Be(6);
        metadados.EncodingDetectado.Should().BeSameAs(EncodingSped.Latin1);
        metadados.CodigoVersaoDeclarado.Should().Be("006");
    }

    [Fact]
    public async Task IdentificarAsync_EfdContribuicoesV006_RetornaMetadados()
    {
        await using var stream = Sped("|0000|006|0|||01022025|28022025|EMPRESA|11222333000181|MG|3126901||00|2|\r\n|0001|0|\r\n");

        var metadados = await SnifferSped.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.EfdContribuicoes);
        metadados.VersaoLeiaute.Should().Be(6);
        metadados.CodigoVersaoDeclarado.Should().Be("006");
        stream.Position.Should().Be(0, "o sniffer deve restaurar stream seekable para replay");
    }

    [Fact]
    public async Task IdentificarAsync_MantemContratoLeveSemMetadadosFiscais()
    {
        await using var stream = Sped("|0000|006|0|||01022025|28022025|EMPRESA|11222333000181|MG|3126901||00|2|\r\n");

        var metadados = await SnifferSped.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.EfdContribuicoes);
        metadados.VersaoLeiaute.Should().Be(6);
        metadados.GetType().GetProperty("Cnpj").Should().BeNull();
        metadados.GetType().GetProperty("DataInicial").Should().BeNull();
        metadados.GetType().GetProperty("DataFinal").Should().BeNull();
    }

    [Fact]
    public async Task IdentificarAsync_EfdIcmsIpiV015_RetornaMetadados()
    {
        await using var stream = Sped("|0000|015|1|01012021|31012021|EMPRESA|11222333000181||MG|123456789|3139409|||B|1|\n");

        var metadados = await SnifferSped.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.EfdIcmsIpi);
        metadados.VersaoLeiaute.Should().Be(15);
        metadados.CodigoVersaoDeclarado.Should().Be("015");
        stream.Position.Should().Be(0);
    }

    [Fact]
    public async Task IdentificarAsync_EcdLecd_RetornaLeiaute9()
    {
        await using var stream = Sped("|0000|LECD|01012023|31122023|EMPRESA|11222333000181|ES|\r\n|0001|0|\r\n");

        var metadados = await SnifferSped.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.Ecd);
        metadados.VersaoLeiaute.Should().Be(9);
        metadados.CodigoVersaoDeclarado.Should().Be("LECD");
        stream.Position.Should().Be(0);
    }

    [Fact]
    public async Task IdentificarAsync_Lecf_RetornaEcfVersaoDeclaradaERestauraPosicao()
    {
        await using var stream = Sped("PREFIXO" + LinhaEcfCompleta + "\r\n|0001|0|\r\n");
        stream.Position = "PREFIXO".Length;

        var metadados = await SnifferSped.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.Ecf);
        metadados.VersaoLeiaute.Should().Be(11);
        metadados.CodigoVersaoDeclarado.Should().Be("0011");
        metadados.PrimeiraLinha.Should().Be(LinhaEcfCompleta);
        stream.Position.Should().Be("PREFIXO".Length);
    }

    [Theory]
    [InlineData("|0000|lecf|0012|11222333000181|EMPRESA TESTE|0|0|||01012025|31122025|N||0||")]
    [InlineData("|0000|XECF|0012|11222333000181|EMPRESA TESTE|0|0|||01012025|31122025|N||0||")]
    [InlineData("|0000|LECF|0007|11222333000181|EMPRESA TESTE|0|0|||01012025|31122025|N||0||")]
    [InlineData("|0000|LECF|0013|11222333000181|EMPRESA TESTE|0|0|||01012025|31122025|N||0||")]
    [InlineData("|0000|LECF|0012|11222333000181|EMPRESA TESTE|0|0|||01012025|31122025|N||0|")]
    [InlineData("|0000|LECF|0012|11222333000181|EMPRESA TESTE|0|0|||01012025|31122025|N||0|||")]
    [InlineData("|0000|LECF|0012|11222333000181|EMPRESA TESTE|0|0|||01012025|31122025|N||0||EXTRA")]
    [InlineData("|0000|LECF|0012|11222333000181|EMPRESA TESTE|")]
    public async Task IdentificarAsync_LecfQuaseValido_RetornaDesconhecido(string linha)
    {
        await using var stream = Sped(linha);

        var metadados = await SnifferSped.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.Desconhecido);
        metadados.VersaoLeiaute.Should().Be(0);
        stream.Position.Should().Be(0);
    }

    [Fact]
    public async Task IdentificarAsync_Cancelado_RestauraPosicaoSeekable()
    {
        await using var stream = Sped("PREFIXO" + LinhaEcfCompleta);
        stream.Position = "PREFIXO".Length;
        using var cancelamento = new CancellationTokenSource();
        cancelamento.Cancel();

        Func<Task> act = async () => _ = await SnifferSped.IdentificarAsync(stream, cancelamento.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        stream.Position.Should().Be("PREFIXO".Length);
    }

    [Fact]
    public async Task IdentificarAsync_IgnoraLinhasVaziasAntesDo0000()
    {
        await using var stream = Sped("\r\n\n|0000|LECD|01012023|31122023|EMPRESA|11222333000181|ES|\r\n");

        var metadados = await SnifferSped.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.Ecd);
        metadados.PrimeiraLinha.Should().StartWith("|0000|LECD|");
        stream.Position.Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("|9999|1|")]
    [InlineData("texto livre")]
    [InlineData("|0000|999|0|")]
    public async Task IdentificarAsync_EntradaDesconhecida_RetornaDesconhecido(string conteudo)
    {
        await using var stream = Sped(conteudo);

        var metadados = await SnifferSped.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.Desconhecido);
        metadados.VersaoLeiaute.Should().Be(0);
        stream.Position.Should().Be(0);
    }

    private static MemoryStream Sped(string conteudo)
        => new(EncodingSped.Latin1.GetBytes(conteudo), writable: false);

    [Fact]
    public async Task AbrirParserAsync_UsaFactoryDoProjetoIdentificado_EReposicionaStream()
    {
        await using var stream = Sped("|0000|LECD|01012023|31122023|EMPRESA|11222333000181|ES|\r\n");
        var parserEcd = new LeitorFake();
        var fabricas = new Dictionary<ProjetoSped, Func<ILeitorSped>>
        {
            [ProjetoSped.Ecd] = () => parserEcd,
        };

        var parser = await SnifferSped.AbrirParserAsync(stream, fabricas, TestContext.Current.CancellationToken);

        parser.Should().BeSameAs(parserEcd);
        stream.Position.Should().Be(0);
    }

    [Fact]
    public async Task AbrirParserAsync_SemFactoryDoProjeto_LancaNotSupportedException()
    {
        await using var stream = Sped("|0000|LECD|01012023|31122023|EMPRESA|11222333000181|ES|\r\n");
        var fabricas = new Dictionary<ProjetoSped, Func<ILeitorSped>>();

        Func<Task> act = async () => _ = await SnifferSped.AbrirParserAsync(
            stream,
            fabricas,
            TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*Ecd*");
        stream.Position.Should().Be(0);
    }

    private sealed class LeitorFake : ILeitorSped
    {
        public async IAsyncEnumerable<RegistroSped> ReadStreamingAsync(
            Stream entrada,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancelamento = default)
        {
            await Task.CompletedTask;
            yield break;
        }
    }
}
