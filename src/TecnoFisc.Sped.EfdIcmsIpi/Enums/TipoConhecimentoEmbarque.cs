using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Tipo de conhecimento de embarque — campo TP_CHC do Registro 1100.
/// Valores conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 270.
/// </summary>
public enum TipoConhecimentoEmbarque
{
    /// <summary>01 - AWB.</summary>
    [SpedValor("01")]
    Awb = 1,

    /// <summary>02 - MAWB.</summary>
    [SpedValor("02")]
    Mawb = 2,

    /// <summary>03 - HAWB.</summary>
    [SpedValor("03")]
    Hawb = 3,

    /// <summary>04 - COMAT.</summary>
    [SpedValor("04")]
    Comat = 4,

    /// <summary>06 - R. EXPRESSAS.</summary>
    [SpedValor("06")]
    RExpressas = 6,

    /// <summary>07 - ETIQ. REXPRESSAS.</summary>
    [SpedValor("07")]
    EtiqRexpressas = 7,

    /// <summary>08 - HR. EXPRESSAS.</summary>
    [SpedValor("08")]
    HrExpressas = 8,

    /// <summary>09 - AV7.</summary>
    [SpedValor("09")]
    Av7 = 9,

    /// <summary>10 - BL.</summary>
    [SpedValor("10")]
    Bl = 10,

    /// <summary>11 - MBL.</summary>
    [SpedValor("11")]
    Mbl = 11,

    /// <summary>12 - HBL.</summary>
    [SpedValor("12")]
    Hbl = 12,

    /// <summary>13 - CRT.</summary>
    [SpedValor("13")]
    Crt = 13,

    /// <summary>14 - DSIC.</summary>
    [SpedValor("14")]
    Dsic = 14,

    /// <summary>16 - COMAT BL.</summary>
    [SpedValor("16")]
    ComatBl = 16,

    /// <summary>17 - RWB.</summary>
    [SpedValor("17")]
    Rwb = 17,

    /// <summary>18 - HRWB.</summary>
    [SpedValor("18")]
    Hrwb = 18,

    /// <summary>19 - TIF/DTA.</summary>
    [SpedValor("19")]
    TifDta = 19,

    /// <summary>20 - CP2.</summary>
    [SpedValor("20")]
    Cp2 = 20,

    /// <summary>91 - NAO IATA.</summary>
    [SpedValor("91")]
    NaoIata = 91,

    /// <summary>92 - MNAO IATA.</summary>
    [SpedValor("92")]
    MNaoIata = 92,

    /// <summary>93 - HNAO IATA.</summary>
    [SpedValor("93")]
    HNaoIata = 93,

    /// <summary>99 - Outros.</summary>
    [SpedValor("99")]
    Outros = 99,
}
