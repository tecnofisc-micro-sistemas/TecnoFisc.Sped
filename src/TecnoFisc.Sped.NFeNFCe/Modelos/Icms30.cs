using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>ICMS30</c> — isenta ou não tributada e com cobrança do ICMS por substituição tributária (CST 30).
/// </summary>
public sealed record Icms30 : Icms
{
    /// <summary><c>CST</c> — situação tributária do ICMS (sempre "30" nesta variante).</summary>
    public required Cst CST { get; init; }

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

    /// <summary><c>vICMSDeson</c> — valor do ICMS desonerado (opcional).</summary>
    public decimal? VICMSDeson { get; init; }

    /// <summary><c>motDesICMS</c> — motivo da desoneração do ICMS: 6, 7, 9 (opcional).</summary>
    public int? MotDesICMS { get; init; }

    /// <summary><c>indDeduzDeson</c> — indica se o valor desonerado deduz do valor do item: 0 ou 1 (opcional).</summary>
    public int? IndDeduzDeson { get; init; }
}
