using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote3;

public sealed class RegistroX410Tests
{
    private const string TextoLote =
        "|0000|LECF|0012|11111111000191|EMPRESA TESTE|0|0|||01012025|31122025|N||0||\n" +
        "|X001|0|\n" +
        "|X400|000005|VENDA DINAMICA|100000,00|\n" +
        "|X410|076|S|N|\n" +
        "|X420|R|076|1234,56||-2,50||||7,25|\n" +
        "|X430|249|100,00||-3,25|||5,50|\n" +
        "|X450|392|\n" +
        "|X451|000001|REMESSA DINAMICA|VALOR LIVRE|\n" +
        "|X460|000002|INOVACAO TECNOLOGICA|TEXTO LONGO SEM NORMALIZACAO|\n" +
        "|X470|000003|INCLUSAO DIGITAL|VALOR DINAMICO|\n" +
        "|X480|000004|BENEFICIO PARTE I|-1234,56|\n" +
        "|X485|12|ATO DECLARATORIO EXECUTIVO SEM LIMITE FIXO|11222333000181|000000000000000123|000000000000000456|000000000000000789|123/2025|06012025|01012025|31122025|\n" +
        "|X490|000006|POLO INDUSTRIAL|VALOR TEXTUAL|\n";

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX410(), "X410", "0:N");
    }

    [Fact]
    public void Parser_LePaisComZeroSignificativoEIndicadoresFechados()
    {
        var resultado = new ParserEcf().ParseLinha("|X410|076|S|N|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX410>().Which;
        registro.Pais.Should().Be("076");
        registro.IndHomeDisp.Should().Be(IndicadorSimNao.Sim);
        registro.IndServDisp.Should().Be(IndicadorSimNao.Nao);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_IndicadoresInvalidos_RegistramErrosDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|X410|076|X|?|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroX410>()
            .Which.ErrosDeFormato.Select(erro => erro.Campo)
            .Should().Contain([
                nameof(RegistroX410.IndHomeDisp),
                nameof(RegistroX410.IndServDisp),
            ]);
    }

    [Fact]
    public async Task ReadAsync_TextoComponivelSemX990_MaterializaHierarquiaDoLote()
    {
        await using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(TextoLote), writable: false);

        var arquivo = await new ParserEcf().ReadAsync(entrada, TestContext.Current.CancellationToken);

        string[] ordemX = [
            "X001", "X400", "X410", "X420", "X430", "X450", "X451",
            "X460", "X470", "X480", "X485", "X490",
        ];
        arquivo.BlocoX.Registros.Select(registro => registro.Codigo).Should().Equal(ordemX);
        arquivo.EnumerarRegistros().Select(registro => registro.Codigo)
            .Should().Equal(["0000", .. ordemX]);
        arquivo.BlocoX.Registros.Should().NotContain(registro => registro.Codigo == "X990");

        var r0000 = arquivo.Bloco0.Registros.Should().ContainSingle()
            .Which.Should().BeOfType<Registro0000>().Which;
        var registros = arquivo.BlocoX.Registros;
        var x001 = (RegistroX001)registros[0];
        var x400 = (RegistroX400)registros[1];
        var x410 = (RegistroX410)registros[2];
        var x420 = (RegistroX420)registros[3];
        var x430 = (RegistroX430)registros[4];
        var x450 = (RegistroX450)registros[5];
        var x451 = (RegistroX451)registros[6];
        var x460 = (RegistroX460)registros[7];
        var x470 = (RegistroX470)registros[8];
        var x480 = (RegistroX480)registros[9];
        var x485 = (RegistroX485)registros[10];
        var x490 = (RegistroX490)registros[11];

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, x001);
        AssertRegistroEcf.ConformsToManifest(
            x001, "X001", "1:1", r0000,
            x400, x410, x420, x430, x450, x460, x470, x480, x485, x490);
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
    }

    [Fact]
    public async Task Writer_TextoComponivelSemFechamento_PreservaCanonicoEHierarquiaNoNovoParse()
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
        registros[6].Should().BeOfType<RegistroX451>().Which.Pai.Should().BeSameAs(registros[5]);
        registros[2].Pai.Should().BeSameAs(registros[0]);
        registros[11].Pai.Should().BeSameAs(registros[0]);
        registros.Should().NotContain(registro => registro.Codigo == "X990");
    }
}
