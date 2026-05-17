namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador do tipo de tarifa aplicada — campo IND_TFA do Registro D150
/// (CT Aéreo de Cargas, cód. 10).
/// Valores conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 172.
/// </summary>
public enum IndicadorTarifaAerea
{
    /// <summary>0 — Exp. (Expedito).</summary>
    Expedito = 0,

    /// <summary>1 — Enc. (Encomenda).</summary>
    Encomenda = 1,

    /// <summary>2 — C.I. (Carga Inferior).</summary>
    CargaInferior = 2,

    /// <summary>9 — Outra.</summary>
    Outra = 9,
}
