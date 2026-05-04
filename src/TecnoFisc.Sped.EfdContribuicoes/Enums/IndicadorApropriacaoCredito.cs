namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador de método de apropriação de créditos comuns no regime não-cumulativo —
/// campo IND_APRO_CRED do Registro 0110. Valores válidos pelo Guia Prático v1.35, p. 72: <c>1, 2</c>.
/// </summary>
public enum IndicadorApropriacaoCredito
{
    /// <summary>1 — Método de Apropriação Direta.</summary>
    ApropriacaoDireta = 1,

    /// <summary>2 — Método de Rateio Proporcional (Receita Bruta). Obriga preenchimento do Registro 0111.</summary>
    RateioProporcionalReceitaBruta = 2,
}
