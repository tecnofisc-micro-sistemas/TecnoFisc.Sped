// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TributacaoIBSCBSGCredPresIBSZFM
{
    /// <summary>XML: <c>competApur</c>.</summary>
    public string? CompetApur { get; init; }

    /// <summary>XML: <c>tpCredPresIBSZFM</c>.</summary>
    public string? TpCredPresIBSZFM { get; init; }

    /// <summary>XML: <c>vCredPresIBSZFM</c>.</summary>
    public decimal? VCredPresIBSZFM { get; init; }
}
