namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador da apuração das contribuições e créditos na escrituração das operações por NF-e e ECF
/// — campo IND_ESCRI do Registro C010. Conforme Guia Prático v1.35, p. 105.
/// </summary>
public enum IndicadorEscrituracaoNfe
{
    /// <summary>1 — Apuração com base nos registros de consolidação das operações por NF-e (C180 e C190) e por ECF (C490).</summary>
    ConsolidacaoPorNfe = 1,

    /// <summary>2 — Apuração com base no registro individualizado de NF-e (C100 e C170) e de ECF (C400).</summary>
    IndividualizadoPorNfe = 2,
}
