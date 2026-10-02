// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record Icms15 : Icms
{
    /// <summary>XML: <c>CST</c>.</summary>
    public string? CST { get; init; }

    /// <summary>XML: <c>qBCMono</c>.</summary>
    public decimal? QBCMono { get; init; }

    /// <summary>XML: <c>adRemICMS</c>.</summary>
    public decimal? AdRemICMS { get; init; }

    /// <summary>XML: <c>vICMSMono</c>.</summary>
    public decimal? VICMSMono { get; init; }

    /// <summary>XML: <c>qBCMonoReten</c>.</summary>
    public decimal? QBCMonoReten { get; init; }

    /// <summary>XML: <c>adRemICMSReten</c>.</summary>
    public decimal? AdRemICMSReten { get; init; }

    /// <summary>XML: <c>vICMSMonoReten</c>.</summary>
    public decimal? VICMSMonoReten { get; init; }

    /// <summary>XML: <c>pRedAdRem</c>.</summary>
    public decimal? PRedAdRem { get; init; }

    /// <summary>XML: <c>motRedAdRem</c>.</summary>
    public string? MotRedAdRem { get; init; }
}
