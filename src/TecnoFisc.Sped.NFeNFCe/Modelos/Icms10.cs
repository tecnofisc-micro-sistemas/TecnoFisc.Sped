using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>ICMS10</c> — tributada e com cobrança do ICMS por substituição tributária (CST 10).
/// </summary>
public sealed record Icms10 : Icms
{
    /// <summary><c>CST</c> — situação tributária do ICMS (sempre "10" nesta variante).</summary>
    public required Cst CST { get; init; }

    /// <summary><c>modBC</c> — modalidade de determinação da BC do ICMS (0–3).</summary>
    public required int ModBC { get; init; }

    /// <summary><c>vBC</c> — valor da base de cálculo do ICMS.</summary>
    public required decimal VBC { get; init; }

    /// <summary><c>pICMS</c> — alíquota do ICMS.</summary>
    public required decimal PICMS { get; init; }

    /// <summary><c>vICMS</c> — valor do ICMS.</summary>
    public required decimal VICMS { get; init; }

    /// <summary><c>vBCFCP</c> — valor da base de cálculo do FCP (opcional).</summary>
    public decimal? VBCFCP { get; init; }

    /// <summary><c>pFCP</c> — percentual de ICMS relativo ao Fundo de Combate à Pobreza (opcional).</summary>
    public decimal? PFCP { get; init; }

    /// <summary><c>vFCP</c> — valor do ICMS relativo ao Fundo de Combate à Pobreza (opcional).</summary>
    public decimal? VFCP { get; init; }

    /// <summary><c>modBCST</c> — modalidade de determinação da BC do ICMS ST (0–6).</summary>
    public required int ModBCST { get; init; }

    /// <summary><c>pMVAST</c> — percentual da margem de valor adicionado do ICMS ST (opcional).</summary>
    public decimal? PMVAST { get; init; }

    /// <summary><c>pRedBCST</c> — percentual de redução da BC do ICMS ST (opcional).</summary>
    public decimal? PRedBCST { get; init; }

    /// <summary><c>vBCST</c> — valor da base de cálculo do ICMS ST.</summary>
    public required decimal VBCST { get; init; }

    /// <summary><c>pICMSST</c> — alíquota do ICMS ST.</summary>
    public required decimal PICMSST { get; init; }

    /// <summary><c>vICMSST</c> — valor do ICMS ST.</summary>
    public required decimal VICMSST { get; init; }

    /// <summary><c>vBCFCPST</c> — valor da base de cálculo do FCP retido por substituição tributária (opcional).</summary>
    public decimal? VBCFCPST { get; init; }

    /// <summary><c>pFCPST</c> — percentual de FCP retido por substituição tributária (opcional).</summary>
    public decimal? PFCPST { get; init; }

    /// <summary><c>vFCPST</c> — valor do FCP retido por substituição tributária (opcional).</summary>
    public decimal? VFCPST { get; init; }

    /// <summary><c>vICMSSTDeson</c> — valor do ICMS-ST desonerado (opcional).</summary>
    public decimal? VICMSSTDeson { get; init; }

    /// <summary><c>motDesICMSST</c> — motivo da desoneração do ICMS-ST: 3, 9, 12 (opcional).</summary>
    public int? MotDesICMSST { get; init; }
}
