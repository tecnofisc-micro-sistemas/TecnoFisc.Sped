using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Tipo de benefício fiscal informado no registro X485.</summary>
public enum TipoBeneficioFiscal
{
    [SpedValor("1")]
    Repes = 1,

    [SpedValor("2")]
    Recap = 2,

    [SpedValor("3")]
    Padis = 3,

    [SpedValor("4")]
    Reidi = 4,

    [SpedValor("5")]
    Recine = 5,

    [SpedValor("6")]
    Retid = 6,

    [SpedValor("7")]
    OleoBunker = 7,

    [SpedValor("8")]
    Reporto = 8,

    [SpedValor("9")]
    RetIi = 9,

    [SpedValor("10")]
    RetPmcmvPcva = 10,

    [SpedValor("11")]
    RetEei = 11,

    [SpedValor("12")]
    EntidadeBeneficente = 12,

    [SpedValor("13")]
    RepetroIndustrializacao = 13,

    [SpedValor("14")]
    RepetroNacional = 14,

    [SpedValor("15")]
    RepetroPermanente = 15,

    [SpedValor("16")]
    RepetroTemporario = 16,
}
