using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>ICMS51</c> — tributação com diferimento (CST 51).
/// Todos os campos, exceto <c>orig</c> e <c>CST</c>, são opcionais — a exigência varia por UF.
/// </summary>
public sealed record Icms51 : Icms
{
    /// <summary><c>CST</c> — situação tributária do ICMS (sempre "51" nesta variante).</summary>
    public required Cst CST { get; init; }

    /// <summary><c>modBC</c> — modalidade de determinação da BC do ICMS: 0–3 (opcional).</summary>
    public int? ModBC { get; init; }

    /// <summary><c>pRedBC</c> — percentual de redução da BC (opcional).</summary>
    public decimal? PRedBC { get; init; }

    /// <summary><c>cBenefRBC</c> — código de benefício fiscal na UF quando houver RBC (opcional; 8 ou 10 caracteres).</summary>
    public string? CBenefRBC { get; init; }

    /// <summary><c>vBC</c> — valor da base de cálculo do ICMS (opcional).</summary>
    public decimal? VBC { get; init; }

    /// <summary><c>pICMS</c> — alíquota do ICMS (opcional).</summary>
    public decimal? PICMS { get; init; }

    /// <summary><c>vICMSOp</c> — valor do ICMS da operação, antes do diferimento (opcional).</summary>
    public decimal? VICMSOp { get; init; }

    /// <summary><c>pDif</c> — percentual do diferimento (opcional).</summary>
    public decimal? PDif { get; init; }

    /// <summary><c>vICMSDif</c> — valor do ICMS diferido (opcional).</summary>
    public decimal? VICMSDif { get; init; }

    /// <summary><c>vICMS</c> — valor do ICMS devido após diferimento (opcional).</summary>
    public decimal? VICMS { get; init; }

    /// <summary><c>vBCFCP</c> — valor da base de cálculo do FCP (opcional; obrigatório no grupo FCP se presente).</summary>
    public decimal? VBCFCP { get; init; }

    /// <summary><c>pFCP</c> — percentual de ICMS relativo ao Fundo de Combate à Pobreza (opcional).</summary>
    public decimal? PFCP { get; init; }

    /// <summary><c>vFCP</c> — valor do FCP (opcional; obrigatório no grupo FCP se presente).</summary>
    public decimal? VFCP { get; init; }

    /// <summary><c>pFCPDif</c> — percentual do diferimento do FCP (opcional; obrigatório no grupo FCPDif se presente).</summary>
    public decimal? PFCPDif { get; init; }

    /// <summary><c>vFCPDif</c> — valor do FCP diferido (opcional; obrigatório no grupo FCPDif se presente).</summary>
    public decimal? VFCPDif { get; init; }

    /// <summary><c>vFCPEfet</c> — valor efetivo do FCP após diferimento (opcional).</summary>
    public decimal? VFCPEfet { get; init; }
}
