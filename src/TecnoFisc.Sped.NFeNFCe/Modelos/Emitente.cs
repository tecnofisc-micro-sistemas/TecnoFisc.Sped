using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Grupo <c>emit</c> — emitente da NF-e/NFC-e. O emitente identifica-se por <c>CNPJ</c> ou,
/// excepcionalmente, por <c>CPF</c> (produtor rural pessoa física) — apenas um dos dois é informado.
/// </summary>
public sealed record Emitente
{
    /// <summary><c>CNPJ</c> do emitente (mutuamente exclusivo com <see cref="CPF"/>).</summary>
    public Cnpj? CNPJ { get; init; }

    /// <summary><c>CPF</c> do emitente (mutuamente exclusivo com <see cref="CNPJ"/>).</summary>
    public Cpf? CPF { get; init; }

    /// <summary><c>xNome</c> — razão social ou nome.</summary>
    public required string XNome { get; init; }

    /// <summary><c>xFant</c> — nome fantasia (opcional).</summary>
    public string? XFant { get; init; }

    /// <summary><c>enderEmit</c> — endereço do emitente.</summary>
    public required Endereco EnderEmit { get; init; }

    /// <summary><c>IE</c> — inscrição estadual (opcional).</summary>
    public InscricaoEstadual? IE { get; init; }

    /// <summary><c>IEST</c> — inscrição estadual do substituto tributário (opcional).</summary>
    public string? IEST { get; init; }

    /// <summary><c>IM</c> — inscrição municipal (opcional).</summary>
    public string? IM { get; init; }

    /// <summary><c>CNAE</c> — código fiscal de atividade econômica (opcional).</summary>
    public string? CNAE { get; init; }

    /// <summary><c>CRT</c> — código de regime tributário: 1 Simples, 2 Simples excesso, 3 Regime normal, 4 MEI.</summary>
    public int CRT { get; init; }
}
