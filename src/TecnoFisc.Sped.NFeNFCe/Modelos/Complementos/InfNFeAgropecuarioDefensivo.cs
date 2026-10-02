// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record InfNFeAgropecuarioDefensivo
{
    /// <summary>XML: <c>nReceituario</c>.</summary>
    public string? NReceituario { get; init; }

    /// <summary>XML: <c>CPFRespTec</c>.</summary>
    public string? CPFRespTec { get; init; }
}
