using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoQ;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoQ;

public sealed class RegistroQ990Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroQ990(), "Q990", "1:1");
    }

    [Fact]
    public void Parser_LeQuantidadeExataDeLinhasDoBloco()
    {
        var resultado = new ParserEcf().ParseLinha("|Q990|6|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroQ990>()
            .Which.QtdLin.Should().Be(6);
    }

    [Fact]
    public void Parser_QuantidadeInvalida_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|Q990|INVALIDA|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroQ990>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == nameof(RegistroQ990.QtdLin) && erro.ValorBruto == "INVALIDA");
    }

    [Fact]
    public async Task ReadAsync_FixtureCompleta_MaterializaOrdemRoteamentoHierarquiaEValores()
    {
        byte[] bytes = await LerFixtureAsync();
        await using var entrada = new MemoryStream(bytes, writable: false);

        var arquivo = await new ParserEcf().ReadAsync(
            entrada,
            TestContext.Current.CancellationToken);

        string[] ordemBlocoQ = ["Q001", "Q100", "Q100", "Q100", "Q100", "Q990"];
        arquivo.BlocoQ.Registros.Select(registro => registro.Codigo).Should().Equal(ordemBlocoQ);
        arquivo.EnumerarRegistros().Select(registro => registro.Codigo)
            .Should().Equal(["0000", .. ordemBlocoQ]);

        var r0000 = arquivo.Bloco0.Registros.Should().ContainSingle()
            .Which.Should().BeOfType<Registro0000>().Which;
        var registros = arquivo.BlocoQ.Registros;
        var q001 = (RegistroQ001)registros[0];
        var q100Saldo = (RegistroQ100)registros[1];
        var q100Entrada = (RegistroQ100)registros[2];
        var q100Saida = (RegistroQ100)registros[3];
        var q100Negativo = (RegistroQ100)registros[4];
        var q990 = (RegistroQ990)registros[5];

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, q001, q990);
        AssertRegistroEcf.ConformsToManifest(
            q001,
            "Q001",
            "1:1",
            r0000,
            q100Saldo,
            q100Entrada,
            q100Saida,
            q100Negativo);
        AssertRegistroEcf.ConformsToManifest(q100Saldo, "Q100", "0:N", q001);
        AssertRegistroEcf.ConformsToManifest(q100Entrada, "Q100", "0:N", q001);
        AssertRegistroEcf.ConformsToManifest(q100Saida, "Q100", "0:N", q001);
        AssertRegistroEcf.ConformsToManifest(q100Negativo, "Q100", "0:N", q001);
        AssertRegistroEcf.ConformsToManifest(q990, "Q990", "1:1", r0000);

        q001.IndDad.Should().Be(IndicadorMovimentoBloco.ComDados);
        q100Saldo.Data.Should().Be(new DateOnly(2025, 1, 1));
        q100Saldo.NumDoc.Should().BeNull();
        q100Saldo.Hist.Should().Be("SALDO ANTERIOR");
        q100Saldo.VlEntrada.Should().Be(1000m);
        q100Saldo.VlSaida.Should().BeNull();
        q100Entrada.NumDoc.Should().Be("000001");
        q100Entrada.VlEntrada.Should().Be(250.75m);
        q100Saida.NumDoc.Should().Be("DOC-0002");
        q100Saida.Hist.Should().Be("PAGAMENTO DE FORNECEDOR");
        q100Saida.VlSaida.Should().Be(50.25m);
        q100Negativo.NumDoc.Should().Be("0000000003");
        q100Negativo.SldFin.Should().Be(-100.25m);
        q990.QtdLin.Should().Be(ordemBlocoQ.Length);
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
            saida,
            arquivo.EnumerarRegistros(),
            TestContext.Current.CancellationToken);

        string esperado = EncodingSped.Latin1.GetString(bytes).Replace("\n", "\r\n");
        string serializado = EncodingSped.Latin1.GetString(saida.ToArray());
        serializado.Should().Be(esperado);

        saida.Position = 0;
        var relido = await parser.ReadAsync(saida, TestContext.Current.CancellationToken);
        relido.EnumerarRegistros().Select(registro => registro.Codigo)
            .Should().Equal(arquivo.EnumerarRegistros().Select(registro => registro.Codigo));
        var registros = relido.BlocoQ.Registros;
        registros[1].Should().BeOfType<RegistroQ100>()
            .Which.Data.Should().Be(new DateOnly(2025, 1, 1));
        registros[2].Should().BeOfType<RegistroQ100>()
            .Which.NumDoc.Should().Be("000001");
        registros[3].Should().BeOfType<RegistroQ100>()
            .Which.Pai.Should().BeSameAs(registros[0]);
        registros[4].Should().BeOfType<RegistroQ100>()
            .Which.SldFin.Should().Be(-100.25m);
        registros[5].Should().BeOfType<RegistroQ990>()
            .Which.QtdLin.Should().Be(6);
    }

    private static async Task<byte[]> LerFixtureAsync()
    {
        string caminho = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "Sinteticas",
            "bloco-q.txt");
        byte[] bytes = await File.ReadAllBytesAsync(caminho, TestContext.Current.CancellationToken);
        bytes.Should().NotBeEmpty();
        bytes[^1].Should().Be(0x0A);
        bytes.Should().NotContain(0x0D);
        EncodingSped.Latin1.GetBytes(EncodingSped.Latin1.GetString(bytes)).Should().Equal(bytes);
        return bytes;
    }
}
