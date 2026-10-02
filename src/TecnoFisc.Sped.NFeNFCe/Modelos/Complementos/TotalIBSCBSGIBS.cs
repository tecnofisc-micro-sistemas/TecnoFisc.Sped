// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TotalIBSCBSGIBS
{
    /// <summary>XML: <c>gIBSUF</c>.</summary>
    public TotalIBSCBSGIBSGIBSUF? GIBSUF { get; init; }

    /// <summary>XML: <c>gIBSMun</c>.</summary>
    public TotalIBSCBSGIBSGIBSMun? GIBSMun { get; init; }

    /// <summary>XML: <c>vIBS</c>.</summary>
    public decimal? VIBS { get; init; }

    /// <summary>XML: <c>vCredPres</c>.</summary>
    public decimal? VCredPres { get; init; }

    /// <summary>XML: <c>vCredPresCondSus</c>.</summary>
    public decimal? VCredPresCondSus { get; init; }
}
