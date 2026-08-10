using TecnoFisc.Sped.Ecf.Enums;
using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoW;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoW;

public sealed class RegistroW990Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroW990(), "W990", "1:1");
    }

    [Fact]
    public void Parser_LeQuantidadeExataDeLinhasDoBloco()
    {
        var resultado = new ParserEcf().ParseLinha("|W990|10|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroW990>()
            .Which.QtdLin.Should().Be(10);
    }

    [Fact]
    public void Parser_QuantidadeInvalida_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|W990|INVALIDA|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroW990>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == "QTD_LIN" && erro.ValorBruto == "INVALIDA");
    }

    [Fact]
    public async Task ReadAsync_FixtureCompleta_MaterializaOrdemRoteamentoHierarquiaEValores()
    {
        byte[] bytes = await LerFixtureAsync();
        await using var entrada = new MemoryStream(bytes, writable: false);

        var arquivo = await new ParserEcf().ReadAsync(
            entrada,
            TestContext.Current.CancellationToken);

        string[] ordemBlocoW = [
            "W001", "W100",
            "W200", "W250", "W250",
            "W200", "W250",
            "W300", "W300",
            "W990",
        ];
        arquivo.BlocoW.Registros.Select(registro => registro.Codigo).Should().Equal(ordemBlocoW);
        arquivo.EnumerarRegistros().Select(registro => registro.Codigo)
            .Should().Equal(["0000", .. ordemBlocoW]);

        var r0000 = arquivo.Bloco0.Registros.Should().ContainSingle()
            .Which.Should().BeOfType<Registro0000>().Which;
        var registros = arquivo.BlocoW.Registros;
        var w001 = (RegistroW001)registros[0];
        var w100 = (RegistroW100)registros[1];
        var w200De = (RegistroW200)registros[2];
        var w250Alfa = (RegistroW250)registros[3];
        var w250Beta = (RegistroW250)registros[4];
        var w200Br = (RegistroW200)registros[5];
        var w250Br = (RegistroW250)registros[6];
        var w300De = (RegistroW300)registros[7];
        var w300Global = (RegistroW300)registros[8];
        var w990 = (RegistroW990)registros[9];

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, w001, w990);
        AssertRegistroEcf.ConformsToManifest(w001, "W001", "1:1", r0000, w100, w300De, w300Global);
        AssertRegistroEcf.ConformsToManifest(w100, "W100", "0:1", w001, w200De, w200Br);
        AssertRegistroEcf.ConformsToManifest(w200De, "W200", "0:N", w100, w250Alfa, w250Beta);
        AssertRegistroEcf.ConformsToManifest(w250Alfa, "W250", "1:N", w200De);
        AssertRegistroEcf.ConformsToManifest(w250Beta, "W250", "1:N", w200De);
        AssertRegistroEcf.ConformsToManifest(w200Br, "W200", "0:N", w100, w250Br);
        AssertRegistroEcf.ConformsToManifest(w250Br, "W250", "1:N", w200Br);
        AssertRegistroEcf.ConformsToManifest(w300De, "W300", "0:N", w001);
        AssertRegistroEcf.ConformsToManifest(w300Global, "W300", "0:N", w001);
        AssertRegistroEcf.ConformsToManifest(w990, "W990", "1:1", r0000);

        w001.IndDad.Should().Be(IndicadorMovimentoBloco.ComDados);
        w100.TinControladora.Should().Be("DE-TIN-000042");
        w100.TinSubstituta.Should().Be("US-EIN-A9");
        w100.IndEntrega.Should().Be(ResponsavelEntregaDpp.OutraEntidade);
        w200De.VlLucPrejAntesIr.Should().Be(-250m);
        w200De.NumEmp.Should().Be(321);
        w250Alfa.Tin.Should().Be("TIN-DE-0001");
        w250Alfa.Ni.Should().Be("LEI-0001");
        w250Beta.Tin.Should().Be("NOTIN");
        w250Beta.DescOutros.Should().Be("OUTRAS ATIVIDADES");
        w200Br.Jurisdicao.Should().Be("BR");
        w250Br.Tin.Should().Be("12345678000195");
        w300De.Observação.Should().Be("CRITERIO ALTERADO");
        w300Global.Jurisdicao.Should().BeNull();
        w300Global.FimObservacao.Should().Be("W300FIM");
        w990.QtdLin.Should().Be(ordemBlocoW.Length);
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
        var registros = relido.BlocoW.Registros;
        registros[3].Should().BeOfType<RegistroW250>().Which.Pai.Should().BeSameAs(registros[2]);
        registros[4].Should().BeOfType<RegistroW250>().Which.Pai.Should().BeSameAs(registros[2]);
        registros[6].Should().BeOfType<RegistroW250>().Which.Pai.Should().BeSameAs(registros[5]);
        registros[7].Should().BeOfType<RegistroW300>().Which.Pai.Should().BeSameAs(registros[0]);
        registros[8].Should().BeOfType<RegistroW300>().Which.FimObservacao.Should().Be("W300FIM");
        registros[9].Should().BeOfType<RegistroW990>().Which.QtdLin.Should().Be(10);
    }

    private static async Task<byte[]> LerFixtureAsync()
    {
        string caminho = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "Sinteticas",
            "bloco-w.txt");
        byte[] bytes = await File.ReadAllBytesAsync(caminho, TestContext.Current.CancellationToken);
        bytes.Should().NotBeEmpty();
        bytes[^1].Should().Be(0x0A);
        bytes.Should().NotContain(0x0D);
        EncodingSped.Latin1.GetBytes(EncodingSped.Latin1.GetString(bytes)).Should().Equal(bytes);
        return bytes;
    }
}
