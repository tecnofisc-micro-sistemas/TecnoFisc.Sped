// Scaffolding revisável a partir do XSD fixado em sped/nfe/schema-reference.json.

namespace TecnoFisc.Sped.NFeNFCe;

public sealed record DetalheExportacao
{
    /// <summary>XML: <c>nDraw</c>.</summary>
    public string? NDraw { get; init; }

    /// <summary>XML: <c>exportInd</c>.</summary>
    public DetalheExportacaoExportInd? ExportInd { get; init; }
}
