// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record Icms53 : Icms
{
    /// <summary>XML: <c>CST</c>.</summary>
    public string? CST { get; init; }

    /// <summary>XML: <c>qBCMono</c>.</summary>
    public decimal? QBCMono { get; init; }

    /// <summary>XML: <c>adRemICMS</c>.</summary>
    public decimal? AdRemICMS { get; init; }

    /// <summary>XML: <c>vICMSMonoOp</c>.</summary>
    public decimal? VICMSMonoOp { get; init; }

    /// <summary>XML: <c>pDif</c>.</summary>
    public decimal? PDif { get; init; }

    /// <summary>XML: <c>vICMSMonoDif</c>.</summary>
    public decimal? VICMSMonoDif { get; init; }

    /// <summary>XML: <c>vICMSMono</c>.</summary>
    public decimal? VICMSMono { get; init; }

    /// <summary>XML: <c>qBCMonoDif</c>.</summary>
    public decimal? QBCMonoDif { get; init; }

    /// <summary>XML: <c>adRemICMSDif</c>.</summary>
    public decimal? AdRemICMSDif { get; init; }
}
