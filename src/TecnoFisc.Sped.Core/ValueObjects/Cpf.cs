namespace TecnoFisc.Sped.Core.ValueObjects;

/// <summary>
/// CPF — Cadastro de Pessoas Físicas. 11 dígitos com dois dígitos verificadores módulo 11.
/// A representação canônica em SPED é a forma de 11 dígitos sem máscara.
/// </summary>
public readonly struct Cpf : IEquatable<Cpf>
{
    public const int Tamanho = 11;

    private readonly string? _valor;

    private Cpf(string valor) => _valor = valor;

    /// <exception cref="FormatException">Quando o valor não é um CPF válido.</exception>
    public static Cpf Criar(ReadOnlySpan<char> valor)
    {
        if (!TentarCriar(valor, out var cpf))
            throw new FormatException($"Valor não é um CPF válido: '{valor}'.");
        return cpf;
    }

    public static bool TentarCriar(ReadOnlySpan<char> valor, out Cpf cpf)
    {
        Span<char> digitos = stackalloc char[Tamanho];
        if (!DigitosSomenteHelper.TentarExtrair(valor, digitos))
        {
            cpf = default;
            return false;
        }

        if (DigitosSomenteHelper.TodosIguais(digitos))
        {
            cpf = default;
            return false;
        }

        if (!ValidarDigitosVerificadores(digitos))
        {
            cpf = default;
            return false;
        }

        cpf = new Cpf(new string(digitos));
        return true;
    }

    private static bool ValidarDigitosVerificadores(ReadOnlySpan<char> digitos)
    {
        int dv1Esperado = CalcularDigito(digitos[..9], pesoInicial: 10);
        if (dv1Esperado != digitos[9] - '0') return false;

        int dv2Esperado = CalcularDigito(digitos[..10], pesoInicial: 11);
        return dv2Esperado == digitos[10] - '0';
    }

    private static int CalcularDigito(ReadOnlySpan<char> base_, int pesoInicial)
    {
        int soma = 0;
        for (int i = 0; i < base_.Length; i++)
            soma += (base_[i] - '0') * (pesoInicial - i);
        int resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    public override string ToString() => _valor ?? new string('0', Tamanho);

    /// <summary>Forma humana: <c>NNN.NNN.NNN-NN</c>.</summary>
    public string ToStringFormatado()
    {
        var s = ToString();
        return $"{s[..3]}.{s.Substring(3, 3)}.{s.Substring(6, 3)}-{s.Substring(9, 2)}";
    }

    public bool Equals(Cpf other) => string.Equals(_valor, other._valor, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is Cpf c && Equals(c);
    public override int GetHashCode() => _valor?.GetHashCode(StringComparison.Ordinal) ?? 0;
    public static bool operator ==(Cpf left, Cpf right) => left.Equals(right);
    public static bool operator !=(Cpf left, Cpf right) => !left.Equals(right);

    public static implicit operator string(Cpf cpf) => cpf.ToString();
}
