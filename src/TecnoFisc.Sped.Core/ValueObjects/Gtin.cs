namespace TecnoFisc.Sped.Core.ValueObjects;

/// <summary>
/// GTIN — Global Trade Item Number (campos cEAN/cEANTrib da NF-e). Comprimentos válidos:
/// 8, 12, 13 ou 14 dígitos, com dígito verificador mod-10 (GS1). Aceita a sentinela literal
/// "SEM GTIN" para produtos sem código de barras (caso documentado pela Receita).
/// </summary>
public readonly struct Gtin : IEquatable<Gtin>
{
    /// <summary>Sentinela para produto sem GTIN.</summary>
    public const string Sentinela = "SEM GTIN";

    private readonly string? _valor;

    private Gtin(string valor) => _valor = valor;

    /// <summary>Verdadeiro quando o valor é a sentinela "SEM GTIN".</summary>
    public bool IsSemGtin => string.Equals(_valor, Sentinela, StringComparison.Ordinal);

    /// <exception cref="FormatException">Quando o valor não é um GTIN válido nem a sentinela.</exception>
    public static Gtin Create(ReadOnlySpan<char> valor)
    {
        if (!TentarCriar(valor, out var gtin))
            throw new FormatException($"Valor não é um GTIN válido: '{valor}'.");
        return gtin;
    }

    public static bool TentarCriar(ReadOnlySpan<char> valor, out Gtin gtin)
    {
        if (valor.Equals(Sentinela, StringComparison.Ordinal))
        {
            gtin = new Gtin(Sentinela);
            return true;
        }

        int len = valor.Length;
        if (len is not (8 or 12 or 13 or 14) || !DigitosSomenteHelper.SaoTodosDigitos(valor))
        {
            gtin = default;
            return false;
        }

        if (!ValidarDv(valor))
        {
            gtin = default;
            return false;
        }

        gtin = new Gtin(new string(valor));
        return true;
    }

    private static bool ValidarDv(ReadOnlySpan<char> digitos)
    {
        int soma = 0;
        int peso = 3;
        for (int i = digitos.Length - 2; i >= 0; i--)
        {
            soma += (digitos[i] - '0') * peso;
            peso = peso == 3 ? 1 : 3;
        }
        int dv = (10 - (soma % 10)) % 10;
        return dv == digitos[^1] - '0';
    }

    public override string ToString() => _valor ?? Sentinela;

    public bool Equals(Gtin other) => string.Equals(_valor, other._valor, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is Gtin g && Equals(g);
    public override int GetHashCode() => _valor?.GetHashCode(StringComparison.Ordinal) ?? 0;
    public static bool operator ==(Gtin left, Gtin right) => left.Equals(right);
    public static bool operator !=(Gtin left, Gtin right) => !left.Equals(right);

    public static implicit operator string(Gtin gtin) => gtin.ToString();
}
