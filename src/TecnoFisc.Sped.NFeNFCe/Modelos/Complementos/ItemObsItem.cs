// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record ItemObsItem
{
    /// <summary>XML: <c>obsCont</c>.</summary>
    public ItemObsItemObsCont? ObsCont { get; init; }

    /// <summary>XML: <c>obsFisco</c>.</summary>
    public ItemObsItemObsFisco? ObsFisco { get; init; }
}
