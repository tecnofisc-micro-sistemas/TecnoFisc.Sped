namespace TecnoFisc.Sped.Txt.Engine.Enums;

/// <summary>
/// Indicador do período de apuração do IPI — campo <c>IND_APUR</c> em registros que
/// escrituram itens de documento fiscal (ex.: C170 no EFD ICMS-IPI e EFD Contribuições).
/// IPI é tributo do domain ICMS-IPI; o enum vive no Core para reuso transversal.
/// </summary>
public enum IndicadorApuracaoIpi
{
    /// <summary>0 — Apuração mensal.</summary>
    Mensal = 0,

    /// <summary>1 — Apuração decendial.</summary>
    Decendial = 1,
}
