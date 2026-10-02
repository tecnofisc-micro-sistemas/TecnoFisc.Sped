using System.Security.Cryptography;
using System.Text.Json;

namespace TecnoFisc.Sped.NFeNFCe.Tests;

public sealed class SchemaCoverageTests
{
    [Fact]
    public void EveryFiscalPathHasBindingAndEveryLeafHasLexicalTest()
    {
        using var inventory = Read("inventory.json");
        using var bindings = Read("field-bindings.json");
        using var fields = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "schema-fields.json")));
        using var groups = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "schema-groups.json")));
        using var repetitions = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "schema-repetitions.json")));
        var bound = bindings.RootElement.EnumerateArray().Select(x => x.GetProperty("path").GetString()).ToHashSet(StringComparer.Ordinal);
        var tested = fields.RootElement.EnumerateArray().Select(x => x.GetProperty("path").GetString()).ToHashSet(StringComparer.Ordinal);
        tested.Add("TProtNFe/@versao"); // DocumentoRoutingTests.ProtocolMetadataIsPreserved exercises the public envelope.
        var attached = groups.RootElement.EnumerateArray().Select(x => x.GetProperty("path").GetString()).ToHashSet(StringComparer.Ordinal);
        attached.UnionWith(["TNFe/infNFe", "TNFe/infNFeSupl", "TProtNFe/infProt"]); // Public routing/supplement/protocol assertions.
        var repeated = repetitions.RootElement.EnumerateArray().Select(x => x.GetProperty("path").GetString()).ToHashSet(StringComparer.Ordinal);
        foreach (var row in inventory.RootElement.EnumerateArray())
        {
            string path = row.GetProperty("path").GetString()!;
            Assert.True(bound.Contains(path), $"Unmapped fiscal schema path: {path}");
            if (row.GetProperty("base").GetString() != "complex") Assert.True(tested.Contains(path), $"Untested leaf: {path}");
            else Assert.True(attached.Contains(path), $"Untested parent/group binding: {path}");
            if (row.GetProperty("max").ToString() != "1") Assert.True(repeated.Contains(path), $"Untested repetition: {path}");
        }
    }

    [Theory]
    [InlineData("schema-reference.json", "schemas")]
    [InlineData("event-schema-reference.json", "schemas-eventos")]
    public void PinnedSchemasMatchOfficialDownloadHashes(string referenceFile, string directory)
    {
        using var reference = Read(referenceFile);
        foreach (var file in reference.RootElement.GetProperty("files").EnumerateArray())
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Schema", directory, file.GetProperty("name").GetString()!);
            using var stream = File.OpenRead(path);
            string hash = Convert.ToHexString(SHA256.HashData(stream));
            Assert.Equal(file.GetProperty("sha256").GetString(), hash);
        }
    }
    private static JsonDocument Read(string file) => JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Schema", file)));
}
