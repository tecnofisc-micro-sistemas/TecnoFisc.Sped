using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace TecnoFisc.Sped.NFeNFCe.Tests;

/// <summary>Reflection is confined to tests: verifies every schema leaf against lexical expectations.</summary>
public sealed class SchemaFieldTests
{
    public static IEnumerable<object[]> Fields()
    {
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "schema-fields.json")));
        foreach (var row in json.RootElement.EnumerateArray())
            foreach (bool reordered in new[] { false, true })
                yield return [row.GetProperty("path").GetString()!, row.GetProperty("reader").GetString()!, row.GetProperty("property").GetString()!, row.GetProperty("xml").GetString()!, row.GetProperty("expected").GetString()!, reordered];
    }

    [Theory]
    [MemberData(nameof(Fields))]
    public async Task SchemaLeafIsReadIntoTypedProperty(string path, string method, string property, string xml, string expected, bool reordered)
    {
        if (reordered)
        {
            var root = XElement.Parse(xml);
            foreach (var element in root.DescendantsAndSelf().ToArray())
            {
                foreach (var attribute in element.Attributes().ToArray())
                {
                    element.Add(new XElement(attribute.Name, attribute.Value));
                    attribute.Remove();
                }
                if (element.HasElements) element.ReplaceNodes(element.Elements().Reverse().ToArray());
            }
            xml = root.ToString(SaveOptions.DisableFormatting);
        }
        using var input = new StringReader(xml);
        using var reader = XmlReader.Create(input, new XmlReaderSettings { Async = true, DtdProcessing = DtdProcessing.Prohibit });
        await reader.ReadAsync();
        var type = typeof(ParserNFe).Assembly.GetType("TecnoFisc.Sped.NFeNFCe.Parser.NFeXmlReader")!;
        object instance = Activator.CreateInstance(type, reader, new ParserNFeOptions { Strict = true })!;
        var readMethod = type.GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!;
        var task = (Task)readMethod.Invoke(instance, [TestContext.Current.CancellationToken])!;
        await task;
        object result = task.GetType().GetProperty("Result")!.GetValue(task)!;
        var prop = result.GetType().GetProperty(property);
        Assert.True(prop is not null, $"{path}: property {property} absent from {result.GetType().Name}");
        object? actual = prop.GetValue(result);
        Assert.NotNull(actual);
        if (actual is System.Collections.IEnumerable collection and not string)
            actual = Assert.Single(collection.Cast<object>());
        if (actual is decimal number) Assert.Equal(decimal.Parse(expected, CultureInfo.InvariantCulture), number);
        else if (actual is DateOnly date) Assert.Equal(DateOnly.Parse(expected, CultureInfo.InvariantCulture), date);
        else if (actual is DateTimeOffset dateTime) Assert.Equal(DateTimeOffset.Parse(expected, CultureInfo.InvariantCulture), dateTime);
        else if (actual is bool flag) Assert.Equal(expected is "1" or "true", flag);
        else if (actual.GetType().IsEnum) Assert.Equal(int.Parse(expected, CultureInfo.InvariantCulture), Convert.ToInt32(actual, CultureInfo.InvariantCulture));
        else Assert.Equal(expected, Convert.ToString(actual, CultureInfo.InvariantCulture));
    }
}
