// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.
namespace TecnoFisc.Sped.NFeNFCe;

public sealed partial record Item
{
    /// <summary>XML: <c>impostoDevol</c>.</summary>
    public ItemImpostoDevol? ImpostoDevol { get; init; }

    /// <summary>XML: <c>infAdProd</c>.</summary>
    public string? InfAdProd { get; init; }

    /// <summary>XML: <c>obsItem</c>.</summary>
    public ItemObsItem? ObsItem { get; init; }

    /// <summary>XML: <c>vItem</c>.</summary>
    public decimal? VItem { get; init; }

    /// <summary>XML: <c>DFeReferenciado</c>.</summary>
    public ItemDFeReferenciado? DFeReferenciado { get; init; }
}
