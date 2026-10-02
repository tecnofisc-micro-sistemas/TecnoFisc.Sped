// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSGCBS
{
    /// <summary>XML: <c>pCBS</c>.</summary>
    public decimal? PCBS { get; init; }

    /// <summary>XML: <c>gDif</c>.</summary>
    public TributacaoIBSCBSGIBSCBSGCBSGDif? GDif { get; init; }

    /// <summary>XML: <c>gDevTrib</c>.</summary>
    public TributacaoIBSCBSGIBSCBSGCBSGDevTrib? GDevTrib { get; init; }

    /// <summary>XML: <c>gRed</c>.</summary>
    public TributacaoIBSCBSGIBSCBSGCBSGRed? GRed { get; init; }

    /// <summary>XML: <c>gALCZFMCBS</c>.</summary>
    public TributacaoIBSCBSGIBSCBSGCBSGALCZFMCBS? GALCZFMCBS { get; init; }

    /// <summary>XML: <c>vCBS</c>.</summary>
    public decimal? VCBS { get; init; }
}
