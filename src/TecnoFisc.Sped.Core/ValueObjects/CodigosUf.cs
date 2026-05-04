namespace TecnoFisc.Sped.Core.ValueObjects;

/// <summary>
/// Códigos IBGE de UF aceitos como prefixo da chave de acesso de DF-e.
/// Inclui o código 91 reservado para o Ambiente Nacional (RFB).
/// </summary>
internal static class CodigosUf
{
    // Códigos IBGE oficiais para as 27 UFs (26 estados + DF) e 91 = Ambiente Nacional.
    private static readonly int[] Validos =
    [
        11, 12, 13, 14, 15, 16, 17,             // Norte
        21, 22, 23, 24, 25, 26, 27, 28, 29,     // Nordeste
        31, 32, 33, 35,                         // Sudeste
        41, 42, 43,                             // Sul
        50, 51, 52, 53,                         // Centro-Oeste
        91,                                     // Ambiente Nacional
    ];

    public static bool EhValido(int codigoUf)
    {
        foreach (int c in Validos)
            if (c == codigoUf) return true;
        return false;
    }
}
