namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador do tipo do veículo transportador — campo IND_VEIC do Registro D140
/// (CT Aquaviário de Cargas, cód. 09).
/// Valores conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 179.
/// </summary>
public enum IndicadorTipoVeiculoAquaviario
{
    /// <summary>0 — Embarcação.</summary>
    Embarcacao = 0,

    /// <summary>1 — Empurrador/rebocador.</summary>
    EmpurradorRebocador = 1,
}
