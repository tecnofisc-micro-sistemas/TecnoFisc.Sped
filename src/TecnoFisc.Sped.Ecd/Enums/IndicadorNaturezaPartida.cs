using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Indicador da natureza da partida contábil — campo <c>IND_DC</c> do Registro I250 da ECD.
/// D = Débito; C = Crédito.
/// </summary>
public enum IndicadorNaturezaPartida
{
    /// <summary>D — Débito.</summary>
    [SpedValor("D")]
    Debito = 0,

    /// <summary>C — Crédito.</summary>
    [SpedValor("C")]
    Credito = 1,
}
