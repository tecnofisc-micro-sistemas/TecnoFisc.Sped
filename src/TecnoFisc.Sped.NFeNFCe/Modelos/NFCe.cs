using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Xml.Engine;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Nota Fiscal de Consumidor Eletrônica — modelo 65, leiaute 4.00. Modelo nativo (espelha a estrutura do XML),
/// read-only. A <see cref="Protocolo"/> é preenchida quando a origem é um <c>nfeProc</c>
/// (nota autorizada) e fica nula quando é uma <c>NFe</c> pura.
/// </summary>
/// <remarks>
/// Identidade de domínio = <see cref="ChaveAcesso"/> (override manual de
/// <see cref="Equals(NFCe)"/>/<see cref="GetHashCode"/>); ver <c>sped/STAGE_14_NFE_NFCE.md</c> §4.
/// Cobertura fiscal vinculada ao pacote PL_010f_v1.04; ver sped/nfe/field-bindings.json.
/// </remarks>
public sealed partial record NFCe : IDocumentoFiscalXml
{
    public InformacaoSuplementar? InfNFeSupl { get; init; }
    public bool IsAutorizacaoEfetiva => Protocolo?.IsAutorizada == true;
    /// <summary>Chave de acesso de 44 caracteres (do atributo/elemento <c>Id</c> de <c>infNFe</c>).</summary>
    public required ChaveAcesso ChaveAcesso { get; init; }

    /// <summary>Versão do leiaute (atributo/elemento <c>versao</c> de <c>infNFe</c>), ex.: "4.00".</summary>
    public required string Versao { get; init; }

    /// <summary>Grupo <c>ide</c> — identificação da NF-e.</summary>
    public required Identificacao Ide { get; init; }

    /// <summary>Grupo <c>emit</c> — emitente.</summary>
    public required Emitente Emit { get; init; }

    /// <summary>Grupo <c>dest</c> — destinatário (opcional em algumas operações).</summary>
    public Destinatario? Dest { get; init; }

    /// <summary>Grupo <c>total</c> — totais da nota.</summary>
    public required Total Total { get; init; }

    /// <summary>Itens (<c>det</c>) na ordem do documento.</summary>
    public required IReadOnlyList<Item> Itens { get; init; }

    /// <summary>Protocolo de autorização (<c>protNFCe</c>); nulo quando a origem é uma <c>NFe</c> pura.</summary>
    public Protocolo? Protocolo { get; init; }

    /// <summary>Verdadeiro quando a nota carrega protocolo de autorização.</summary>
    public bool IsAutorizada => Protocolo is not null;

    internal static NFCe FromNFe(NFe note) => new()
    {
        InfNFeSupl = note.InfNFeSupl,
        ChaveAcesso = note.ChaveAcesso,
        Versao = note.Versao,
        Ide = note.Ide,
        Emit = note.Emit,
        Dest = note.Dest,
        Total = note.Total,
        Itens = note.Itens,
        Protocolo = note.Protocolo,
        Avulsa = note.Avulsa,
        Retirada = note.Retirada,
        Entrega = note.Entrega,
        AutXML = note.AutXML,
        Transp = note.Transp,
        Cobr = note.Cobr,
        Pag = note.Pag,
        InfIntermed = note.InfIntermed,
        InfAdic = note.InfAdic,
        Exporta = note.Exporta,
        Compra = note.Compra,
        Cana = note.Cana,
        InfRespTec = note.InfRespTec,
        InfSolicNFF = note.InfSolicNFF,
        Agropecuario = note.Agropecuario,
        InfPAA = note.InfPAA,
    };

    /// <summary>Igualdade por identidade de domínio: a chave de acesso.</summary>
    public bool Equals(NFCe? other) => other is not null && ChaveAcesso == other.ChaveAcesso;

    /// <inheritdoc />
    public override int GetHashCode() => ChaveAcesso.GetHashCode();
}
