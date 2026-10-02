// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemGMonoPadrao
{
    /// <summary>XML: <c>qBCMono</c>.</summary>
    public decimal? QBCMono { get; init; }

    /// <summary>XML: <c>adRemIBS</c>.</summary>
    public decimal? AdRemIBS { get; init; }

    /// <summary>XML: <c>vIBSMono</c>.</summary>
    public decimal? VIBSMono { get; init; }
}
