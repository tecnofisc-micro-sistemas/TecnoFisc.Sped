namespace TecnoFisc.Sped.Core.ValueObjects;

/// <summary>
/// CNPJ — 12 caracteres ASCII alfanuméricos maiúsculos e dois dígitos verificadores.
/// Módulo 11 com valor ASCII menos 48 (compatível com o CNPJ numérico).
/// </summary>
public readonly struct Cnpj : IEquatable<Cnpj>
{
    public const int Tamanho = 14;

    private readonly string? _valor;

    private Cnpj(string valor) => _valor = valor;

    /// <summary>Cria um CNPJ a partir de uma string com ou sem máscara.</summary>
    /// <exception cref="FormatException">Quando o valor não é um CNPJ válido.</exception>
    public static Cnpj Create(ReadOnlySpan<char> valor)
    {
        if (!TentarCriar(valor, out var cnpj))
            throw new FormatException($"Valor não é um CNPJ válido: '{valor}'.");
        return cnpj;
    }

    public static bool TentarCriar(ReadOnlySpan<char> valor, out Cnpj cnpj)
    {
        Span<char> digitos = stackalloc char[Tamanho];
        if (!IdentificadorAlfanumericoHelper.TentarExtrair(valor, digitos, 0, 12))
        {
            cnpj = default;
            return false;
        }

        if (DigitosSomenteHelper.TodosIguais(digitos))
        {
            cnpj = default;
            return false;
        }

        if (!ValidarDigitosVerificadores(digitos))
        {
            cnpj = default;
            return false;
        }

        cnpj = new Cnpj(new string(digitos));
        return true;
    }

    private static bool ValidarDigitosVerificadores(ReadOnlySpan<char> digitos)
    {
        // Pesos oficiais para o cálculo dos DV do CNPJ.
        ReadOnlySpan<int> pesos1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        ReadOnlySpan<int> pesos2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        int dv1Esperado = CalcularDigito(digitos[..12], pesos1);
        if (dv1Esperado != digitos[12] - '0') return false;

        int dv2Esperado = CalcularDigito(digitos[..13], pesos2);
        return dv2Esperado == digitos[13] - '0';
    }

    private static int CalcularDigito(ReadOnlySpan<char> base_, ReadOnlySpan<int> pesos)
    {
        int soma = 0;
        for (int i = 0; i < base_.Length; i++)
            soma += (base_[i] - '0') * pesos[i];
        int resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    /// <summary>Forma canônica: 14 caracteres sem máscara.</summary>
    public override string ToString() => _valor ?? new string('0', Tamanho);

    /// <summary>Forma humana: <c>NN.NNN.NNN/NNNN-NN</c>.</summary>
    public string ToStringFormatado()
    {
        var s = ToString();
        return $"{s[..2]}.{s.Substring(2, 3)}.{s.Substring(5, 3)}/{s.Substring(8, 4)}-{s.Substring(12, 2)}";
    }

    public bool Equals(Cnpj other) => string.Equals(_valor, other._valor, StringComparison.Ordinal);
    public override bool Equals(object? obj) => obj is Cnpj c && Equals(c);
    public override int GetHashCode() => _valor?.GetHashCode(StringComparison.Ordinal) ?? 0;
    public static bool operator ==(Cnpj left, Cnpj right) => left.Equals(right);
    public static bool operator !=(Cnpj left, Cnpj right) => !left.Equals(right);

    public static implicit operator string(Cnpj cnpj) => cnpj.ToString();
}
