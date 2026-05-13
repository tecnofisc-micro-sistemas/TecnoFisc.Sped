namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador do tipo da navegação — campo IND_NAV do Registro D140
/// (CT Aquaviário de Cargas, cód. 09).
/// Valores conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 179.
/// </summary>
public enum IndicadorTipoNavegacao
{
    /// <summary>0 — Interior.</summary>
    Interior = 0,

    /// <summary>1 — Cabotagem.</summary>
    Cabotagem = 1,
}
