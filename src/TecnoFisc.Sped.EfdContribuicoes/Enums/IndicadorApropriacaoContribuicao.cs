namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador de apropriação do ajuste — campo IND_APROP do Registro 1050.
/// Identifica a qual contribuição social se refere o ajuste de base de cálculo.
/// Valores conforme Guia Prático EFD Contribuições v1.35, p. 382.
/// </summary>
public enum IndicadorApropriacaoContribuicao
{
    /// <summary>01 — Referente ao PIS/Pasep e à Cofins.</summary>
    PisECofins = 1,

    /// <summary>02 — Referente unicamente ao PIS/Pasep.</summary>
    SomentePis = 2,

    /// <summary>03 — Referente unicamente à Cofins.</summary>
    SomenteCofins = 3,
}
