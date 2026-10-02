// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGAjusteCompet
{
    /// <summary>XML: <c>competApur</c>.</summary>
    public string? CompetApur { get; init; }

    /// <summary>XML: <c>vIBS</c>.</summary>
    public decimal? VIBS { get; init; }

    /// <summary>XML: <c>vCBS</c>.</summary>
    public decimal? VCBS { get; init; }
}
