namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador de movimento de FCP e ICMS Diferencial de Alíquota — campo IND_MOV_FCP_DIFAL do Registro E310.
/// Valores conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 223-227.
/// </summary>
public enum IndicadorMovimentoFcpDifal
{
    /// <summary>0 — Sem operações de FCP/Difal no período.</summary>
    SemOperacoes = 0,

    /// <summary>1 — Com operações de FCP/Difal no período.</summary>
    ComOperacoes = 1,
}
