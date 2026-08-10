using System.Security.Cryptography;

namespace TecnoFisc.Sped.Ecf.Tests.Manifesto;

public sealed class ArtefatosCopiadosTests
{
    [Theory]
    [InlineData(
        "TecnoFisc.Sped.Ecf.Tests.Source.layout-12-manifest.json",
        "Manifesto",
        "layout-12-manifest.json")]
    [InlineData(
        "TecnoFisc.Sped.Ecf.Tests.Source.layout-12-manifest.schema.json",
        "Manifesto",
        "layout-12-manifest.schema.json")]
    [InlineData(
        "TecnoFisc.Sped.Ecf.Tests.Source.minimo.txt",
        "Fixtures/Sinteticas",
        "minimo.txt")]
    public void ArtefatoCopiado_TemMesmoHashDaFonteUsadaNoBuild(
        string recurso,
        string subdiretorio,
        string arquivo)
    {
        using var fonte = typeof(ArtefatosCopiadosTests).Assembly.GetManifestResourceStream(recurso);
        fonte.Should().NotBeNull($"o recurso '{recurso}' deveria representar a fonte usada pelo build");

        using var memoria = new MemoryStream();
        fonte!.CopyTo(memoria);
        byte[] hashFonte = SHA256.HashData(memoria.ToArray());

        string caminhoCopiado = Path.Combine(
            AppContext.BaseDirectory,
            subdiretorio.Replace('/', Path.DirectorySeparatorChar),
            arquivo);
        byte[] hashCopiado = SHA256.HashData(File.ReadAllBytes(caminhoCopiado));

        Convert.ToHexString(hashCopiado).Should().Be(Convert.ToHexString(hashFonte));
    }
}
