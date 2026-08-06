using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Benefício fiscal de atividade incentivada informado no X280.</summary>
public enum BeneficioFiscalIncentivado
{
    [SpedValor("00")]
    NaoPreenchido = 0,

    [SpedValor("01")]
    Isencao = 1,

    [SpedValor("02")]
    Reducao100 = 2,

    [SpedValor("03")]
    Reducao75 = 3,

    [SpedValor("04")]
    Reducao70 = 4,

    [SpedValor("05")]
    Reducao50 = 5,

    [SpedValor("06")]
    Reducao33 = 6,

    [SpedValor("07")]
    Reducao25 = 7,

    [SpedValor("08")]
    Reducao12 = 8,

    [SpedValor("09")]
    Reinvestimento = 9,

    [SpedValor("10")]
    Perse = 10,

    [SpedValor("99")]
    SubvencaoInvestimento = 99,
}
