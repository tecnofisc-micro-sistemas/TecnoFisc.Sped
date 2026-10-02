// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TransporteRetTransp
{
    /// <summary>XML: <c>vServ</c>.</summary>
    public decimal? VServ { get; init; }

    /// <summary>XML: <c>vBCRet</c>.</summary>
    public decimal? VBCRet { get; init; }

    /// <summary>XML: <c>pICMSRet</c>.</summary>
    public decimal? PICMSRet { get; init; }

    /// <summary>XML: <c>vICMSRet</c>.</summary>
    public decimal? VICMSRet { get; init; }

    /// <summary>XML: <c>CFOP</c>.</summary>
    public string? CFOP { get; init; }

    /// <summary>XML: <c>cMunFG</c>.</summary>
    public string? CMunFG { get; init; }
}
