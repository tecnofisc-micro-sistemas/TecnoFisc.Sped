namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Indicador de situação no início do período — campo IND_SIT_INI_PER do Registro 0000 da
/// ECD (Tabela de Situação no Início do Período publicada pelo Sped).
/// </summary>
public enum SituacaoInicioPeriodo
{
    /// <summary>0 — Normal (início no primeiro dia do ano ou do mês).</summary>
    Normal = 0,

    /// <summary>1 — Abertura.</summary>
    Abertura = 1,

    /// <summary>2 — Resultante de cisão/fusão ou remanescente de cisão, ou realizou incorporação.</summary>
    ResultanteCisaoFusaoIncorporacao = 2,

    /// <summary>3 — Início de obrigatoriedade da entrega da ECD no curso do ano-calendário.</summary>
    InicioObrigatoriedade = 3,
}
