using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Parser;

public sealed class SnifferSpedFiscalTests
{
    private const string LinhaEcfCompleta =
        "|0000|LECF|0011|11111111000191|EMPRESA TESTE|0|0|||01012025|31122025|N||0||";

    [Fact]
    public void MetadadosFiscaisArquivoSped_ArmazenaIdentificacaoECamposFiscais()
    {
        var identificacao = new MetadadosArquivoSped(
            ProjetoSped.EfdContribuicoes,
            6,
            EncodingSped.Latin1,
            "|0000|006|0|||01022025|28022025|EMPRESA|11222333000181|MG|3126901||00|2|",
            "006");

        var metadados = new MetadadosFiscaisArquivoSped(
            identificacao,
            Cnpj.Create("11222333000181"),
            new DateOnly(2025, 2, 1),
            new DateOnly(2025, 2, 28));

        metadados.Identificacao.Should().BeSameAs(identificacao);
        metadados.Projeto.Should().Be(ProjetoSped.EfdContribuicoes);
        metadados.VersaoLeiaute.Should().Be(6);
        metadados.Cnpj.Should().Be(Cnpj.Create("11222333000181"));
        metadados.DataInicial.Should().Be(new DateOnly(2025, 2, 1));
        metadados.DataFinal.Should().Be(new DateOnly(2025, 2, 28));
    }

    [Fact]
    public async Task IdentificarAsync_EfdContribuicoes_RetornaIdentificacaoCnpjEPeriodo()
    {
        await using var stream = Sped("|0000|006|0|||01022025|28022025|EMPRESA|11222333000181|MG|3126901||00|2|\r\n|0001|0|\r\n");

        var metadados = await SnifferSpedFiscal.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.EfdContribuicoes);
        metadados.VersaoLeiaute.Should().Be(6);
        metadados.CodigoVersaoDeclarado.Should().Be("006");
        metadados.Cnpj.Should().Be(Cnpj.Create("11222333000181"));
        metadados.DataInicial.Should().Be(new DateOnly(2025, 2, 1));
        metadados.DataFinal.Should().Be(new DateOnly(2025, 2, 28));
        stream.Position.Should().Be(0);
    }

    [Fact]
    public async Task IdentificarAsync_EfdIcmsIpi_RetornaIdentificacaoCnpjEPeriodo()
    {
        await using var stream = Sped("|0000|015|1|01012021|31012021|EMPRESA|11222333000181||MG|123456789|3139409|||B|1|\n");

        var metadados = await SnifferSpedFiscal.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.EfdIcmsIpi);
        metadados.VersaoLeiaute.Should().Be(15);
        metadados.CodigoVersaoDeclarado.Should().Be("015");
        metadados.Cnpj.Should().Be(Cnpj.Create("11222333000181"));
        metadados.DataInicial.Should().Be(new DateOnly(2021, 1, 1));
        metadados.DataFinal.Should().Be(new DateOnly(2021, 1, 31));
        stream.Position.Should().Be(0);
    }

    [Fact]
    public async Task IdentificarAsync_Ecd_RetornaIdentificacaoCnpjEPeriodo()
    {
        await using var stream = Sped("|0000|LECD|01012023|31122023|EMPRESA|11222333000181|ES|\r\n|0001|0|\r\n");

        var metadados = await SnifferSpedFiscal.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.Ecd);
        metadados.VersaoLeiaute.Should().Be(9);
        metadados.CodigoVersaoDeclarado.Should().Be("LECD");
        metadados.Cnpj.Should().Be(Cnpj.Create("11222333000181"));
        metadados.DataInicial.Should().Be(new DateOnly(2023, 1, 1));
        metadados.DataFinal.Should().Be(new DateOnly(2023, 12, 31));
        stream.Position.Should().Be(0);
    }

    [Fact]
    public async Task IdentificarAsync_Lecf_RetornaEcfVersaoCnpjEPeriodo()
    {
        await using var stream = Sped(LinhaEcfCompleta + "\r\n|0001|0|\r\n");

        var metadados = await SnifferSpedFiscal.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.Ecf);
        metadados.VersaoLeiaute.Should().Be(11);
        metadados.CodigoVersaoDeclarado.Should().Be("0011");
        metadados.Cnpj.Should().Be(Cnpj.Create("11111111000191"));
        metadados.DataInicial.Should().Be(new DateOnly(2025, 1, 1));
        metadados.DataFinal.Should().Be(new DateOnly(2025, 12, 31));
        stream.Position.Should().Be(0);
    }

    [Fact]
    public async Task IdentificarAsync_LecfComCnpjEDatasInvalidos_MantemProjetoESemDadosFiscais()
    {
        const string linha =
            "|0000|LECF|0012|00000000000000|EMPRESA TESTE|0|0|||99022025|31022025|N||0||";
        await using var stream = Sped(linha);

        var metadados = await SnifferSpedFiscal.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.Ecf);
        metadados.VersaoLeiaute.Should().Be(12);
        metadados.Cnpj.Should().BeNull();
        metadados.DataInicial.Should().BeNull();
        metadados.DataFinal.Should().BeNull();
        stream.Position.Should().Be(0);
    }

    [Fact]
    public async Task IdentificarAsync_EntradaDesconhecida_PreservaIdentificacaoENaoRetornaDadosFiscais()
    {
        await using var stream = Sped("|0000|999|0|01012025|31012025|EMPRESA|11222333000181|\r\n");

        var metadados = await SnifferSpedFiscal.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.Desconhecido);
        metadados.VersaoLeiaute.Should().Be(0);
        metadados.Cnpj.Should().BeNull();
        metadados.DataInicial.Should().BeNull();
        metadados.DataFinal.Should().BeNull();
        stream.Position.Should().Be(0);
    }

    [Fact]
    public async Task IdentificarAsync_CamposFiscaisInvalidos_NaoFalhaENaoRetornaCamposInvalidos()
    {
        await using var stream = Sped("|0000|006|0|||99022025|31022025|EMPRESA|00000000000000|MG|3126901||00|2|\r\n");

        var metadados = await SnifferSpedFiscal.IdentificarAsync(stream, TestContext.Current.CancellationToken);

        metadados.Projeto.Should().Be(ProjetoSped.EfdContribuicoes);
        metadados.VersaoLeiaute.Should().Be(6);
        metadados.Cnpj.Should().BeNull();
        metadados.DataInicial.Should().BeNull();
        metadados.DataFinal.Should().BeNull();
        stream.Position.Should().Be(0);
    }

    private static MemoryStream Sped(string conteudo)
        => new(EncodingSped.Latin1.GetBytes(conteudo), writable: false);
}
