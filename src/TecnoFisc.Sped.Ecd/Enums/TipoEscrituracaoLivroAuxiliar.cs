using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Tipo de escrituração do livro associado — campo <c>TIPO</c> do Registro I012 da ECD.
/// </summary>
public enum TipoEscrituracaoLivroAuxiliar
{
    /// <summary>0 — Digital (incluído no Sped).</summary>
    [SpedValor("0")]
    Digital = 0,

    /// <summary>1 — Outros.</summary>
    [SpedValor("1")]
    Outros = 1,
}
