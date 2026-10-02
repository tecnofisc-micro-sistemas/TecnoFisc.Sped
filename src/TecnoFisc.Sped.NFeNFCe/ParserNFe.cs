using System.Xml;

using TecnoFisc.Sped.Xml.Engine;
using TecnoFisc.Sped.NFeNFCe.Parser;
using TecnoFisc.Sped.NFeNFCe.IO;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Leitor dos documentos fiscais XML do leiaute 4.00 (NF-e modelo 55, NFC-e modelo 65 e
/// eventos), read-only. Thread-safe e sem estado mutável — uma instância pode ser compartilhada
/// entre threads. A desserialização é <b>order-independent</b> (tolera tanto o XML canônico
/// quanto a reordenação do envelope SERPRO); ver <c>sped/STAGE_14_NFE_NFCE.md</c> §3.
/// </summary>
/// <remarks>Leiaute 4.00, com importação limitada de XML/ZIP; ver docs/nfe-importacao.md.</remarks>
public sealed partial class ParserNFe
{
    private static readonly XmlReaderSettings Settings = new()
    {
        Async = true,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
        IgnoreWhitespace = true,
        DtdProcessing = DtdProcessing.Prohibit, // bloqueia XXE
        CloseInput = false, // não fecha o stream do chamador
    };

    /// <summary>Cria o parser com as opções padrão (<see cref="ParserNFeOptions.Default"/>).</summary>
    public ParserNFe() : this(ParserNFeOptions.Default)
    {
    }

    /// <summary>Cria o parser com opções customizadas.</summary>
    public ParserNFe(ParserNFeOptions opcoes)
    {
        ArgumentNullException.ThrowIfNull(opcoes);
        opcoes.Validate();
        Options = opcoes;
    }

    /// <summary>Opções com que este parser foi construído.</summary>
    public ParserNFeOptions Options { get; }

    /// <summary>
    /// Lê uma NF-e (modelo 55) de <paramref name="entrada"/>. Aceita o XML canônico
    /// (<c>nfeProc</c> ou <c>NFe</c> pura) e o envelope SERPRO de documento único
    /// (SOAP → <c>retConsNFeLog</c> → <c>NFeLog</c>), de onde extrai a nota e o protocolo
    /// independentemente da ordem dos elementos.
    /// </summary>
    /// <param name="entrada">Stream do XML (UTF-8).</param>
    /// <param name="cancelamento">Token de cancelamento.</param>
    /// <returns>A <see cref="NFe"/> lida, com <see cref="NFe.Protocolo"/> preenchido quando presente na origem.</returns>
    /// <exception cref="FormatException">Quando o documento não contém um grupo <c>infNFe</c>.</exception>
    public async Task<NFe> ReadNFeAsync(Stream entrada, CancellationToken cancelamento = default)
    {
        var documento = await ReadAsync(entrada, cancelamento).ConfigureAwait(false);
        return documento as NFe ?? throw new FormatException("Documento não é NF-e modelo 55.");
    }

    /// <summary>
    /// Lê um documento fiscal de <paramref name="entrada"/> e devolve o modelo tipado pelo
    /// contrato comum <see cref="IDocumentoFiscalXml"/> — o consumidor faz pattern matching para
    /// os tipos concretos (NF-e/NFC-e/evento).
    /// </summary>
    /// <param name="entrada">Stream do XML (canônico ou envelope SERPRO).</param>
    /// <param name="cancelamento">Token de cancelamento.</param>
    /// <remarks>Uma nota acompanhada de eventos retorna a nota; use ReadDocumentsAsync para receber todos.</remarks>
    public async Task<IDocumentoFiscalXml> ReadAsync(Stream entrada, CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        IDocumentoFiscalXml? result = null;
        Evento? firstEvent = null;
        int eventCount = 0;
        await foreach (var document in ReadDocumentsAsync(entrada, cancelamento).ConfigureAwait(false))
        {
            if (document is Evento evt)
            { firstEvent ??= evt; eventCount++; continue; }
            if (result is not null)
                throw new FormatException("Múltiplos documentos: utilize ReadDocumentsAsync.");
            result = document;
        }
        return result ?? (eventCount == 1 ? firstEvent : null)
            ?? throw new FormatException(eventCount > 1 ? "Múltiplos eventos: utilize ReadDocumentsAsync." : "Nenhum documento fiscal encontrado no XML.");
    }

    public async Task<Evento> ReadEventoAsync(Stream entrada, CancellationToken cancelamento = default)
        => await ReadAsync(entrada, cancelamento).ConfigureAwait(false) as Evento
            ?? throw new FormatException("Documento não é um evento fiscal.");

    public async Task<NFCe> ReadNFCeAsync(Stream entrada, CancellationToken cancelamento = default)
        => await ReadAsync(entrada, cancelamento).ConfigureAwait(false) as NFCe
            ?? throw new FormatException("Documento não é NFC-e modelo 65.");

    public async IAsyncEnumerable<IDocumentoFiscalXml> ReadDocumentsAsync(
        Stream entrada, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        using var limited = new LimitedReadStream(entrada, Options.MaxXmlBytes, true, cancelamento);
        using var reader = new GuardedXmlReader(XmlReader.Create(limited, Settings), Options, cancelamento);
        var router = new DocumentoXmlReader(reader, Options);
        await foreach (var document in router.ReadAsync(cancelamento).ConfigureAwait(false))
            yield return document;
    }
}
