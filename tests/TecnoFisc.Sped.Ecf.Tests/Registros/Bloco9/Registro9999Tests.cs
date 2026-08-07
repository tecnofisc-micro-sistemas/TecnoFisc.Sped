using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.Bloco9;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.Bloco9;

public sealed class Registro9999Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistro9999()
    {
        AssertRegistroEcf.CodesAreImplemented("9999");
    }

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new Registro9999(), "9999", "1:1");
    }

    [Fact]
    public void Parser_LeQuantidadeDeLinhasDoArquivo()
    {
        var resultado = new ParserEcf().ParseLinha("|9999|11|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<Registro9999>().Which.QtdLin.Should().Be(11);
    }

    [Fact]
    public void Parser_QuantidadeInvalida_RegistraErroDeFormato()
    {
        var registro = new ParserEcf().ParseLinha("|9999|INVALIDA|").Valor
            .Should().BeOfType<Registro9999>().Which;

        registro.ErrosDeFormato.Should().ContainSingle(erro =>
            erro.Campo == "QTD_LIN" && erro.ValorBruto == "INVALIDA");
    }

    [Fact]
    public async Task ReadAsync_FixtureCompleta_MaterializaContagensOrdemRoteamentoHierarquiaEValores()
    {
        byte[] bytes = await LerFixtureAsync();
        await using var entrada = new MemoryStream(bytes, writable: false);

        var arquivo = await new ParserEcf().ReadAsync(entrada, TestContext.Current.CancellationToken);

        AssertFixtureCompleta(arquivo);
    }

    [Fact]
    public async Task Writer_FixtureCompleta_ProduzCrLfCanonicoEPreservaContagensOrdemEGrafoNoNovoParse()
    {
        byte[] bytes = await LerFixtureAsync();
        await using var entrada = new MemoryStream(bytes, writable: false);
        var parser = new ParserEcf();
        var arquivo = await parser.ReadAsync(entrada, TestContext.Current.CancellationToken);

        await using var saida = new MemoryStream();
        await new EscritorSpedTxt(new CatalogoSpedGerado()).WriteAsync(
            saida,
            arquivo.EnumerarRegistros(),
            TestContext.Current.CancellationToken);

        string esperado = EncodingSped.Latin1.GetString(bytes).Replace("\n", "\r\n", StringComparison.Ordinal);
        EncodingSped.Latin1.GetString(saida.ToArray()).Should().Be(esperado);

        saida.Position = 0;
        var relido = await parser.ReadAsync(saida, TestContext.Current.CancellationToken);
        AssertFixtureCompleta(relido);
    }

    private static void AssertFixtureCompleta(ArquivoEcf arquivo)
    {
        string[] ordemBloco9 = ["9001", "9100", "9900", "9900", "9900", "9900", "9900", "9900", "9990", "9999"];
        arquivo.Bloco9.Registros.Select(registro => registro.Codigo).Should().Equal(ordemBloco9);
        arquivo.EnumerarRegistros().Select(registro => registro.Codigo)
            .Should().Equal(["0000", .. ordemBloco9]);

        var r0000 = arquivo.Bloco0.Registros.Should().ContainSingle()
            .Which.Should().BeOfType<Registro0000>().Which;
        var registros = arquivo.Bloco9.Registros;
        var r9001 = (Registro9001)registros[0];
        var r9100 = (Registro9100)registros[1];
        Registro9900[] r9900 = registros.Skip(2).Take(6).Cast<Registro9900>().ToArray();
        var r9990 = (Registro9990)registros[8];
        var r9999 = (Registro9999)registros[9];

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, r9001, r9990, r9999);
        AssertRegistroEcf.ConformsToManifest(r9001, "9001", "1:1", r0000, r9100, r9900[0], r9900[1], r9900[2], r9900[3], r9900[4], r9900[5]);
        AssertRegistroEcf.ConformsToManifest(r9100, "9100", "0:N", r9001);
        foreach (Registro9900 totalizador in r9900)
            AssertRegistroEcf.ConformsToManifest(totalizador, "9900", "0:N", r9001);
        AssertRegistroEcf.ConformsToManifest(r9990, "9990", "1:1", r0000);
        AssertRegistroEcf.ConformsToManifest(r9999, "9999", "1:1", r0000);

        r9001.IndDad.Should().Be(IndicadorMovimentoBloco.ComDados);
        r9100.NomRegra.Should().Be("00123");
        r9100.Conteudo.Should().Be(-100.25m);
        r9100.ValorEsperado.Should().Be(99.50m);
        r9100.CampoCodigo.Should().Be("000001");
        r9100.CnpjEstab.ToString().Should().Be("11111111000191");
        r9100.Cnae.Should().Be("0123456");

        r9900.Select(totalizador => totalizador.RegBlc).Should()
            .Equal("0000", "9001", "9100", "9900", "9990", "9999");
        r9900.ToDictionary(totalizador => totalizador.RegBlc!, totalizador => totalizador.QtdRegBlc)
            .Should().Equal(new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["0000"] = 1,
                ["9001"] = 1,
                ["9100"] = 1,
                ["9900"] = 6,
                ["9990"] = 1,
                ["9999"] = 1,
            });
        r9900.Should().OnlyContain(totalizador => totalizador.Versao == null && totalizador.IdTabDin == null);

        int linhasDoBloco9AteFechamento = Array.IndexOf(ordemBloco9, "9990") + 1;
        r9990.QtdLin.Should().Be(linhasDoBloco9AteFechamento).And.Be(9);
        r9999.QtdLin.Should().Be(arquivo.EnumerarRegistros().Count()).And.Be(11);
    }

    private static async Task<byte[]> LerFixtureAsync()
    {
        string caminho = Path.Combine(AppContext.BaseDirectory, "Fixtures", "Sinteticas", "bloco-9.txt");
        byte[] bytes = await File.ReadAllBytesAsync(caminho, TestContext.Current.CancellationToken);
        bytes.Should().NotBeEmpty();
        bytes[^1].Should().Be(0x0A);
        bytes.Should().NotContain(0x0D);
        bytes.Should().OnlyContain(valor => valor <= 0x7F, "ASCII e subconjunto byte-estavel de Latin1");
        EncodingSped.Latin1.GetBytes(EncodingSped.Latin1.GetString(bytes)).Should().Equal(bytes);
        EncodingSped.Latin1.GetString(bytes).Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Should().HaveCount(11);
        return bytes;
    }
}
