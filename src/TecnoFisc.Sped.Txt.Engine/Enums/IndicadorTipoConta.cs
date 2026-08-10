using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Txt.Engine.Enums;

/// <summary>
/// Indicador do tipo de conta — campo <c>IND_CTA</c> do Registro C050 da ECD.
/// A = Analítica (conta individual); S = Sintética (grupo de contas).
/// Compartilhado por ECD e ECF: a ECF recupera contas originadas na ECD.
/// </summary>
public enum IndicadorTipoConta
{
    /// <summary>A — Analítica (conta).</summary>
    [SpedValor("A")]
    Analitica = 0,

    /// <summary>S — Sintética (grupo de contas).</summary>
    [SpedValor("S")]
    Sintetica = 1,
}
