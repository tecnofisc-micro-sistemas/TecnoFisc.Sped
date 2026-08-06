using System.Text.RegularExpressions;

using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Parser;

public sealed partial class ParserEcfFixturePrivadaTests
{
    private const string VariavelFixtures = "TECNOFISC_SPED_ECF_FIXTURES_DIR";
    private static readonly string[] BlocosCanonicos =
        ["0", "C", "E", "J", "K", "L", "M", "N", "P", "Q", "T", "U", "V", "W", "X", "Y", "9"];

    [Theory]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    public async Task ParserLeFixtureDaVersaoSemCodigoDesconhecido(int versao)
    {
        byte[] conteudo = versao == 12
            ? ComporFixturePublicaV12()
            : LerFixturePrivadaUnica(versao);

        await using var entrada = new MemoryStream(conteudo, writable: false);
        var registros = new List<RegistroSped>();
        await foreach (var registro in new ParserEcf().ReadStreamingAsync(
            entrada,
            TestContext.Current.CancellationToken))
        {
            registros.Add(registro);
        }

        registros.Should().NotBeEmpty();
        registros[0].Should().BeOfType<Registro0000>().Which.VersaoLeiaute.Should().Be(versao);
        registros.Should().ContainSingle(registro => registro.Codigo == "0000");
        registros[^1].Codigo.Should().Be("9999");
        registros.Should().NotContain(registro => registro is RegistroNaoReconhecido);
    }

    [Fact]
    public void FixturePublicaV12_CompoeOs17BlocosUmaAberturaEUmEncerramentoFinal()
    {
        string texto = EncodingSped.Latin1.GetString(ComporFixturePublicaV12());
        string[] linhas = texto.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
        string diretorio = Path.Combine(AppContext.BaseDirectory, "Fixtures", "Sinteticas");
        string[] fixtures = Directory.EnumerateFiles(diretorio, "bloco-*.txt", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileNameWithoutExtension)
            .Order(StringComparer.Ordinal)
            .ToArray()!;

        fixtures.Should().Equal(BlocosCanonicos.Select(bloco => $"bloco-{bloco.ToLowerInvariant()}")
            .Order(StringComparer.Ordinal));
        linhas.Should().ContainSingle(linha => linha.StartsWith("|0000|", StringComparison.Ordinal));
        linhas[^1].Should().StartWith("|9999|");
        linhas.Select(ObterBloco).Distinct().Should().Equal(BlocosCanonicos);
        linhas.Select(ObterCodigo).Distinct().Should().Equal(
            new Generated.CatalogoSpedGerado().EnumerarRegistros().Select(registro => registro.Codigo));
    }

    [Theory]
    [InlineData(9)]
    [InlineData(10)]
    public async Task Registro0021_PreservaTokensLexicaisAbaixoENaMudancaDoLeiaute10(int versao)
    {
        const string linha = "|0021|S|N|S|N|S|N|S|N|S|N|S|N|S|N|S|N|";
        var resultado = new ParserEcf().ParseLinha(linha);
        resultado.Sucesso.Should().BeTrue($"os tokens S/N do leiaute {versao} são válidos");

        await using var saida = new MemoryStream();
        await new EscritorSpedTxt(new Generated.CatalogoSpedGerado()).WriteAsync(
            saida,
            [resultado.Valor],
            TestContext.Current.CancellationToken);

        EncodingSped.Latin1.GetString(saida.ToArray()).Should().Be(linha + "\r\n");
    }

    [Theory]
    [InlineData("ausente")]
    [InlineData("versao-incorreta")]
    [InlineData("multipla")]
    public void FixturePrivadaConfiguradaInvalida_FalhaSemExporCaminhoOuNome(string cenario)
    {
        string diretorio = Path.Combine(Path.GetTempPath(), $"ecf-private-contract-{Guid.NewGuid():N}");
        try
        {
            if (cenario != "ausente")
            {
                Directory.CreateDirectory(diretorio);
                int versao = cenario == "versao-incorreta" ? 9 : 8;
                File.WriteAllText(
                    Path.Combine(diretorio, "entrada-a.txt"),
                    $"|0000|LECF|{versao:0000}|\r\n",
                    EncodingSped.Latin1);
                if (cenario == "multipla")
                {
                    File.WriteAllText(
                        Path.Combine(diretorio, "entrada-b.txt"),
                        "|0000|LECF|0008|\r\n",
                        EncodingSped.Latin1);
                }
            }

            var act = () => LerFixturePrivadaUnica(8, diretorio);

            var excecao = act.Should().Throw<InvalidDataException>()
                .WithMessage("Conjunto privado ECF inválido.").Which;
            excecao.Message.Should().NotContain(diretorio);
            excecao.Message.Should().NotContain("entrada-");
        }
        finally
        {
            if (Directory.Exists(diretorio))
                Directory.Delete(diretorio, recursive: true);
        }
    }

    private static byte[] LerFixturePrivadaUnica(int versao)
    {
        string? diretorio = Environment.GetEnvironmentVariable(VariavelFixtures);
        if (string.IsNullOrWhiteSpace(diretorio))
            Assert.Skip("Fixture privada ECF não configurada.");

        return LerFixturePrivadaUnica(versao, diretorio);
    }

    private static byte[] LerFixturePrivadaUnica(int versao, string diretorio)
    {
        try
        {
            if (!Directory.Exists(diretorio))
                throw new InvalidDataException("Conjunto privado ECF inválido.");

            byte[][] correspondentes = Directory.EnumerateFiles(diretorio, "*.txt", SearchOption.TopDirectoryOnly)
                .Select(File.ReadAllBytes)
                .Where(conteudo => LerVersaoDeclarada(conteudo) == versao)
                .Take(2)
                .ToArray();
            if (correspondentes.Length != 1)
                throw new InvalidDataException("Conjunto privado ECF inválido.");

            return correspondentes[0];
        }
        catch (InvalidDataException)
        {
            throw;
        }
        catch
        {
            throw new InvalidDataException("Conjunto privado ECF inválido.");
        }
    }

    private static int LerVersaoDeclarada(byte[] conteudo)
    {
        string texto = EncodingSped.Latin1.GetString(conteudo);
        var correspondencia = Linha0000().Match(texto);
        return correspondencia.Success && int.TryParse(correspondencia.Groups[1].Value, out int versao)
            ? versao
            : 0;
    }

    private static byte[] ComporFixturePublicaV12()
    {
        var linhas = new List<string>();
        foreach (string bloco in BlocosCanonicos)
        {
            string caminho = Path.Combine(
                AppContext.BaseDirectory,
                "Fixtures",
                "Sinteticas",
                $"bloco-{bloco.ToLowerInvariant()}.txt");
            string[] linhasBloco = File.ReadAllText(caminho, EncodingSped.Latin1)
                .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
            linhas.AddRange(bloco == "0"
                ? linhasBloco
                : linhasBloco.Where(linha => !linha.StartsWith("|0000|", StringComparison.Ordinal)));
        }

        return EncodingSped.Latin1.GetBytes(string.Join("\r\n", linhas) + "\r\n");
    }

    private static string ObterBloco(string linha)
        => char.ToUpperInvariant(ObterCodigo(linha)[0]).ToString();

    private static string ObterCodigo(string linha)
    {
        int segundoSeparador = linha.IndexOf('|', 1);
        return linha[1..segundoSeparador];
    }

    [GeneratedRegex(@"(?m)^\|0000\|[^|]*\|0*([0-9]+)\|")]
    private static partial Regex Linha0000();
}
