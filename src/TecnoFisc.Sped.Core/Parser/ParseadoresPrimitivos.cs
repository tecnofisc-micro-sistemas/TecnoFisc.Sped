using System.Globalization;

namespace TecnoFisc.Sped.Core.Parser;

/// <summary>
/// Conversores de string para os tipos primitivos usados em campos SPED.
/// Trabalham sobre <see cref="ReadOnlySpan{Char}"/> para minimizar alocações.
/// As strings SPED usam ponto-e-vírgula como delimitador? Não — usam pipe (`|`).
/// Os campos numéricos seguem cultura pt-BR (vírgula decimal); as datas seguem
/// "ddMMyyyy" (campos diários) ou "MMyyyy" (campos de período).
/// </summary>
public static class ParseadoresPrimitivos
{
    /// <summary>Converte para <see cref="int"/>. Vazio é inválido — use <see cref="InteiroOuPadrao"/>.</summary>
    public static int Inteiro(ReadOnlySpan<char> valor)
        => int.Parse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public static bool TentarInteiro(ReadOnlySpan<char> valor, out int resultado)
        => int.TryParse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out resultado);

    public static int InteiroOuPadrao(ReadOnlySpan<char> valor, int padrao = 0)
        => valor.IsEmpty ? padrao : Inteiro(valor);

    public static long Longo(ReadOnlySpan<char> valor)
        => long.Parse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture);

    public static bool TentarLongo(ReadOnlySpan<char> valor, out long resultado)
        => long.TryParse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out resultado);

    /// <summary>
    /// Converte um campo decimal SPED. O layout usa vírgula como separador decimal e nunca
    /// emite separador de milhar; valores podem ter sinal negativo.
    /// </summary>
    public static decimal ParaDecimal(ReadOnlySpan<char> valor)
    {
        if (valor.IsEmpty)
            throw new FormatException("Campo decimal SPED vazio.");

        Span<char> normalizado = valor.Length <= 64
            ? stackalloc char[valor.Length]
            : new char[valor.Length];

        for (int i = 0; i < valor.Length; i++)
            normalizado[i] = valor[i] == ',' ? '.' : valor[i];

        return decimal.Parse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture);
    }

    public static bool TentarDecimal(ReadOnlySpan<char> valor, out decimal resultado)
    {
        if (valor.IsEmpty)
        {
            resultado = 0m;
            return false;
        }

        Span<char> normalizado = valor.Length <= 64
            ? stackalloc char[valor.Length]
            : new char[valor.Length];

        for (int i = 0; i < valor.Length; i++)
            normalizado[i] = valor[i] == ',' ? '.' : valor[i];

        return decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out resultado);
    }

    /// <summary>
    /// Converte uma data no formato "ddMMyyyy" (8 caracteres). Outros formatos são suportados
    /// via <see cref="DataComFormato"/>.
    /// </summary>
    public static DateOnly Data(ReadOnlySpan<char> valor) => DataComFormato(valor, formato: null);

    /// <summary>
    /// Converte uma data conforme o formato declarado (ex.: "ddMMyyyy", "MMyyyy"). Se o
    /// formato for nulo ou vazio, assume "ddMMyyyy" (padrão SPED para campos diários).
    /// </summary>
    public static DateOnly DataComFormato(ReadOnlySpan<char> valor, string? formato)
    {
        if (string.IsNullOrEmpty(formato) || formato.Equals("ddMMyyyy", StringComparison.Ordinal))
            return ParseDataDDMMYYYY(valor);

        if (formato.Equals("MMyyyy", StringComparison.Ordinal))
            return ParsePeriodoMMYYYY(valor);

        if (formato.Equals("yyyyMM", StringComparison.Ordinal))
            return ParsePeriodoYYYYMM(valor);

        return DateOnly.ParseExact(valor, formato, CultureInfo.InvariantCulture);
    }

    public static bool TentarDataComFormato(ReadOnlySpan<char> valor, string? formato, out DateOnly resultado)
    {
        try
        {
            resultado = DataComFormato(valor, formato);
            return true;
        }
        catch (FormatException)
        {
            resultado = default;
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            resultado = default;
            return false;
        }
    }

    private static DateOnly ParseDataDDMMYYYY(ReadOnlySpan<char> valor)
    {
        if (valor.Length != 8)
            throw new FormatException($"Data ddMMyyyy precisa de 8 caracteres, recebeu {valor.Length}: '{valor}'.");

        int dia = LerDigitos(valor[..2]);
        int mes = LerDigitos(valor.Slice(2, 2));
        int ano = LerDigitos(valor.Slice(4, 4));
        return new DateOnly(ano, mes, dia);
    }

    private static DateOnly ParsePeriodoMMYYYY(ReadOnlySpan<char> valor)
    {
        if (valor.Length != 6)
            throw new FormatException($"Período MMyyyy precisa de 6 caracteres, recebeu {valor.Length}: '{valor}'.");

        int mes = LerDigitos(valor[..2]);
        int ano = LerDigitos(valor.Slice(2, 4));
        return new DateOnly(ano, mes, 1);
    }

    private static DateOnly ParsePeriodoYYYYMM(ReadOnlySpan<char> valor)
    {
        if (valor.Length != 6)
            throw new FormatException($"Período yyyyMM precisa de 6 caracteres, recebeu {valor.Length}: '{valor}'.");

        int ano = LerDigitos(valor[..4]);
        int mes = LerDigitos(valor.Slice(4, 2));
        return new DateOnly(ano, mes, 1);
    }

    private static int LerDigitos(ReadOnlySpan<char> valor)
    {
        int resultado = 0;
        for (int i = 0; i < valor.Length; i++)
        {
            char c = valor[i];
            if (c is < '0' or > '9')
                throw new FormatException($"Esperado dígito decimal, encontrou '{c}' em '{valor}'.");
            resultado = (resultado * 10) + (c - '0');
        }
        return resultado;
    }
}
