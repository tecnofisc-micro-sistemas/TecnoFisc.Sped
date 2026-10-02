// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record InformacoesAdicionaisObsCont
{
    /// <summary>XML: <c>xTexto</c>.</summary>
    public string? XTexto { get; init; }

    /// <summary>XML: <c>xCampo</c>.</summary>
    public string? XCampo { get; init; }
}
