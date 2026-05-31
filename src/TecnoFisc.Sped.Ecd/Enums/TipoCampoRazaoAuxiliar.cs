using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Tipo do campo definido no Livro Razão Auxiliar com Leiaute Parametrizável —
/// campo <c>TIPO_CAMPO</c> do Registro I510 da ECD.
/// </summary>
public enum TipoCampoRazaoAuxiliar
{
    /// <summary>N — Numérico.</summary>
    [SpedValor("N")]
    Numerico = 0,

    /// <summary>C — Caractere.</summary>
    [SpedValor("C")]
    Caractere = 1,
}
