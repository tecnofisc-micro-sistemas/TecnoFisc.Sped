using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Core.Enums;

/// <summary>
/// Indicador textual sim/nao usado por campos fiscais com valores "S" e "N".
/// </summary>
public enum IndicadorSimNao
{
    /// <summary>N - Nao.</summary>
    [SpedValor("N")]
    Nao = 0,

    /// <summary>S - Sim.</summary>
    [SpedValor("S")]
    Sim = 1,
}
