using System.Globalization;

namespace TecnoFisc.Sped.Txt.Engine.Gerador;

/// <summary>
/// Conversores dos tipos primitivos de volta para a representação canônica SPED.
/// Stage 2 ainda não inclui o <c>EscritorSpedTxt</c>; estes helpers já existem para
/// permitir o invariante de round-trip ser exercitado nos testes (parse → serializar
/// → comparar). O escritor completo é Stage 3.
/// </summary>
public static class SerializadoresPrimitivos
{
    public static string Inteiro(int valor)
        => valor.ToString(CultureInfo.InvariantCulture);

    public static string Longo(long valor)
        => valor.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Serializa um decimal para a forma SPED: vírgula decimal, sem separador de milhar,
    /// quantidade fixa de casas decimais conforme o layout.
    /// </summary>
    public static string DeDecimal(decimal valor, int decimais)
    {
        string formato = decimais <= 0
            ? "0"
            : "0." + new string('0', decimais);
        string texto = valor.ToString(formato, CultureInfo.InvariantCulture);
        return texto.Replace('.', ',');
    }

    /// <summary>Data no formato "ddMMyyyy" (padrão SPED para campos diários).</summary>
    public static string Data(DateOnly valor)
        => valor.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

    public static string DataComFormato(DateOnly valor, string? formato)
        => string.IsNullOrEmpty(formato)
            ? Data(valor)
            : valor.ToString(formato, CultureInfo.InvariantCulture);
}
