namespace TecnoFisc.Sped.Core.Xml;

/// <summary>
/// Tipo de documento fiscal XML reconhecido pelo <see cref="IdentificadorXmlFiscal"/>
/// a partir do início do stream.
/// </summary>
public enum TipoDocumentoFiscalXml
{
    /// <summary>Não foi possível identificar o documento.</summary>
    Desconhecido = 0,

    /// <summary>nfeProc (NF-e autorizada, modelo 55).</summary>
    NFeProc,

    /// <summary>NFe pura (sem protocolo), modelo 55.</summary>
    NFe,

    /// <summary>nfeProc (NFC-e autorizada, modelo 65).</summary>
    NFCeProc,

    /// <summary>NFe pura (sem protocolo), modelo 65.</summary>
    NFCe,

    /// <summary>procEventoNFe (evento autorizado).</summary>
    ProcEventoNFe,

    /// <summary>eventoNFe / envEvento (evento não autorizado).</summary>
    EventoNFe,

    /// <summary>Envelope proprietário da SERPRO (retConsNFeLog / NFeLog), que empacota documento + eventos.</summary>
    NFeLogSerpro,
}
