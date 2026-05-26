using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Base abstrata das variantes de ICMS de um item (grupo <c>ICMS</c>). Cada CST/CSOSN é um
/// <c>sealed record</c> concreto; o consumidor faz pattern matching sobre o tipo. A slice 14.3
/// implementa apenas <see cref="Icms60"/>; as demais variantes entram na slice 14.4.
/// </summary>
public abstract record Icms
{
    /// <summary><c>orig</c> — origem da mercadoria (nacional/estrangeira).</summary>
    public required OrigemMercadoria Orig { get; init; }
}
