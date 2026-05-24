namespace TecnoFisc.Sped.Core.ValueObjects;

/// <summary>
/// NCM — Nomenclatura Comum do Mercosul. 8 dígitos no padrão SH/Mercosul.
/// O SPED valida apenas o formato; a existência do código na tabela TIPI/NCM vigente é responsabilidade do consumidor.
/// </summary>
public readonly struct Ncm : IEquatable<Ncm>
{
    public const int Tamanho = 8;

    private readonly string? _valor;

    private Ncm(string valor) => _valor = valor;

    /// <exception cref="FormatException">Quando o valor não tem 8 dígitos.</exception>
    public static Ncm Create(ReadOnlySpan<char> valor)
    {
        if (!TentarCriar(valor, out var ncm))
            throw new FormatException($"Valor não é um NCM válido: '{valor}'.");
        return ncm;
    }

    public static bool TentarCriar(ReadOnlySpan<char> valor, out Ncm ncm)
    {
        Span<char> digitos = stackalloc char[Tamanho];
        if (!DigitosSomenteHelper.TentarExtrair(valor, digitos))
        {
            ncm = default;
            return false;
        }

        ncm = new Ncm(new string(digitos));
        return true;
    }

    public override string ToString() => _valor ?? new string('0', Tamanho);

    public bool Equals(Ncm other) => string.Equals(_valor, other._valor, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is Ncm n && Equals(n);
    public override int GetHashCode() => _valor?.GetHashCode(StringComparison.Ordinal) ?? 0;
    public static bool operator ==(Ncm left, Ncm right) => left.Equals(right);
    public static bool operator !=(Ncm left, Ncm right) => !left.Equals(right);

    public static implicit operator string(Ncm ncm) => ncm.ToString();
}
