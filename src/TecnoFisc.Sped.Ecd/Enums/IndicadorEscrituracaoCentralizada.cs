namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Indicador da modalidade de escrituração centralizada ou descentralizada — campo
/// IND_CENTRALIZADA do Registro 0000 da ECD.
/// </summary>
public enum IndicadorEscrituracaoCentralizada
{
    /// <summary>0 — Escrituração Centralizada.</summary>
    Centralizada = 0,

    /// <summary>1 — Escrituração Descentralizada.</summary>
    Descentralizada = 1,
}
