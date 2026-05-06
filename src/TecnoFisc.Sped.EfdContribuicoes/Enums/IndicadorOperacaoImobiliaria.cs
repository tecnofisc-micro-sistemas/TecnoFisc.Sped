namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador do tipo da operação da atividade imobiliária — campo IND_OPER do Registro F200.
/// Valores conforme Guia Prático EFD Contribuições v1.35, p. 250.
/// </summary>
public enum IndicadorOperacaoImobiliaria
{
    /// <summary>01 — Venda a Vista de Unidade Concluída.</summary>
    VendaVistaUnidadeConcluida = 1,

    /// <summary>02 — Venda a Prazo de Unidade Concluída.</summary>
    VendaPrazoUnidadeConcluida = 2,

    /// <summary>03 — Venda a Vista de Unidade em Construção.</summary>
    VendaVistaUnidadeConstrucao = 3,

    /// <summary>04 — Venda a Prazo de Unidade em Construção.</summary>
    VendaPrazoUnidadeConstrucao = 4,

    /// <summary>05 — Outras.</summary>
    Outras = 5,
}
