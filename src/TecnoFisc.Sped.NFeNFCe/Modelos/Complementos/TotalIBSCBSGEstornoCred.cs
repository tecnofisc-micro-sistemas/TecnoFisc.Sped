// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TotalIBSCBSGEstornoCred
{
    /// <summary>XML: <c>vIBSEstCred</c>.</summary>
    public decimal? VIBSEstCred { get; init; }

    /// <summary>XML: <c>vCBSEstCred</c>.</summary>
    public decimal? VCBSEstCred { get; init; }
}
