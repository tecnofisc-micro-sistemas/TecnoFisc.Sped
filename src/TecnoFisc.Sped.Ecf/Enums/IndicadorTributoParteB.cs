using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Tributo associado a uma adição ou exclusão da Parte B do e-Lalur.</summary>
public enum IndicadorTributoParteB
{
    /// <summary>I - Imposto de Renda da Pessoa Jurídica.</summary>
    [SpedValor("I")]
    Irpj = 0,

    /// <summary>C - Contribuição Social sobre o Lucro Líquido.</summary>
    [SpedValor("C")]
    Csll = 1,

    /// <summary>A - ambos os tributos.</summary>
    [SpedValor("A")]
    Ambos = 2,
}
