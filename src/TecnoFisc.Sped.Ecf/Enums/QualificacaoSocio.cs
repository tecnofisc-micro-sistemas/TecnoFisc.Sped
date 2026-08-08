using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Qualificação do sócio, titular, dirigente ou conselheiro no registro Y600.</summary>
public enum QualificacaoSocio
{
    /// <summary>01 - acionista pessoa física domiciliado no Brasil.</summary>
    [SpedValor("01")]
    AcionistaPessoaFisicaBrasil = 1,

    /// <summary>02 - sócio pessoa física domiciliado no Brasil.</summary>
    [SpedValor("02")]
    SocioPessoaFisicaBrasil = 2,

    /// <summary>03 - acionista pessoa jurídica domiciliado no Brasil.</summary>
    [SpedValor("03")]
    AcionistaPessoaJuridicaBrasil = 3,

    /// <summary>04 - sócio pessoa jurídica domiciliado no Brasil.</summary>
    [SpedValor("04")]
    SocioPessoaJuridicaBrasil = 4,

    /// <summary>05 - acionista pessoa física domiciliado no exterior.</summary>
    [SpedValor("05")]
    AcionistaPessoaFisicaExterior = 5,

    /// <summary>06 - sócio pessoa física domiciliado no exterior.</summary>
    [SpedValor("06")]
    SocioPessoaFisicaExterior = 6,

    /// <summary>07 - acionista pessoa jurídica domiciliado no exterior.</summary>
    [SpedValor("07")]
    AcionistaPessoaJuridicaExterior = 7,

    /// <summary>08 - sócio pessoa jurídica domiciliado no exterior.</summary>
    [SpedValor("08")]
    SocioPessoaJuridicaExterior = 8,

    /// <summary>09 - titular.</summary>
    [SpedValor("09")]
    Titular = 9,

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

    /// <summary>18 - usufrutuário de quotas ou ações.</summary>
    [SpedValor("18")]
    UsufrutuarioQuotasOuAcoes = 18,
}
