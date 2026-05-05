namespace TecnoFisc.Sped.Core.Enums;

/// <summary>
/// Indicador de movimentação física do item/produto — campo <c>IND_MOV</c> em registros
/// que escrituram itens de documento fiscal (ex.: C170 no EFD ICMS-IPI e EFD Contribuições).
/// Origem: leiaute EFD ICMS-IPI (regente do Ato COTEPE/ICMS).
/// </summary>
public enum IndicadorMovimentacaoFisica
{
    /// <summary>0 — Sim, há movimentação física do item.</summary>
    Sim = 0,

    /// <summary>1 — Não há movimentação física do item (ex.: NF complementar, remessa simbólica).</summary>
    Nao = 1,
}
