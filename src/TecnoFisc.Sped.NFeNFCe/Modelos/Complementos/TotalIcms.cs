// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.
namespace TecnoFisc.Sped.NFeNFCe;

public sealed partial record TotalIcms
{
    /// <summary>XML: <c>vFCPUFDest</c>.</summary>
    public decimal? VFCPUFDest { get; init; }

    /// <summary>XML: <c>vICMSUFDest</c>.</summary>
    public decimal? VICMSUFDest { get; init; }

    /// <summary>XML: <c>vICMSUFRemet</c>.</summary>
    public decimal? VICMSUFRemet { get; init; }

    /// <summary>XML: <c>qBCMono</c>.</summary>
    public decimal? QBCMono { get; init; }

    /// <summary>XML: <c>vICMSMono</c>.</summary>
    public decimal? VICMSMono { get; init; }

    /// <summary>XML: <c>qBCMonoReten</c>.</summary>
    public decimal? QBCMonoReten { get; init; }

    /// <summary>XML: <c>vICMSMonoReten</c>.</summary>
    public decimal? VICMSMonoReten { get; init; }

    /// <summary>XML: <c>qBCMonoRet</c>.</summary>
    public decimal? QBCMonoRet { get; init; }

    /// <summary>XML: <c>vICMSMonoRet</c>.</summary>
    public decimal? VICMSMonoRet { get; init; }

    /// <summary>XML: <c>vTotTrib</c>.</summary>
    public decimal? VTotTrib { get; init; }
}
