// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record InfNFeAgropecuario
{
    /// <summary>XML: <c>defensivo</c>.</summary>
    public IReadOnlyList<InfNFeAgropecuarioDefensivo> Defensivo { get; init; } = [];

    /// <summary>XML: <c>guiaTransito</c>.</summary>
    public InfNFeAgropecuarioGuiaTransito? GuiaTransito { get; init; }
}
