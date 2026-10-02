// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record ProdutoInfProdEmb
{
    /// <summary>XML: <c>xEmb</c>.</summary>
    public string? XEmb { get; init; }

    /// <summary>XML: <c>qVolEmb</c>.</summary>
    public decimal? QVolEmb { get; init; }

    /// <summary>XML: <c>uEmb</c>.</summary>
    public string? UEmb { get; init; }
}
