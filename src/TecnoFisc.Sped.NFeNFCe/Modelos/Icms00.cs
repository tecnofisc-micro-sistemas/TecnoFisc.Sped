using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>ICMS00</c> — tributação integralmente pelo ICMS (CST 00).
/// </summary>
public sealed record Icms00 : Icms
{
    /// <summary><c>CST</c> — situação tributária do ICMS (sempre "00" nesta variante).</summary>
    public required Cst CST { get; init; }

    /// <summary><c>modBC</c> — modalidade de determinação da BC do ICMS (0–3).</summary>
    public required int ModBC { get; init; }

    /// <summary><c>vBC</c> — valor da base de cálculo do ICMS.</summary>
    public required decimal VBC { get; init; }

    /// <summary><c>pICMS</c> — alíquota do ICMS.</summary>
    public required decimal PICMS { get; init; }

    /// <summary><c>vICMS</c> — valor do ICMS.</summary>
    public required decimal VICMS { get; init; }

    /// <summary><c>pFCP</c> — percentual de ICMS relativo ao Fundo de Combate à Pobreza (opcional).</summary>
    public decimal? PFCP { get; init; }

    /// <summary><c>vFCP</c> — valor do ICMS relativo ao Fundo de Combate à Pobreza (opcional).</summary>
    public decimal? VFCP { get; init; }
}
