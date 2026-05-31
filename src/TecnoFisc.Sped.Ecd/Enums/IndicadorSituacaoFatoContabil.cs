using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Indicador da situação do saldo do fato contábil — campo <c>IND_DC_FAT</c> do Registro J215 da ECD.
/// D = Devedor; C = Credor; P = Subtotal ou total positivo; N = Subtotal ou total negativo.
/// </summary>
public enum IndicadorSituacaoFatoContabil
{
    /// <summary>D — Devedor.</summary>
    [SpedValor("D")]
    Devedor = 0,

    /// <summary>C — Credor.</summary>
    [SpedValor("C")]
    Credor = 1,

    /// <summary>P — Subtotal ou total positivo.</summary>
    [SpedValor("P")]
    TotalPositivo = 2,

    /// <summary>N — Subtotal ou total negativo.</summary>
    [SpedValor("N")]
    TotalNegativo = 3,
}
