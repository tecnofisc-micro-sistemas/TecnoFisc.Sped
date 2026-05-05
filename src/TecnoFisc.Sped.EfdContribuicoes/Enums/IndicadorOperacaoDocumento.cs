namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador do tipo de operação do documento fiscal — campo IND_OPER do Registro C100.
/// Valores conforme Guia Prático v1.35, p. 108.
/// </summary>
public enum IndicadorOperacaoDocumento
{
    /// <summary>0 — Entrada.</summary>
    Entrada = 0,

    /// <summary>1 — Saída.</summary>
    Saida = 1,
}
