using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>ICMS20</c> — com redução de base de cálculo (CST 20).
/// </summary>
public sealed record Icms20 : Icms
{
    /// <summary><c>CST</c> — situação tributária do ICMS (sempre "20" nesta variante).</summary>
    public required Cst CST { get; init; }

    /// <summary><c>modBC</c> — modalidade de determinação da BC do ICMS (0–3).</summary>
    public required int ModBC { get; init; }

    /// <summary><c>pRedBC</c> — percentual de redução da BC.</summary>
    public required decimal PRedBC { get; init; }

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

    /// <summary><c>vICMSDeson</c> — valor do ICMS desonerado (opcional).</summary>
    public decimal? VICMSDeson { get; init; }

    /// <summary><c>motDesICMS</c> — motivo da desoneração do ICMS: 3, 9, 10, 11, 12 (opcional).</summary>
    public int? MotDesICMS { get; init; }

    /// <summary><c>indDeduzDeson</c> — indica se o valor desonerado deduz do valor do item: 0 ou 1 (opcional).</summary>
    public int? IndDeduzDeson { get; init; }
}
