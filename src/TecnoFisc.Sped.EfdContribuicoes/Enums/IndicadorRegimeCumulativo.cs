namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Código indicador do critério de escrituração e apuração no regime cumulativo (lucro presumido) —
/// campo IND_REG_CUM do Registro 0110. Valores válidos pelo Guia Prático v1.35, p. 72: <c>1, 2, 9</c>.
/// Usado apenas quando <see cref="CodigoIncidenciaTributaria.RegimeCumulativo"/> (COD_INC_TRIB = 2).
/// </summary>
public enum IndicadorRegimeCumulativo
{
    /// <summary>1 — Regime de Caixa – Escrituração consolidada (Registro F500).</summary>
    RegimeDeCaixa = 1,

    /// <summary>2 — Regime de Competência - Escrituração consolidada (Registro F550).</summary>
    RegimeDeCompetenciaConsolidado = 2,

    /// <summary>9 — Regime de Competência - Escrituração detalhada com base nos Blocos A, C, D e F.</summary>
    RegimeDeCompetenciaDetalhado = 9,
}
