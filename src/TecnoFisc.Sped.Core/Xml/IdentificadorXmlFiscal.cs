using System.Xml;

namespace TecnoFisc.Sped.Core.Xml;

/// <summary>
/// Identifica o tipo de documento fiscal XML (NF-e/NFC-e/evento/envelope SERPRO) lendo apenas
/// o início do stream com <see cref="XmlReader"/> forward-only. Order-independent e seguro a XXE
/// (DTD proibido). Consome a partir da posição corrente do stream; o chamador reposiciona para reler.
/// </summary>
public static class IdentificadorXmlFiscal
{
    private static readonly XmlReaderSettings Settings = new()
    {
        Async = true,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
        IgnoreWhitespace = true,
        DtdProcessing = DtdProcessing.Prohibit,
        CloseInput = false,
    };

    public static async ValueTask<TipoDocumentoFiscalXml> IdentificarAsync(
        Stream stream, CancellationToken cancellationToken = default)
    {
        using var reader = XmlReader.Create(stream, Settings);

        string? raiz = null;
        try
        {
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (reader.NodeType != XmlNodeType.Element)
                    continue;

                switch (reader.LocalName)
                {
                    // Envelope SERPRO: reconhecido pelo wrapper, antes de qualquer <mod>.
                    case "NFeLog":
                    case "retConsNFeLog":
                    case "nfeConsultaNFeLogResult":
                        return TipoDocumentoFiscalXml.NFeLogSerpro;

                    // Eventos (canônico).
                    case "procEventoNFe":
                        return TipoDocumentoFiscalXml.ProcEventoNFe;
                    case "eventoNFe":
                    case "envEvento":
                        return TipoDocumentoFiscalXml.EventoNFe;

                    // Documento principal: lembrar a raiz e procurar <mod> (55 vs 65).
                    case "nfeProc":
                    case "NFe":
                        raiz ??= reader.LocalName;
                        break;

                    case "mod":
                        string mod = (await reader.ReadElementContentAsStringAsync().ConfigureAwait(false)).Trim();
                        bool nfce = mod == "65";
                        return raiz switch
                        {
                            "nfeProc" => nfce ? TipoDocumentoFiscalXml.NFCeProc : TipoDocumentoFiscalXml.NFeProc,
                            "NFe" => nfce ? TipoDocumentoFiscalXml.NFCe : TipoDocumentoFiscalXml.NFe,
                            _ => TipoDocumentoFiscalXml.Desconhecido,
                        };
                }
            }
        }
        catch (XmlException)
        {
            // XML malformado / EOF prematuro: sniffer não lança, devolve Desconhecido.
            return TipoDocumentoFiscalXml.Desconhecido;
        }

        return TipoDocumentoFiscalXml.Desconhecido;
    }
}
