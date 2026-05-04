namespace TecnoFisc.Sped.Core.ValueObjects;

/// <summary>
/// CFOP — Código Fiscal de Operações e Prestações. 4 dígitos.
/// O primeiro dígito identifica a natureza da operação:
/// 1/2/3 = entradas (UF, outras UFs, exterior); 5/6/7 = saídas (UF, outras UFs, exterior).
/// </summary>
public readonly struct Cfop : IEquatable<Cfop>
{
    public const int Tamanho = 4;

    private readonly string? _valor;

    private Cfop(string valor) => _valor = valor;

    /// <exception cref="FormatException">Quando o valor não é um CFOP válido.</exception>
    public static Cfop Criar(ReadOnlySpan<char> valor)
    {
        if (!TentarCriar(valor, out var cfop))
            throw new FormatException($"Valor não é um CFOP válido: '{valor}'.");
        return cfop;
    }

    public static bool TentarCriar(ReadOnlySpan<char> valor, out Cfop cfop)
    {
        Span<char> digitos = stackalloc char[Tamanho];
        if (!DigitosSomenteHelper.TentarExtrair(valor, digitos))
        {
            cfop = default;
            return false;
        }

        char primeiro = digitos[0];
        if (primeiro is not ('1' or '2' or '3' or '5' or '6' or '7'))
        {
            cfop = default;
            return false;
        }

        cfop = new Cfop(new string(digitos));
        return true;
    }

    /// <summary>Indica se o CFOP representa uma entrada (primeiro dígito 1, 2 ou 3).</summary>
    public bool EhEntrada => _valor is { Length: > 0 } v && v[0] is '1' or '2' or '3';

    /// <summary>Indica se o CFOP representa uma saída (primeiro dígito 5, 6 ou 7).</summary>
    public bool EhSaida => _valor is { Length: > 0 } v && v[0] is '5' or '6' or '7';

    public override string ToString() => _valor ?? "0000";

    public bool Equals(Cfop other) => string.Equals(_valor, other._valor, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is Cfop c && Equals(c);
    public override int GetHashCode() => _valor?.GetHashCode(StringComparison.Ordinal) ?? 0;
    public static bool operator ==(Cfop left, Cfop right) => left.Equals(right);
    public static bool operator !=(Cfop left, Cfop right) => !left.Equals(right);

    public static implicit operator string(Cfop cfop) => cfop.ToString();
}
