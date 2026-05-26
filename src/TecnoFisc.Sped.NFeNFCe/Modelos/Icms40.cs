using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>ICMS40</c> — isenta, não tributada ou suspensa (CST 40, 41 ou 50).
/// Os campos de desoneração são opcionais e agrupados: se <c>vICMSDeson</c> presente,
/// <c>motDesICMS</c> é obrigatório; <c>indDeduzDeson</c> é opcional dentro do grupo.
/// </summary>
public sealed record Icms40 : Icms
{
    /// <summary><c>CST</c> — situação tributária do ICMS: 40 (Isenta), 41 (Não tributada) ou 50 (Suspensão).</summary>
    public required Cst CST { get; init; }

    /// <summary><c>vICMSDeson</c> — valor do ICMS desonerado (opcional; obrigatório apenas em operações com desoneração condicional).</summary>
    public decimal? VICMSDeson { get; init; }

    /// <summary><c>motDesICMS</c> — motivo da desoneração do ICMS: 1, 3, 4, 5, 6, 7, 8, 9, 10, 11, 16, 90 (opcional; presente quando <c>vICMSDeson</c> informado).</summary>
    public int? MotDesICMS { get; init; }

    /// <summary><c>indDeduzDeson</c> — indica se o valor desonerado deduz do valor do item: 0 ou 1 (opcional).</summary>
    public int? IndDeduzDeson { get; init; }
}
