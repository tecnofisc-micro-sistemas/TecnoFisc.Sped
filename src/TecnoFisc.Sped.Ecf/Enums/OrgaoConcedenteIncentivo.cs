using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Órgão concedente da isenção ou redução informada no X280.</summary>
public enum OrgaoConcedenteIncentivo
{
    [SpedValor("AM")]
    Sudam = 0,

    [SpedValor("NE")]
    Sudene = 1,

    [SpedValor("OU")]
    Outros = 2,
}
