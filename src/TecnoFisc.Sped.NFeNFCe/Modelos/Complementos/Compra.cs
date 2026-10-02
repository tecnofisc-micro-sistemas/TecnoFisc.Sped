// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record Compra
{
    /// <summary>XML: <c>xNEmp</c>.</summary>
    public string? XNEmp { get; init; }

    /// <summary>XML: <c>xPed</c>.</summary>
    public string? XPed { get; init; }

    /// <summary>XML: <c>xCont</c>.</summary>
    public string? XCont { get; init; }
}
