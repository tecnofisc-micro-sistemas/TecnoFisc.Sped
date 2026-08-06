using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Tipo de lançamento da Parte A do e-Lalur ou e-Lacs.</summary>
public enum TipoLancamentoParteA
{
    /// <summary>A - adição.</summary>
    [SpedValor("A")]
    Adicao = 0,

    /// <summary>E - exclusão.</summary>
    [SpedValor("E")]
    Exclusao = 1,

    /// <summary>P - compensação de prejuízo.</summary>
    [SpedValor("P")]
    CompensacaoPrejuizo = 2,

    /// <summary>L - lucro.</summary>
    [SpedValor("L")]
    Lucro = 3,
}
