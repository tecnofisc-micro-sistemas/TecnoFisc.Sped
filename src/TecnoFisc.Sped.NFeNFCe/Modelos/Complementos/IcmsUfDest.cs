// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record IcmsUfDest
{
    /// <summary>XML: <c>vBCUFDest</c>.</summary>
    public decimal? VBCUFDest { get; init; }

    /// <summary>XML: <c>vBCFCPUFDest</c>.</summary>
    public decimal? VBCFCPUFDest { get; init; }

    /// <summary>XML: <c>pFCPUFDest</c>.</summary>
    public decimal? PFCPUFDest { get; init; }

    /// <summary>XML: <c>pICMSUFDest</c>.</summary>
    public decimal? PICMSUFDest { get; init; }

    /// <summary>XML: <c>pICMSInter</c>.</summary>
    public string? PICMSInter { get; init; }

    /// <summary>XML: <c>pICMSInterPart</c>.</summary>
    public decimal? PICMSInterPart { get; init; }

    /// <summary>XML: <c>vFCPUFDest</c>.</summary>
    public decimal? VFCPUFDest { get; init; }

    /// <summary>XML: <c>vICMSUFDest</c>.</summary>
    public decimal? VICMSUFDest { get; init; }

    /// <summary>XML: <c>vICMSUFRemet</c>.</summary>
    public decimal? VICMSUFRemet { get; init; }
}
