using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoJ;

namespace TecnoFisc.Sped.Ecd.Tests;

/// <summary>
/// Parse end-to-end contra arquivos ECD reais em <c>sped/fixtures/</c> (gitignored, local only).
/// Os arquivos do PVA da Receita podem trazer assinatura digital PKCS#7 anexada após o <c>9999</c>
/// e contêm os registros multi-linha J800/J801 (campo-arquivo ARQ_RTF com CRLF). Pula automaticamente
/// quando nenhuma fixture ECD está presente (CI público). Pacote ECD é read-only — sem round-trip.
/// Cobre o critério de aceite da issue #498: ler a fixture real inteira sem erro.
/// </summary>
public sealed class ParserEcdFixtureRealTests
{
    [Fact]
    public async Task ParserLeFixtureRealInteira_MaterializandoJ800EJ801ComArqRtfCompleto()
    {
        var fixtures = EnumerarFixturesEcd().ToList();
        if (fixtures.Count == 0)
        {
            Assert.Skip("Nenhuma fixture ECD presente em sped/fixtures/. Adicione um arquivo SPED-ECD real para exercitar o parser.");
            return;
        }

        foreach (var caminho in fixtures)
        {
            var bytes = await File.ReadAllBytesAsync(caminho, TestContext.Current.CancellationToken);
            using var entrada = new MemoryStream(bytes, writable: false);

            var parser = new ParserEcd();
            var registros = new List<RegistroSped>();
            await foreach (var r in parser.ReadStreamingAsync(entrada, TestContext.Current.CancellationToken))
                registros.Add(r);

            var nome = Path.GetFileName(caminho);
            registros.Should().NotBeEmpty(because: $"a fixture {nome} deve produzir registros");
            registros[^1].Codigo.Should().Be("9999",
                because: $"o parser deve encerrar no |9999| de {nome}, ignorando assinatura PKCS#7 anexa");

            // Todo J800/J801 presente deve ter sido materializado com ARQ_RTF não vazio (multi-linha).
            foreach (var j800 in registros.OfType<RegistroJ800>())
                j800.ArqRtf.Should().NotBeNullOrEmpty(because: $"ARQ_RTF do J800 em {nome} deve ser remontado");
            foreach (var j801 in registros.OfType<RegistroJ801>())
                j801.ArqRtf.Should().NotBeNullOrEmpty(because: $"ARQ_RTF do J801 em {nome} deve ser remontado");
        }
    }

    private static IEnumerable<string> EnumerarFixturesEcd()
    {
        var diretorio = LocalizarPastaFixtures();
        if (diretorio is null)
            return [];

        return Directory.EnumerateFiles(diretorio, "*.txt", SearchOption.TopDirectoryOnly)
            .Where(EhFixtureEcd);
    }

    /// <summary>
    /// Discrimina ECD de outros leiautes pelo <c>Registro0000</c>: o segundo campo (após
    /// <c>|0000|</c>) é o literal <c>LECD</c> na ECD.
    /// </summary>
    private static bool EhFixtureEcd(string caminho)
    {
        using var fs = File.OpenRead(caminho);
        using var leitor = new StreamReader(fs, EncodingSped.Latin1);
        var primeiraLinha = leitor.ReadLine();
        return primeiraLinha is not null && primeiraLinha.StartsWith("|0000|LECD|", StringComparison.Ordinal);
    }

    private static string? LocalizarPastaFixtures()
    {
        var atual = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(atual))
        {
            var candidato = Path.Combine(atual, "sped", "fixtures");
            if (Directory.Exists(candidato))
                return candidato;

            var pai = Directory.GetParent(atual)?.FullName;
            if (pai == atual)
                return null;
            atual = pai;
        }
        return null;
    }
}
