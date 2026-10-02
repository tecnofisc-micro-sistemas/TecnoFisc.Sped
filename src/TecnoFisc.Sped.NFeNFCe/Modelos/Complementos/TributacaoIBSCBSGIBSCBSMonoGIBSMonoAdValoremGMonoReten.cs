// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdValoremGMonoReten
{
    /// <summary>XML: <c>vBCMonoReten</c>.</summary>
    public decimal? VBCMonoReten { get; init; }

    /// <summary>XML: <c>pAliqMonoReten</c>.</summary>
    public decimal? PAliqMonoReten { get; init; }

    /// <summary>XML: <c>vIBSMonoReten</c>.</summary>
    public decimal? VIBSMonoReten { get; init; }
}
