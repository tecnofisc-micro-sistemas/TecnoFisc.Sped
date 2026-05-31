using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Forma de escrituração contábil — campo <c>IND_ESC</c> da ECD. Indica a modalidade do livro
/// contábil escriturado (Registro I010). No Bloco C (Registro C040) somente os valores
/// <c>G</c>, <c>R</c> e <c>B</c> são aplicáveis.
/// </summary>
public enum FormaEscrituracaoContabil
{
    /// <summary>G — Livro Diário (Completo sem escrituração auxiliar).</summary>
    [SpedValor("G")]
    LivroDiarioGeral = 0,

    /// <summary>R — Livro Diário com Escrituração Resumida (com escrituração auxiliar).</summary>
    [SpedValor("R")]
    LivroDiarioEscrituracaoResumida = 1,

    /// <summary>A — Livro Diário Auxiliar ao Diário com Escrituração Resumida.</summary>
    [SpedValor("A")]
    LivroDiarioAuxiliar = 2,

    /// <summary>B — Livro Balancetes Diários e Balanços.</summary>
    [SpedValor("B")]
    LivroBalancetesDiarios = 3,

    /// <summary>Z — Razão Auxiliar (Livro Contábil Auxiliar conforme leiaute definido nos registros I500 a I555).</summary>
    [SpedValor("Z")]
    RazaoAuxiliar = 4,
}
