using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Ecf.Tests.Parser;

public sealed class ParserEcfFixtureAnonimizadaTests
{
    [Theory]
    [InlineData(8, 171)]
    [InlineData(9, 164)]
    [InlineData(10, 164)]
    [InlineData(11, 166)]
    public async Task FixtureAnonimizadaLeAte9999SemCodigoDesconhecido(
        int versao,
        int quantidadeEsperada)
    {
        string diretorio = Path.Combine(AppContext.BaseDirectory, "Fixtures", $"Layout{versao}");
        string caminho = Directory.EnumerateFiles(diretorio, "*.txt", SearchOption.TopDirectoryOnly)
            .Should().ContainSingle().Which;
        byte[] conteudo = await File.ReadAllBytesAsync(
            caminho,
            TestContext.Current.CancellationToken);

        await using var entrada = new MemoryStream(conteudo, writable: false);
        var registros = new List<RegistroSped>();
        await foreach (var registro in new ParserEcf().ReadStreamingAsync(
            entrada,
            TestContext.Current.CancellationToken))
        {
            registros.Add(registro);
        }

        registros.Should().HaveCount(quantidadeEsperada);
        registros[0].Should().BeOfType<Registro0000>().Which.VersaoLeiaute.Should().Be(versao);
        registros.Should().ContainSingle(registro => registro.Codigo == "0000");
        registros[^1].Codigo.Should().Be("9999");
        registros.Should().NotContain(registro => registro is RegistroNaoReconhecido);
    }
}
