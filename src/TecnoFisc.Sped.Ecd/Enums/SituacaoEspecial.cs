namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Indicador de situação especial — campo IND_SIT_ESP do Registro 0000 da ECD
/// (Tabela de Situação Especial publicada pelo Sped). Preenchido apenas quando há evento
/// societário no período.
/// </summary>
public enum SituacaoEspecial
{
    /// <summary>1 — Cisão.</summary>
    Cisao = 1,

    /// <summary>2 — Fusão.</summary>
    Fusao = 2,

    /// <summary>3 — Incorporação.</summary>
    Incorporacao = 3,

    /// <summary>4 — Extinção.</summary>
    Extincao = 4,
}
