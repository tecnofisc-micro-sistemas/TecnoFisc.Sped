using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Registros.BlocoV;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoV;

public sealed class RegistroV990Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new RegistroV990(), "V990", "1:1");
    }

    [Fact]
    public void Parser_LeQuantidadeExataDeLinhasDoBloco()
    {
        var resultado = new ParserEcf().ParseLinha("|V990|14|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroV990>()
            .Which.QtdLinM.Should().Be(14);
    }

    [Fact]
    public void Parser_QuantidadeInvalida_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|V990|INVALIDA|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<RegistroV990>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == nameof(RegistroV990.QtdLinM) && erro.ValorBruto == "INVALIDA");
    }

    [Fact]
    public async Task ReadAsync_FixtureCompleta_MaterializaOrdemRoteamentoHierarquiaEValores()
    {
        byte[] bytes = await LerFixtureAsync();
        await using var entrada = new MemoryStream(bytes, writable: false);

        var arquivo = await new ParserEcf().ReadAsync(
            entrada,
            TestContext.Current.CancellationToken);

        string[] ordemBlocoV = [
            "V001",
            "V010", "V020", "V020", "V030", "V100", "V100", "V030", "V100",
            "V010", "V020", "V030", "V100",
            "V990",
        ];
        arquivo.BlocoV.Registros.Select(registro => registro.Codigo).Should().Equal(ordemBlocoV);
        arquivo.EnumerarRegistros().Select(registro => registro.Codigo)
            .Should().Equal(["0000", .. ordemBlocoV]);

        var r0000 = arquivo.Bloco0.Registros.Should().ContainSingle()
            .Which.Should().BeOfType<Registro0000>().Which;
        var registros = arquivo.BlocoV.Registros;
        var v001 = (RegistroV001)registros[0];
        var v010De = (RegistroV010)registros[1];
        var v020De1 = (RegistroV020)registros[2];
        var v020De2 = (RegistroV020)registros[3];
        var v030Jan = (RegistroV030)registros[4];
        var v100Jan1 = (RegistroV100)registros[5];
        var v100Jan2 = (RegistroV100)registros[6];
        var v030Dez = (RegistroV030)registros[7];
        var v100Dez = (RegistroV100)registros[8];
        var v010Us = (RegistroV010)registros[9];
        var v020Us = (RegistroV020)registros[10];
        var v030Fev = (RegistroV030)registros[11];
        var v100Fev = (RegistroV100)registros[12];
        var v990 = (RegistroV990)registros[13];

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, v001, v990);
        AssertRegistroEcf.ConformsToManifest(v001, "V001", "1:1", r0000, v010De, v010Us);
        AssertRegistroEcf.ConformsToManifest(
            v010De,
            "V010",
            "1:N",
            v001,
            v020De1,
            v020De2,
            v030Jan,
            v030Dez);
        AssertRegistroEcf.ConformsToManifest(v020De1, "V020", "1:N", v010De);
        AssertRegistroEcf.ConformsToManifest(v020De2, "V020", "1:N", v010De);
        AssertRegistroEcf.ConformsToManifest(v030Jan, "V030", "1:12", v010De, v100Jan1, v100Jan2);
        AssertRegistroEcf.ConformsToManifest(v100Jan1, "V100", "1:N", v030Jan);
        AssertRegistroEcf.ConformsToManifest(v100Jan2, "V100", "1:N", v030Jan);
        AssertRegistroEcf.ConformsToManifest(v030Dez, "V030", "1:12", v010De, v100Dez);
        AssertRegistroEcf.ConformsToManifest(v100Dez, "V100", "1:N", v030Dez);
        AssertRegistroEcf.ConformsToManifest(v010Us, "V010", "1:N", v001, v020Us, v030Fev);
        AssertRegistroEcf.ConformsToManifest(v020Us, "V020", "1:N", v010Us);
        AssertRegistroEcf.ConformsToManifest(v030Fev, "V030", "1:12", v010Us, v100Fev);
        AssertRegistroEcf.ConformsToManifest(v100Fev, "V100", "1:N", v030Fev);
        AssertRegistroEcf.ConformsToManifest(v990, "V990", "1:1", r0000);

        v001.IndDad.Should().Be(IndicadorMovimentoBloco.ComDados);
        v010De.NomeInstituicao.Should().Be("INSTITUICAO UM");
        v010De.Pais.Should().Be("DE");
        v010De.TipMoeda.Should().Be("EUR");
        v020De1.Ni.Should().Be("12345678900");
        v020De2.Ni.Should().Be("12345678000195");
        v030Jan.Mes.Should().Be("01");
        v100Jan1.CampoCodigo.Should().Be("61");
        v100Jan1.Valor.Should().Be("10000,00");
        v100Jan2.Valor.Should().Be("-25,50");
        v030Dez.Mes.Should().Be("12");
        v100Dez.Valor.Should().Be("12,3400%");
        v010Us.TipMoeda.Should().Be("USD");
        v020Us.Ni.Should().Be("PASS-12345");
        v100Fev.Descricao.Should().BeNull();
        v100Fev.Valor.Should().Be("VALOR-TABELA");
        v990.QtdLinM.Should().Be(ordemBlocoV.Length);
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
        var registros = relido.BlocoV.Registros;
        registros[2].Should().BeOfType<RegistroV020>().Which.Pai.Should().BeSameAs(registros[1]);
        registros[5].Should().BeOfType<RegistroV100>().Which.Pai.Should().BeSameAs(registros[4]);
        registros[8].Should().BeOfType<RegistroV100>().Which.Pai.Should().BeSameAs(registros[7]);
        registros[12].Should().BeOfType<RegistroV100>().Which.Valor.Should().Be("VALOR-TABELA");
        registros[13].Should().BeOfType<RegistroV990>().Which.QtdLinM.Should().Be(14);
    }

    private static async Task<byte[]> LerFixtureAsync()
    {
        string caminho = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "Sinteticas",
            "bloco-v.txt");
        byte[] bytes = await File.ReadAllBytesAsync(caminho, TestContext.Current.CancellationToken);
        bytes.Should().NotBeEmpty();
        bytes[^1].Should().Be(0x0A);
        bytes.Should().NotContain(0x0D);
        EncodingSped.Latin1.GetBytes(EncodingSped.Latin1.GetString(bytes)).Should().Equal(bytes);
        return bytes;
    }
}
