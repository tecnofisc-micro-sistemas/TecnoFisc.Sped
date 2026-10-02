// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record Cana
{
    /// <summary>XML: <c>safra</c>.</summary>
    public string? Safra { get; init; }

    /// <summary>XML: <c>ref</c>.</summary>
    public string? Ref { get; init; }

    /// <summary>XML: <c>forDia</c>.</summary>
    public IReadOnlyList<CanaForDia> ForDia { get; init; } = [];

    /// <summary>XML: <c>qTotMes</c>.</summary>
    public decimal? QTotMes { get; init; }

    /// <summary>XML: <c>qTotAnt</c>.</summary>
    public decimal? QTotAnt { get; init; }

    /// <summary>XML: <c>qTotGer</c>.</summary>
    public decimal? QTotGer { get; init; }

    /// <summary>XML: <c>deduc</c>.</summary>
    public IReadOnlyList<CanaDeduc> Deduc { get; init; } = [];

    /// <summary>XML: <c>vFor</c>.</summary>
    public decimal? VFor { get; init; }

    /// <summary>XML: <c>vTotDed</c>.</summary>
    public decimal? VTotDed { get; init; }

    /// <summary>XML: <c>vLiqFor</c>.</summary>
    public decimal? VLiqFor { get; init; }
}
