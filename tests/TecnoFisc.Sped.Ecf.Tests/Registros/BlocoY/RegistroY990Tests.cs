using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoY;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoY.Lote2;

public sealed class RegistroY990Tests
{
    private static readonly string[] OrdemBlocoY = [
        "Y001", "Y520", "Y570", "Y590", "Y600", "Y612", "Y620", "Y630", "Y640", "Y650",
        "Y660", "Y672", "Y680", "Y681", "Y682", "Y720", "Y730", "Y750", "Y800", "Y990",
    ];

    [Fact]
    public void Catalogo_ImplementaRegistroY990()
    {
        AssertRegistroEcf.CodesAreImplemented("Y990");
    }

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroY990(), "Y990", "1:1");
    }

    [Fact]
    public void Parser_LeQuantidadeExataDeRegistrosDoBloco()
    {
        var resultado = new ParserEcf().ParseLinha("|Y990|20|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroY990>().Which.QtdLin.Should().Be(20);
    }

    [Fact]
    public void Parser_QuantidadeInvalida_RegistraErroDeFormato()
    {
        var registro = new ParserEcf().ParseLinha("|Y990|INVALIDA|").Valor
            .Should().BeOfType<RegistroY990>().Which;

        registro.ErrosDeFormato.Should().ContainSingle(erro => erro.Campo == "QTD_LIN");
    }

    [Fact]
    public async Task ReadAsync_FixtureCompleta_MaterializaOrdemRoteamentoHierarquiaEValores()
    {
        byte[] bytes = await LerFixtureAsync();
        await using var entrada = new MemoryStream(bytes, writable: false);

        var arquivo = await new ParserEcf().ReadAsync(entrada, TestContext.Current.CancellationToken);

        AssertFixtureCompleta(arquivo);
    }

    [Fact]
    public async Task Writer_FixtureCompleta_PreservaTextoCanonicoEPermiteNovoParse()
    {
        byte[] bytes = await LerFixtureAsync();
        await using var entrada = new MemoryStream(bytes, writable: false);
        var parser = new ParserEcf();
        var arquivo = await parser.ReadAsync(entrada, TestContext.Current.CancellationToken);

        await using var saida = new MemoryStream();
        await new EscritorSpedTxt(new CatalogoSpedGerado()).WriteAsync(
            saida, arquivo.EnumerarRegistros(), TestContext.Current.CancellationToken);

        string esperado = EncodingSped.Latin1.GetString(bytes).Replace("\n", "\r\n", StringComparison.Ordinal);
        EncodingSped.Latin1.GetString(saida.ToArray()).Should().Be(esperado);

        saida.Position = 0;
        var relido = await parser.ReadAsync(saida, TestContext.Current.CancellationToken);
        AssertFixtureCompleta(relido);
    }

    private static void AssertFixtureCompleta(ArquivoEcf arquivo)
    {
        OrdemBlocoY.Should().HaveCount(20);
        arquivo.BlocoY.Registros.Select(registro => registro.Codigo).Should().Equal(OrdemBlocoY);
        arquivo.EnumerarRegistros().Select(registro => registro.Codigo)
            .Should().Equal(["0000", .. OrdemBlocoY]);

        var r0000 = arquivo.Bloco0.Registros.Should().ContainSingle()
            .Which.Should().BeOfType<Registro0000>().Which;
        var registros = arquivo.BlocoY.Registros;
        var y001 = (RegistroY001)registros[0];
        var y520 = (RegistroY520)registros[1];
        var y570 = (RegistroY570)registros[2];
        var y590 = (RegistroY590)registros[3];
        var y600 = (RegistroY600)registros[4];
        var y612 = (RegistroY612)registros[5];
        var y620 = (RegistroY620)registros[6];
        var y630 = (RegistroY630)registros[7];
        var y640 = (RegistroY640)registros[8];
        var y650 = (RegistroY650)registros[9];
        var y660 = (RegistroY660)registros[10];
        var y672 = (RegistroY672)registros[11];
        var y680 = (RegistroY680)registros[12];
        var y681 = (RegistroY681)registros[13];
        var y682 = (RegistroY682)registros[14];
        var y720 = (RegistroY720)registros[15];
        var y730 = (RegistroY730)registros[16];
        var y750 = (RegistroY750)registros[17];
        var y800 = (RegistroY800)registros[18];
        var y990 = (RegistroY990)registros[19];

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, y001, y990);
        AssertRegistroEcf.ConformsToManifest(
            y001, "Y001", "1:1", r0000,
            y520, y570, y590, y600, y612, y620, y630, y640, y660, y672,
            y680, y682, y720, y730, y750, y800);
        AssertRegistroEcf.ConformsToManifest(y520, "Y520", "0:N", y001);
        AssertRegistroEcf.ConformsToManifest(y570, "Y570", "0:N", y001);
        AssertRegistroEcf.ConformsToManifest(y590, "Y590", "0:N", y001);
        AssertRegistroEcf.ConformsToManifest(y600, "Y600", "0:N", y001);
        AssertRegistroEcf.ConformsToManifest(y612, "Y612", "0:N", y001);
        AssertRegistroEcf.ConformsToManifest(y620, "Y620", "0:N", y001);
        AssertRegistroEcf.ConformsToManifest(y630, "Y630", "0:N", y001);
        AssertRegistroEcf.ConformsToManifest(y640, "Y640", "0:N", y001, y650);
        AssertRegistroEcf.ConformsToManifest(y650, "Y650", "0:N", y640);
        AssertRegistroEcf.ConformsToManifest(y660, "Y660", "0:N", y001);
        AssertRegistroEcf.ConformsToManifest(y672, "Y672", "0:1", y001);
        AssertRegistroEcf.ConformsToManifest(y680, "Y680", "0:12", y001, y681);
        AssertRegistroEcf.ConformsToManifest(y681, "Y681", "0:N", y680);
        AssertRegistroEcf.ConformsToManifest(y682, "Y682", "0:12", y001);
        AssertRegistroEcf.ConformsToManifest(y720, "Y720", "0:1", y001);
        AssertRegistroEcf.ConformsToManifest(y730, "Y730", "0:N", y001);
        AssertRegistroEcf.ConformsToManifest(y750, "Y750", "0:N", y001);
        AssertRegistroEcf.ConformsToManifest(y800, "Y800", "0:N", y001);
        AssertRegistroEcf.ConformsToManifest(y990, "Y990", "1:1", r0000);

        y600.CpfCnpj.Should().Be("00000000000");
        y650.Pai.Should().BeSameAs(y640);
        y660.PercPatLiq.Should().Be(40.1250m);
        y672.IndAvalEstoq.Should().Be(MetodoAvaliacaoEstoque.Peps);
        y681.CampoCodigo.Should().Be("000001");
        y682.AcresPatr.Should().Be(-200000m);
        y720.DtLucLiq.Should().Be(new DateOnly(2021, 12, 31));
        y730.Destinatario.Should().Be("00394460000141");
        y750.Valor.Should().Be("R$ 1.234,56");
        y800.ArqRtf.Should().Be("{\\rtf1\\ansi CONTEUDO SINTETICO}");
        y990.QtdLin.Should().Be(20);
    }

    private static async Task<byte[]> LerFixtureAsync()
    {
        string caminho = Path.Combine(AppContext.BaseDirectory, "Fixtures", "Sinteticas", "bloco-y.txt");
        byte[] bytes = await File.ReadAllBytesAsync(caminho, TestContext.Current.CancellationToken);
        bytes.Should().NotBeEmpty();
        bytes[^1].Should().Be(0x0A);
        bytes.Should().NotContain(0x0D);
        bytes.Should().OnlyContain(valor => valor <= 0x7F, "a fixture ASCII é um subconjunto byte-estável de Latin1");
        EncodingSped.Latin1.GetBytes(EncodingSped.Latin1.GetString(bytes)).Should().Equal(bytes);
        EncodingSped.Latin1.GetString(bytes).Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Should().HaveCount(21);
        return bytes;
    }
}
