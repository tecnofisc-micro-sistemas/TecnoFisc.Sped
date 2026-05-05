namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador de movimentação física do item/produto — campo IND_MOV do Registro C170.
/// Valores válidos pelo Guia Prático v1.35: <c>0, 1</c>.
/// </summary>
public enum IndicadorMovimentacaoFisica
{
    /// <summary>0 — Sim, há movimentação física do item.</summary>
    Sim = 0,

    /// <summary>1 — Não há movimentação física do item (ex.: NF complementar, remessa simbólica).</summary>
    Nao = 1,
}
