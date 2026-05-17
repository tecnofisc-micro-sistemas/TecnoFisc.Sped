using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Gerador;
using TecnoFisc.Sped.EfdIcmsIpi.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests;

/// <summary>
/// Round-trip end-to-end contra arquivos SPED reais armazenados em <c>sped/fixtures/</c>
/// (gitignored). Os arquivos são emitidos pelo PVA da Receita e podem trazer assinatura digital
/// PKCS#7 anexada após o último registro <c>9999</c> — a parte binária é descartada antes
/// do round-trip; o teste exercita apenas a porção textual de registros SPED.
///
/// Pula automaticamente quando nenhuma fixture EFD ICMS-IPI estiver presente (CI público,
/// máquina sem arquivo de cliente). Fixtures de outros leiautes (EFD Contribuições) são
/// distinguidas pelo cabeçalho do <c>Registro0000</c> e ignoradas.
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
    [Fact]
    public async Task ParserESerializadorSaoEstaveisSobReSerializacao()
    {
        var fixtures = EnumerarFixturesEfdIcmsIpi().ToList();
        if (fixtures.Count == 0)
        {
            Assert.Skip("Nenhuma fixture EFD ICMS-IPI presente em sped/fixtures/. Adicione um arquivo SPED real para exercitar o round-trip.");
            return;
        }

        foreach (var caminhoFixture in fixtures)
            await ExercitarFixtureAsync(caminhoFixture);
    }

    [Fact]
    public async Task ParserAceitaArquivoComBlocoAssinaturaPKCS7Anexo()
    {
        var fixtures = EnumerarFixturesEfdIcmsIpi().ToList();
        if (fixtures.Count == 0)
        {
            Assert.Skip("Nenhuma fixture EFD ICMS-IPI presente em sped/fixtures/.");
            return;
        }

        var parser = new ParserEfdIcmsIpi();

        foreach (var caminhoFixture in fixtures)
        {
            var bytesCrus = await File.ReadAllBytesAsync(caminhoFixture, TestContext.Current.CancellationToken);
            using var entrada = new MemoryStream(bytesCrus, writable: false);

            var registros = new List<TecnoFisc.Sped.Core.Abstracoes.RegistroSped>();
            await foreach (var registro in parser.LerStreamingAsync(entrada, TestContext.Current.CancellationToken))
                registros.Add(registro);

            registros.Should().NotBeEmpty(because: $"fixture {Path.GetFileName(caminhoFixture)} deve produzir registros");
            registros[^1].Codigo.Should().Be("9999",
                because: $"parser deve encerrar no |9999| de {Path.GetFileName(caminhoFixture)}, ignorando bloco de assinatura PKCS#7 anexo");
        }
    }

    private static IEnumerable<string> EnumerarFixturesEfdIcmsIpi()
    {
        var diretorio = LocalizarPastaFixtures();
        if (diretorio is null)
            return [];

        return Directory.EnumerateFiles(diretorio, "*.txt", SearchOption.TopDirectoryOnly)
            .Where(EhFixtureEfdIcmsIpi);
    }

    /// <summary>
    /// Discrimina EFD ICMS-IPI de outros leiautes pelo <c>Registro0000</c>: o segundo campo
    /// (logo após <c>|0000|</c>) é o código de versão do leiaute. EFD ICMS-IPI usa códigos
    /// numéricos (<c>015</c>, <c>016</c>, …); EFD Contribuições usa <c>006</c> em layout
    /// diferente (segundo campo é o código da operação, depois CNPJ etc.). Heurística simples:
    /// se a contagem de campos do <c>0000</c> casar com EFD ICMS-IPI (15 campos delimitados =
    /// 16 pipes) tratamos como nosso; senão, ignora.
    /// </summary>
    private static bool EhFixtureEfdIcmsIpi(string caminho)
    {
        using var fs = File.OpenRead(caminho);
        using var leitor = new StreamReader(fs, EncodingSped.Latin1);
        var primeiraLinha = leitor.ReadLine();
        if (string.IsNullOrEmpty(primeiraLinha) || !primeiraLinha.StartsWith("|0000|", StringComparison.Ordinal))
            return false;

        // |0000|COD_VER|COD_FIN|DT_INI|DT_FIN|NOME|CNPJ|CPF|UF|IE|COD_MUN|IM|SUFRAMA|IND_PERFIL|IND_ATIV|
        // 16 pipes total no leiaute EFD ICMS-IPI. EFD Contribuições tem layout muito diferente
        // (campo 14 = "VL_CONS_NEAJ" etc., totalizando mais pipes), e o segundo campo lá é
        // "0" ou "1" (código de operação) e não "015"/"016".
        var totalPipes = primeiraLinha.Count(c => c == '|');
        if (totalPipes != 16)
            return false;

        var campos = primeiraLinha.Split('|');
        // campos[0] = "" (antes do primeiro pipe), campos[1] = "0000", campos[2] = COD_VER.
        var codVer = campos[2];
        return codVer is "014" or "015" or "016" or "017" or "018" or "019" or "020" or "021" or "022";
    }

    private static async Task ExercitarFixtureAsync(string caminhoFixture)
    {
        var bytesOriginais = ExtrairPorcaoTextual(
            await File.ReadAllBytesAsync(caminhoFixture, TestContext.Current.CancellationToken));

        var parser = new ParserEfdIcmsIpi();
        var gerador = new GeradorEfdIcmsIpi();

        var arquivo1 = await CarregarAsync(parser, bytesOriginais);
        var bytesGerados = await SerializarAsync(gerador, arquivo1);

        var arquivo2 = await CarregarAsync(parser, bytesGerados);
        var bytesDuplos = await SerializarAsync(gerador, arquivo2);

        var codigos1 = arquivo1.EnumerarRegistros().Select(r => r.Codigo).ToList();
        var codigos2 = arquivo2.EnumerarRegistros().Select(r => r.Codigo).ToList();
        codigos2.Should().Equal(codigos1, because: "segundo parse deve produzir a mesma sequência de registros");

        if (!bytesGerados.AsSpan().SequenceEqual(bytesDuplos))
        {
            var (linha, esperada, obtida) = LocalizarPrimeiraDivergencia(bytesGerados, bytesDuplos);
            throw new Xunit.Sdk.XunitException(
                $"Serialização não é idempotente — divergência na linha {linha}.\n" +
                $"  Primeira passagem: {esperada}\n" +
                $"  Segunda  passagem: {obtida}\n" +
                $"  Bytes 1ª: {bytesGerados.Length}, 2ª: {bytesDuplos.Length}.");
        }

        var linhasOriginais = ContarLinhasNaoVazias(bytesOriginais);
        codigos1.Should().HaveCount(linhasOriginais,
            because: $"parser deve materializar uma instância por linha SPED ({linhasOriginais} linhas no original — {Path.GetFileName(caminhoFixture)})");
    }

    private static async Task<ArquivoEfdIcmsIpi> CarregarAsync(
        ParserEfdIcmsIpi parser, byte[] bytes)
    {
        using var entrada = new MemoryStream(bytes, writable: false);
        return await ArquivoEfdIcmsIpi.CarregarAsync(
            parser.LerStreamingAsync(entrada, TestContext.Current.CancellationToken),
            TestContext.Current.CancellationToken);
    }

    private static async Task<byte[]> SerializarAsync(
        GeradorEfdIcmsIpi gerador, ArquivoEfdIcmsIpi arquivo)
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
