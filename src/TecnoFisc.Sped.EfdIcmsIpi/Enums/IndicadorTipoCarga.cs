namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador do tipo de transporte (modal de carga) — campo IND_CARGA no Registro C115.
/// Valores conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 70.
/// </summary>
public enum IndicadorTipoCarga
{
    /// <summary>0 — Rodoviário.</summary>
    Rodoviario = 0,

    /// <summary>1 — Ferroviário.</summary>
    Ferroviario = 1,

    /// <summary>2 — Rodo-Ferroviário.</summary>
    RodoFerroviario = 2,

    /// <summary>3 — Aquaviário.</summary>
    Aquaviario = 3,

    /// <summary>4 — Dutoviário.</summary>
    Dutoviario = 4,

    /// <summary>5 — Aéreo.</summary>
    Aereo = 5,

    /// <summary>9 — Outros.</summary>
    Outros = 9,
}
