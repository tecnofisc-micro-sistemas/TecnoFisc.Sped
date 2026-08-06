using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Método de avaliação do estoque final informado no registro L200.</summary>
public enum MetodoAvaliacaoEstoque
{
    /// <summary>1 - custo médio ponderado.</summary>
    [SpedValor("1")]
    CustoMedioPonderado = 1,

    /// <summary>2 - primeiro que entra, primeiro que sai.</summary>
    [SpedValor("2")]
    Peps = 2,

    /// <summary>3 - arbitramento.</summary>
    [SpedValor("3")]
    Arbitramento = 3,

    /// <summary>4 - custo específico.</summary>
    [SpedValor("4")]
    CustoEspecifico = 4,

    /// <summary>5 - valor realizável líquido.</summary>
    [SpedValor("5")]
    ValorRealizavelLiquido = 5,

    /// <summary>6 - inventário periódico.</summary>
    [SpedValor("6")]
    InventarioPeriodico = 6,

    /// <summary>7 - outros métodos.</summary>
    [SpedValor("7")]
    Outros = 7,

    /// <summary>8 - não há estoque final sujeito a avaliação.</summary>
    [SpedValor("8")]
    NaoHa = 8,
}
