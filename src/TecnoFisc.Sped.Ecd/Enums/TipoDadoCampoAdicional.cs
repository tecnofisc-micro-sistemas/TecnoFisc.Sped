using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Indicação do tipo de dado do campo adicional — campo <c>TIPO</c> do Registro I020 da ECD.
/// </summary>
public enum TipoDadoCampoAdicional
{
    /// <summary>N — Numérico; campos adicionais com valores em espécie (moeda), com duas decimais.</summary>
    [SpedValor("N")]
    Numerico = 0,

    /// <summary>C — Caractere; campos adicionais com outras informações (códigos, CNPJ, CPF etc.).</summary>
    [SpedValor("C")]
    Caractere = 1,
}
