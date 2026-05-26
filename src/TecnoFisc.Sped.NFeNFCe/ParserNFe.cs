using TecnoFisc.Sped.Core.Xml;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Leitor dos documentos fiscais XML do leiaute 4.00 (NF-e modelo 55, NFC-e modelo 65 e
/// eventos), read-only. Thread-safe e sem estado mutável — uma instância pode ser compartilhada
/// entre threads. A desserialização é <b>order-independent</b> (tolera tanto o XML canônico
/// quanto a reordenação do envelope SERPRO); ver <c>sped/STAGE_14_NFE_NFCE.md</c> §3.
/// </summary>
/// <remarks>
/// Esqueleto da slice 14.2: o corpo de leitura entra a partir da slice 14.3.
/// </remarks>
public sealed class ParserNFe
{
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
    /// Lê um documento fiscal de <paramref name="entrada"/> e devolve o modelo tipado pelo
    /// contrato comum <see cref="IDocumentoFiscalXml"/> — o consumidor faz pattern matching para
    /// os tipos concretos (NF-e/NFC-e/evento).
    /// </summary>
    /// <param name="entrada">Stream do XML (canônico ou envelope SERPRO).</param>
    /// <param name="cancelamento">Token de cancelamento.</param>
    public Task<IDocumentoFiscalXml> ReadAsync(Stream entrada, CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        _ = Options; // consumido a partir da slice 14.3; mantém o método de instância (CA1822)
        throw new NotImplementedException(
            "Desserialização entra a partir da slice 14.3 (ver sped/STAGE_14_NFE_NFCE.md §8).");
    }
}
