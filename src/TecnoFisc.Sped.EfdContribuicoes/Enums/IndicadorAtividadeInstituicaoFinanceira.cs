namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador do tipo de atividade da pessoa jurídica — campo IND_ATIV do Registro I010.
/// Valores conforme Guia Prático EFD Contribuições v1.35, p. 285.
/// </summary>
public enum IndicadorAtividadeInstituicaoFinanceira
{
    /// <summary>01 — Exclusivamente operações de Instituições Financeiras e Assemelhadas.</summary>
    InstituicoesFinanceiras = 1,

    /// <summary>02 — Exclusivamente operações de Seguros Privados.</summary>
    SegurosPrivados = 2,

    /// <summary>03 — Exclusivamente operações de Previdência Complementar.</summary>
    PrevidenciaComplementar = 3,

    /// <summary>04 — Exclusivamente operações de Capitalização.</summary>
    Capitalizacao = 4,

    /// <summary>05 — Exclusivamente operações de Planos de Assistência à Saúde.</summary>
    PlanosAssistenciaSaude = 5,

    /// <summary>06 — Realizou operações referentes a mais de um dos indicadores acima.</summary>
    MultiplosTipos = 6,
}
