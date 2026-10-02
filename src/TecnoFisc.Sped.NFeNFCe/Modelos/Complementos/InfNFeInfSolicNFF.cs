// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record InfNFeInfSolicNFF
{
    /// <summary>XML: <c>xSolic</c>.</summary>
    public string? XSolic { get; init; }
}
