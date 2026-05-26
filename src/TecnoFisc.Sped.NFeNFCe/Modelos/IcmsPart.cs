using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>ICMSPart</c> — partilha do ICMS entre a UF de origem e a UF de destino
/// (operação interestadual para consumidor final, CST 10, 20 ou 90).
/// </summary>
public sealed record IcmsPart : Icms
{
    /// <summary><c>CST</c> — situação tributária do ICMS: 10 (tributada com ST), 20 (redução de BC) ou 90 (outros).</summary>
    public required Cst CST { get; init; }

    /// <summary><c>modBC</c> — modalidade de determinação da BC do ICMS: 0–3.</summary>
    public required int ModBC { get; init; }

    /// <summary><c>vBC</c> — valor da base de cálculo do ICMS.</summary>
    public required decimal VBC { get; init; }

    /// <summary><c>pRedBC</c> — percentual de redução da BC (opcional).</summary>
    public decimal? PRedBC { get; init; }

    /// <summary><c>pICMS</c> — alíquota do ICMS.</summary>
    public required decimal PICMS { get; init; }

    /// <summary><c>vICMS</c> — valor do ICMS.</summary>
    public required decimal VICMS { get; init; }

    /// <summary><c>modBCST</c> — modalidade de determinação da BC do ICMS ST: 0–6.</summary>
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

    /// <summary><c>pBCOp</c> — percentual para determinação do valor da BC da operação própria (partilha).</summary>
    public required decimal PBCOp { get; init; }

    /// <summary><c>UFST</c> — sigla da UF para a qual é devido o ICMS ST da operação.</summary>
    public required string UFST { get; init; }

    /// <summary><c>vICMSDeson</c> — valor do ICMS desonerado (opcional).</summary>
    public decimal? VICMSDeson { get; init; }

    /// <summary><c>motDesICMS</c> — motivo da desoneração do ICMS: 9, 10, 11 (opcional).</summary>
    public int? MotDesICMS { get; init; }

    /// <summary><c>indDeduzDeson</c> — indica se o valor desonerado deduz do valor do item: 0 ou 1 (opcional).</summary>
    public int? IndDeduzDeson { get; init; }
}
