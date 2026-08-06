using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoT;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoT;

public sealed class RegistroT990Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroT990(), "T990", "1:1");
    }

    [Fact]
    public void Parser_LeQuantidadeExataDeLinhasDoBloco()
    {
        var resultado = new ParserEcf().ParseLinha("|T990|12|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroT990>()
            .Which.QtdLin.Should().Be(12);
    }

    [Fact]
    public void Parser_QuantidadeInvalida_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|T990|INVALIDA|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroT990>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == nameof(RegistroT990.QtdLin) && erro.ValorBruto == "INVALIDA");
    }

    [Fact]
    public async Task ReadAsync_FixtureCompleta_MaterializaOrdemRoteamentoHierarquiaEValores()
    {
        byte[] bytes = await LerFixtureAsync();
        await using var entrada = new MemoryStream(bytes, writable: false);

        var arquivo = await new ParserEcf().ReadAsync(
            entrada,
            TestContext.Current.CancellationToken);

        string[] ordemBlocoT = [
            "T001", "T030", "T120", "T150", "T170", "T181",
            "T030", "T120", "T150", "T170", "T181", "T990",
        ];
        arquivo.BlocoT.Registros.Select(registro => registro.Codigo).Should().Equal(ordemBlocoT);
        arquivo.EnumerarRegistros().Select(registro => registro.Codigo)
            .Should().Equal(["0000", .. ordemBlocoT]);

        var r0000 = arquivo.Bloco0.Registros.Should().ContainSingle()
            .Which.Should().BeOfType<Registro0000>().Which;
        var registros = arquivo.BlocoT.Registros;
        var t001 = (RegistroT001)registros[0];
        var t030T01 = (RegistroT030)registros[1];
        var t120T01 = (RegistroT120)registros[2];
        var t150T01 = (RegistroT150)registros[3];
        var t170T01 = (RegistroT170)registros[4];
        var t181T01 = (RegistroT181)registros[5];
        var t030T02 = (RegistroT030)registros[6];
        var t120T02 = (RegistroT120)registros[7];
        var t150T02 = (RegistroT150)registros[8];
        var t170T02 = (RegistroT170)registros[9];
        var t181T02 = (RegistroT181)registros[10];
        var t990 = (RegistroT990)registros[11];

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, t001, t990);
        AssertRegistroEcf.ConformsToManifest(t001, "T001", "1:1", r0000, t030T01, t030T02);
        AssertRegistroEcf.ConformsToManifest(
            t030T01,
            "T030",
            "0:4",
            t001,
            t120T01,
            t150T01,
            t170T01,
            t181T01);
        AssertRegistroEcf.ConformsToManifest(t120T01, "T120", "0:N", t030T01);
        AssertRegistroEcf.ConformsToManifest(t150T01, "T150", "0:N", t030T01);
        AssertRegistroEcf.ConformsToManifest(t170T01, "T170", "0:N", t030T01);
        AssertRegistroEcf.ConformsToManifest(t181T01, "T181", "0:N", t030T01);
        AssertRegistroEcf.ConformsToManifest(
            t030T02,
            "T030",
            "0:4",
            t001,
            t120T02,
            t150T02,
            t170T02,
            t181T02);
        AssertRegistroEcf.ConformsToManifest(t120T02, "T120", "0:N", t030T02);
        AssertRegistroEcf.ConformsToManifest(t150T02, "T150", "0:N", t030T02);
        AssertRegistroEcf.ConformsToManifest(t170T02, "T170", "0:N", t030T02);
        AssertRegistroEcf.ConformsToManifest(t181T02, "T181", "0:N", t030T02);
        AssertRegistroEcf.ConformsToManifest(t990, "T990", "1:1", r0000);

        t001.IndDad.Should().Be(IndicadorMovimentoBloco.ComDados);
        t030T01.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        t030T01.PerApur.Should().Be("T01");
        t120T01.CampoCodigo.Should().Be("0026");
        t120T01.Valor.Should().Be("0001000000,00");
        t150T01.Valor.Should().Be("-100000,00");
        t170T01.Valor.Should().Be("+100000,00");
        t181T01.Valor.Should().Be("-0,00");
        t030T02.PerApur.Should().Be("T02");
        t120T02.Valor.Should().BeNull();
        t150T02.Valor.Should().Be("12,3400%");
        t170T02.Valor.Should().Be("VALOR-TABELA");
        t181T02.Descricao.Should().BeNull();
        t181T02.Valor.Should().BeNull();
        t990.QtdLin.Should().Be(ordemBlocoT.Length);
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
        var registros = relido.BlocoT.Registros;
        registros[2].Should().BeOfType<RegistroT120>()
            .Which.Pai.Should().BeSameAs(registros[1]);
        registros[5].Should().BeOfType<RegistroT181>()
            .Which.Valor.Should().Be("-0,00");
        registros[7].Should().BeOfType<RegistroT120>()
            .Which.Pai.Should().BeSameAs(registros[6]);
        registros[8].Should().BeOfType<RegistroT150>()
            .Which.Valor.Should().Be("12,3400%");
        registros[11].Should().BeOfType<RegistroT990>()
            .Which.QtdLin.Should().Be(12);
    }

    private static async Task<byte[]> LerFixtureAsync()
    {
        string caminho = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "Sinteticas",
            "bloco-t.txt");
        byte[] bytes = await File.ReadAllBytesAsync(caminho, TestContext.Current.CancellationToken);
        bytes.Should().NotBeEmpty();
        bytes[^1].Should().Be(0x0A);
        bytes.Should().NotContain(0x0D);
        EncodingSped.Latin1.GetBytes(EncodingSped.Latin1.GetString(bytes)).Should().Equal(bytes);
        return bytes;
    }
}
