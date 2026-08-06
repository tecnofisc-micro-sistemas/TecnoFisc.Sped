using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote4;

public sealed class RegistroX990Tests
{
    private static readonly string[] OrdemBlocoX = [
        "X001", "X280", "X292", "X340", "X350", "X351", "X352", "X353", "X354", "X355",
        "X356", "X357", "X360", "X365", "X366", "X370", "X371", "X375", "X390", "X400",
        "X410", "X420", "X430", "X450", "X451", "X460", "X470", "X480", "X485", "X490",
        "X500", "X510", "X990",
    ];

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX990(), "X990", "1:1");
    }

    [Fact]
    public void Parser_LeQuantidadeExataDeLinhasDoBloco()
    {
        var resultado = new ParserEcf().ParseLinha("|X990|33|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroX990>()
            .Which.QtdLin.Should().Be(33);
    }

    [Fact]
    public void Parser_QuantidadeInvalida_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|X990|INVALIDA|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroX990>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == nameof(RegistroX990.QtdLin) && erro.ValorBruto == "INVALIDA");
    }

    [Fact]
    public async Task ReadAsync_FixtureCompleta_MaterializaOrdemRoteamentoHierarquiaEValores()
    {
        byte[] bytes = await LerFixtureAsync();
        await using var entrada = new MemoryStream(bytes, writable: false);

        var arquivo = await new ParserEcf().ReadAsync(
            entrada,
            TestContext.Current.CancellationToken);

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
            saida,
            arquivo.EnumerarRegistros(),
            TestContext.Current.CancellationToken);

        string esperado = EncodingSped.Latin1.GetString(bytes).Replace(
            "\n",
            "\r\n",
            StringComparison.Ordinal);
        string serializado = EncodingSped.Latin1.GetString(saida.ToArray());
        serializado.Should().Be(esperado);

        saida.Position = 0;
        var relido = await parser.ReadAsync(saida, TestContext.Current.CancellationToken);
        AssertFixtureCompleta(relido);
    }

    private static void AssertFixtureCompleta(ArquivoEcf arquivo)
    {
        OrdemBlocoX.Should().HaveCount(33);
        arquivo.BlocoX.Registros.Select(registro => registro.Codigo).Should().Equal(OrdemBlocoX);
        arquivo.EnumerarRegistros().Select(registro => registro.Codigo)
            .Should().Equal(["0000", .. OrdemBlocoX]);

        var r0000 = arquivo.Bloco0.Registros.Should().ContainSingle()
            .Which.Should().BeOfType<Registro0000>().Which;
        var registros = arquivo.BlocoX.Registros;
        var x001 = (RegistroX001)registros[0];
        var x280 = (RegistroX280)registros[1];
        var x292 = (RegistroX292)registros[2];
        var x340 = (RegistroX340)registros[3];
        var x350 = (RegistroX350)registros[4];
        var x351 = (RegistroX351)registros[5];
        var x352 = (RegistroX352)registros[6];
        var x353 = (RegistroX353)registros[7];
        var x354 = (RegistroX354)registros[8];
        var x355 = (RegistroX355)registros[9];
        var x356 = (RegistroX356)registros[10];
        var x357 = (RegistroX357)registros[11];
        var x360 = (RegistroX360)registros[12];
        var x365 = (RegistroX365)registros[13];
        var x366 = (RegistroX366)registros[14];
        var x370 = (RegistroX370)registros[15];
        var x371 = (RegistroX371)registros[16];
        var x375 = (RegistroX375)registros[17];
        var x390 = (RegistroX390)registros[18];
        var x400 = (RegistroX400)registros[19];
        var x410 = (RegistroX410)registros[20];
        var x420 = (RegistroX420)registros[21];
        var x430 = (RegistroX430)registros[22];
        var x450 = (RegistroX450)registros[23];
        var x451 = (RegistroX451)registros[24];
        var x460 = (RegistroX460)registros[25];
        var x470 = (RegistroX470)registros[26];
        var x480 = (RegistroX480)registros[27];
        var x485 = (RegistroX485)registros[28];
        var x490 = (RegistroX490)registros[29];
        var x500 = (RegistroX500)registros[30];
        var x510 = (RegistroX510)registros[31];
        var x990 = (RegistroX990)registros[32];

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, x001, x990);
        AssertRegistroEcf.ConformsToManifest(
            x001,
            "X001",
            "1:1",
            r0000,
            x280, x292, x340, x360, x365, x370, x390, x400, x410, x420,
            x430, x450, x460, x470, x480, x485, x490, x500, x510);
        AssertRegistroEcf.ConformsToManifest(x280, "X280", "0:N", x001);
        AssertRegistroEcf.ConformsToManifest(x292, "X292", "0:N", x001);
        AssertRegistroEcf.ConformsToManifest(
            x340,
            "X340",
            "0:N",
            x001,
            x350, x351, x352, x353, x354, x355, x356, x357);
        AssertRegistroEcf.ConformsToManifest(x350, "X350", "0:1", x340);
        AssertRegistroEcf.ConformsToManifest(x351, "X351", "0:1", x340);
        AssertRegistroEcf.ConformsToManifest(x352, "X352", "0:1", x340);
        AssertRegistroEcf.ConformsToManifest(x353, "X353", "0:1", x340);
        AssertRegistroEcf.ConformsToManifest(x354, "X354", "0:1", x340);
        AssertRegistroEcf.ConformsToManifest(x355, "X355", "0:1", x340);
        AssertRegistroEcf.ConformsToManifest(x356, "X356", "0:1", x340);
        AssertRegistroEcf.ConformsToManifest(x357, "X357", "0:N", x340);
        AssertRegistroEcf.ConformsToManifest(x360, "X360", "0:N", x001);
        AssertRegistroEcf.ConformsToManifest(x365, "X365", "0:N", x001, x366);
        AssertRegistroEcf.ConformsToManifest(x366, "X366", "0:N", x365);
        AssertRegistroEcf.ConformsToManifest(x370, "X370", "0:N", x001, x371, x375);
        AssertRegistroEcf.ConformsToManifest(x371, "X371", "0:N", x370);
        AssertRegistroEcf.ConformsToManifest(x375, "X375", "0:N", x370);
        AssertRegistroEcf.ConformsToManifest(x390, "X390", "0:N", x001);
        AssertRegistroEcf.ConformsToManifest(x400, "X400", "0:N", x001);
        AssertRegistroEcf.ConformsToManifest(x410, "X410", "0:N", x001);
        AssertRegistroEcf.ConformsToManifest(x420, "X420", "0:N", x001);
        AssertRegistroEcf.ConformsToManifest(x430, "X430", "0:N", x001);
        AssertRegistroEcf.ConformsToManifest(x450, "X450", "0:N", x001, x451);
        AssertRegistroEcf.ConformsToManifest(x451, "X451", "0:N", x450);
        AssertRegistroEcf.ConformsToManifest(x460, "X460", "0:N", x001);
        AssertRegistroEcf.ConformsToManifest(x470, "X470", "0:N", x001);
        AssertRegistroEcf.ConformsToManifest(x480, "X480", "0:N", x001);
        AssertRegistroEcf.ConformsToManifest(x485, "X485", "0:N", x001);
        AssertRegistroEcf.ConformsToManifest(x490, "X490", "0:N", x001);
        AssertRegistroEcf.ConformsToManifest(x500, "X500", "0:N", x001);
        AssertRegistroEcf.ConformsToManifest(x510, "X510", "0:N", x001);
        AssertRegistroEcf.ConformsToManifest(x990, "X990", "1:1", r0000);

        x280.VigIni.Should().Be(new DateOnly(2021, 1, 1));
        x340.Nif.Should().Be("0000");
        x357.NifCnpj.Should().Be("NIF-X-0001");
        x370.VlTransacao.Should().Be(1234.56m);
        x451.CampoCodigo.Should().Be("000006");
        x485.DtFinPortCebas.Should().Be(new DateOnly(2025, 12, 31));
        x500.CampoCodigo.Should().Be("ZPE-0001");
        x500.Valor.Should().Be("100000,00");
        x510.CampoCodigo.Should().Be("ALC-0002");
        x510.Valor.Should().BeNull();
        x990.QtdLin.Should().Be(33);
    }

    private static async Task<byte[]> LerFixtureAsync()
    {
        string caminho = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "Sinteticas",
            "bloco-x.txt");
        byte[] bytes = await File.ReadAllBytesAsync(caminho, TestContext.Current.CancellationToken);
        bytes.Should().NotBeEmpty();
        bytes[^1].Should().Be(0x0A);
        bytes.Should().NotContain(0x0D);
        EncodingSped.Latin1.GetBytes(EncodingSped.Latin1.GetString(bytes)).Should().Equal(bytes);
        return bytes;
    }
}
