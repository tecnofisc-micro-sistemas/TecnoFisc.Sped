using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>ICMS60</c> — ICMS cobrado anteriormente por substituição tributária (CST 60).
/// Os campos do ICMS efetivo (<c>pRedBCEfet</c>…<c>vICMSEfet</c>) são opcionais no leiaute.
/// </summary>
public sealed record Icms60 : Icms
{
    /// <summary><c>CST</c> — situação tributária do ICMS (sempre "60" nesta variante).</summary>
    public Cst CST { get; init; }

    /// <summary><c>vBCSTRet</c> — base de cálculo do ICMS-ST retido anteriormente.</summary>
    public decimal? VBCSTRet { get; init; }

    /// <summary><c>pST</c> — alíquota suportada pelo consumidor final.</summary>
    public decimal? PST { get; init; }

    /// <summary><c>vICMSSubstituto</c> — valor do ICMS próprio do substituto.</summary>
    public decimal? VICMSSubstituto { get; init; }

    /// <summary><c>vICMSSTRet</c> — valor do ICMS-ST retido anteriormente.</summary>
    public decimal? VICMSSTRet { get; init; }

    /// <summary><c>pRedBCEfet</c> — percentual de redução da base de cálculo efetiva (opcional).</summary>
    public decimal? PRedBCEfet { get; init; }

    /// <summary><c>vBCEfet</c> — base de cálculo efetiva (opcional).</summary>
    public decimal? VBCEfet { get; init; }

    /// <summary><c>pICMSEfet</c> — alíquota do ICMS efetiva (opcional).</summary>
    public decimal? PICMSEfet { get; init; }

    /// <summary><c>vICMSEfet</c> — valor do ICMS efetivo (opcional).</summary>
    public decimal? VICMSEfet { get; init; }
}
