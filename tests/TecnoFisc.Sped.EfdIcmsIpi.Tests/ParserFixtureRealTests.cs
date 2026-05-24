using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests;

/// <summary>
/// Parse end-to-end contra arquivos SPED reais armazenados em <c>sped/fixtures/</c>
/// (gitignored). Os arquivos são emitidos pelo PVA da Receita e podem trazer assinatura digital
/// PKCS#7 anexada após o último registro <c>9999</c> — a parte binária é ignorada pelo parser,
/// que encerra ao encontrar o <c>9999</c>.
///
/// Pula automaticamente quando nenhuma fixture EFD ICMS-IPI estiver presente (CI público,
/// máquina sem arquivo de cliente). Fixtures de outros leiautes (EFD Contribuições) são
/// distinguidas pelo cabeçalho do <c>Registro0000</c> e ignoradas.
///
/// Pacote EFD ICMS-IPI é read-only (ARCHITECTURE §2.5) — não há teste de round-trip
/// parse → generate.
/// </summary>
public sealed class ParserFixtureRealTests
{
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

            var registros = new List<RegistroSped>();
            await foreach (var registro in parser.LerStreamingAsync(entrada, TestContext.Current.CancellationToken))
                registros.Add(registro);

            registros.Should().NotBeEmpty(because: $"fixture {Path.GetFileName(caminhoFixture)} deve produzir registros");
            registros[^1].Codigo.Should().Be("9999",
                because: $"parser deve encerrar no |9999| de {Path.GetFileName(caminhoFixture)}, ignorando bloco de assinatura PKCS#7 anexo");
        }
    }

    [Fact]
    public async Task ParserMaterializaUmRegistroPorLinhaSpedDoArquivoOriginal()
    {
        var fixtures = EnumerarFixturesEfdIcmsIpi().ToList();
        if (fixtures.Count == 0)
        {
            Assert.Skip("Nenhuma fixture EFD ICMS-IPI presente em sped/fixtures/. Adicione um arquivo SPED real para exercitar o parser.");
            return;
        }

        var parser = new ParserEfdIcmsIpi();

        foreach (var caminhoFixture in fixtures)
        {
            var bytesOriginais = ExtrairPorcaoTextual(
                await File.ReadAllBytesAsync(caminhoFixture, TestContext.Current.CancellationToken));

            using var entrada = new MemoryStream(bytesOriginais, writable: false);
            var arquivo = await ArquivoEfdIcmsIpi.CarregarAsync(
                parser.LerStreamingAsync(entrada, TestContext.Current.CancellationToken),
                TestContext.Current.CancellationToken);

            var totalRegistros = arquivo.EnumerarRegistros().Count();
            var linhasOriginais = ContarLinhasNaoVazias(bytesOriginais);

            totalRegistros.Should().Be(linhasOriginais,
                because: $"parser deve materializar uma instância por linha SPED ({linhasOriginais} linhas no original — {Path.GetFileName(caminhoFixture)})");
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
}
