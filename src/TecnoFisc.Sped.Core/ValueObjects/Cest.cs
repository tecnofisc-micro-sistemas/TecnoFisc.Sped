namespace TecnoFisc.Sped.Core.ValueObjects;

/// <summary>
/// CEST — Código Especificador da Substituição Tributária. 7 dígitos, sem dígito verificador.
/// Value object opaco quanto ao conjunto de códigos válidos (valida apenas formato);
/// a existência do código na tabela vigente é responsabilidade do consumidor.
/// </summary>
public readonly struct Cest : IEquatable<Cest>
{
    public const int Tamanho = 7;

    private readonly string? _valor;

    private Cest(string valor) => _valor = valor;

    /// <exception cref="FormatException">Quando o valor não tem 7 dígitos.</exception>
    public static Cest Create(ReadOnlySpan<char> valor)
    {
        if (!TentarCriar(valor, out var cest))
            throw new FormatException($"Valor não é um CEST válido: '{valor}'.");
        return cest;
    }

    public static bool TentarCriar(ReadOnlySpan<char> valor, out Cest cest)
    {
        Span<char> digitos = stackalloc char[Tamanho];
        if (!DigitosSomenteHelper.TentarExtrair(valor, digitos))
        {
            cest = default;
            return false;
        }

        cest = new Cest(new string(digitos));
        return true;
    }

    public override string ToString() => _valor ?? new string('0', Tamanho);

    public bool Equals(Cest other) => string.Equals(_valor, other._valor, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is Cest c && Equals(c);
    public override int GetHashCode() => _valor?.GetHashCode(StringComparison.Ordinal) ?? 0;
    public static bool operator ==(Cest left, Cest right) => left.Equals(right);
    public static bool operator !=(Cest left, Cest right) => !left.Equals(right);

    public static implicit operator string(Cest cest) => cest.ToString();
}
