namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador da natureza da ação administrativa — campo IND_NAT_ACAO do Registro 1020.
/// Decorrente de Processo Administrativo na Secretaria da Receita Federal do Brasil.
/// Valores conforme Guia Prático EFD Contribuições v1.35, p. 381.
/// </summary>
public enum IndicadorNaturezaAcaoAdministrativa
{
    /// <summary>01 — Processo Administrativo de Consulta.</summary>
    ProcessoAdministrativoDeConsulta = 1,

    /// <summary>02 — Despacho Decisório.</summary>
    DespachoDecisorio = 2,

    /// <summary>03 — Ato Declaratório Executivo.</summary>
    AtoDeclaratorioExecutivo = 3,

    /// <summary>04 — Ato Declaratório Interpretativo.</summary>
    AtoDeclaratorioInterpretativo = 4,

    /// <summary>05 — Decisão Administrativa de DRJ ou do CARF.</summary>
    DecisaoAdministrativaDrjOuCarf = 5,

    /// <summary>06 — Auto de Infração.</summary>
    AutoDeInfracao = 6,

    /// <summary>99 — Outros.</summary>
    Outros = 99,
}
