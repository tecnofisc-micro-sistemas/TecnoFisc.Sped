using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote1;

public sealed class RegistroX340Tests
{
    private const string TextoLote =
        "|0000|LECF|0012|11111111000191|EMPRESA TESTE|0|0|||01012025|31122025|N||0||\n" +
        "|X001|0|\n" +
        "|X280|01|AM|01|ATO-0001|01012021|31122026|11222333000181|00001234|100000,50|25000,25|\n" +
        "|X292|000014|ROYALTIES|100000,00|\n" +
        "|X340|INVESTIDA ALFA|0000|1|249|N|S||00123456000199|USD|\n" +
        "|X350|1000,00|600,00|400,00|100,00|0,00|40,00|10,00|450,00|50,00|0,00|0,00|500,00|0,00|100,00|400,00|\n" +
        "|X351|100,00|250,00|10,00|25,00|5,00|12,50|90,00|225,00|50,00|125,00|20,00|50,00|5,00|12,50|7,00|\n" +
        "|X352|-100,00|-250,00|0,00|0,00|\n" +
        "|X353|100,00|250,00|50,00|125,00|10,00|50,00|\n" +
        "|X354|100,00|250,00|50,00|\n" +
        "|X355|100,00|250,00|1000,00|2500,00|900,00|2250,00|90,0000|\n" +
        "|X340|INVESTIDA BETA|NIF-A9|4|105|S|N|0007||BRL|\n" +
        "|X352|10,00|25,00|5,00|12,50|\n";

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX340(), "X340", "0:N");
    }

    [Fact]
    public void Parser_PreservaChaveNifCnpjCodigosDinamicosEDominios()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X340|INVESTIDA|0000|10|005|S|N|0007|00123456000199|USD|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX340>().Which;
        registro.RazSocial.Should().Be("INVESTIDA");
        registro.Nif.Should().Be("0000");
        registro.IndControle.Should().Be(TipoControleExterior.ColigadaCompetenciaOpcao);
        registro.Pais.Should().Be("005");
        registro.IndIsenPetr.Should().Be(IndicadorSimNao.Sim);
        registro.IndConsol.Should().Be(IndicadorSimNao.Nao);
        registro.MotNaoConsol.Should().Be("0007");
        registro.Cnpj.Should().Be("00123456000199");
        registro.TipMoeda.Should().Be("USD");
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_OpcionaisVazios_PreservaNulos()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X340|INVESTIDA EXTERIOR|NIF-A9|7|249|N|S|||EUR|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX340>().Which;
        registro.Nif.Should().Be("NIF-A9");
        registro.MotNaoConsol.Should().BeNull();
        registro.Cnpj.Should().BeNull();
    }

    [Fact]
    public void Parser_DominiosInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha(
            "|X340|INVESTIDA|NIF|99|249|X|Y||||");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroX340>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([
                "IND_CONTROLE",
                "IND_ISEN_PETR",
                "IND_CONSOL",
            ]);
    }

    [Fact]
    public async Task ReadAsync_TextoComponivelSemX990_MaterializaRamosEOrdemDoLote()
    {
        await using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(TextoLote), writable: false);

        var arquivo = await new ParserEcf().ReadAsync(
            entrada,
            TestContext.Current.CancellationToken);

        string[] ordemX = [
            "X001", "X280", "X292", "X340", "X350", "X351", "X352", "X353", "X354", "X355", "X340", "X352",
        ];
        arquivo.BlocoX.Registros.Select(registro => registro.Codigo).Should().Equal(ordemX);
        arquivo.EnumerarRegistros().Select(registro => registro.Codigo)
            .Should().Equal(["0000", .. ordemX]);
        arquivo.BlocoX.Registros.Should().NotContain(registro => registro.Codigo == "X990");

        var r0000 = arquivo.Bloco0.Registros.Should().ContainSingle()
            .Which.Should().BeOfType<Registro0000>().Which;
        var registros = arquivo.BlocoX.Registros;
        var x001 = (RegistroX001)registros[0];
        var x280 = (RegistroX280)registros[1];
        var x292 = (RegistroX292)registros[2];
        var x340Alfa = (RegistroX340)registros[3];
        var x350 = (RegistroX350)registros[4];
        var x351 = (RegistroX351)registros[5];
        var x352Alfa = (RegistroX352)registros[6];
        var x353 = (RegistroX353)registros[7];
        var x354 = (RegistroX354)registros[8];
        var x355 = (RegistroX355)registros[9];
        var x340Beta = (RegistroX340)registros[10];
        var x352Beta = (RegistroX352)registros[11];

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, x001);
        AssertRegistroEcf.ConformsToManifest(x001, "X001", "1:1", r0000, x280, x292, x340Alfa, x340Beta);
        AssertRegistroEcf.ConformsToManifest(x280, "X280", "0:N", x001);
        AssertRegistroEcf.ConformsToManifest(x292, "X292", "0:N", x001);
        AssertRegistroEcf.ConformsToManifest(x340Alfa, "X340", "0:N", x001, x350, x351, x352Alfa, x353, x354, x355);
        AssertRegistroEcf.ConformsToManifest(x350, "X350", "0:1", x340Alfa);
        AssertRegistroEcf.ConformsToManifest(x351, "X351", "0:1", x340Alfa);
        AssertRegistroEcf.ConformsToManifest(x352Alfa, "X352", "0:1", x340Alfa);
        AssertRegistroEcf.ConformsToManifest(x353, "X353", "0:1", x340Alfa);
        AssertRegistroEcf.ConformsToManifest(x354, "X354", "0:1", x340Alfa);
        AssertRegistroEcf.ConformsToManifest(x355, "X355", "0:1", x340Alfa);
        AssertRegistroEcf.ConformsToManifest(x340Beta, "X340", "0:N", x001, x352Beta);
        AssertRegistroEcf.ConformsToManifest(x352Beta, "X352", "0:1", x340Beta);
    }

    [Fact]
    public async Task Writer_TextoComponivelSemFechamento_PreservaCanonicoEPermiteNovoParse()
    {
        await using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(TextoLote), writable: false);
        var parser = new ParserEcf();
        var arquivo = await parser.ReadAsync(entrada, TestContext.Current.CancellationToken);

        await using var saida = new MemoryStream();
        await new EscritorSpedTxt(new CatalogoSpedGerado()).WriteAsync(
            saida,
            arquivo.EnumerarRegistros(),
            TestContext.Current.CancellationToken);

        string serializado = EncodingSped.Latin1.GetString(saida.ToArray());
        serializado.Should().Be(TextoLote.Replace("\n", "\r\n", StringComparison.Ordinal));
        serializado.Should().NotContain("|X990|");

        saida.Position = 0;
        var relido = await parser.ReadAsync(saida, TestContext.Current.CancellationToken);
        relido.EnumerarRegistros().Select(registro => registro.Codigo)
            .Should().Equal(arquivo.EnumerarRegistros().Select(registro => registro.Codigo));
        var registros = relido.BlocoX.Registros;
        registros[4].Should().BeOfType<RegistroX350>().Which.Pai.Should().BeSameAs(registros[3]);
        registros[9].Should().BeOfType<RegistroX355>().Which.Pai.Should().BeSameAs(registros[3]);
        registros[11].Should().BeOfType<RegistroX352>().Which.Pai.Should().BeSameAs(registros[10]);
        registros.Should().NotContain(registro => registro.Codigo == "X990");
    }
}
