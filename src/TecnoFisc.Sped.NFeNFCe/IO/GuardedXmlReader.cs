using System.Xml;

namespace TecnoFisc.Sped.NFeNFCe.IO;

internal sealed class GuardedXmlReader(XmlReader inner, ParserNFeOptions options, CancellationToken cancellationToken) : XmlReader, IXmlLineInfo
{
    private bool Check(bool result)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (result && inner.Depth >= options.MaxXmlDepth)
            throw new LimiteLeituraException($"Profundidade XML excede {options.MaxXmlDepth}.");
        return result;
    }
    public void ReportIgnored() => options.OnIgnoredElement?.Invoke(new(NamespaceURI, LocalName, LineNumber, LinePosition));
    public override bool Read() => Check(inner.Read());
    public override async Task<bool> ReadAsync() => Check(await inner.ReadAsync().ConfigureAwait(false));
    public override async Task SkipAsync()
    {
        if (NodeType == XmlNodeType.Element && !IsEmptyElement)
        {
            int depth = Depth;
            while (await ReadAsync().ConfigureAwait(false) && Depth > depth) { }
        }
        await ReadAsync().ConfigureAwait(false);
    }
    public override void Skip()
    {
        if (NodeType == XmlNodeType.Element && !IsEmptyElement)
        {
            int depth = Depth;
            while (Read() && Depth > depth) { }
        }
        Read();
    }
    public override async Task<string> ReadElementContentAsStringAsync()
    {
        cancellationToken.ThrowIfCancellationRequested();
        string value = await inner.ReadElementContentAsStringAsync().ConfigureAwait(false);
        Check(!inner.EOF);
        return value;
    }
    public override string ReadElementContentAsString()
    {
        cancellationToken.ThrowIfCancellationRequested();
        string value = inner.ReadElementContentAsString();
        Check(!inner.EOF);
        return value;
    }
    public override int AttributeCount => inner.AttributeCount;
    public override string BaseURI => inner.BaseURI;
    public override int Depth => inner.Depth;
    public override bool EOF => inner.EOF;
    public override bool IsEmptyElement => inner.IsEmptyElement;
    public override string LocalName => inner.LocalName;
    public override string NamespaceURI => inner.NamespaceURI;
    public override XmlNameTable NameTable => inner.NameTable;
    public override XmlNodeType NodeType => inner.NodeType;
    public override string Prefix => inner.Prefix;
    public override ReadState ReadState => inner.ReadState;
    public override string Value => inner.Value;
    public override XmlReaderSettings? Settings => inner.Settings;
    public override string GetAttribute(int i) => inner.GetAttribute(i);
    public override string? GetAttribute(string name) => inner.GetAttribute(name);
    public override string? GetAttribute(string name, string? namespaceURI) => inner.GetAttribute(name, namespaceURI);
    public override string? LookupNamespace(string prefix) => inner.LookupNamespace(prefix);
    public override bool MoveToAttribute(string name) => inner.MoveToAttribute(name);
    public override bool MoveToAttribute(string name, string? ns) => inner.MoveToAttribute(name, ns);
    public override bool MoveToElement() => inner.MoveToElement();
    public override bool MoveToFirstAttribute() => inner.MoveToFirstAttribute();
    public override bool MoveToNextAttribute() => inner.MoveToNextAttribute();
    public override bool ReadAttributeValue() => inner.ReadAttributeValue();
    public override void ResolveEntity() => inner.ResolveEntity();
    public bool HasLineInfo() => inner is IXmlLineInfo info && info.HasLineInfo();
    public int LineNumber => inner is IXmlLineInfo info ? info.LineNumber : 0;
    public int LinePosition => inner is IXmlLineInfo info ? info.LinePosition : 0;
    public override void Close() => inner.Close();
}
