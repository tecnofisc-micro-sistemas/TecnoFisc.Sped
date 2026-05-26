using TecnoFisc.Sped.Core.Enums;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Base abstrata das variantes de ICMS de um item (grupo <c>ICMS</c>). Cada CST/CSOSN é um
/// <c>sealed record</c> concreto; o consumidor faz pattern matching sobre o tipo. As variantes
/// de regime normal (CST 00/10/20/30/40/51/60/70/90, ICMSPart, ICMSST) estão implementadas;
/// CSOSN (Simples Nacional) e variantes de combustível entram em slices posteriores.
/// </summary>
public abstract record Icms
{
    /// <summary><c>orig</c> — origem da mercadoria (nacional/estrangeira).</summary>
    public required OrigemMercadoria Orig { get; init; }
}
