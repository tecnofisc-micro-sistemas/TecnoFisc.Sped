namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Grupo <c>imposto</c> — tributos de um item. Na slice 14.3 (piloto) cobre apenas o ICMS;
/// IPI, PIS, COFINS, II e ISSQN entram na slice 14.4 (polimorfismo de impostos completo).
/// </summary>
public sealed record Imposto
{
    /// <summary>Grupo <c>ICMS</c> — variante de ICMS do item (nula quando ausente).</summary>
    public Icms? Icms { get; init; }
}
