using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Xml;

namespace TecnoFisc.Sped.NFeNFCe.Tests;

public sealed class SchemaGroupTests
{
    public static IEnumerable<object[]> Groups()
    {
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "schema-groups.json")));
        foreach (var row in json.RootElement.EnumerateArray())
            yield return [row.GetProperty("path").GetString()!, row.GetProperty("reader").GetString()!, row.GetProperty("property").GetString()!, row.GetProperty("model").GetString()!, row.GetProperty("xml").GetString()!];
    }

    [Theory]
    [MemberData(nameof(Groups))]
    public async Task ParentReaderAttachesEveryTypedGroup(string path, string method, string property, string model, string xml)
    {
        using var input = new StringReader(xml);
        using var reader = XmlReader.Create(input, new XmlReaderSettings { Async = true, DtdProcessing = DtdProcessing.Prohibit });
        await reader.ReadAsync();
        var assembly = typeof(ParserNFe).Assembly;
        var type = assembly.GetType("TecnoFisc.Sped.NFeNFCe.Parser.NFeXmlReader")!;
        object instance = Activator.CreateInstance(type, reader, new ParserNFeOptions { Strict = true })!;
        var task = (Task)type.GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(instance, [TestContext.Current.CancellationToken])!;
        await task;
        object result = task.GetType().GetProperty("Result")!.GetValue(task)!;
        object? actual = property == "$variant" ? result : result.GetType().GetProperty(property)?.GetValue(result);
        Assert.True(actual is not null, $"{path}: missing property/group {property}");
        if (actual is IEnumerable values and not string) actual = Assert.Single(values.Cast<object>());
        var expectedType = assembly.GetType("TecnoFisc.Sped.NFeNFCe." + model)!;
        Assert.True(expectedType.IsInstanceOfType(actual), $"{path}: expected {model}, got {actual.GetType().Name}");
    }
}
