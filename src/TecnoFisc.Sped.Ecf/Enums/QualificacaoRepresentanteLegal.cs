using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Qualificação do representante legal no registro Y600.</summary>
public enum QualificacaoRepresentanteLegal
{
    /// <summary>01 - procurador.</summary>
    [SpedValor("01")]
    Procurador = 1,

    /// <summary>02 - curador.</summary>
    [SpedValor("02")]
    Curador = 2,

    /// <summary>03 - mãe.</summary>
    [SpedValor("03")]
    Mae = 3,

    /// <summary>04 - pai.</summary>
    [SpedValor("04")]
    Pai = 4,

    /// <summary>05 - tutor.</summary>
    [SpedValor("05")]
    Tutor = 5,

    /// <summary>06 - outro.</summary>
    [SpedValor("06")]
    Outro = 6,
}
