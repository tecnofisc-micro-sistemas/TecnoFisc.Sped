// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TotalIBSCBSGCBS
{
    /// <summary>XML: <c>vDif</c>.</summary>
    public decimal? VDif { get; init; }

    /// <summary>XML: <c>vDevTrib</c>.</summary>
    public decimal? VDevTrib { get; init; }

    /// <summary>XML: <c>vCBS</c>.</summary>
    public decimal? VCBS { get; init; }

    /// <summary>XML: <c>vCredPres</c>.</summary>
    public decimal? VCredPres { get; init; }

    /// <summary>XML: <c>vCredPresCondSus</c>.</summary>
    public decimal? VCredPresCondSus { get; init; }
}
