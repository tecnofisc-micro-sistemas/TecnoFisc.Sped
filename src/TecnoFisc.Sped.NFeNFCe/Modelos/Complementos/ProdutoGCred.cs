// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record ProdutoGCred
{
    /// <summary>XML: <c>cCredPresumido</c>.</summary>
    public string? CCredPresumido { get; init; }

    /// <summary>XML: <c>pCredPresumido</c>.</summary>
    public decimal? PCredPresumido { get; init; }

    /// <summary>XML: <c>vCredPresumido</c>.</summary>
    public decimal? VCredPresumido { get; init; }
}
