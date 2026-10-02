// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record Icms61 : Icms
{
    /// <summary>XML: <c>CST</c>.</summary>
    public string? CST { get; init; }

    /// <summary>XML: <c>qBCMonoRet</c>.</summary>
    public decimal? QBCMonoRet { get; init; }

    /// <summary>XML: <c>adRemICMSRet</c>.</summary>
    public decimal? AdRemICMSRet { get; init; }

    /// <summary>XML: <c>vICMSMonoRet</c>.</summary>
    public decimal? VICMSMonoRet { get; init; }
}
