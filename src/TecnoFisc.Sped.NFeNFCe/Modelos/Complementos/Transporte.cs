// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record Transporte
{
    /// <summary>XML: <c>modFrete</c>.</summary>
    public string? ModFrete { get; init; }

    /// <summary>XML: <c>transporta</c>.</summary>
    public TransporteTransporta? Transporta { get; init; }

    /// <summary>XML: <c>retTransp</c>.</summary>
    public TransporteRetTransp? RetTransp { get; init; }

    /// <summary>XML: <c>veicTransp</c>.</summary>
    public TransporteVeicTransp? VeicTransp { get; init; }

    /// <summary>XML: <c>reboque</c>.</summary>
    public IReadOnlyList<TransporteReboque> Reboque { get; init; } = [];

    /// <summary>XML: <c>vagao</c>.</summary>
    public string? Vagao { get; init; }

    /// <summary>XML: <c>balsa</c>.</summary>
    public string? Balsa { get; init; }

    /// <summary>XML: <c>vol</c>.</summary>
    public IReadOnlyList<TransporteVol> Vol { get; init; } = [];
}
