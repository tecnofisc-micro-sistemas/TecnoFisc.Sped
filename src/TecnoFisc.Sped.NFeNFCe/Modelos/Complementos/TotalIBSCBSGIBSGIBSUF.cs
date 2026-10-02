// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TotalIBSCBSGIBSGIBSUF
{
    /// <summary>XML: <c>vDif</c>.</summary>
    public decimal? VDif { get; init; }

    /// <summary>XML: <c>vDevTrib</c>.</summary>
    public decimal? VDevTrib { get; init; }

    /// <summary>XML: <c>vIBSUF</c>.</summary>
    public decimal? VIBSUF { get; init; }
}
