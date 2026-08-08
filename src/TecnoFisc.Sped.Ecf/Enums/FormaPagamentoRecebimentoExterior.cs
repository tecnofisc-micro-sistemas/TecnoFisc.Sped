using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Enums;

/// <summary>Forma de pagamento ou recebimento no exterior do registro Y520.</summary>
public enum FormaPagamentoRecebimentoExterior
{
    /// <summary>1 - operação de câmbio.</summary>
    [SpedValor("1")]
    OperacaoCambio = 1,

    /// <summary>2 - transferência internacional em reais.</summary>
    [SpedValor("2")]
    TransferenciaInternacionalReais = 2,

    /// <summary>3 - cartão de crédito.</summary>
    [SpedValor("3")]
    CartaoCredito = 3,

    /// <summary>4 - depósito em conta do exterior.</summary>
    [SpedValor("4")]
    DepositoContaExterior = 4,

    /// <summary>5 - utilização de recursos mantidos no exterior.</summary>
    [SpedValor("5")]
    RecursosMantidosExterior = 5,

    /// <summary>6 - em moeda nacional ou estrangeira.</summary>
    [SpedValor("6")]
    MoedaNacionalOuEstrangeira = 6,
}
