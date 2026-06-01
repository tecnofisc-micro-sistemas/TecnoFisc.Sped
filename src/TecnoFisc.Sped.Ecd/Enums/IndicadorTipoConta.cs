using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Indicador do tipo de conta — campo <c>IND_CTA</c> do Registro C050 da ECD.
/// A = Analítica (conta individual); S = Sintética (grupo de contas).
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
