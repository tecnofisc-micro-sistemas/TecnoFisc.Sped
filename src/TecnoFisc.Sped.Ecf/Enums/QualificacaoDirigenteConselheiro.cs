using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Qualificação do dirigente ou conselheiro no registro Y612.</summary>
public enum QualificacaoDirigenteConselheiro
{
    /// <summary>10 - administrador sem vínculo empregatício.</summary>
    [SpedValor("10")]
    AdministradorSemVinculo = 10,

    /// <summary>11 - diretor sem vínculo empregatício.</summary>
    [SpedValor("11")]
    DiretorSemVinculo = 11,

    /// <summary>12 - presidente sem vínculo empregatício.</summary>
    [SpedValor("12")]
    PresidenteSemVinculo = 12,

    /// <summary>13 - administrador com vínculo empregatício.</summary>
    [SpedValor("13")]
    AdministradorComVinculo = 13,

    /// <summary>14 - conselheiro de administração ou fiscal.</summary>
    [SpedValor("14")]
    ConselheiroAdministracaoOuFiscal = 14,

    /// <summary>15 - diretor com vínculo empregatício.</summary>
    [SpedValor("15")]
    DiretorComVinculo = 15,

    /// <summary>16 - fundador.</summary>
    [SpedValor("16")]
    Fundador = 16,

    /// <summary>17 - presidente com vínculo empregatício.</summary>
    [SpedValor("17")]
    PresidenteComVinculo = 17,
}
