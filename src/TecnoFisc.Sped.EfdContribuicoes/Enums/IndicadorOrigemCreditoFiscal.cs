namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador da origem do crédito fiscal — campo ORIG_CRED dos Registros 1100 e 1220.
/// Distingue crédito apurado pela própria pessoa jurídica de crédito transferido por
/// pessoa jurídica sucedida em evento de incorporação, cisão ou fusão.
/// Valores conforme Guia Prático EFD Contribuições v1.35.
/// </summary>
public enum IndicadorOrigemCreditoFiscal
{
    /// <summary>01 — Crédito decorrente de operações próprias.</summary>
    Proprias = 1,

    /// <summary>02 — Crédito transferido por pessoa jurídica sucedida.</summary>
    Sucedida = 2,
}
