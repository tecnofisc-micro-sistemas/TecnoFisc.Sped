// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record ProdutoInfProdNFF
{
    /// <summary>XML: <c>cProdFisco</c>.</summary>
    public string? CProdFisco { get; init; }

    /// <summary>XML: <c>cOperNFF</c>.</summary>
    public string? COperNFF { get; init; }
}
