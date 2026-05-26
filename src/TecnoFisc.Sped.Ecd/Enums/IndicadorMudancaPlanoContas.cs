namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Indicador de mudança de plano de contas — campo IND_MUDANC_PC do Registro 0000 da ECD.
/// </summary>
public enum IndicadorMudancaPlanoContas
{
    /// <summary>0 — Não houve mudança no plano de contas.</summary>
    SemMudanca = 0,

    /// <summary>1 — Houve mudança no plano de contas.</summary>
    ComMudanca = 1,
}
