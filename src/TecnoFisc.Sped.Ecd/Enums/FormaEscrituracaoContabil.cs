using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Forma de escrituração contábil — campo <c>IND_ESC</c> da ECD. Indica a modalidade do livro
/// contábil escriturado. Valores válidos no Bloco C: <c>G</c>, <c>R</c> e <c>B</c>.
/// </summary>
public enum FormaEscrituracaoContabil
{
    /// <summary>G — Livro Diário Geral.</summary>
    [SpedValor("G")]
    LivroDiarioGeral = 0,

    /// <summary>R — Livro Diário com Escrituração Resumida.</summary>
    [SpedValor("R")]
    LivroDiarioEscrituracaoResumida = 1,

    /// <summary>B — Livro de Balancetes Diários.</summary>
    [SpedValor("B")]
    LivroBalancetesDiarios = 2,
}
