using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoJ;

namespace TecnoFisc.Sped.Ecd.Tests.Parser;

/// <summary>
/// Exercita o <see cref="ParserEcd"/> (catálogo gerado em compile-time, mesmo caminho de produção)
/// na leitura de registros multi-linha J800/J801, cujo campo <c>ARQ_RTF</c> carrega um arquivo com
/// quebras CRLF internas e ocupa várias linhas físicas. Cobre a issue #498.
/// </summary>
public sealed class ParserEcdMultilinhaTests
{
    // 0000 mínimo da ECD (campos finais omitidos — o parser ignora colunas ausentes no fim).
    private const string Cabecalho =
        "|0000|LECD|01012023|31122023|EMPRESA TESTE LTDA|11222333000181|GO|\r\n";

    private static async Task<List<RegistroSped>> LerAsync(string sped)
    {
        var parser = new ParserEcd();
        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var r in parser.ReadStreamingAsync(entrada, TestContext.Current.CancellationToken))
            registros.Add(r);
        return registros;
    }

    [Fact]
    public async Task ParserEcd_J800Multilinha_RemontaArqRtfPreservandoCrlf()
    {
        // ARQ_RTF em base64 quebrado a cada linha por CRLF, como nos arquivos reais da Receita.
        const string arqRtf =
            "e1xydGYxXGFuc2kgZG9jdW1lbnRv\r\n" +
            "dGVzdGUgY29tIHF1ZWJyYXMgZGUgbGluaGE=\r\n" +
            "ZmltIGRvIGFycXVpdm99";
        string sped = Cabecalho +
            "|J800|010|NOTAS EXPLICATIVAS|58B430489BBE825090F798E321777E07D68C68F3|" +
            arqRtf + "|J800FIM|\r\n" +
            "|9999|3|\r\n";

        var registros = await LerAsync(sped);

        var j800 = registros.OfType<RegistroJ800>().Single();
        j800.ArqRtf.Should().Be(arqRtf, because: "as quebras CRLF internas do ARQ_RTF devem ser preservadas byte-a-byte");
        j800.HashRtf.Should().Be("58B430489BBE825090F798E321777E07D68C68F3");
        j800.DescRtf.Should().Be("NOTAS EXPLICATIVAS");
        j800.IndFimRtf.Should().Be("J800FIM");
        registros[^1].Codigo.Should().Be("9999");
    }

    [Fact]
    public async Task ParserEcd_J801Multilinha_RemontaArqRtfECampos()
    {
        const string arqRtf =
            "dGVybW8gZGUgdmVyaWZpY2FjYW8=\r\n" +
            "c2VndW5kYSBsaW5oYSBkbyB0ZXJtbw==";
        string sped = Cabecalho +
            "|J801|001|TERMO VERIFICACAO|099|8CDB3CAAB92DBB3D4B3FA40B73E8C29D090650E1|" +
            arqRtf + "|J801FIM|\r\n" +
            "|9999|3|\r\n";

        var registros = await LerAsync(sped);

        var j801 = registros.OfType<RegistroJ801>().Single();
        j801.ArqRtf.Should().Be(arqRtf);
        j801.TipoDoc.Should().Be("001");
        j801.HashRtf.Should().Be("8CDB3CAAB92DBB3D4B3FA40B73E8C29D090650E1");
        j801.IndFimRtf.Should().Be("J801FIM");
    }

    [Fact]
    public async Task ParserEcd_AposRegistroMultilinha_RetomaRegistrosNormais()
    {
        // Espelha o arquivo real: J800 ×2 e J801, depois registros normais (J990/9999) retomam.
        string sped = Cabecalho +
            "|J800|010|A|HASH1|YQ==\r\nYmJi|J800FIM|\r\n" +
            "|J800|099|B|HASH2|Y2Nj\r\nZGRk|J800FIM|\r\n" +
            "|J801|001|C|099|HASH3|ZWVl\r\nZmZm|J801FIM|\r\n" +
            "|J990|6|\r\n" +
            "|9999|7|\r\n";

        var registros = await LerAsync(sped);

        registros.Select(r => r.Codigo).Should().Equal(["0000", "J800", "J800", "J801", "J990", "9999"]);
        registros.OfType<RegistroJ800>().Should().HaveCount(2);
        registros.OfType<RegistroJ800>().All(j => j.ArqRtf!.Contains("\r\n")).Should().BeTrue();
        registros.OfType<RegistroJ801>().Single().ArqRtf.Should().Be("ZWVl\r\nZmZm");
    }

    [Fact]
    public async Task ParserEcd_FixtureVersionada_LeJ800J801Multilinha()
    {
        var caminho = Path.Combine(AppContext.BaseDirectory, "Fixtures", "ecd-j800-j801-multilinha.txt");
        File.Exists(caminho).Should().BeTrue(because: "a fixture versionada deve ser copiada para a saída");

        var bytes = await File.ReadAllBytesAsync(caminho, TestContext.Current.CancellationToken);
        using var entrada = new MemoryStream(bytes, writable: false);

        var parser = new ParserEcd();
        var registros = new List<RegistroSped>();
        await foreach (var r in parser.ReadStreamingAsync(entrada, TestContext.Current.CancellationToken))
            registros.Add(r);

        registros.OfType<RegistroJ800>().Should().HaveCount(2);
        registros.OfType<RegistroJ801>().Should().HaveCount(1);
        registros[^1].Codigo.Should().Be("9999");

        foreach (var j800 in registros.OfType<RegistroJ800>())
        {
            j800.ArqRtf.Should().NotBeNullOrEmpty();
            j800.ArqRtf!.Should().Contain("\r\n", because: "o ARQ_RTF da fixture ocupa várias linhas físicas");
            j800.IndFimRtf.Should().Be("J800FIM");
        }
        registros.OfType<RegistroJ801>().Single().IndFimRtf.Should().Be("J801FIM");
    }
}
