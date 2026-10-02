using System.Runtime.CompilerServices;
using System.Xml;
using TecnoFisc.Sped.NFeNFCe.IO;
using TecnoFisc.Sped.Xml.Engine;

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed class DocumentoXmlReader(XmlReader reader, ParserNFeOptions options)
{
    internal const string FiscalNamespace = "http://www.portalfiscal.inf.br/nfe";
    internal static bool IsFiscal(XmlReader r) => r.NamespaceURI is "" or FiscalNamespace;

    public async IAsyncEnumerable<IDocumentoFiscalXml> ReadAsync([EnumeratorCancellation] CancellationToken ct)
    {
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (reader.NodeType != XmlNodeType.Element) continue;
            if (IsFiscal(reader) && reader.LocalName is "nfeProc" or "procNFe" or "NFe" or "infNFe")
            {
                string? version = reader.GetAttribute("versao");
                using var subtree = new GuardedXmlReader(reader.ReadSubtree(), options, ct);
                var note = await new NFeXmlReader(subtree, options).ReadAsync(ct).ConfigureAwait(false);
                if (note.Versao.Length == 0 && version is not null) note = note with { Versao = version };
                yield return note.Ide.Mod.Codigo switch
                {
                    "55" => note,
                    "65" => NFCe.FromNFe(note),
                    _ => throw new FormatException($"Modelo não suportado: {note.Ide.Mod.Codigo}."),
                };
            }
        }
    }
}
