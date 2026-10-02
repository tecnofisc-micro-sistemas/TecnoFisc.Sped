// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremGMonoPadrao
{
    /// <summary>XML: <c>vBCMono</c>.</summary>
    public decimal? VBCMono { get; init; }

    /// <summary>XML: <c>pAliqMonoCBS</c>.</summary>
    public decimal? PAliqMonoCBS { get; init; }

    /// <summary>XML: <c>vCBSMono</c>.</summary>
    public decimal? VCBSMono { get; init; }
}
