namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador de opção de utilização do crédito disponível no período — campo IND_DESC_CRED do Registro M100.
/// Valores conforme Guia Prático EFD Contribuições v1.35, p. 297.
/// </summary>
public enum IndicadorDescontoCredito
{
    /// <summary>0 — Utilização do valor total do crédito disponível para desconto no Registro M200.</summary>
    UtilizacaoTotal = 0,

    /// <summary>1 — Utilização de valor parcial do crédito disponível para desconto no Registro M200.</summary>
    UtilizacaoParcial = 1,
}
