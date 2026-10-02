// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGCredPresOperGCBSCredPres
{
    /// <summary>XML: <c>pCredPres</c>.</summary>
    public decimal? PCredPres { get; init; }

    /// <summary>XML: <c>vCredPres</c>.</summary>
    public decimal? VCredPres { get; init; }

    /// <summary>XML: <c>vCredPresCondSus</c>.</summary>
    public decimal? VCredPresCondSus { get; init; }
}
