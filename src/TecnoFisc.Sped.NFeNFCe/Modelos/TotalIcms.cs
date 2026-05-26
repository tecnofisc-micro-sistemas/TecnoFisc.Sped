namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Grupo <c>ICMSTot</c> — totalizadores de ICMS e da nota (leiaute 4.00). Todos os valores são
/// monetários em reais. Campos ausentes no XML ficam em <c>0</c> (o leiaute exige a maioria deles).
/// </summary>
public sealed record TotalIcms
{
    /// <summary><c>vBC</c> — base de cálculo do ICMS.</summary>
    public decimal VBC { get; init; }

    /// <summary><c>vICMS</c> — valor total do ICMS.</summary>
    public decimal VICMS { get; init; }

    /// <summary><c>vICMSDeson</c> — valor total do ICMS desonerado.</summary>
    public decimal VICMSDeson { get; init; }

    /// <summary><c>vFCP</c> — valor total do FCP (Fundo de Combate à Pobreza).</summary>
    public decimal VFCP { get; init; }

    /// <summary><c>vBCST</c> — base de cálculo do ICMS-ST.</summary>
    public decimal VBCST { get; init; }

    /// <summary><c>vST</c> — valor total do ICMS-ST.</summary>
    public decimal VST { get; init; }

    /// <summary><c>vFCPST</c> — valor total do FCP retido por ST.</summary>
    public decimal VFCPST { get; init; }

    /// <summary><c>vFCPSTRet</c> — valor total do FCP retido anteriormente por ST.</summary>
    public decimal VFCPSTRet { get; init; }

    /// <summary><c>vProd</c> — valor total dos produtos e serviços.</summary>
    public decimal VProd { get; init; }

    /// <summary><c>vFrete</c> — valor total do frete.</summary>
    public decimal VFrete { get; init; }

    /// <summary><c>vSeg</c> — valor total do seguro.</summary>
    public decimal VSeg { get; init; }

    /// <summary><c>vDesc</c> — valor total do desconto.</summary>
    public decimal VDesc { get; init; }

    /// <summary><c>vII</c> — valor total do imposto de importação.</summary>
    public decimal VII { get; init; }

    /// <summary><c>vIPI</c> — valor total do IPI.</summary>
    public decimal VIPI { get; init; }

    /// <summary><c>vIPIDevol</c> — valor total do IPI devolvido.</summary>
    public decimal VIPIDevol { get; init; }

    /// <summary><c>vPIS</c> — valor total do PIS.</summary>
    public decimal VPIS { get; init; }

    /// <summary><c>vCOFINS</c> — valor total da COFINS.</summary>
    public decimal VCOFINS { get; init; }

    /// <summary><c>vOutro</c> — outras despesas acessórias.</summary>
    public decimal VOutro { get; init; }

    /// <summary><c>vNF</c> — valor total da NF-e.</summary>
    public decimal VNF { get; init; }
}
