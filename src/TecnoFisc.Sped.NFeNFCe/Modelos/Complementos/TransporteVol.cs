// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record TransporteVol
{
    /// <summary>XML: <c>qVol</c>.</summary>
    public string? QVol { get; init; }

    /// <summary>XML: <c>esp</c>.</summary>
    public string? Esp { get; init; }

    /// <summary>XML: <c>marca</c>.</summary>
    public string? Marca { get; init; }

    /// <summary>XML: <c>nVol</c>.</summary>
    public string? NVol { get; init; }

    /// <summary>XML: <c>pesoL</c>.</summary>
    public decimal? PesoL { get; init; }

    /// <summary>XML: <c>pesoB</c>.</summary>
    public decimal? PesoB { get; init; }

    /// <summary>XML: <c>lacres</c>.</summary>
    public IReadOnlyList<TransporteVolLacres> Lacres { get; init; } = [];
}
