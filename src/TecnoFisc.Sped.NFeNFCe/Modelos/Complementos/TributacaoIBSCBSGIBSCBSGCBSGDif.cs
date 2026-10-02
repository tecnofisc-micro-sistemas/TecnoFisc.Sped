// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSGCBSGDif
{
    /// <summary>XML: <c>pDif</c>.</summary>
    public decimal? PDif { get; init; }

    /// <summary>XML: <c>vDif</c>.</summary>
    public decimal? VDif { get; init; }
}
