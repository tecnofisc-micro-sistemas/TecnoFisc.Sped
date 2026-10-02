// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record ItemDFeReferenciado
{
    /// <summary>XML: <c>chaveAcesso</c>.</summary>
    public string? ChaveAcesso { get; init; }

    /// <summary>XML: <c>nItem</c>.</summary>
    public string? NItem { get; init; }
}
