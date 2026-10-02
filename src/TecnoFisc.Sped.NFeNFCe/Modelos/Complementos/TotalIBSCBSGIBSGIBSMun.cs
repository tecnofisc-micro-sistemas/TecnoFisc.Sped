// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TotalIBSCBSGIBSGIBSMun
{
    /// <summary>XML: <c>vDif</c>.</summary>
    public decimal? VDif { get; init; }

    /// <summary>XML: <c>vDevTrib</c>.</summary>
    public decimal? VDevTrib { get; init; }

    /// <summary>XML: <c>vIBSMun</c>.</summary>
    public decimal? VIBSMun { get; init; }
}
