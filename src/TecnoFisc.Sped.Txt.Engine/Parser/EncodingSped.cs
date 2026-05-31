using System.Text;

namespace TecnoFisc.Sped.Txt.Engine.Parser;

/// <summary>
/// Encoding canônico dos arquivos SPED textuais: Latin1 / Windows-1252 (codepage 1252).
/// Os projetos baseados em XML (NF-e, NFC-e, CT-e) usam UTF-8 por especificação
/// da Receita Federal e não passam por aqui.
/// </summary>
public static class EncodingSped
{
    private static readonly Encoding _latin1 = CarregarLatin1();

    /// <summary>Encoding para leitura/escrita de arquivos SPED .txt.</summary>
    public static Encoding Latin1 => _latin1;

    /// <summary>Delimitador de campo dentro da linha (pipe).</summary>
    public const byte PipeAscii = (byte)'|';

    /// <summary>LF (line feed) — final de linha em arquivos SPED.</summary>
    public const byte LfAscii = (byte)'\n';

    /// <summary>CR (carriage return) — pode preceder o LF em arquivos gerados em Windows.</summary>
    public const byte CrAscii = (byte)'\r';

    private static Encoding CarregarLatin1()
    {
        // Necessário registrar o provider para acessar codepages legacy em .NET Core+.
        // O encoding ISO-8859-1 (codepage 28591) é compatível com Windows-1252 nos pontos
        // ASCII e atende ao layout SPED, que não usa caracteres especiais fora do bloco
        // Latin-1.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(1252);
    }
}
