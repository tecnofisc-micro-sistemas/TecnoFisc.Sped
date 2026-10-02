// Scaffolding revisÃƒÂ¡vel a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record CobrancaDup
{
    /// <summary>XML: <c>nDup</c>.</summary>
    public string? NDup { get; init; }

    /// <summary>XML: <c>dVenc</c>.</summary>
    public DateOnly? DVenc { get; init; }

    /// <summary>XML: <c>vDup</c>.</summary>
    public decimal? VDup { get; init; }
}
