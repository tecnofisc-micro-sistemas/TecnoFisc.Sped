using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Grupo <c>prod</c> — produto ou serviço de um item da NF-e/NFC-e (leiaute 4.00).
/// Cobre os campos essenciais do item; grupos específicos (combustível, medicamento, veículo,
/// importação detalhada, etc.) estão fora do escopo da v1.
/// </summary>
public sealed record Produto
{
    /// <summary><c>cProd</c> — código do produto no emitente.</summary>
    public required string CProd { get; init; }

    /// <summary><c>cEAN</c> — GTIN comercial (ou a sentinela "SEM GTIN").</summary>
    public Gtin CEAN { get; init; }

    /// <summary><c>xProd</c> — descrição do produto.</summary>
    public required string XProd { get; init; }

    /// <summary><c>NCM</c> — Nomenclatura Comum do Mercosul.</summary>
    public required Ncm NCM { get; init; }

    /// <summary><c>CEST</c> — Código Especificador da Substituição Tributária (opcional).</summary>
    public Cest? CEST { get; init; }

    /// <summary><c>EXTIPI</c> — código de exceção da TIPI (opcional).</summary>
    public string? EXTIPI { get; init; }

    /// <summary><c>CFOP</c> — Código Fiscal de Operações e Prestações.</summary>
    public required Cfop CFOP { get; init; }

    /// <summary><c>uCom</c> — unidade comercial.</summary>
    public required string UCom { get; init; }

    /// <summary><c>qCom</c> — quantidade comercial.</summary>
    public decimal QCom { get; init; }

    /// <summary><c>vUnCom</c> — valor unitário de comercialização.</summary>
    public decimal VUnCom { get; init; }

    /// <summary><c>vProd</c> — valor total bruto dos produtos.</summary>
    public decimal VProd { get; init; }

    /// <summary><c>cEANTrib</c> — GTIN da unidade tributável (ou a sentinela "SEM GTIN").</summary>
    public Gtin CEANTrib { get; init; }

    /// <summary><c>uTrib</c> — unidade tributável.</summary>
    public required string UTrib { get; init; }

    /// <summary><c>qTrib</c> — quantidade tributável.</summary>
    public decimal QTrib { get; init; }

    /// <summary><c>vUnTrib</c> — valor unitário de tributação.</summary>
    public decimal VUnTrib { get; init; }

    /// <summary><c>indTot</c> — indica se o <c>vProd</c> compõe o total da nota (0 não, 1 sim).</summary>
    public bool IndTot { get; init; }
}
