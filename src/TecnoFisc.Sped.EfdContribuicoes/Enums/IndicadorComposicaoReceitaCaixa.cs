namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>
/// Indicador da composição da receita recebida no período pelo regime de caixa — campo IND_REC do Registro F525.
/// Valores conforme Guia Prático EFD Contribuições v1.35, p. 266.
/// </summary>
public enum IndicadorComposicaoReceitaCaixa
{
    /// <summary>01 — Clientes.</summary>
    Clientes = 1,

    /// <summary>02 — Administradora de cartão de débito/crédito.</summary>
    AdminCartao = 2,

    /// <summary>03 — Título de crédito (duplicata, nota promissória, cheque, etc.).</summary>
    TituloCredito = 3,

    /// <summary>04 — Documento fiscal.</summary>
    DocumentoFiscal = 4,

    /// <summary>05 — Item vendido (produtos e serviços).</summary>
    ItemVendido = 5,

    /// <summary>99 — Outros (detalhar no campo INFO_COMPL).</summary>
    Outros = 99,
}
