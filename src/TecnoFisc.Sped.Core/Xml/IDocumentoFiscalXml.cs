using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.Core.Xml;

/// <summary>
/// Contrato comum dos documentos fiscais eletrônicos em XML (NF-e, NFC-e e eventos) produzidos
/// pelos parsers de leiaute (ex.: <c>TecnoFisc.Sped.NFeNFCe</c>). Permite que o fluxo genérico
/// (<c>ParserNFe.ReadAsync</c>/<c>ReadDirectoryAsync</c>) e o <c>Correlator</c> tratem qualquer
/// documento pela sua chave de acesso sem conhecer o tipo concreto (o consumidor faz pattern
/// matching para os tipos específicos). Transversal a leiautes XML fiscais (NF-e/NFC-e hoje,
/// CT-e no futuro) — por isso reside no Core, ao lado de <see cref="IdentificadorXmlFiscal"/>.
/// </summary>
public interface IDocumentoFiscalXml
{
    /// <summary>
    /// Chave de acesso de 44 dígitos do documento (em eventos, a chave referenciada em
    /// <c>chNFe</c>). É a chave de correlação entre uma nota e seus eventos.
    /// </summary>
    public ChaveAcesso ChaveAcesso { get; }
}
