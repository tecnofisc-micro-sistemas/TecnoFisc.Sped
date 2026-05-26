using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>ICMS90</c> — outras situações tributárias (CST 90).
/// Os grupos da operação própria (BC + ICMS), ST e desoneração são todos opcionais no leiaute.
/// </summary>
public sealed record Icms90 : Icms
{
    /// <summary><c>CST</c> — situação tributária do ICMS (sempre "90" nesta variante).</summary>
    public required Cst CST { get; init; }

    /// <summary><c>modBC</c> — modalidade de determinação da BC do ICMS: 0–3 (opcional).</summary>
    public int? ModBC { get; init; }

    /// <summary><c>vBC</c> — valor da base de cálculo do ICMS (opcional).</summary>
    public decimal? VBC { get; init; }

    /// <summary><c>pRedBC</c> — percentual de redução da BC (opcional).</summary>
    public decimal? PRedBC { get; init; }

    /// <summary><c>cBenefRBC</c> — código de benefício fiscal na UF quando houver RBC (opcional; 8 ou 10 caracteres).</summary>
    public string? CBenefRBC { get; init; }

    /// <summary><c>pICMS</c> — alíquota do ICMS (opcional).</summary>
    public decimal? PICMS { get; init; }

    /// <summary><c>vICMSOp</c> — valor do ICMS da operação, antes do diferimento (opcional).</summary>
    public decimal? VICMSOp { get; init; }

    /// <summary><c>pDif</c> — percentual do diferimento (opcional).</summary>
    public decimal? PDif { get; init; }

    /// <summary><c>vICMSDif</c> — valor do ICMS diferido (opcional).</summary>
    public decimal? VICMSDif { get; init; }

    /// <summary><c>vICMS</c> — valor do ICMS (opcional).</summary>
    public decimal? VICMS { get; init; }

    /// <summary><c>vBCFCP</c> — valor da base de cálculo do FCP (opcional).</summary>
    public decimal? VBCFCP { get; init; }

    /// <summary><c>pFCP</c> — percentual de ICMS relativo ao Fundo de Combate à Pobreza (opcional).</summary>
    public decimal? PFCP { get; init; }

    /// <summary><c>vFCP</c> — valor do FCP (opcional).</summary>
    public decimal? VFCP { get; init; }

    /// <summary><c>pFCPDif</c> — percentual do diferimento do FCP (opcional).</summary>
    public decimal? PFCPDif { get; init; }

    /// <summary><c>vFCPDif</c> — valor do FCP diferido (opcional).</summary>
    public decimal? VFCPDif { get; init; }

    /// <summary><c>vFCPEfet</c> — valor efetivo do FCP após diferimento (opcional).</summary>
    public decimal? VFCPEfet { get; init; }

    /// <summary><c>modBCST</c> — modalidade de determinação da BC do ICMS ST: 0–6 (opcional).</summary>
    public int? ModBCST { get; init; }

    /// <summary><c>pMVAST</c> — percentual da margem de valor adicionado do ICMS ST (opcional).</summary>
    public decimal? PMVAST { get; init; }

    /// <summary><c>pRedBCST</c> — percentual de redução da BC do ICMS ST (opcional).</summary>
    public decimal? PRedBCST { get; init; }

    /// <summary><c>vBCST</c> — valor da base de cálculo do ICMS ST (opcional).</summary>
    public decimal? VBCST { get; init; }

    /// <summary><c>pICMSST</c> — alíquota do ICMS ST (opcional).</summary>
    public decimal? PICMSST { get; init; }

    /// <summary><c>vICMSST</c> — valor do ICMS ST (opcional).</summary>
    public decimal? VICMSST { get; init; }

    /// <summary><c>vBCFCPST</c> — valor da base de cálculo do FCP retido por substituição tributária (opcional).</summary>
    public decimal? VBCFCPST { get; init; }

    /// <summary><c>pFCPST</c> — percentual de FCP retido por substituição tributária (opcional).</summary>
    public decimal? PFCPST { get; init; }

    /// <summary><c>vFCPST</c> — valor do FCP retido por substituição tributária (opcional).</summary>
    public decimal? VFCPST { get; init; }

    /// <summary><c>vICMSDeson</c> — valor do ICMS desonerado (opcional).</summary>
    public decimal? VICMSDeson { get; init; }

    /// <summary><c>motDesICMS</c> — motivo da desoneração do ICMS: 3, 9, 12 (opcional).</summary>
    public int? MotDesICMS { get; init; }

    /// <summary><c>indDeduzDeson</c> — indica se o valor desonerado deduz do valor do item: 0 ou 1 (opcional).</summary>
    public int? IndDeduzDeson { get; init; }

    /// <summary><c>vICMSSTDeson</c> — valor do ICMS-ST desonerado (opcional).</summary>
    public decimal? VICMSSTDeson { get; init; }

    /// <summary><c>motDesICMSST</c> — motivo da desoneração do ICMS-ST: 3, 9, 12 (opcional).</summary>
    public int? MotDesICMSST { get; init; }
}
