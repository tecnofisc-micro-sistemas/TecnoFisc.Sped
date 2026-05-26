using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Enums;

/// <summary>
/// Código do motivo preponderante da substituição da ECD — campo <c>COD_MOT_SUBS</c> do Registro J801.
/// O código a ser adotado deve ser aquele cujo motivo é o preponderante na substituição da ECD.
/// Conforme Manual de Orientação do Leiaute 9 da ECD, p. 193–194.
/// </summary>
public enum CodigoMotivoSubstituicaoEcd
{
    /// <summary>001 — Mudanças de saldos das contas que não podem ser realizadas por meio de lançamentos extemporâneos.</summary>
    [SpedValor("001")]
    MudancaSaldosConta = 0,

    /// <summary>002 — Alteração de assinatura.</summary>
    [SpedValor("002")]
    AlteracaoAssinatura = 1,

    /// <summary>003 — Alteração de demonstrações contábeis.</summary>
    [SpedValor("003")]
    AlteracaoDemonstracoes = 2,

    /// <summary>004 — Alteração da forma de escrituração contábil.</summary>
    [SpedValor("004")]
    AlteracaoFormaEscrituracao = 3,

    /// <summary>005 — Alteração do número do livro.</summary>
    [SpedValor("005")]
    AlteracaoNumeroLivro = 4,

    /// <summary>099 — Outros.</summary>
    [SpedValor("099")]
    Outros = 5,
}
