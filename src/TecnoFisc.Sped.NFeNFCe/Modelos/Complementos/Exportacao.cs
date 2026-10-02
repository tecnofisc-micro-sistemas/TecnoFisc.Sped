// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record Exportacao
{
    /// <summary>XML: <c>UFSaidaPais</c>.</summary>
    public string? UFSaidaPais { get; init; }

    /// <summary>XML: <c>xLocExporta</c>.</summary>
    public string? XLocExporta { get; init; }

    /// <summary>XML: <c>xLocDespacho</c>.</summary>
    public string? XLocDespacho { get; init; }
}
