// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdRemGMonoPadrao
{
    /// <summary>XML: <c>qBCMono</c>.</summary>
    public decimal? QBCMono { get; init; }

    /// <summary>XML: <c>adRemCBS</c>.</summary>
    public decimal? AdRemCBS { get; init; }

    /// <summary>XML: <c>vCBSMono</c>.</summary>
    public decimal? VCBSMono { get; init; }
}
