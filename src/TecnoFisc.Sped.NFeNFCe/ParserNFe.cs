using System.Xml;

using TecnoFisc.Sped.Core.Xml;
using TecnoFisc.Sped.NFeNFCe.Parser;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Leitor dos documentos fiscais XML do leiaute 4.00 (NF-e modelo 55, NFC-e modelo 65 e
/// eventos), read-only. Thread-safe e sem estado mutável — uma instância pode ser compartilhada
/// entre threads. A desserialização é <b>order-independent</b> (tolera tanto o XML canônico
/// quanto a reordenação do envelope SERPRO); ver <c>sped/STAGE_14_NFE_NFCE.md</c> §3.
/// </summary>
/// <remarks>
/// Slice 14.3 (piloto): apenas a leitura de NF-e modelo 55 (<see cref="ReadNFeAsync"/>), incluindo
/// o desembrulho do envelope SERPRO de documento único. NFC-e (65) e eventos chegam nas slices
/// 14.6/14.7; <c>ReadDirectoryAsync</c> na 14.8.
/// </remarks>
public sealed class ParserNFe
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
    /// <returns>A <see cref="NFe"/> lida, com <see cref="NFe.Protocolo"/> preenchido quando autorizada.</returns>
    /// <exception cref="FormatException">Quando o documento não contém um grupo <c>infNFe</c>.</exception>
    public async Task<NFe> ReadNFeAsync(Stream entrada, CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);

        using var reader = XmlReader.Create(entrada, Settings);
        var leitor = new NFeXmlReader(reader, Options);
        return await leitor.ReadAsync(cancelamento).ConfigureAwait(false);
    }

    /// <summary>
    /// Lê um documento fiscal de <paramref name="entrada"/> e devolve o modelo tipado pelo
    /// contrato comum <see cref="IDocumentoFiscalXml"/> — o consumidor faz pattern matching para
    /// os tipos concretos (NF-e/NFC-e/evento).
    /// </summary>
    /// <param name="entrada">Stream do XML (canônico ou envelope SERPRO).</param>
    /// <param name="cancelamento">Token de cancelamento.</param>
    /// <remarks>Na slice 14.3 o fluxo genérico resolve para <see cref="ReadNFeAsync"/> (NF-e 55).</remarks>
    public async Task<IDocumentoFiscalXml> ReadAsync(Stream entrada, CancellationToken cancelamento = default)
        => await ReadNFeAsync(entrada, cancelamento).ConfigureAwait(false);
}
