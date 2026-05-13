using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Core.Enums;

/// <summary>
/// Tipo de movimentação de bem ou componente do CIAP — campo <c>TIPO_MOV</c> do Registro G125.
/// CIAP é domínio ICMS; o enum vive no Core para reuso transversal.
/// </summary>
public enum TipoMovimentacaoBemCiAp
{
    /// <summary>SI — Saldo inicial de bens imobilizados.</summary>
    [SpedValor("SI")]
    SaldoInicial = 1,

    /// <summary>IM — Imobilização de bem individual.</summary>
    [SpedValor("IM")]
    ImobilizacaoBemIndividual = 2,

    /// <summary>IA — Imobilização em andamento, componente.</summary>
    [SpedValor("IA")]
    ImobilizacaoEmAndamentoComponente = 3,

    /// <summary>CI — Conclusão de imobilização em andamento, bem resultante.</summary>
    [SpedValor("CI")]
    ConclusaoImobilizacaoEmAndamento = 4,

    /// <summary>MC — Imobilização oriunda do ativo circulante.</summary>
    [SpedValor("MC")]
    ImobilizacaoAtivoCirculante = 5,

    /// <summary>BA — Baixa do bem por fim do período de apropriação.</summary>
    [SpedValor("BA")]
    BaixaFimApropriacao = 6,

    /// <summary>AT — Alienação ou transferência.</summary>
    [SpedValor("AT")]
    AlienacaoOuTransferencia = 7,

    /// <summary>PE — Perecimento, extravio ou deterioração.</summary>
    [SpedValor("PE")]
    PerecimentoExtravioDeterioracao = 8,

    /// <summary>OT — Outras saídas do imobilizado.</summary>
    [SpedValor("OT")]
    OutrasSaidasImobilizado = 9,
}
