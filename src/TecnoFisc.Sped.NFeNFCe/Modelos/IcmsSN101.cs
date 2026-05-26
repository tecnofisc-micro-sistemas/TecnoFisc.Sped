using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Variante <c>ICMSSN101</c> — Tributação pelo Simples Nacional com permissão de crédito
/// (CSOSN 101). Aplica-se quando o emitente é optante do Simples Nacional (CRT = 1).
/// </summary>
public sealed record IcmsSN101 : Icms
{
    /// <summary><c>CSOSN</c> — código de situação da operação no Simples Nacional (sempre "101" nesta variante).</summary>
    public required Csosn CSOSN { get; init; }

    /// <summary><c>pCredSN</c> — alíquota aplicável de cálculo do crédito (Simples Nacional).</summary>
    public required decimal PCredSN { get; init; }

    /// <summary><c>vCredICMSSN</c> — valor crédito do ICMS aproveitável nos termos do art. 23 da LC 123.</summary>
    public required decimal VCredICMSSN { get; init; }
}
