namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador da origem do processo judicial ou administrativo — campo IND_PROC do Registro A111.
/// Valores conforme Guia Prático v1.35, p. 99.
/// </summary>
public enum IndicadorOrigemProcesso
{
    /// <summary>1 — Justiça Federal.</summary>
    JusticaFederal = 1,

    /// <summary>3 — Secretaria da Receita Federal do Brasil.</summary>
    ReceitaFederal = 3,

    /// <summary>9 — Outros.</summary>
    Outros = 9,
}
