// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.
namespace TecnoFisc.Sped.NFeNFCe;

public sealed partial record Destinatario
{
    /// <summary>XML: <c>ISUF</c>.</summary>
    public string? ISUF { get; init; }

    /// <summary>XML: <c>IM</c>.</summary>
    public string? IM { get; init; }
}
