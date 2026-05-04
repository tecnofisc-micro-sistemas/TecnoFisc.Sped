namespace TecnoFisc.Sped.Core.ValueObjects;

/// <summary>
/// Tributo ao qual um <see cref="Cst"/> se refere. Determina o comprimento aceito do código.
/// </summary>
public enum TipoTributo
{
    /// <summary>CST de ICMS — 3 caracteres (ex.: "000", "060").</summary>
    Icms,

    /// <summary>CST de IPI — 2 caracteres (ex.: "00", "49", "99").</summary>
    Ipi,

    /// <summary>CST de PIS/PASEP — 2 caracteres (ex.: "01", "49", "99").</summary>
    Pis,

    /// <summary>CST de COFINS — 2 caracteres (ex.: "01", "49", "99").</summary>
    Cofins,
}
