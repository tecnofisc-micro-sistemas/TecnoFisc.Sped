// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSGIBSMun
{
    /// <summary>XML: <c>pIBSMun</c>.</summary>
    public decimal? PIBSMun { get; init; }

    /// <summary>XML: <c>gDif</c>.</summary>
    public TributacaoIBSCBSGIBSCBSGIBSMunGDif? GDif { get; init; }

    /// <summary>XML: <c>gDevTrib</c>.</summary>
    public TributacaoIBSCBSGIBSCBSGIBSMunGDevTrib? GDevTrib { get; init; }

    /// <summary>XML: <c>gRed</c>.</summary>
    public TributacaoIBSCBSGIBSCBSGIBSMunGRed? GRed { get; init; }

    /// <summary>XML: <c>vIBSMun</c>.</summary>
    public decimal? VIBSMun { get; init; }
}
