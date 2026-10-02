// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.
namespace TecnoFisc.Sped.NFeNFCe;

public sealed partial record Emitente
{
    /// <summary>XML: <c>ISUFEmit</c>.</summary>
    public string? ISUFEmit { get; init; }
}
