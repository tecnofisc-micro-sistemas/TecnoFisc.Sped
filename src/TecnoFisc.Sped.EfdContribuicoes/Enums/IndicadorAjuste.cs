namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador do tipo de ajuste de crédito ou contribuição — campo IND_AJ dos Registros M110,
/// M220, M510 e M620. Valores válidos: <c>0</c> (redução) e <c>1</c> (acréscimo).
/// </summary>
public enum IndicadorAjuste
{
    /// <summary>0 — Ajuste de redução.</summary>
    Reducao = 0,

    /// <summary>1 — Ajuste de acréscimo.</summary>
    Acrescimo = 1,
}
