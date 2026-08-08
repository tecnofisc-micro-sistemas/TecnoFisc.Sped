using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Relacionamento de um lançamento da Parte A.</summary>
public enum IndicadorRelacionamentoParteA
{
    /// <summary>1 - com conta da Parte B.</summary>
    [SpedValor("1")]
    ContaParteB = 0,

    /// <summary>2 - com conta contábil.</summary>
    [SpedValor("2")]
    ContaContabil = 1,

    /// <summary>3 - com conta da Parte B e conta contábil.</summary>
    [SpedValor("3")]
    ContaParteBContaContabil = 2,

    /// <summary>4 - sem relacionamento.</summary>
    [SpedValor("4")]
    SemRelacionamento = 3,
}
