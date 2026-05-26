using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Grupo <c>IPI</c> — dados do IPI de um item. Contém campos comuns do tipo <c>TIpi</c>
/// e uma tributação polimórfica (<see cref="IpiTrib"/> ou <see cref="IpiNt"/>).
/// </summary>
public sealed record Ipi
{
    /// <summary>
    /// <c>CNPJProd</c> — CNPJ do produtor da mercadoria, quando diferente do emitente.
    /// Utilizado em exportação direta ou indireta (opcional).
    /// </summary>
    public Cnpj? CNPJProd { get; init; }

    /// <summary><c>cSelo</c> — código do selo de controle do IPI (opcional, até 60 caracteres).</summary>
    public string? CSelo { get; init; }

    /// <summary>
    /// <c>qSelo</c> — quantidade de selo de controle do IPI (opcional, até 12 dígitos).
    /// Tipado como <see langword="long"/> porque o XSD permite até 12 dígitos, excedendo <c>int.MaxValue</c> (10 dígitos).
    /// </summary>
    public long? QSelo { get; init; }

    /// <summary><c>cEnq</c> — código de enquadramento legal do IPI (obrigatório, até 3 caracteres).</summary>
    public required string CEnq { get; init; }

    /// <summary>
    /// Tributação do item — <see cref="IpiTrib"/> (tributado) ou <see cref="IpiNt"/> (não tributado).
    /// </summary>
    public required IpiTributacao Tributacao { get; init; }
}
