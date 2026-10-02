// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TotalIBSCBS
{
    /// <summary>XML: <c>vBCIBSCBS</c>.</summary>
    public decimal? VBCIBSCBS { get; init; }

    /// <summary>XML: <c>gIBS</c>.</summary>
    public TotalIBSCBSGIBS? GIBS { get; init; }

    /// <summary>XML: <c>gCBS</c>.</summary>
    public TotalIBSCBSGCBS? GCBS { get; init; }

    /// <summary>XML: <c>gMono</c>.</summary>
    public TotalIBSCBSGMono? GMono { get; init; }

    /// <summary>XML: <c>gEstornoCred</c>.</summary>
    public TotalIBSCBSGEstornoCred? GEstornoCred { get; init; }
}
