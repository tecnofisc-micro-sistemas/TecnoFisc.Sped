using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Xml;

namespace TecnoFisc.Sped.NFeNFCe.Tests;

public sealed class SchemaRepetitionTests
{
    public static IEnumerable<object[]> Groups()
    {
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "schema-repetitions.json")));
        foreach (var row in json.RootElement.EnumerateArray())
            yield return [row.GetProperty("path").GetString()!, row.GetProperty("reader").GetString()!, row.GetProperty("property").GetString()!, row.GetProperty("xml").GetString()!];
    }

    [Theory]
    [MemberData(nameof(Groups))]
    public async Task RepeatedGroupsKeepBothOccurrences(string path, string method, string property, string xml)
    {
        using var input = new StringReader(xml);
        using var reader = XmlReader.Create(input, new XmlReaderSettings { Async = true, DtdProcessing = DtdProcessing.Prohibit });
        await reader.ReadAsync();
        var type = typeof(ParserNFe).Assembly.GetType("TecnoFisc.Sped.NFeNFCe.Parser.NFeXmlReader")!;
        object instance = Activator.CreateInstance(type, reader, new ParserNFeOptions { Strict = true })!;
        var task = (Task)type.GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(instance, [TestContext.Current.CancellationToken])!;
        await task;
        object result = task.GetType().GetProperty("Result")!.GetValue(task)!;
        var collection = Assert.IsAssignableFrom<IEnumerable>(result.GetType().GetProperty(property)!.GetValue(result));
        Assert.True(collection.Cast<object>().Count() == 2, $"{path}: both occurrences must survive");
    }
}
