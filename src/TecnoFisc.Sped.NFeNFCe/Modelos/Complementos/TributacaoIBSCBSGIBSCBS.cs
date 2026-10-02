// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBS
{
    /// <summary>XML: <c>vBC</c>.</summary>
    public decimal? VBC { get; init; }

    /// <summary>XML: <c>gIBSUF</c>.</summary>
    public TributacaoIBSCBSGIBSCBSGIBSUF? GIBSUF { get; init; }

    /// <summary>XML: <c>gIBSMun</c>.</summary>
    public TributacaoIBSCBSGIBSCBSGIBSMun? GIBSMun { get; init; }

    /// <summary>XML: <c>vIBS</c>.</summary>
    public decimal? VIBS { get; init; }

    /// <summary>XML: <c>gCBS</c>.</summary>
    public TributacaoIBSCBSGIBSCBSGCBS? GCBS { get; init; }

    /// <summary>XML: <c>gTribRegular</c>.</summary>
    public TributacaoIBSCBSGIBSCBSGTribRegular? GTribRegular { get; init; }

    /// <summary>XML: <c>gTribCompraGov</c>.</summary>
    public TributacaoIBSCBSGIBSCBSGTribCompraGov? GTribCompraGov { get; init; }
}
