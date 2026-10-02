// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record Medicamento
{
    /// <summary>XML: <c>cProdANVISA</c>.</summary>
    public string? CProdANVISA { get; init; }

    /// <summary>XML: <c>xMotivoIsencao</c>.</summary>
    public string? XMotivoIsencao { get; init; }

    /// <summary>XML: <c>vPMC</c>.</summary>
    public decimal? VPMC { get; init; }
}
