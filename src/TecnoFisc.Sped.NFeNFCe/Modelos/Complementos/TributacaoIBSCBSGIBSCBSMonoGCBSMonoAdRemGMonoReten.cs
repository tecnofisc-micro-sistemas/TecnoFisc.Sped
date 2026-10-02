// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemGMonoReten
{
    /// <summary>XML: <c>qBCMonoReten</c>.</summary>
    public decimal? QBCMonoReten { get; init; }

    /// <summary>XML: <c>adRemCBSReten</c>.</summary>
    public decimal? AdRemCBSReten { get; init; }

    /// <summary>XML: <c>vCBSMonoReten</c>.</summary>
    public decimal? VCBSMonoReten { get; init; }
}
