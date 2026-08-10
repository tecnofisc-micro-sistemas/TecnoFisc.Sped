using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Txt.Engine.Enums;

/// <summary>
/// Indicador da situação de um saldo contábil. Compartilhado por ECD e ECF: mesma semântica,
/// mesmos tokens SPED. Não é regido pelo Ato COTEPE — é convenção contábil transversal.
/// </summary>
public enum IndicadorDebitoCredito
{
    /// <summary>D - saldo devedor.</summary>
    [SpedValor("D")]
    Devedor = 0,

    /// <summary>C - saldo credor.</summary>
    [SpedValor("C")]
    Credor = 1,
}
