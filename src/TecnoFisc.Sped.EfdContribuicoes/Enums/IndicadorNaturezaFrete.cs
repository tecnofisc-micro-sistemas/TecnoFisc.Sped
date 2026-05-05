namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador da Natureza do Frete Contratado — campo IND_NAT_FRT dos Registros D101 e D105.
/// Classifica a operação para fins de apuração de crédito de PIS/Pasep e Cofins sobre
/// serviços de transporte. Guia Prático v1.35, p. 197.
/// </summary>
public enum IndicadorNaturezaFrete
{
    /// <summary>0 — Operações de vendas, com ônus suportado pelo estabelecimento vendedor.</summary>
    VendasOnusVendedor = 0,

    /// <summary>1 — Operações de vendas, com ônus suportado pelo adquirente.</summary>
    VendasOnusAdquirente = 1,

    /// <summary>2 — Operações de compras (bens para revenda, matérias-prima e outros produtos, geradores de crédito).</summary>
    ComprasGeradoresCredito = 2,

    /// <summary>3 — Operações de compras (bens para revenda, matérias-prima e outros produtos, não geradores de crédito).</summary>
    ComprasNaoGeradoresCredito = 3,

    /// <summary>4 — Transferência de produtos acabados entre estabelecimentos da pessoa jurídica.</summary>
    TransferenciaProdutosAcabados = 4,

    /// <summary>5 — Transferência de produtos em elaboração entre estabelecimentos da pessoa jurídica.</summary>
    TransferenciaProdutosEmElaboracao = 5,

    /// <summary>9 — Outras.</summary>
    Outras = 9,
}
