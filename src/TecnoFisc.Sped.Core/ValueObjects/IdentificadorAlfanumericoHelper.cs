namespace TecnoFisc.Sped.Core.ValueObjects;

internal static class IdentificadorAlfanumericoHelper
{
    internal static bool TentarExtrair(ReadOnlySpan<char> origem, Span<char> destino, int inicioLetras, int quantidadeLetras)
    {
        int index = 0;
        foreach (char c in origem)
        {
            if (c is '.' or '-' or '/' or ' ' or '\t') continue;
            if (index >= destino.Length) return false;
            if (c is >= '0' and <= '9' || c is >= 'A' and <= 'Z' && index >= inicioLetras && index < inicioLetras + quantidadeLetras)
                destino[index++] = c;
            else return false;
        }
        return index == destino.Length;
    }
}
