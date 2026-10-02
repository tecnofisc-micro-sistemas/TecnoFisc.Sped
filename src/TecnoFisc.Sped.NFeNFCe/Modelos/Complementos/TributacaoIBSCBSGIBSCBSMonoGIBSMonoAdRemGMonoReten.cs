// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemGMonoReten
{
    /// <summary>XML: <c>qBCMonoReten</c>.</summary>
    public decimal? QBCMonoReten { get; init; }

    /// <summary>XML: <c>adRemIBSReten</c>.</summary>
    public decimal? AdRemIBSReten { get; init; }

    /// <summary>XML: <c>vIBSMonoReten</c>.</summary>
    public decimal? VIBSMonoReten { get; init; }
}
