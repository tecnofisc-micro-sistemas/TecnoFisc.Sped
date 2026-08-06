using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Indicador da situação de um saldo contábil.</summary>
public enum IndicadorDebitoCredito
{
    /// <summary>D - saldo devedor.</summary>
    [SpedValor("D")]
    Devedor = 0,

    /// <summary>C - saldo credor.</summary>
    [SpedValor("C")]
    Credor = 1,
}
