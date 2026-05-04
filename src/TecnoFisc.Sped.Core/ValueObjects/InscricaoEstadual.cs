namespace TecnoFisc.Sped.Core.ValueObjects;

/// <summary>
/// Inscrição Estadual. O SPED aceita o valor literal <c>ISENTO</c> ou um código alfanumérico
/// de 2 a 14 caracteres (cada UF tem o próprio formato e algoritmo de DV).
/// Esta value object valida apenas conformidade de formato; algoritmos por UF ficam em camada superior.
/// </summary>
public readonly struct InscricaoEstadual : IEquatable<InscricaoEstadual>
{
    public const int TamanhoMinimo = 2;
    public const int TamanhoMaximo = 14;
    public const string Isento = "ISENTO";

    private readonly string? _valor;

    private InscricaoEstadual(string valor) => _valor = valor;

    /// <exception cref="FormatException">Quando o valor não é uma IE válida em formato.</exception>
    public static InscricaoEstadual Criar(ReadOnlySpan<char> valor)
    {
        if (!TentarCriar(valor, out var ie))
            throw new FormatException($"Valor não é uma Inscrição Estadual válida: '{valor}'.");
        return ie;
    }

    public static bool TentarCriar(ReadOnlySpan<char> valor, out InscricaoEstadual ie)
    {
        var bruto = valor.Trim();
        if (bruto.IsEmpty)
        {
            ie = default;
            return false;
        }

        if (bruto.Equals(Isento, StringComparison.OrdinalIgnoreCase))
        {
            ie = new InscricaoEstadual(Isento);
            return true;
        }

        Span<char> normalizado = stackalloc char[TamanhoMaximo];
        int idx = 0;
        foreach (char c in bruto)
        {
            if (c is '.' or '-' or '/' or ' ') continue;
            if (idx >= TamanhoMaximo)
            {
                ie = default;
                return false;
            }
            if (!char.IsAsciiLetterOrDigit(c))
            {
                ie = default;
                return false;
            }
            normalizado[idx++] = char.ToUpperInvariant(c);
        }

        if (idx < TamanhoMinimo)
        {
            ie = default;
            return false;
        }

        ie = new InscricaoEstadual(new string(normalizado[..idx]));
        return true;
    }

    public bool EhIsento => string.Equals(_valor, Isento, StringComparison.Ordinal);

    public override string ToString() => _valor ?? string.Empty;

    public bool Equals(InscricaoEstadual other) => string.Equals(_valor, other._valor, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is InscricaoEstadual ie && Equals(ie);
    public override int GetHashCode() => _valor?.GetHashCode(StringComparison.Ordinal) ?? 0;
    public static bool operator ==(InscricaoEstadual left, InscricaoEstadual right) => left.Equals(right);
    public static bool operator !=(InscricaoEstadual left, InscricaoEstadual right) => !left.Equals(right);

    public static implicit operator string(InscricaoEstadual ie) => ie.ToString();
}
