using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.NFeNFCe;

/// <summary>
/// Grupo <c>dest</c> — destinatário. Identifica-se por <c>CNPJ</c>, <c>CPF</c> ou
/// <c>idEstrangeiro</c> (operação com estrangeiro). Opcional na NFC-e (modelo 65).
/// </summary>
public sealed record Destinatario
{
    /// <summary><c>CNPJ</c> do destinatário.</summary>
    public Cnpj? CNPJ { get; init; }

    /// <summary><c>CPF</c> do destinatário.</summary>
    public Cpf? CPF { get; init; }

    /// <summary><c>idEstrangeiro</c> — identificação do destinatário estrangeiro.</summary>
    public string? IdEstrangeiro { get; init; }

    /// <summary><c>xNome</c> — razão social ou nome (opcional na NFC-e).</summary>
    public string? XNome { get; init; }

    /// <summary><c>enderDest</c> — endereço do destinatário (opcional).</summary>
    public Endereco? EnderDest { get; init; }

    /// <summary><c>indIEDest</c> — indicador da inscrição estadual: 1 contribuinte, 2 isento, 9 não contribuinte.</summary>
    public int? IndIEDest { get; init; }

    /// <summary><c>IE</c> — inscrição estadual (opcional).</summary>
    public InscricaoEstadual? IE { get; init; }

    /// <summary><c>email</c> — endereço de e-mail (opcional).</summary>
    public string? Email { get; init; }
}
