namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador do tipo de sociedade cooperativa — campo IND_TIP_COOP do Registro M211.
/// Valores válidos conforme Guia Prático EFD Contribuições v1.35, p. 318.
/// </summary>
public enum IndicadorTipoCooperativa
{
    /// <summary>01 — Cooperativa de Produção Agropecuária.</summary>
    ProducaoAgropecuaria = 1,

    /// <summary>02 — Cooperativa de Consumo.</summary>
    Consumo = 2,

    /// <summary>03 — Cooperativa de Crédito.</summary>
    Credito = 3,

    /// <summary>04 — Cooperativa de Eletrificação Rural.</summary>
    EletrificacaoRural = 4,

    /// <summary>05 — Cooperativa de Transporte Rodoviário de Cargas.</summary>
    TransporteRodoviarioCargas = 5,

    /// <summary>06 — Cooperativa de Médicos.</summary>
    Medicos = 6,

    /// <summary>99 — Outras.</summary>
    Outras = 99,
}
