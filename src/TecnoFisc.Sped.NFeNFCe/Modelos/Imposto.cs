namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Grupo <c>imposto</c> — tributos de um item.
/// </summary>
public sealed record Imposto
{
    /// <summary>Grupo <c>ICMS</c> — variante de ICMS do item (nula quando ausente).</summary>
    public Icms? Icms { get; init; }

    /// <summary>Grupo <c>IPI</c> — dados de IPI do item (nulo quando ausente).</summary>
    public Ipi? Ipi { get; init; }

    /// <summary>
    /// <c>vTotTrib</c> — valor aproximado total de tributos federais, estaduais e municipais
    /// (Lei 12.741/2012 — informativo, opcional).
    /// </summary>
    public decimal? VTotTrib { get; init; }
}
