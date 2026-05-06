namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador da origem do crédito — campo IND_ORIG_CRED do Registro A170.
/// Valores conforme Guia Prático v1.35, p. 101.
/// </summary>
public enum IndicadorOrigemCredito
{
    /// <summary>0 — Operação no Mercado Interno.</summary>
    MercadoInterno = 0,

    /// <summary>1 — Operação de Importação.</summary>
    Importacao = 1,
}
