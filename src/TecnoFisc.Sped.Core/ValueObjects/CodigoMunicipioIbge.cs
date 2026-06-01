namespace TecnoFisc.Sped.Core.ValueObjects;

/// <summary>
/// Código de município do IBGE. 7 dígitos. Valida apenas formato — aceita o código especial
/// 9999999 (operações com o exterior). Os 2 primeiros dígitos correspondem ao código de UF.
/// </summary>
public readonly struct CodigoMunicipioIbge : IEquatable<CodigoMunicipioIbge>
{
    public const int Tamanho = 7;

    private readonly string? _valor;

    private CodigoMunicipioIbge(string valor) => _valor = valor;

    /// <exception cref="FormatException">Quando o valor não tem 7 dígitos.</exception>
    public static CodigoMunicipioIbge Create(ReadOnlySpan<char> valor)
    {
        if (!TentarCriar(valor, out var municipio))
            throw new FormatException($"Valor não é um código de município IBGE válido: '{valor}'.");
        return municipio;
    }

    public static bool TentarCriar(ReadOnlySpan<char> valor, out CodigoMunicipioIbge municipio)
    {
        Span<char> digitos = stackalloc char[Tamanho];
        if (!DigitosSomenteHelper.TentarExtrair(valor, digitos))
        {
            municipio = default;
            return false;
        }

        municipio = new CodigoMunicipioIbge(new string(digitos));
        return true;
    }

    /// <summary>Código IBGE da UF (2 primeiros dígitos).</summary>
    public int CodigoUf => _valor is null ? 0 : (_valor[0] - '0') * 10 + (_valor[1] - '0');

    public override string ToString() => _valor ?? new string('0', Tamanho);

    public bool Equals(CodigoMunicipioIbge other) => string.Equals(_valor, other._valor, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is CodigoMunicipioIbge m && Equals(m);
    public override int GetHashCode() => _valor?.GetHashCode(StringComparison.Ordinal) ?? 0;
    public static bool operator ==(CodigoMunicipioIbge left, CodigoMunicipioIbge right) => left.Equals(right);
    public static bool operator !=(CodigoMunicipioIbge left, CodigoMunicipioIbge right) => !left.Equals(right);

    public static implicit operator string(CodigoMunicipioIbge municipio) => municipio.ToString();
}
