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

    /// <summary>Grupo <c>PIS</c> — tributação PIS do item (nula quando ausente).</summary>
    public Pis? Pis { get; init; }

    /// <summary>Grupo <c>PISST</c> — PIS substituição tributária do item (nulo quando ausente).</summary>
    public PisSt? PisSt { get; init; }

    /// <summary>Grupo <c>COFINS</c> — tributação COFINS do item (nula quando ausente).</summary>
    public Cofins? Cofins { get; init; }

    /// <summary>Grupo <c>COFINSST</c> — COFINS substituição tributária do item (nulo quando ausente).</summary>
    public CofinsSt? CofinsSt { get; init; }

    /// <summary>Grupo <c>II</c> — dados do Imposto de Importação do item (nulo quando ausente).</summary>
    public Ii? Ii { get; init; }

    /// <summary>Grupo <c>ISSQN</c> — dados do ISSQN do item (nulo quando ausente).</summary>
    public Issqn? Issqn { get; init; }

    /// <summary>
    /// <c>vTotTrib</c> — valor aproximado total de tributos federais, estaduais e municipais
    /// (Lei 12.741/2012 — informativo, opcional).
    /// </summary>
    public decimal? VTotTrib { get; init; }
}
