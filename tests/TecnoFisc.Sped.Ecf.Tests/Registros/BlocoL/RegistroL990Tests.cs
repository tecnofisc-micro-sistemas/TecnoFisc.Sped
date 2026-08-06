using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoL;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoL;

public sealed class RegistroL990Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroL990(), "L990", "1:1");
    }

    [Fact]
    public void Parser_LeQuantidadeDeLinhas()
    {
        var resultado = new ParserEcf().ParseLinha("|L990|7|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroL990>()
            .Which.QtdLin.Should().Be(7);
    }

    [Fact]
    public void Parser_QuantidadeInvalida_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|L990|INVALIDA|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroL990>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == nameof(RegistroL990.QtdLin) && erro.ValorBruto == "INVALIDA");
    }

    [Fact]
    public async Task ReadAsync_FixtureCompleta_MaterializaOrdemRoteamentoHierarquiaEValores()
    {
        byte[] bytes = await LerFixtureAsync();
        await using var entrada = new MemoryStream(bytes, writable: false);

        var arquivo = await new ParserEcf().ReadAsync(
            entrada,
            TestContext.Current.CancellationToken);

        arquivo.BlocoL.Registros.Select(registro => registro.Codigo).Should().Equal(
            "L001", "L030", "L100", "L200", "L210", "L300", "L990");
        arquivo.EnumerarRegistros().Select(registro => registro.Codigo).Should().Equal(
            "0000", "L001", "L030", "L100", "L200", "L210", "L300", "L990");

        var r0000 = arquivo.Bloco0.Registros.Should().ContainSingle()
            .Which.Should().BeOfType<Registro0000>().Which;
        var registros = arquivo.BlocoL.Registros;
        var l001 = (RegistroL001)registros[0];
        var l030 = (RegistroL030)registros[1];
        var l100 = (RegistroL100)registros[2];
        var l200 = (RegistroL200)registros[3];
        var l210 = (RegistroL210)registros[4];
        var l300 = (RegistroL300)registros[5];
        var l990 = (RegistroL990)registros[6];

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, l001, l990);
        AssertRegistroEcf.ConformsToManifest(l001, "L001", "1:1", r0000, l030);
        AssertRegistroEcf.ConformsToManifest(l030, "L030", "0:13", l001, l100, l200, l210, l300);
        AssertRegistroEcf.ConformsToManifest(l100, "L100", "0:N", l030);
        AssertRegistroEcf.ConformsToManifest(l200, "L200", "0:13", l030);
        AssertRegistroEcf.ConformsToManifest(l210, "L210", "0:N", l030);
        AssertRegistroEcf.ConformsToManifest(l300, "L300", "0:N", l030);
        AssertRegistroEcf.ConformsToManifest(l990, "L990", "1:1", r0000);

        l001.IndDad.Should().Be(IndicadorMovimentoBloco.ComDados);
        l030.PerApur.Should().Be("T01");
        l100.CampoCodigo.Should().Be("02.03.04.01.99");
        l100.ValCtaRefDeb.Should().Be(5000m);
        l100.ValCtaRefFin.Should().Be("00020000,00");
        l200.IndAvalEstoq.Should().Be(MetodoAvaliacaoEstoque.Peps);
        l210.CampoCodigo.Should().Be("0092");
        l210.Valor.Should().Be("00001000,00");
        l300.CodNat.Should().Be("04");
        l300.Valor.Should().Be(10000m);
        l990.QtdLin.Should().Be(7);
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
        relido.BlocoL.Registros[2].Should().BeOfType<RegistroL100>()
            .Which.ValCtaRefFin.Should().Be("00020000,00");
        relido.BlocoL.Registros[4].Should().BeOfType<RegistroL210>()
            .Which.Valor.Should().Be("00001000,00");
        relido.BlocoL.Registros[5].Should().BeOfType<RegistroL300>()
            .Which.Valor.Should().Be(10000m);
    }

    private static async Task<byte[]> LerFixtureAsync()
    {
        string caminho = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "Sinteticas",
            "bloco-l.txt");
        byte[] bytes = await File.ReadAllBytesAsync(
            caminho,
            TestContext.Current.CancellationToken);
        bytes.Should().NotBeEmpty();
        bytes[^1].Should().Be(0x0A);
        bytes.Should().NotContain(0x0D);
        EncodingSped.Latin1.GetBytes(EncodingSped.Latin1.GetString(bytes)).Should().Equal(bytes);
        return bytes;
    }
}
