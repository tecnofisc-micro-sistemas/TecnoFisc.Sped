// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TransporteReboque
{
    /// <summary>XML: <c>placa</c>.</summary>
    public string? Placa { get; init; }

    /// <summary>XML: <c>UF</c>.</summary>
    public string? UF { get; init; }

    /// <summary>XML: <c>RNTC</c>.</summary>
    public string? RNTC { get; init; }
}
