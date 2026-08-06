using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoU;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoU;

public sealed class RegistroU990Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroU990(), "U990", "1:1");
    }

    [Fact]
    public void Parser_LeQuantidadeExataDeLinhasDoBloco()
    {
        var resultado = new ParserEcf().ParseLinha("|U990|12|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroU990>()
            .Which.QtdLin.Should().Be(12);
    }

    [Fact]
    public void Parser_QuantidadeInvalida_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|U990|INVALIDA|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroU990>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == nameof(RegistroU990.QtdLin) && erro.ValorBruto == "INVALIDA");
    }

    [Fact]
    public async Task ReadAsync_FixtureCompleta_MaterializaOrdemRoteamentoHierarquiaEValores()
    {
        byte[] bytes = await LerFixtureAsync();
        await using var entrada = new MemoryStream(bytes, writable: false);

        var arquivo = await new ParserEcf().ReadAsync(
            entrada,
            TestContext.Current.CancellationToken);

        string[] ordemBlocoU = [
            "U001", "U030", "U100", "U150", "U180", "U182",
            "U030", "U100", "U150", "U180", "U182", "U990",
        ];
        arquivo.BlocoU.Registros.Select(registro => registro.Codigo).Should().Equal(ordemBlocoU);
        arquivo.EnumerarRegistros().Select(registro => registro.Codigo)
            .Should().Equal(["0000", .. ordemBlocoU]);

        var r0000 = arquivo.Bloco0.Registros.Should().ContainSingle()
            .Which.Should().BeOfType<Registro0000>().Which;
        var registros = arquivo.BlocoU.Registros;
        var u001 = (RegistroU001)registros[0];
        var u030T01 = (RegistroU030)registros[1];
        var u100T01 = (RegistroU100)registros[2];
        var u150T01 = (RegistroU150)registros[3];
        var u180T01 = (RegistroU180)registros[4];
        var u182T01 = (RegistroU182)registros[5];
        var u030T02 = (RegistroU030)registros[6];
        var u100T02 = (RegistroU100)registros[7];
        var u150T02 = (RegistroU150)registros[8];
        var u180T02 = (RegistroU180)registros[9];
        var u182T02 = (RegistroU182)registros[10];
        var u990 = (RegistroU990)registros[11];

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, u001, u990);
        AssertRegistroEcf.ConformsToManifest(u001, "U001", "1:1", r0000, u030T01, u030T02);
        AssertRegistroEcf.ConformsToManifest(
            u030T01,
            "U030",
            "0:13",
            u001,
            u100T01,
            u150T01,
            u180T01,
            u182T01);
        AssertRegistroEcf.ConformsToManifest(u100T01, "U100", "0:N", u030T01);
        AssertRegistroEcf.ConformsToManifest(u150T01, "U150", "0:N", u030T01);
        AssertRegistroEcf.ConformsToManifest(u180T01, "U180", "0:N", u030T01);
        AssertRegistroEcf.ConformsToManifest(u182T01, "U182", "1:N", u030T01);
        AssertRegistroEcf.ConformsToManifest(
            u030T02,
            "U030",
            "0:13",
            u001,
            u100T02,
            u150T02,
            u180T02,
            u182T02);
        AssertRegistroEcf.ConformsToManifest(u100T02, "U100", "0:N", u030T02);
        AssertRegistroEcf.ConformsToManifest(u150T02, "U150", "0:N", u030T02);
        AssertRegistroEcf.ConformsToManifest(u180T02, "U180", "0:N", u030T02);
        AssertRegistroEcf.ConformsToManifest(u182T02, "U182", "1:N", u030T02);
        AssertRegistroEcf.ConformsToManifest(u990, "U990", "1:1", r0000);

        u001.IndDad.Should().Be(IndicadorMovimentoBloco.ComDados);
        u030T01.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        u030T01.PerApur.Should().Be("T01");
        u100T01.Tipo.Should().Be(IndicadorTipoConta.Analitica);
        u100T01.Descricao.Should().Be("SUPERÁVIT ACUMULADO");
        u100T01.ValCtaRefIni.Should().Be(10000m);
        u100T01.ValCtaRefFin.Should().Be("00020000,50");
        u150T01.Valor.Should().Be(10000m);
        u180T01.CampoCodigo.Should().Be("000012");
        u180T01.Valor.Should().Be("10000,00");
        u182T01.CampoCodigo.Should().Be("000001");
        u030T02.PerApur.Should().Be("T02");
        u100T02.Descricao.Should().BeNull();
        u100T02.ValCtaRefFin.Should().Be("CALCULADO");
        u150T02.Valor.Should().Be(-100.25m);
        u180T02.Valor.Should().Be("12,3400%");
        u182T02.Valor.Should().Be("VALOR-TABELA");
        u990.QtdLin.Should().Be(ordemBlocoU.Length);
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
        var registros = relido.BlocoU.Registros;
        registros[2].Should().BeOfType<RegistroU100>().Which.Pai.Should().BeSameAs(registros[1]);
        registros[5].Should().BeOfType<RegistroU182>().Which.Pai.Should().BeSameAs(registros[1]);
        registros[7].Should().BeOfType<RegistroU100>().Which.Pai.Should().BeSameAs(registros[6]);
        registros[9].Should().BeOfType<RegistroU180>().Which.Valor.Should().Be("12,3400%");
        registros[10].Should().BeOfType<RegistroU182>().Which.Valor.Should().Be("VALOR-TABELA");
        registros[11].Should().BeOfType<RegistroU990>().Which.QtdLin.Should().Be(12);
    }

    private static async Task<byte[]> LerFixtureAsync()
    {
        string caminho = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "Sinteticas",
            "bloco-u.txt");
        byte[] bytes = await File.ReadAllBytesAsync(caminho, TestContext.Current.CancellationToken);
        bytes.Should().NotBeEmpty();
        bytes[^1].Should().Be(0x0A);
        bytes.Should().NotContain(0x0D);
        EncodingSped.Latin1.GetBytes(EncodingSped.Latin1.GetString(bytes)).Should().Equal(bytes);
        return bytes;
    }
}
