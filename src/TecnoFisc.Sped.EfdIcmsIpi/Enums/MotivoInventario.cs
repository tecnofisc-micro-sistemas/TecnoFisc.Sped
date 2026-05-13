using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Enums;

/// <summary>
/// Motivo do inventário — campo MOT_INV do Registro H005.
/// Valores conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 246.
/// </summary>
public enum MotivoInventario
{
    /// <summary>01 — No final do período.</summary>
    [SpedValor("01")]
    FinalPeriodo = 1,

    /// <summary>02 — Na mudança de forma de tributação da mercadoria.</summary>
    [SpedValor("02")]
    MudancaFormaTributacao = 2,

    /// <summary>03 — Na solicitação de baixa cadastral, paralisação temporária e outras situações.</summary>
    [SpedValor("03")]
    BaixaCadastralParalisacao = 3,

    /// <summary>04 — Na alteração de regime de pagamento.</summary>
    [SpedValor("04")]
    AlteracaoRegimePagamento = 4,

    /// <summary>05 — Por determinação dos fiscos.</summary>
    [SpedValor("05")]
    DeterminacaoFisco = 5,

    /// <summary>06 — Para controle das mercadorias sujeitas ao regime de substituição tributária.</summary>
    [SpedValor("06")]
    ControleSubstituicaoTributaria = 6,
}
