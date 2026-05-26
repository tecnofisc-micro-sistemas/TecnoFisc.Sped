namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Grupo <c>total</c> — totais da NF-e/NFC-e. Na slice 14.3 cobre apenas <c>ICMSTot</c>;
/// <c>ISSQNtot</c> e <c>retTrib</c> entram em slices posteriores.
/// </summary>
public sealed record Total
{
    /// <summary><c>ICMSTot</c> — totais de ICMS e da nota.</summary>
    public required TotalIcms ICMSTot { get; init; }
}
