using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdContribuicoes.Gerador;
using TecnoFisc.Sped.EfdContribuicoes.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests;

/// <summary>
/// Round-trip end-to-end contra arquivos SPED reais armazenados em <c>sped/fixtures/</c>
/// (gitignored). Os arquivos são emitidos pelo PVA da Receita e contêm assinatura digital
/// PKCS#7 anexada após o último registro <c>9999</c> — a parte binária é descartada antes
/// do round-trip; o teste exercita apenas a porção textual de registros SPED.
///
/// Pula automaticamente quando nenhuma fixture estiver presente (CI público, máquina sem
/// arquivo de cliente). Quando um <c>.txt</c> existir na pasta, exercita-o por completo.
///
/// Invariante exigida: <b>parse → serialize é idempotente</b> — o segundo passo de serialização
/// produz exatamente os mesmos bytes do primeiro. Não assertamos byte-equality contra o arquivo
/// original porque o leiaute permite normalizações deliberadas (ex.: campo decimal opcional
/// emitido pelo PVA como <c>0</c> é normalizado para <c>0,00</c> conforme a coluna Dec do
/// Guia Prático). A invariante mais forte que importa para uma biblioteca SPED é estabilidade
/// sob round-trip, não fidelidade textual contra geradores externos heterogêneos.
/// </summary>
public sealed class RoundTripFixtureRealTests
{
    public static TheoryData<string> Fixtures()
    {
        var data = new TheoryData<string>();
        var diretorio = LocalizarPastaFixtures();
        if (diretorio is null)
            return data;

        foreach (var arquivo in Directory.EnumerateFiles(diretorio, "*.txt", SearchOption.TopDirectoryOnly))
            data.Add(arquivo);
        return data;
    }

    [Theory]
    [MemberData(nameof(Fixtures))]
    public async Task ParserESerializadorSaoEstaveisSobReSerializacao(string caminhoFixture)
    {
        if (string.IsNullOrEmpty(caminhoFixture) || !File.Exists(caminhoFixture))
        {
            Assert.Skip($"Fixture não encontrada: {caminhoFixture}");
            return;
        }

        var bytesOriginais = ExtrairPorcaoTextual(
            await File.ReadAllBytesAsync(caminhoFixture, TestContext.Current.CancellationToken));

        var parser = new ParserEfdContribuicoes();
        var gerador = new GeradorEfdContribuicoes();

        var arquivo1 = await CarregarAsync(parser, bytesOriginais);
        var bytesGerados = await SerializarAsync(gerador, arquivo1);

        var arquivo2 = await CarregarAsync(parser, bytesGerados);
        var bytesDuplos = await SerializarAsync(gerador, arquivo2);

        // Parser não perde registros entre os dois passes.
        var codigos1 = arquivo1.EnumerarRegistros().Select(r => r.Codigo).ToList();
        var codigos2 = arquivo2.EnumerarRegistros().Select(r => r.Codigo).ToList();
        codigos2.Should().Equal(codigos1, because: "segundo parse deve produzir a mesma sequência de registros");

        // Serialização é idempotente.
        if (!bytesGerados.AsSpan().SequenceEqual(bytesDuplos))
        {
            var (linha, esperada, obtida) = LocalizarPrimeiraDivergencia(bytesGerados, bytesDuplos);
            throw new Xunit.Sdk.XunitException(
                $"Serialização não é idempotente — divergência na linha {linha}.\n" +
                $"  Primeira passagem: {esperada}\n" +
                $"  Segunda  passagem: {obtida}\n" +
                $"  Bytes 1ª: {bytesGerados.Length}, 2ª: {bytesDuplos.Length}.");
        }

        // Quantidade de linhas SPED original deve casar com a contagem de registros (descartando
        // linhas vazias). Garante que nada foi engolido em silêncio pelo parser.
        var linhasOriginais = ContarLinhasNaoVazias(bytesOriginais);
        codigos1.Should().HaveCount(linhasOriginais,
            because: $"parser deve materializar uma instância por linha SPED ({linhasOriginais} linhas no original)");
    }

    private static async Task<ArquivoEfdContribuicoes> CarregarAsync(
        ParserEfdContribuicoes parser, byte[] bytes)
    {
        using var entrada = new MemoryStream(bytes, writable: false);
        return await ArquivoEfdContribuicoes.CarregarAsync(
            parser.LerAsync(entrada, TestContext.Current.CancellationToken),
            TestContext.Current.CancellationToken);
    }

    private static async Task<byte[]> SerializarAsync(
        GeradorEfdContribuicoes gerador, ArquivoEfdContribuicoes arquivo)
    {
        using var saida = new MemoryStream();
        await gerador.EscreverAsync(saida, arquivo.EnumerarRegistros(), TestContext.Current.CancellationToken);
        return saida.ToArray();
    }

    private static string? LocalizarPastaFixtures()
    {
        var atual = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(atual))
        {
            var candidato = Path.Combine(atual, "sped", "fixtures");
            if (Directory.Exists(candidato))
                return candidato;

            var pai = Directory.GetParent(atual)?.FullName;
            if (pai == atual)
                return null;
            atual = pai;
        }
        return null;
    }

    private static byte[] ExtrairPorcaoTextual(byte[] bytes)
    {
        ReadOnlySpan<byte> marcador = "\n|9999|"u8;
        int idx = UltimoIndiceDe(bytes, marcador);
        if (idx < 0)
            return bytes;

        int inicio9999 = idx + 1;
        int fim = Array.IndexOf(bytes, (byte)'\n', inicio9999);
        return fim < 0 ? bytes : bytes[..(fim + 1)];
    }

    private static int UltimoIndiceDe(ReadOnlySpan<byte> origem, ReadOnlySpan<byte> alvo)
    {
        for (int i = origem.Length - alvo.Length; i >= 0; i--)
        {
            if (origem.Slice(i, alvo.Length).SequenceEqual(alvo))
                return i;
        }
        return -1;
    }

    private static int ContarLinhasNaoVazias(byte[] bytes)
    {
        int total = 0;
        int inicio = 0;
        for (int i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] != (byte)'\n') continue;

            int fim = i;
            if (fim > inicio && bytes[fim - 1] == (byte)'\r') fim--;
            if (fim > inicio) total++;
            inicio = i + 1;
        }
        if (inicio < bytes.Length) total++;
        return total;
    }

    private static (int linha, string esperada, string obtida) LocalizarPrimeiraDivergencia(
        byte[] esperado, byte[] obtido)
    {
        var linhasEsperadas = EncodingSped.Latin1.GetString(esperado).Split('\n');
        var linhasObtidas = EncodingSped.Latin1.GetString(obtido).Split('\n');
        int min = Math.Min(linhasEsperadas.Length, linhasObtidas.Length);
        for (int i = 0; i < min; i++)
        {
            if (!string.Equals(linhasEsperadas[i], linhasObtidas[i], StringComparison.Ordinal))
                return (i + 1, linhasEsperadas[i], linhasObtidas[i]);
        }
        return (min + 1,
            linhasEsperadas.Length > min ? linhasEsperadas[min] : "<EOF>",
            linhasObtidas.Length > min ? linhasObtidas[min] : "<EOF>");
    }

}
