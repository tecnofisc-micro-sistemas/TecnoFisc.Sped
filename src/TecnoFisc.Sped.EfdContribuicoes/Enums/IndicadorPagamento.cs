namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador do tipo de pagamento — campo IND_PGTO do Registro A100.
/// Valores conforme Guia Prático v1.35, p. 96.
/// </summary>
public enum IndicadorPagamento
{
    /// <summary>0 — À vista.</summary>
    AVista = 0,

    /// <summary>1 — A prazo.</summary>
    APrazo = 1,

    /// <summary>9 — Sem pagamento.</summary>
    SemPagamento = 9,
}
