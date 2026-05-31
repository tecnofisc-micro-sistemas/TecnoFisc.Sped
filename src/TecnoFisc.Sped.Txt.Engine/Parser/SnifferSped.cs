using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Txt.Engine.Parser;

/// <summary>
/// Sniffer do mundo SPED-TXT. Identifica o projeto a partir da primeira linha nao vazia
/// <c>|0000|...|</c>, sem materializar registros.
/// </summary>
public static class SnifferSped
{
    public static async ValueTask<MetadadosArquivoSped> IdentificarAsync(
        Stream entrada,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);

        long? posicaoInicial = entrada.CanSeek ? entrada.Position : null;
        try
        {
            string? primeiraLinha = await ReadPrimeiraLinhaNaoVaziaAsync(entrada, cancelamento)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(primeiraLinha))
                return Desconhecido(string.Empty, null);

            return Classificar(primeiraLinha);
        }
        finally
        {
            if (posicaoInicial is long posicao)
                entrada.Position = posicao;
        }
    }

    public static async ValueTask<ILeitorSped> AbrirParserAsync(
        Stream entrada,
        IReadOnlyDictionary<ProjetoSped, Func<ILeitorSped>> fabricas,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(entrada);
        ArgumentNullException.ThrowIfNull(fabricas);

        if (!entrada.CanSeek)
            throw new NotSupportedException("AbrirParserAsync requer stream seekable para reposicionar antes da leitura.");

        long origem = entrada.Position;
        var metadados = await IdentificarAsync(entrada, cancelamento).ConfigureAwait(false);
        entrada.Position = origem;

        if (!fabricas.TryGetValue(metadados.Projeto, out var fabrica))
            throw new NotSupportedException($"Nenhum parser registrado para o projeto SPED '{metadados.Projeto}'.");

        return fabrica();
    }

    private static MetadadosArquivoSped Classificar(string linha)
    {
        if (!linha.StartsWith("|0000|", StringComparison.Ordinal))
            return Desconhecido(linha, null);

        var campos = linha.Split('|');
        if (campos.Length < 4 || campos[1] != "0000")
            return Desconhecido(linha, null);

        string discriminador = campos[2];
        if (discriminador == "LECD")
            return new MetadadosArquivoSped(ProjetoSped.Ecd, 9, EncodingSped.Latin1, linha, discriminador);

        if (!int.TryParse(discriminador, out int versao))
            return Desconhecido(linha, discriminador);

        int pipes = linha.Count(c => c == '|');
        return discriminador switch
        {
            "001" or "002" or "003" or "004" or "005" or "006" when pipes == 15
                => new MetadadosArquivoSped(ProjetoSped.EfdContribuicoes, versao, EncodingSped.Latin1, linha, discriminador),

            "015" or "016" or "017" or "018" or "019" or "020" when pipes == 16
                => new MetadadosArquivoSped(ProjetoSped.EfdIcmsIpi, versao, EncodingSped.Latin1, linha, discriminador),

            _ => Desconhecido(linha, discriminador),
        };
    }

    private static MetadadosArquivoSped Desconhecido(string primeiraLinha, string? codigoVersaoDeclarado)
        => new(ProjetoSped.Desconhecido, 0, EncodingSped.Latin1, primeiraLinha, codigoVersaoDeclarado);

    private static async ValueTask<string?> ReadPrimeiraLinhaNaoVaziaAsync(
        Stream entrada,
        CancellationToken cancelamento)
    {
        using var linha = new MemoryStream();
        var buffer = new byte[1];

        while (await entrada.ReadAsync(buffer, cancelamento).ConfigureAwait(false) == 1)
        {
            byte b = buffer[0];
            if (b == EncodingSped.LfAscii)
            {
                string texto = DecodificarLinha(linha);
                if (texto.Length > 0)
                    return texto;

                linha.SetLength(0);
                continue;
            }

            linha.WriteByte(b);
        }

        if (linha.Length == 0)
            return null;

        string ultima = DecodificarLinha(linha);
        return ultima.Length == 0 ? null : ultima;
    }

    private static string DecodificarLinha(MemoryStream linha)
    {
        var bytes = linha.ToArray();
        int length = bytes.Length;
        if (length > 0 && bytes[length - 1] == EncodingSped.CrAscii)
            length--;

        return length == 0 ? string.Empty : EncodingSped.Latin1.GetString(bytes, 0, length);
    }
}
