namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Indicador de movimento de ICMS Substituição Tributária — campo IND_MOV_ST do Registro E210.
/// Valores conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 214.
/// </summary>
public enum IndicadorMovimentoSt
{
    /// <summary>0 — Sem operações com Substituição Tributária no período.</summary>
    SemOperacoes = 0,

    /// <summary>1 — Com operações de Substituição Tributária no período.</summary>
    ComOperacoes = 1,
}
