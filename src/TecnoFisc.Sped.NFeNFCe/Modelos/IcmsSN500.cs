using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>ICMSSN500</c> — ICMS cobrado anteriormente por substituição tributária
/// (substituído) ou por antecipação no Simples Nacional (CSOSN 500).
/// Os grupos de campos retidos e efetivos são todos opcionais no XSD (<c>minOccurs="0"</c>).
/// </summary>
public sealed record IcmsSN500 : Icms
{
    /// <summary><c>CSOSN</c> — código de situação da operação no Simples Nacional (sempre "500" nesta variante).</summary>
    public required Csosn CSOSN { get; init; }

    /// <summary><c>vBCSTRet</c> — valor da BC do ICMS ST retido anteriormente (opcional).</summary>
    public decimal? VBCSTRet { get; init; }

    /// <summary><c>pST</c> — alíquota suportada pelo consumidor final (opcional).</summary>
    public decimal? PST { get; init; }

    /// <summary><c>vICMSSubstituto</c> — valor do ICMS próprio do substituto (opcional).</summary>
    public decimal? VICMSSubstituto { get; init; }

    /// <summary><c>vICMSSTRet</c> — valor do ICMS ST retido anteriormente (opcional).</summary>
    public decimal? VICMSSTRet { get; init; }

    /// <summary><c>vBCFCPSTRet</c> — valor da base de cálculo do FCP retido anteriormente (opcional).</summary>
    public decimal? VBCFCPSTRet { get; init; }

    /// <summary><c>pFCPSTRet</c> — percentual do FCP retido anteriormente por ST (opcional).</summary>
    public decimal? PFCPSTRet { get; init; }

    /// <summary><c>vFCPSTRet</c> — valor do FCP retido por substituição tributária (opcional).</summary>
    public decimal? VFCPSTRet { get; init; }

    /// <summary><c>pRedBCEfet</c> — percentual de redução da base de cálculo efetiva (opcional).</summary>
    public decimal? PRedBCEfet { get; init; }

    /// <summary><c>vBCEfet</c> — valor da base de cálculo efetiva (opcional).</summary>
    public decimal? VBCEfet { get; init; }

    /// <summary><c>pICMSEfet</c> — alíquota do ICMS efetiva (opcional).</summary>
    public decimal? PICMSEfet { get; init; }

    /// <summary><c>vICMSEfet</c> — valor do ICMS efetivo (opcional).</summary>
    public decimal? VICMSEfet { get; init; }
}
