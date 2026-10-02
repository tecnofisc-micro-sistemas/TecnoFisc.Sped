// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record DetalheExportacaoExportInd
{
    /// <summary>XML: <c>nRE</c>.</summary>
    public string? NRE { get; init; }

    /// <summary>XML: <c>chNFe</c>.</summary>
    public string? ChNFe { get; init; }

    /// <summary>XML: <c>qExport</c>.</summary>
    public decimal? QExport { get; init; }
}
