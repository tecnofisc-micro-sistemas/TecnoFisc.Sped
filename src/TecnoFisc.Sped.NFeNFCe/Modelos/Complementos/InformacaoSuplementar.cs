// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record InformacaoSuplementar
{
    /// <summary>XML: <c>qrCode</c>.</summary>
    public string? QrCode { get; init; }

    /// <summary>XML: <c>urlChave</c>.</summary>
    public string? UrlChave { get; init; }
}
