using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoY;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoY.Lote1;

public sealed class RegistroY001Tests
{
    private const string TextoLote =
        "|0000|LECF|0012|11111111000191|EMPRESA TESTE|0|0|||01012025|31122025|N||0||\n" +
        "|Y001|0|\n" +
        "|Y520|R|001|1|10500|100000,00|\n" +
        "|Y570|11111111000191|FONTE PAGADORA|S|5928|100000,00|1500,00|500,00|\n" +
        "|Y590|00000000000331|249|ATIVO EXTERIOR COM IDENTIFICADOR LONGO|0,00|300000,00|\n" +
        "|Y600|01012012||105|PF|00000000000|SOCIO TESTE|01|60,0000|60,0000|||100000,00|10000,00|5000,00|3000,00|9000,00|\n" +
        "|Y612|52998224725|DIRIGENTE TESTE|12|50000,00|10000,00|8000,00|\n" +
        "|Y620|01012024|1|105|44444444000191|EMPRESA COLIGADA|1000000,00|1000000,00|25,0000|30,0000|-100000,00|31102013|N|||N||\n" +
        "|Y630|44444444000191|100|5000000|100000000,00|10012010||\n" +
        "|Y640|44444444000191|1|500000,00|22222222000191|400000,00|\n" +
        "|Y650|11111111000191|100000,00|\n";

    [Fact]
    public void Catalogo_ImplementaRegistroY001()
    {
        AssertRegistroEcf.CodesAreImplemented("Y001");
    }

    [Fact]
    public void Parser_LeIndicadorDeMovimento()
    {
        var resultado = new ParserEcf().ParseLinha("|Y001|0|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroY001>()
            .Which.IndDad.Should().Be(IndicadorMovimentoBloco.ComDados);
    }

    [Fact]
    public void Parser_IndicadorInvalido_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|Y001|9|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroY001>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == "IND_DAD" && erro.ValorBruto == "9");
    }

    [Fact]
    public async Task ReadAsync_TextoComponivelSemY990_MaterializaHierarquiaDoLote()
    {
        await using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(TextoLote), writable: false);

        var arquivo = await new ParserEcf().ReadAsync(entrada, TestContext.Current.CancellationToken);

        string[] ordemY = ["Y001", "Y520", "Y570", "Y590", "Y600", "Y612", "Y620", "Y630", "Y640", "Y650"];
        arquivo.BlocoY.Registros.Select(registro => registro.Codigo).Should().Equal(ordemY);
        arquivo.EnumerarRegistros().Select(registro => registro.Codigo)
            .Should().Equal(["0000", .. ordemY]);
        arquivo.BlocoY.Registros.Should().NotContain(registro => registro.Codigo == "Y990");

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

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, y001);
        AssertRegistroEcf.ConformsToManifest(
            y001, "Y001", "1:1", r0000,
            y520, y570, y590, y600, y612, y620, y630, y640);
        AssertRegistroEcf.ConformsToManifest(y520, "Y520", "0:N", y001);
        AssertRegistroEcf.ConformsToManifest(y570, "Y570", "0:N", y001);
        AssertRegistroEcf.ConformsToManifest(y590, "Y590", "0:N", y001);
        AssertRegistroEcf.ConformsToManifest(y600, "Y600", "0:N", y001);
        AssertRegistroEcf.ConformsToManifest(y612, "Y612", "0:N", y001);
        AssertRegistroEcf.ConformsToManifest(y620, "Y620", "0:N", y001);
        AssertRegistroEcf.ConformsToManifest(y630, "Y630", "0:N", y001);
        AssertRegistroEcf.ConformsToManifest(y640, "Y640", "0:N", y001, y650);
        AssertRegistroEcf.ConformsToManifest(y650, "Y650", "0:N", y640);
    }

    [Fact]
    public async Task Writer_TextoComponivelSemFechamento_PreservaOrdemValoresEGrafoNoNovoParse()
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
        serializado.Should().NotContain("|Y990|");

        saida.Position = 0;
        var relido = await parser.ReadAsync(saida, TestContext.Current.CancellationToken);
        relido.EnumerarRegistros().Select(registro => registro.Codigo)
            .Should().Equal(arquivo.EnumerarRegistros().Select(registro => registro.Codigo));
        var registros = relido.BlocoY.Registros;
        registros[9].Pai.Should().BeSameAs(registros[8]);
        registros[8].Filhos.Should().ContainSingle().Which.Should().BeSameAs(registros[9]);
        registros.Take(8).Skip(1).Should().OnlyContain(registro => ReferenceEquals(registro.Pai, registros[0]));
        ((RegistroY620)registros[6]).ResEqPat.Should().Be(-100000m);
        ((RegistroY600)registros[4]).CpfCnpj.Should().Be("00000000000");
    }
}
