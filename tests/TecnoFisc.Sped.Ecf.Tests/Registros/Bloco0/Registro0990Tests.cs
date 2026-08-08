using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Ecf.Tests.Manifesto;
using TecnoFisc.Sped.Txt.Engine.Enums;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.Bloco0;

public sealed class Registro0990Tests
{
    [Fact]
    public void Registro_ConformeManifesto()
    {
        AssertRegistroEcf.ConformsToManifest(new Registro0990(), "0990", "1:1");
    }

    [Fact]
    public void Parser_LeQuantidadeDeLinhas()
    {
        var resultado = new ParserEcf().ParseLinha("|0990|9|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<Registro0990>()
            .Which.QtdLin.Should().Be(9);
    }

    [Fact]
    public void Parser_QuantidadeInvalida_RegistraErroDeFormato()
    {
        var resultado = new ParserEcf().ParseLinha("|0990|INVALIDA|");

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeOfType<Registro0990>()
            .Which.ErrosDeFormato.Should().ContainSingle(erro =>
                erro.Campo == "QTD_LIN" && erro.ValorBruto == "INVALIDA");
    }

    [Fact]
    public async Task ReadAsync_FixtureCompleta_MaterializaOrdemHierarquiaEValores()
    {
        string caminho = Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "Sinteticas",
            "bloco-0.txt");
        byte[] bytes = await File.ReadAllBytesAsync(caminho, TestContext.Current.CancellationToken);
        bytes.Should().NotBeEmpty();
        bytes[^1].Should().Be(0x0A);
        bytes.Should().NotContain(0x0D);
        EncodingSped.Latin1.GetBytes(EncodingSped.Latin1.GetString(bytes)).Should().Equal(bytes);

        await using var entrada = new MemoryStream(bytes, writable: false);
        var arquivo = await new ParserEcf().ReadAsync(
            entrada,
            TestContext.Current.CancellationToken);

        arquivo.Bloco0.Registros.Select(registro => registro.Codigo).Should().Equal(
            "0000", "0001", "0010", "0020", "0021", "0030", "0035", "0930", "0990");
        arquivo.EnumerarRegistros().Should().Equal(arquivo.Bloco0.Registros);

        var r0000 = arquivo.Bloco0.Registros[0].Should().BeOfType<Registro0000>().Which;
        var r0001 = arquivo.Bloco0.Registros[1].Should().BeOfType<Registro0001>().Which;
        var r0010 = arquivo.Bloco0.Registros[2].Should().BeOfType<Registro0010>().Which;
        var r0020 = arquivo.Bloco0.Registros[3].Should().BeOfType<Registro0020>().Which;
        var r0021 = arquivo.Bloco0.Registros[4].Should().BeOfType<Registro0021>().Which;
        var r0030 = arquivo.Bloco0.Registros[5].Should().BeOfType<Registro0030>().Which;
        var r0035 = arquivo.Bloco0.Registros[6].Should().BeOfType<Registro0035>().Which;
        var r0930 = arquivo.Bloco0.Registros[7].Should().BeOfType<Registro0930>().Which;
        var r0990 = arquivo.Bloco0.Registros[8].Should().BeOfType<Registro0990>().Which;

        AssertRegistroEcf.ConformsToManifest(r0000, "0000", "1:1", null, r0001, r0990);
        AssertRegistroEcf.ConformsToManifest(
            r0001,
            "0001",
            "1:1",
            r0000,
            r0010,
            r0020,
            r0021,
            r0030,
            r0035,
            r0930);
        AssertRegistroEcf.ConformsToManifest(r0010, "0010", "1:1", r0001);
        AssertRegistroEcf.ConformsToManifest(r0020, "0020", "1:1", r0001);
        AssertRegistroEcf.ConformsToManifest(r0021, "0021", "0:1", r0001);
        AssertRegistroEcf.ConformsToManifest(r0030, "0030", "1:1", r0001);
        AssertRegistroEcf.ConformsToManifest(r0035, "0035", "0:N", r0001);
        AssertRegistroEcf.ConformsToManifest(r0930, "0930", "1:N", r0001);
        AssertRegistroEcf.ConformsToManifest(r0990, "0990", "1:1", r0000);

        r0001.IndDad.Should().Be(IndicadorMovimentoBloco.ComDados);
        r0020.IndQteScp.Should().Be(1);
        r0020.Cebas.Should().BeNull();
        r0030.CodNat.Should().Be("0204");
        r0035.CodScp.ToString().Should().Be("11222333000181");
        r0930.IdentCpfCnpj.Should().Be("12345678909");
        r0990.QtdLin.Should().Be(9);
    }
}
