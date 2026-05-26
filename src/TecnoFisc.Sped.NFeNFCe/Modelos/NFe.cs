using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Core.Xml;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Nota Fiscal Eletrônica — modelo 55, leiaute 4.00. Modelo nativo (espelha a estrutura do XML),
/// read-only. A <see cref="Protocolo"/> é preenchida quando a origem é um <c>nfeProc</c>
/// (nota autorizada) e fica nula quando é uma <c>NFe</c> pura.
/// </summary>
/// <remarks>
/// Identidade de domínio = <see cref="ChaveAcesso"/> (override manual de
/// <see cref="Equals(NFe)"/>/<see cref="GetHashCode"/>); ver <c>sped/STAGE_14_NFE_NFCE.md</c> §4.
/// Escopo da slice 14.3 (piloto): <c>ide</c>, <c>emit</c>, <c>dest</c>, <c>total</c> e os
/// <c>det</c>/<c>prod</c> com ICMS60. Demais grupos (<c>transp</c>, <c>cobr</c>, <c>pag</c>,
/// impostos completos, etc.) entram nas slices 14.4–14.6.
/// </remarks>
public sealed record NFe : IDocumentoFiscalXml
{
    /// <summary>Chave de acesso de 44 dígitos (do atributo/elemento <c>Id</c> de <c>infNFe</c>).</summary>
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

    /// <summary>Protocolo de autorização (<c>protNFe</c>); nulo quando a origem é uma <c>NFe</c> pura.</summary>
    public Protocolo? Protocolo { get; init; }

    /// <summary>Verdadeiro quando a nota carrega protocolo de autorização.</summary>
    public bool IsAutorizada => Protocolo is not null;

    /// <summary>Igualdade por identidade de domínio: a chave de acesso.</summary>
    public bool Equals(NFe? other) => other is not null && ChaveAcesso == other.ChaveAcesso;

    /// <inheritdoc />
    public override int GetHashCode() => ChaveAcesso.GetHashCode();
}
