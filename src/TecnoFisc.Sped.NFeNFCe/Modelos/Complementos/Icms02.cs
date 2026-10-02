// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record Icms02 : Icms
{
    /// <summary>XML: <c>CST</c>.</summary>
    public string? CST { get; init; }

    /// <summary>XML: <c>qBCMono</c>.</summary>
    public decimal? QBCMono { get; init; }

    /// <summary>XML: <c>adRemICMS</c>.</summary>
    public decimal? AdRemICMS { get; init; }

    /// <summary>XML: <c>vICMSMono</c>.</summary>
    public decimal? VICMSMono { get; init; }
}
