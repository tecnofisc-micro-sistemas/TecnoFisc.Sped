using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Indicador do tipo do lançamento contábil — campo <c>IND_LCTO</c> do Registro I200 da ECD.
/// Distingue lançamentos normais, de encerramento de contas de resultado e extemporâneos.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 143.
/// </summary>
public enum IndicadorTipoLancamento
{
    /// <summary>N — Lançamento normal (todos os lançamentos, exceto os de encerramento de contas de resultado).</summary>
    [SpedValor("N")]
    Normal = 0,

    /// <summary>E — Lançamento de encerramento de contas de resultado.</summary>
    [SpedValor("E")]
    Encerramento = 1,

    /// <summary>X — Lançamento extemporâneo (itens 35 e 36 da ITG 2000 (R1) — CFC, dez/2014).</summary>
    [SpedValor("X")]
    Extemporaneo = 2,
}
