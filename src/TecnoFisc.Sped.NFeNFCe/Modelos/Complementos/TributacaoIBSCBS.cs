// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBS
{
    /// <summary>XML: <c>CST</c>.</summary>
    public string? CST { get; init; }

    /// <summary>XML: <c>cClassTrib</c>.</summary>
    public string? CClassTrib { get; init; }

    /// <summary>XML: <c>indDoacao</c>.</summary>
    public string? IndDoacao { get; init; }

    /// <summary>XML: <c>gIBSCBS</c>.</summary>
    public TributacaoIBSCBSGIBSCBS? GIBSCBS { get; init; }

    /// <summary>XML: <c>gIBSCBSMono</c>.</summary>
    public TributacaoIBSCBSGIBSCBSMono? GIBSCBSMono { get; init; }

    /// <summary>XML: <c>gTransfCred</c>.</summary>
    public TributacaoIBSCBSGTransfCred? GTransfCred { get; init; }

    /// <summary>XML: <c>gAjusteCompet</c>.</summary>
    public TributacaoIBSCBSGAjusteCompet? GAjusteCompet { get; init; }

    /// <summary>XML: <c>gEstornoCred</c>.</summary>
    public TributacaoIBSCBSGEstornoCred? GEstornoCred { get; init; }

    /// <summary>XML: <c>gCredPresOper</c>.</summary>
    public TributacaoIBSCBSGCredPresOper? GCredPresOper { get; init; }

    /// <summary>XML: <c>gCredPresIBSZFM</c>.</summary>
    public TributacaoIBSCBSGCredPresIBSZFM? GCredPresIBSZFM { get; init; }
}
