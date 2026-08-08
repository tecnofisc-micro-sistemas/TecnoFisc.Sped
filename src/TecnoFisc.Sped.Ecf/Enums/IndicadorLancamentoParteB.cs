using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Natureza de um lançamento sem reflexo na Parte A.</summary>
public enum IndicadorLancamentoParteB
{
    /// <summary>CR - crédito.</summary>
    [SpedValor("CR")]
    Credito = 0,

    /// <summary>DB - débito.</summary>
    [SpedValor("DB")]
    Debito = 1,

    /// <summary>PF - prejuízo fiscal do exercício.</summary>
    [SpedValor("PF")]
    PrejuizoFiscal = 2,

    /// <summary>BC - base de cálculo negativa da CSLL.</summary>
    [SpedValor("BC")]
    BaseCalculoNegativa = 3,
}
