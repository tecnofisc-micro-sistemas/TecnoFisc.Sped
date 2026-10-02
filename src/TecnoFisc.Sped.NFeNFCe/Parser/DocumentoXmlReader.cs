using System.Runtime.CompilerServices;
using System.Xml;
using TecnoFisc.Sped.Xml.Engine;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe.Parser;

internal sealed class DocumentoXmlReader(XmlReader reader, ParserNFeOptions options)
{
    internal const string FiscalNamespace = "http://www.portalfiscal.inf.br/nfe";
    internal static bool IsFiscal(XmlReader r) => r.NamespaceURI is "" or FiscalNamespace;

    public async IAsyncEnumerable<IDocumentoFiscalXml> ReadAsync([EnumeratorCancellation] CancellationToken ct)
    {
        int logDepth = -1, noteCount = 0;
        ChaveAcesso? contextKey = null;
        List<Evento>? pending = null;
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            ct.ThrowIfCancellationRequested();
            if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == logDepth)
            {
                if (pending is not null)
                {
                    if (noteCount != 1 || contextKey is null)
                        throw new FormatException("Resumo de evento sem contexto de chave inequívoco.");
                    foreach (var evt in pending)
                        yield return evt with { ChaveAcesso = contextKey.Value };
                }
                pending = null;
                contextKey = null;
                noteCount = 0;
                logDepth = -1;
            }
            if (reader.NodeType != XmlNodeType.Element)
                continue;
            if (reader.LocalName == "NFeLog" && IsFiscal(reader))
            {
                if (logDepth >= 0)
                    throw new FormatException("NFeLog aninhado não suportado.");
                logDepth = reader.Depth;
            }
            if (IsFiscal(reader) && reader.LocalName is "procEventoNFe" or "evento" or "eventoNFe")
            {
                var evt = await new EventoXmlReader(reader, options).ReadAsync(ct).ConfigureAwait(false);
                if (evt.ChaveAcesso == default)
                {
                    if (logDepth < 0)
                        throw new FormatException("Resumo de evento sem chave/contexto SERPRO.");
                    (pending ??= []).Add(evt);
                }
                else
                    yield return evt;
                continue;
            }
            if (IsFiscal(reader) && reader.LocalName is "nfeProc" or "procNFe" or "NFe")
            {
                string? version = reader.GetAttribute("versao");
                var note = await new NFeXmlReader(reader, options).ReadAsync(ct, positioned: true).ConfigureAwait(false);
                if (logDepth >= 0)
                { contextKey = note.ChaveAcesso; noteCount++; }
                if (note.Versao.Length == 0 && version is not null)
                    note = note with { Versao = version };
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
