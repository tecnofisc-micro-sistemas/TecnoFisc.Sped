// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSMonoGIBSMonoAdRemGpBioDiferenca
{
    /// <summary>XML: <c>qBCBioComb</c>.</summary>
    public decimal? QBCBioComb { get; init; }

    /// <summary>XML: <c>vIBSDiferenca</c>.</summary>
    public decimal? VIBSDiferenca { get; init; }
}
