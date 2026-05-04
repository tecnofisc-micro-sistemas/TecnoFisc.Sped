namespace TecnoFisc.Sped.Core.ValueObjects;

internal static class DigitosSomenteHelper
{
    /// <summary>
    /// Extrai apenas os dígitos da entrada para um buffer de tamanho fixo.
    /// Aceita máscaras comuns (pontos, traços, barras, espaços) descartando-as.
    /// </summary>
    /// <param name="origem">Texto de origem (pode conter máscara).</param>
    /// <param name="destino">Buffer de destino — seu tamanho define quantos dígitos são esperados.</param>
    /// <returns><c>true</c> se exatamente <c>destino.Length</c> dígitos foram encontrados.</returns>
    public static bool TentarExtrair(ReadOnlySpan<char> origem, Span<char> destino)
    {
        int idx = 0;
        foreach (char c in origem)
        {
            if (c >= '0' && c <= '9')
            {
                if (idx >= destino.Length)
                    return false;
                destino[idx++] = c;
            }
            else if (c is '.' or '-' or '/' or ' ' or '\t')
            {
                continue;
            }
            else
            {
                return false;
            }
        }

        return idx == destino.Length;
    }

    /// <summary>
    /// Verifica se todos os caracteres são iguais (ex.: "00000000000"). Útil para rejeitar
    /// CPF/CNPJ que passam no mod 11 trivialmente mas são inválidos por convenção.
    /// </summary>
    public static bool TodosIguais(ReadOnlySpan<char> digitos)
    {
        if (digitos.IsEmpty) return false;
        char primeiro = digitos[0];
        for (int i = 1; i < digitos.Length; i++)
        {
            if (digitos[i] != primeiro) return false;
        }
        return true;
    }

    public static bool SaoTodosDigitos(ReadOnlySpan<char> valor)
    {
        foreach (char c in valor)
        {
            if (c < '0' || c > '9') return false;
        }
        return true;
    }
}
