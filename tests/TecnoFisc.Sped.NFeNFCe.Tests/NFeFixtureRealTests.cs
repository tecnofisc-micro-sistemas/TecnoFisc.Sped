namespace TecnoFisc.Sped.NFeNFCe.Tests;

/// <summary>
/// Parse end-to-end contra os XMLs reais (não anonimizados) em <c>sped/fixtures/xml/</c>
/// (gitignored). Pula automaticamente quando a pasta não existe (CI público / máquina sem
/// arquivo de cliente). Os fixtures committados em <c>Fixtures/</c> cobrem o caso assertivo;
/// estes exercitam o parser contra o dado real (NF-e 55 canônica e envelope SERPRO).
/// </summary>
public sealed class NFeFixtureRealTests
{
    [Fact]
    public async Task ParseiaNFe55Reais_Canonica_E_Serpro()
    {
        var ct = TestContext.Current.CancellationToken;
        var fixtures = EnumerarFixturesXml().ToList();
        if (fixtures.Count == 0)
        {
            Assert.Skip("Nenhuma fixture XML real presente em sped/fixtures/xml/.");
            return;
        }

        var parser = new ParserNFe();

        foreach (var caminho in fixtures)
        {
            await using var stream = File.OpenRead(caminho);
            var nfe = await parser.ReadNFeAsync(stream, ct);

            nfe.Itens.Should().NotBeEmpty(because: $"{Path.GetFileName(caminho)} deve produzir itens");
            nfe.Ide.Mod.Codigo.Should().Be("55", because: $"{Path.GetFileName(caminho)} é NF-e modelo 55");
            nfe.IsAutorizada.Should().BeTrue(because: $"{Path.GetFileName(caminho)} traz protNFe autorizado");
            nfe.Protocolo!.ChNFe.Should().Be(nfe.ChaveAcesso,
                because: "o protocolo deve referenciar a chave da própria nota");
        }
    }

    private static IEnumerable<string> EnumerarFixturesXml()
    {
        var diretorio = LocalizarPastaFixturesXml();
        return diretorio is null
            ? []
            : Directory.EnumerateFiles(diretorio, "*.xml", SearchOption.TopDirectoryOnly);
    }

    private static string? LocalizarPastaFixturesXml()
    {
        var atual = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(atual))
        {
            var candidato = Path.Combine(atual, "sped", "fixtures", "xml");
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
