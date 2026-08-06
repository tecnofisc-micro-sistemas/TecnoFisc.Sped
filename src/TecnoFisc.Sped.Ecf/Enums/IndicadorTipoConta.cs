using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Indicador do tipo de conta recuperada da ECD.</summary>
public enum IndicadorTipoConta
{
    /// <summary>A - conta analítica.</summary>
    [SpedValor("A")]
    Analitica = 0,

    /// <summary>S - conta sintética.</summary>
    [SpedValor("S")]
    Sintetica = 1,
}
