using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoK;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoK;

public sealed class RegistroK990Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroK990(), "K990", "1:1");
    }

    [Fact]
    public void Parser_LeQuantidadeDeLinhas()
    {
        var resultado = new ParserEcf().ParseLinha("|K990|9|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroK990>()
            .Which.QtdLin.Should().Be(9);
    }

    [Fact]
    public void Parser_QuantidadeInvalida_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|K990|INVALIDA|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroK990>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == nameof(RegistroK990.QtdLin) && erro.ValorBruto == "INVALIDA");
    }

    [Fact]
    public async Task ReadAsync_FixtureCompleta_MaterializaOrdemRoteamentoHierarquiaEValores()
    {
        byte[] bytes = await LerFixtureAsync();
        await using var entrada = new MemoryStream(bytes, writable: false);

        var arquivo = await new ParserEcf().ReadAsync(
            entrada,
            TestContext.Current.CancellationToken);

        arquivo.BlocoK.Registros.Select(registro => registro.Codigo).Should().Equal(
            "K001", "K030", "K155", "K156", "K355", "K356", "K915", "K935", "K990");
        arquivo.EnumerarRegistros().Select(registro => registro.Codigo).Should().Equal(
            "0000", "K001", "K030", "K155", "K156", "K355", "K356", "K915", "K935", "K990");

        var r0000 = arquivo.Bloco0.Registros.Should().ContainSingle()
            .Which.Should().BeOfType<Registro0000>().Which;
        var registros = arquivo.BlocoK.Registros;
        var k001 = (RegistroK001)registros[0];
        var k030 = (RegistroK030)registros[1];
        var k155 = (RegistroK155)registros[2];
        var k156 = (RegistroK156)registros[3];
        var k355 = (RegistroK355)registros[4];
        var k356 = (RegistroK356)registros[5];
        var k915 = (RegistroK915)registros[6];
        var k935 = (RegistroK935)registros[7];
        var k990 = (RegistroK990)registros[8];

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, k001, k990);
        AssertRegistroEcf.ConformsToManifest(k001, "K001", "1:1", r0000, k030, k915, k935);
        AssertRegistroEcf.ConformsToManifest(k030, "K030", "0:13", k001, k155, k355);
        AssertRegistroEcf.ConformsToManifest(k155, "K155", "0:N", k030, k156);
        AssertRegistroEcf.ConformsToManifest(k156, "K156", "1:N", k155);
        AssertRegistroEcf.ConformsToManifest(k355, "K355", "0:N", k030, k356);
        AssertRegistroEcf.ConformsToManifest(k356, "K356", "1:N", k355);
        AssertRegistroEcf.ConformsToManifest(k915, "K915", "1:N", k001);
        AssertRegistroEcf.ConformsToManifest(k935, "K935", "1:N", k001);
        AssertRegistroEcf.ConformsToManifest(k990, "K990", "1:1", r0000);

        k001.IndDad.Should().Be(IndicadorMovimentoBloco.ComDados);
        k030.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        k030.DtFin.Should().Be(new DateOnly(2025, 3, 31));
        k030.PerApur.Should().Be("T01");
        k155.CodCta.Should().Be("0001.01.01.001");
        k155.VlSldFin.Should().Be(2500m);
        k156.CodCtaRef.Should().Be("1.01.01.01.01");
        k156.IndVlSldFin.Should().Be(IndicadorDebitoCredito.Devedor);
        k355.CodCcus.Should().BeNull();
        k355.VlSldFin.Should().Be(5000m);
        k356.CodCtaRef.Should().Be("3.01.01.01.01.01");
        k915.IdRegra.Should().Be("REGRA_COMPATIBILIDADE_K155_E155");
        k915.Justificativa.Should().Be("JUSTIFICATIVA SINTETICA PARA DIVERGENCIA");
        k935.IdRegra.Should().Be("REGRA_COMPATIBILIDADE_K355_E355");
        k935.Justificativa.Should().Be("JUSTIFICATIVA SINTETICA PARA DIVERGENCIA");
        k990.QtdLin.Should().Be(9);
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
        relido.BlocoK.Registros[1].Should().BeOfType<RegistroK030>()
            .Which.DtFin.Should().Be(new DateOnly(2025, 3, 31));
        relido.BlocoK.Registros[3].Should().BeOfType<RegistroK156>()
            .Which.CodCtaRef.Should().Be("1.01.01.01.01");
        relido.BlocoK.Registros[6].Should().BeOfType<RegistroK915>()
            .Which.SldFinPre.Should().Be(40m);
    }

    private static async Task<byte[]> LerFixtureAsync()
    {
        string caminho = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "Sinteticas",
            "bloco-k.txt");
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
