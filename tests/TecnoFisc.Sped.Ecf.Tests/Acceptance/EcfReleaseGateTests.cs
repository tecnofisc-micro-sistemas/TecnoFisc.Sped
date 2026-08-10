using System.Text.RegularExpressions;

using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Ecf.Tests.Acceptance;

public sealed partial class EcfReleaseGateTests
{
    private static readonly string[] _blocos =
        ["0", "C", "E", "J", "K", "L", "M", "N", "P", "Q", "T", "U", "V", "W", "X", "Y", "9"];

    [Fact]
    public void CatalogoTemExatamenteOs180RegistrosDoManifesto()
    {
        var manifesto = ManifestoEcf.Carregar();
        // O manifesto descreve só o leiaute 12 vigente: os sete registros removidos no leiaute 11
        // (DescontinuadoEm != 0) ficam de fora desta comparação de 1:1 — ver
        // ManifestoCatalogoTests.Catalogo_SoDivergeDoManifestoNosSeteRemovidosNoLeiaute11.
        var catalogo = new CatalogoSpedGerado().EnumerarRegistros()
            .Where(registro => registro.DescontinuadoEm == 0)
            .ToArray();

        manifesto.Registros.Should().HaveCount(180).And.OnlyContain(registro => registro.Reviewed);
        catalogo.Should().HaveCount(180);
        catalogo.Select(registro => registro.Codigo)
            .Should().Equal(manifesto.Registros.Select(registro => registro.Code));
        catalogo.Select(registro => registro.Bloco).Distinct()
            .Should().Equal(_blocos);
        AssertRegistroEcf.CatalogMatchesManifest(catalogo);
    }

    [Fact]
    public void TodosOs17BlocosTemFixtureSinteticaCompleta()
    {
        var manifesto = ManifestoEcf.Carregar();
        string diretorio = Path.Combine(AppContext.BaseDirectory, "Fixtures", "Sinteticas");
        string[] caminhos = Directory.EnumerateFiles(diretorio, "bloco-*.txt", SearchOption.TopDirectoryOnly)
            .OrderBy(caminho => Array.IndexOf(_blocos, Path.GetFileNameWithoutExtension(caminho)[6..].ToUpperInvariant()))
            .ToArray();

        caminhos.Should().HaveCount(17);
        caminhos.Select(caminho => Path.GetFileNameWithoutExtension(caminho)[6..].ToUpperInvariant())
            .Should().Equal(_blocos);

        foreach (string caminho in caminhos)
        {
            string bloco = Path.GetFileNameWithoutExtension(caminho)[6..].ToUpperInvariant();
            string[] codigos = File.ReadLines(caminho)
                .Where(linha => linha.StartsWith('|'))
                .Select(linha => linha.Split('|')[1])
                .ToArray();
            string[] esperados = manifesto.Registros
                .Where(registro => registro.Block == bloco)
                .Select(registro => registro.Code)
                .ToArray();

            string[] esperadosComRaiz = bloco == "0" ? esperados : ["0000", .. esperados];
            codigos.Should().OnlyContain(
                codigo => esperadosComRaiz.Contains(codigo, StringComparer.Ordinal),
                $"a fixture sintética do bloco {bloco} não deve conter códigos de outro bloco");
            codigos.Distinct(StringComparer.Ordinal).Should().Equal(
                esperadosComRaiz,
                $"a fixture sintética do bloco {bloco} deve cobrir o bloco inteiro na ordem canônica");
        }
    }

    [Fact]
    public async Task FixturesAnonimizadasV008AV011LeemAte9999SemDesconhecidos()
    {
        foreach (int versao in Enumerable.Range(8, 4))
        {
            string diretorio = Path.Combine(AppContext.BaseDirectory, "Fixtures", $"Layout{versao}");
            string caminho = Directory.EnumerateFiles(diretorio, "*.txt", SearchOption.TopDirectoryOnly)
                .Should().ContainSingle().Which;
            await using var entrada = File.OpenRead(caminho);
            var registros = new List<RegistroSped>();

            await foreach (var registro in new ParserEcf().ReadStreamingAsync(
                entrada,
                TestContext.Current.CancellationToken))
            {
                registros.Add(registro);
            }

            registros.Should().NotBeEmpty();
            registros[0].Should().BeOfType<Registro0000>().Which.VersaoLeiaute.Should().Be(versao);
            registros[^1].Codigo.Should().Be("9999");
            registros.Should().NotContain(registro => registro is RegistroNaoReconhecido);
        }
    }

    [Fact]
    public void TrackerNaoTemSubstageDeRegistroAberta()
    {
        string caminho = Path.Combine(
            EncontrarRaizRepositorio().FullName,
            "sped",
            "STAGE_17_ECF_BASELINE.md");
        string tracker = File.ReadAllText(caminho);
        Match[] substages = RecordSubstage().Matches(tracker).Cast<Match>().ToArray();
        string[] esperadas = Enumerable.Range(2, 180)
            .Select(numero => $"17.{numero:000}")
            .ToArray();

        tracker.Should().Contain("Status: concluída");
        substages.Select(match => match.Groups["id"].Value).Should().Equal(esperadas);
        substages.Select(match => match.Groups["status"].Value)
            .Should().OnlyContain(status => status == "[x]");
    }

    private static DirectoryInfo EncontrarRaizRepositorio()
    {
        for (var atual = new DirectoryInfo(AppContext.BaseDirectory); atual is not null; atual = atual.Parent)
        {
            if (File.Exists(Path.Combine(atual.FullName, "TecnoFisc.Sped.slnx")))
                return atual;
        }

        throw new DirectoryNotFoundException("A raiz do repositório não foi localizada para validar o tracker da Stage 17.");
    }

    [GeneratedRegex(@"^\| (?<id>17\.\d{3}) \|.*\| (?<status>\[[ x]\]) \|\r?$", RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex RecordSubstage();
}
