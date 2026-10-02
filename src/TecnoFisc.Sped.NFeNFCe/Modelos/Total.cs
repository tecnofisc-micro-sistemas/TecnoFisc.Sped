namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Grupo <c>total</c> — totais da NF-e/NFC-e. Na slice 14.3 cobre apenas <c>ICMSTot</c>;
/// Inclui ICMSTot, ISSQNtot, retenções e totais IBS/CBS/IS da referência fixada.
/// </summary>
public sealed partial record Total
{
    /// <summary><c>ICMSTot</c> — totais de ICMS e da nota.</summary>
    public required TotalIcms ICMSTot { get; init; }
}
