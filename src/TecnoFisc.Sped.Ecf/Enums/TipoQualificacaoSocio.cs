using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Natureza da pessoa informada no registro Y600.</summary>
public enum TipoQualificacaoSocio
{
    /// <summary>PF - pessoa física.</summary>
    [SpedValor("PF")]
    PessoaFisica = 0,

    /// <summary>PJ - pessoa jurídica.</summary>
    [SpedValor("PJ")]
    PessoaJuridica = 1,

    /// <summary>FI - fundo de investimento.</summary>
    [SpedValor("FI")]
    FundoInvestimento = 2,
}
