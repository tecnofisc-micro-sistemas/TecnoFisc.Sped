namespace TecnoFisc.Sped.Core.Enums;

/// <summary>
/// Tipo do item — campo TIPO_ITEM do Registro 0200. Campo transversal ao EFD ICMS-IPI e ao
/// EFD Contribuições; valores definidos pelo Ato COTEPE/ICMS nº 44/2018.
/// </summary>
public enum TipoItem
{
    /// <summary>00 — Mercadoria para Revenda.</summary>
    MercadoriaParaRevenda = 0,

    /// <summary>01 — Matéria-Prima.</summary>
    MateriaPrima = 1,

    /// <summary>02 — Embalagem.</summary>
    Embalagem = 2,

    /// <summary>03 — Produto em Processo.</summary>
    ProdutoEmProcesso = 3,

    /// <summary>04 — Produto Acabado.</summary>
    ProdutoAcabado = 4,

    /// <summary>05 — Subproduto.</summary>
    Subproduto = 5,

    /// <summary>06 — Produto Intermediário.</summary>
    ProdutoIntermediario = 6,

    /// <summary>07 — Material de Uso e Consumo.</summary>
    MaterialDeUsoEConsumo = 7,

    /// <summary>08 — Ativo Imobilizado.</summary>
    AtivoImobilizado = 8,

    /// <summary>09 — Serviços.</summary>
    Servicos = 9,

    /// <summary>10 — Outros insumos.</summary>
    OutrosInsumos = 10,

    /// <summary>99 — Outras.</summary>
    Outras = 99,
}
