using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>ICMSSN102</c> — Tributação pelo Simples Nacional sem permissão de crédito
/// (CSOSN 102, 103, 300 ou 400). Aplica-se quando o emitente é optante do Simples Nacional
/// (CRT = 1). No XSD o <c>orig</c> é opcional (<c>minOccurs="0"</c>); como <c>Icms.Orig</c>
/// é obrigatório no modelo, quando o elemento está ausente o parser aplica o default
/// <c>OrigemMercadoria.Nacional</c> (0).
/// </summary>
public sealed record IcmsSN102 : Icms
{
    /// <summary><c>CSOSN</c> — código de situação: 102 (sem crédito), 103 (isento por faixa de RB),
    /// 300 (imune) ou 400 (não tributada pelo Simples Nacional).</summary>
    public required Csosn CSOSN { get; init; }
}
