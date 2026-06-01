namespace TecnoFisc.Sped.Core.ValueObjects;

/// <summary>
/// CSOSN — Código de Situação da Operação no Simples Nacional. 3 dígitos.
/// Usado no lugar do CST de ICMS quando o emitente é optante do Simples (CRT = 1).
/// Value object opaco quanto ao conjunto de códigos (valida apenas formato);
/// a validação semântica (101, 102, 103, 201, 202, 203, 300, 400, 500, 900) é do consumidor.
/// </summary>
public readonly struct Csosn : IEquatable<Csosn>
{
    public const int Tamanho = 3;

    private readonly string? _valor;

    private Csosn(string valor) => _valor = valor;

    /// <exception cref="FormatException">Quando o valor não tem 3 dígitos.</exception>
    public static Csosn Create(ReadOnlySpan<char> valor)
    {
        if (!TentarCriar(valor, out var csosn))
            throw new FormatException($"Valor não é um CSOSN válido: '{valor}'.");
        return csosn;
    }

    public static bool TentarCriar(ReadOnlySpan<char> valor, out Csosn csosn)
    {
        Span<char> digitos = stackalloc char[Tamanho];
        if (!DigitosSomenteHelper.TentarExtrair(valor, digitos))
        {
            csosn = default;
            return false;
        }

        csosn = new Csosn(new string(digitos));
        return true;
    }

    public override string ToString() => _valor ?? string.Empty;

    public bool Equals(Csosn other) => string.Equals(_valor, other._valor, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is Csosn c && Equals(c);
    public override int GetHashCode() => _valor?.GetHashCode(StringComparison.Ordinal) ?? 0;
    public static bool operator ==(Csosn left, Csosn right) => left.Equals(right);
    public static bool operator !=(Csosn left, Csosn right) => !left.Equals(right);

    public static implicit operator string(Csosn csosn) => csosn.ToString();
}
