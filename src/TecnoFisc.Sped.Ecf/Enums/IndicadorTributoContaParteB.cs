using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Tributo ao qual pertence uma conta da Parte B do e-Lalur ou e-Lacs.</summary>
public enum IndicadorTributoContaParteB
{
    /// <summary>I - Imposto de Renda da Pessoa Jurídica.</summary>
    [SpedValor("I")]
    Irpj = 0,

    /// <summary>C - Contribuição Social sobre o Lucro Líquido.</summary>
    [SpedValor("C")]
    Csll = 1,
}
