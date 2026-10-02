// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record CanaForDia
{
    /// <summary>XML: <c>qtde</c>.</summary>
    public decimal? Qtde { get; init; }

    /// <summary>XML: <c>dia</c>.</summary>
    public string? Dia { get; init; }
}
