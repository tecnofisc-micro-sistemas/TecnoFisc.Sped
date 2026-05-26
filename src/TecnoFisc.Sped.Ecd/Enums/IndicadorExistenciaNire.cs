namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Indicador de existência de NIRE — campo IND_NIRE do Registro 0000 da ECD.
/// </summary>
public enum IndicadorExistenciaNire
{
    /// <summary>0 — Empresa não possui registro na Junta Comercial (não possui NIRE).</summary>
    NaoPossui = 0,

    /// <summary>1 — Empresa possui registro na Junta Comercial (possui NIRE).</summary>
    Possui = 1,
}
