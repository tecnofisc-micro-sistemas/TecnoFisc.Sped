using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoX.Lote2;

public sealed class RegistroX356Tests
{
    private const string TextoLote =
        "|0000|LECF|0012|11111111000191|EMPRESA TESTE|0|0|||01012025|31122025|N||0||\n" +
        "|X001|0|\n" +
        "|X340|INVESTIDA ALFA|NIF-A9|1|249|N|S|||USD|\n" +
        "|X356|60,1234|100000,50|-25000,25|\n" +
        "|X357|005|0000|INVESTIDORA UM|25,5000|\n" +
        "|X357|249|00123456000199|INVESTIDORA DOIS|74,5000|\n" +
        "|X360|000001|DESCRICAO DINAMICA|R$ -1.234,56|\n" +
        "|X365|E00001|ENTIDADE CONTROLADA|\n" +
        "|X366|000002|RELACAO DINAMICA|VALOR LIVRE|\n" +
        "|X370|E00001|03|ENTIDADE CONTROLADA|076||101|SERVICOS CONTROLADOS|1234,56|S|100,00|25,50|01|MLT|JUSTIFICATIVA|S|N|S|N|S|\n" +
        "|X371|2328.2.0001||25,00|D|\n" +
        "|X375|000003|METODO DINAMICO|VALOR SEM NORMALIZACAO|\n" +
        "|X390|000004|ORIGEM DINAMICA|9.876,54|\n" +
        "|X400|000005|VENDA DINAMICA|100000,00|\n";

    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroX356(), "X356", "0:1");
    }

    [Fact]
    public void Parser_LePercentualMontantesESinalPermitidoSemCalcularEstrutura()
    {
        var resultado = new ParserEcf().ParseLinha("|X356|60,1234|100000,50|-25000,25|");

        resultado.Sucesso.Should().BeTrue();
        var registro = resultado.Valor.Should().BeOfType<RegistroX356>().Which;
        registro.PercPart.Should().Be(60.1234m);
        registro.AtivoTotal.Should().Be(100000.50m);
        registro.PatLiquido.Should().Be(-25000.25m);
        registro.ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public void Parser_PercentualInvalido_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|X356|INVALIDO|100000,50|-25000,25|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroX356>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == nameof(RegistroX356.PercPart) && erro.ValorBruto == "INVALIDO");
    }

    [Fact]
    public async Task ReadAsync_TextoComponivelSemX990_MaterializaTodosOsRamosDoLote()
    {
        await using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(TextoLote), writable: false);

        var arquivo = await new ParserEcf().ReadAsync(entrada, TestContext.Current.CancellationToken);

        string[] ordemX = [
            "X001", "X340", "X356", "X357", "X357", "X360", "X365", "X366",
            "X370", "X371", "X375", "X390", "X400",
        ];
        arquivo.BlocoX.Registros.Select(registro => registro.Codigo).Should().Equal(ordemX);
        arquivo.EnumerarRegistros().Select(registro => registro.Codigo)
            .Should().Equal(["0000", .. ordemX]);
        arquivo.BlocoX.Registros.Should().NotContain(registro => registro.Codigo == "X990");

        var r0000 = arquivo.Bloco0.Registros.Should().ContainSingle()
            .Which.Should().BeOfType<Registro0000>().Which;
        var registros = arquivo.BlocoX.Registros;
        var x001 = (RegistroX001)registros[0];
        var x340 = (RegistroX340)registros[1];
        var x356 = (RegistroX356)registros[2];
        var x357Um = (RegistroX357)registros[3];
        var x357Dois = (RegistroX357)registros[4];
        var x360 = (RegistroX360)registros[5];
        var x365 = (RegistroX365)registros[6];
        var x366 = (RegistroX366)registros[7];
        var x370 = (RegistroX370)registros[8];
        var x371 = (RegistroX371)registros[9];
        var x375 = (RegistroX375)registros[10];
        var x390 = (RegistroX390)registros[11];
        var x400 = (RegistroX400)registros[12];

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, x001);
        AssertRegistroEcf.ConformsToManifest(x001, "X001", "1:1", r0000, x340, x360, x365, x370, x390, x400);
        AssertRegistroEcf.ConformsToManifest(x340, "X340", "0:N", x001, x356, x357Um, x357Dois);
        AssertRegistroEcf.ConformsToManifest(x356, "X356", "0:1", x340);
        AssertRegistroEcf.ConformsToManifest(x357Um, "X357", "0:N", x340);
        AssertRegistroEcf.ConformsToManifest(x357Dois, "X357", "0:N", x340);
        AssertRegistroEcf.ConformsToManifest(x360, "X360", "0:N", x001);
        AssertRegistroEcf.ConformsToManifest(x365, "X365", "0:N", x001, x366);
        AssertRegistroEcf.ConformsToManifest(x366, "X366", "0:N", x365);
        AssertRegistroEcf.ConformsToManifest(x370, "X370", "0:N", x001, x371, x375);
        AssertRegistroEcf.ConformsToManifest(x371, "X371", "0:N", x370);
        AssertRegistroEcf.ConformsToManifest(x375, "X375", "0:N", x370);
        AssertRegistroEcf.ConformsToManifest(x390, "X390", "0:N", x001);
        AssertRegistroEcf.ConformsToManifest(x400, "X400", "0:N", x001);
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
        registros[2].Should().BeOfType<RegistroX356>().Which.Pai.Should().BeSameAs(registros[1]);
        registros[4].Should().BeOfType<RegistroX357>().Which.Pai.Should().BeSameAs(registros[1]);
        registros[7].Should().BeOfType<RegistroX366>().Which.Pai.Should().BeSameAs(registros[6]);
        registros[9].Should().BeOfType<RegistroX371>().Which.Pai.Should().BeSameAs(registros[8]);
        registros[10].Should().BeOfType<RegistroX375>().Which.Pai.Should().BeSameAs(registros[8]);
        registros.Should().NotContain(registro => registro.Codigo == "X990");
    }
}
