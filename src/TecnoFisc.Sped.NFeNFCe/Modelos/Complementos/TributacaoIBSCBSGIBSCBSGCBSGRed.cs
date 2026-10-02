// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSGCBSGRed
{
    /// <summary>XML: <c>pRedAliq</c>.</summary>
    public decimal? PRedAliq { get; init; }

    /// <summary>XML: <c>pAliqEfet</c>.</summary>
    public decimal? PAliqEfet { get; init; }
}
