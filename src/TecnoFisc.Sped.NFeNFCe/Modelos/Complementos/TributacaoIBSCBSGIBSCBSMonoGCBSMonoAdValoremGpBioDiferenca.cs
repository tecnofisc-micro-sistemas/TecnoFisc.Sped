// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGIBSCBSMonoGCBSMonoAdValoremGpBioDiferenca
{
    /// <summary>XML: <c>qBCBioComb</c>.</summary>
    public decimal? QBCBioComb { get; init; }

    /// <summary>XML: <c>vCBSDiferenca</c>.</summary>
    public decimal? VCBSDiferenca { get; init; }
}
