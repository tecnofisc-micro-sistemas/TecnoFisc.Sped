using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>ICMSST</c> — repasse de ICMS ST retido anteriormente na UF remetente para a UF de destino,
/// em operações interestaduais com substituição tributária (CST 41 ou 60).
/// </summary>
public sealed record IcmsST : Icms
{
    /// <summary><c>CST</c> — situação tributária do ICMS: 41 (Não tributada) ou 60 (cobrado anteriormente por ST).</summary>
    public required Cst CST { get; init; }

    /// <summary><c>vBCSTRet</c> — valor da BC do ICMS ST retido na UF remetente.</summary>
    public required decimal VBCSTRet { get; init; }

    /// <summary><c>pST</c> — alíquota suportada pelo consumidor final (opcional).</summary>
    public decimal? PST { get; init; }

    /// <summary><c>vICMSSubstituto</c> — valor do ICMS próprio do substituto cobrado em operação anterior (opcional).</summary>
    public decimal? VICMSSubstituto { get; init; }

    /// <summary><c>vICMSSTRet</c> — valor do ICMS ST retido na UF remetente.</summary>
    public required decimal VICMSSTRet { get; init; }

    /// <summary><c>vBCFCPSTRet</c> — valor da BC do FCP retido anteriormente por ST (opcional).</summary>
    public decimal? VBCFCPSTRet { get; init; }

    /// <summary><c>pFCPSTRet</c> — percentual do FCP retido anteriormente por ST (opcional).</summary>
    public decimal? PFCPSTRet { get; init; }

    /// <summary><c>vFCPSTRet</c> — valor do FCP retido anteriormente por ST (opcional).</summary>
    public decimal? VFCPSTRet { get; init; }

    /// <summary><c>vBCSTDest</c> — valor da BC do ICMS ST da UF de destino.</summary>
    public required decimal VBCSTDest { get; init; }

    /// <summary><c>vICMSSTDest</c> — valor do ICMS ST da UF de destino.</summary>
    public required decimal VICMSSTDest { get; init; }

    /// <summary><c>pRedBCEfet</c> — percentual de redução da base de cálculo efetiva (opcional).</summary>
    public decimal? PRedBCEfet { get; init; }

    /// <summary><c>vBCEfet</c> — valor da base de cálculo efetiva (opcional).</summary>
    public decimal? VBCEfet { get; init; }

    /// <summary><c>pICMSEfet</c> — alíquota do ICMS efetivo (opcional).</summary>
    public decimal? PICMSEfet { get; init; }

    /// <summary><c>vICMSEfet</c> — valor do ICMS efetivo (opcional).</summary>
    public decimal? VICMSEfet { get; init; }
}
