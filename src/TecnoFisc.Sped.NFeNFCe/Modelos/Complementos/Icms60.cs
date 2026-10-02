// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.
namespace TecnoFisc.Sped.NFeNFCe;

public sealed partial record Icms60
{
    /// <summary>XML: <c>vBCFCPSTRet</c>.</summary>
    public decimal? VBCFCPSTRet { get; init; }

    /// <summary>XML: <c>pFCPSTRet</c>.</summary>
    public decimal? PFCPSTRet { get; init; }

    /// <summary>XML: <c>vFCPSTRet</c>.</summary>
    public decimal? VFCPSTRet { get; init; }
}
