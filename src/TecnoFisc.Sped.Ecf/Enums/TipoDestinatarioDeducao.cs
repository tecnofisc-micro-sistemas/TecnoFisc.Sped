using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Natureza do destinatário da dedução informada no registro Y730.</summary>
public enum TipoDestinatarioDeducao
{
    /// <summary>PF - pessoa física.</summary>
    [SpedValor("PF")]
    PessoaFisica = 0,

    /// <summary>PJ - pessoa jurídica.</summary>
    [SpedValor("PJ")]
    PessoaJuridica = 1,
}
