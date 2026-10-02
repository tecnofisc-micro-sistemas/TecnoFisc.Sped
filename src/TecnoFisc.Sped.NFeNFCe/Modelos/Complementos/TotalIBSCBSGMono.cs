// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TotalIBSCBSGMono
{
    /// <summary>XML: <c>vIBSMono</c>.</summary>
    public decimal? VIBSMono { get; init; }

    /// <summary>XML: <c>vCBSMono</c>.</summary>
    public decimal? VCBSMono { get; init; }

    /// <summary>XML: <c>vIBSMonoReten</c>.</summary>
    public decimal? VIBSMonoReten { get; init; }

    /// <summary>XML: <c>vCBSMonoReten</c>.</summary>
    public decimal? VCBSMonoReten { get; init; }

    /// <summary>XML: <c>vIBSMonoRet</c>.</summary>
    public decimal? VIBSMonoRet { get; init; }

    /// <summary>XML: <c>vCBSMonoRet</c>.</summary>
    public decimal? VCBSMonoRet { get; init; }
}
