// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record InfNFeAgropecuarioGuiaTransito
{
    /// <summary>XML: <c>tpGuia</c>.</summary>
    public string? TpGuia { get; init; }

    /// <summary>XML: <c>UFGuia</c>.</summary>
    public string? UFGuia { get; init; }

    /// <summary>XML: <c>serieGuia</c>.</summary>
    public string? SerieGuia { get; init; }

    /// <summary>XML: <c>nGuia</c>.</summary>
    public string? NGuia { get; init; }
}
