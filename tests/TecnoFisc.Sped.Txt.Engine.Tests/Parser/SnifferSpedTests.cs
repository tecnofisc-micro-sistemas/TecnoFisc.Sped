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
    [InlineData("|0000|LECF|0012|11222333000181|EMPRESA TESTE|0|0|||01012025|31122025|N||0||EXTRA")]
    public async Task IdentificarAsync_LecfQuaseValido_RetornaDesconhecido(string linha)
    {
        await using var stream = Sped(linha);

        var metadados = await SnifferSped.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.Desconhecido);
        metadados.VersaoLeiaute.Should().Be(0);
        stream.Position.Should().Be(0);
    }

    /// <summary>
    /// Quem classifica é o discriminador <c>LECF</c>, não a versão: um leiaute fora da faixa que a
    /// biblioteca modela (8–12) continua sendo ECF e precisa chegar ao <c>ParserEcf</c>, que o lê em
    /// modo tolerante. Antes o sniffer devolvia <c>Desconhecido</c> para 7 e 13, e quem roteava por
    /// ele nunca alcançava o parser.
    /// </summary>
    [Theory]
    [InlineData("0007", 7)]
    [InlineData("0008", 8)]
    [InlineData("0012", 12)]
    [InlineData("0013", 13)]
    [InlineData("0100", 100)]
    public async Task IdentificarAsync_LecfForaDaFaixaModelada_RetornaEcfComVersaoDeclarada(
        string codVer, int versaoEsperada)
    {
        await using var stream = Sped(
            $"|0000|LECF|{codVer}|11222333000181|EMPRESA TESTE|0|0|||01012025|31122025|N||0||");

        var metadados = await SnifferSped.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.Ecf);
        metadados.VersaoLeiaute.Should().Be(versaoEsperada);
        metadados.CodigoVersaoDeclarado.Should().Be(codVer);
    }

    /// <summary>
    /// A largura da linha é checada por mínimo, não por igualdade — senão um leiaute futuro que
    /// acrescente uma coluna ao <c>0000</c> deixaria de ser roteado. Validar a largura do registro
    /// é trabalho do parser, que reporta linha, registro e campo.
    /// </summary>
    [Theory]
    [InlineData("|0000|LECF|0012|11222333000181|EMPRESA TESTE|0|0|||01012025|31122025|N||0|")]
    [InlineData("|0000|LECF|0012|11222333000181|EMPRESA TESTE|0|0|||01012025|31122025|N||0|||")]
    [InlineData("|0000|LECF|0012|11222333000181|EMPRESA TESTE|")]
    [InlineData("|0000|LECF|0012|")]
    public async Task IdentificarAsync_LecfComLarguraDiferente_ContinuaSendoEcf(string linha)
    {
        await using var stream = Sped(linha);

        var metadados = await SnifferSped.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.Ecf);
        metadados.VersaoLeiaute.Should().Be(12);
    }

    /// <summary>
    /// <c>COD_VER</c> ilegível (ausente, com comprimento diferente de 4 ou não numérico) é arquivo
    /// inválido, não leiaute novo: continua <c>Desconhecido</c>.
    /// </summary>
    [Theory]
    [InlineData("ABCD")]
    [InlineData("")]
    [InlineData("012")]
    [InlineData("00123")]
    [InlineData("00 1")]
    [InlineData("0000")]
    public async Task IdentificarAsync_LecfComCodVerIlegivel_RetornaDesconhecido(string codVer)
    {
        await using var stream = Sped(
            $"|0000|LECF|{codVer}|11222333000181|EMPRESA TESTE|0|0|||01012025|31122025|N||0||");

        var metadados = await SnifferSped.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.Desconhecido);
        metadados.VersaoLeiaute.Should().Be(0);
        metadados.CodigoVersaoDeclarado.Should().Be(codVer);
    }

    /// <summary>
    /// Guarda de regressão dos três leiautes já publicados: a mudança do caminho ECF não pode
    /// mexer na classificação de ECD, EFD Contribuições nem EFD ICMS-IPI.
    /// </summary>
    [Theory]
    [InlineData("|0000|LECD|01012023|31122023|EMPRESA|11222333000181|ES|", ProjetoSped.Ecd, 9)]
    [InlineData("|0000|006|0|||01022025|28022025|EMPRESA|11222333000181|MG|3126901||00|2|",
        ProjetoSped.EfdContribuicoes, 6)]
    [InlineData("|0000|015|1|01012021|31012021|EMPRESA|11222333000181||MG|123456789|3139409|||B|1|",
        ProjetoSped.EfdIcmsIpi, 15)]
    [InlineData("|0000|020|1|01012026|31012026|EMPRESA|11222333000181||MG|123456789|3139409|||B|1|",
        ProjetoSped.EfdIcmsIpi, 20)]
    // Largura errada continua desqualificando os leiautes numéricos: o afrouxamento é só do ECF.
    [InlineData("|0000|006|0|||01022025|28022025|EMPRESA|11222333000181|MG|3126901||00|2||",
        ProjetoSped.Desconhecido, 0)]
    [InlineData("|0000|015|1|01012021|31012021|EMPRESA|11222333000181||MG|123456789|3139409|||B|",
        ProjetoSped.Desconhecido, 0)]
    [InlineData("|0000|007|0|||01022025|28022025|EMPRESA|11222333000181|MG|3126901||00|2|",
        ProjetoSped.Desconhecido, 0)]
    [InlineData("|0000|021|1|01012026|31012026|EMPRESA|11222333000181||MG|123456789|3139409|||B|1|",
        ProjetoSped.Desconhecido, 0)]
    public async Task IdentificarAsync_ProjetosPublicados_MantemClassificacao(
        string linha, ProjetoSped projetoEsperado, int versaoEsperada)
    {
        await using var stream = Sped(linha);

        var metadados = await SnifferSped.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(projetoEsperado);
        metadados.VersaoLeiaute.Should().Be(versaoEsperada);
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
