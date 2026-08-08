using System.Text;

using TecnoFisc.Sped.Core.Erros;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Ecf.Registros.Bloco0;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Ecf.Tests.Versionamento;

public sealed class LeiauteForaDaFaixaTests
{
    [Theory]
    [InlineData("0008", true)]
    [InlineData("0012", true)]
    [InlineData("0007", false)]
    [InlineData("0013", false)]
    [InlineData("ABCD", false)]
    public void IsLeiauteConhecido_RefleteAFaixaDoLayoutEcf(string codVer, bool esperado)
        => new Registro0000 { CodVer = codVer }.IsLeiauteConhecido.Should().Be(esperado);

    [Fact]
    public async Task Leitura_DeLeiauteForaDaFaixa_AvisaNoZeroZeroZeroZeroSemAbortar()
    {
        var registros = await ReadAsync(13, "|0001|0|");

        var zero = registros.OfType<Registro0000>().Single();
        var erro = zero.ErrosDeFormato.Should().ContainSingle().Which;
        erro.Mensagem.Should().Contain("fora da faixa");
        erro.ValorBruto.Should().Be("0013");
        registros.Should().Contain(registro => registro.Codigo == "0001");
    }

    [Fact]
    public async Task Leitura_DeLeiauteConhecido_NaoAvisa()
    {
        var registros = await ReadAsync(12, "|0001|0|");

        registros.OfType<Registro0000>().Single().ErrosDeFormato.Should().BeEmpty();
    }

    [Fact]
    public async Task Leitura_DeLeiaute13ComRegistroNovo_ViraSentinelaEmVezDeAbortar()
    {
        var registros = await ReadAsync(13, "|X999|conteudo novo|");

        var sentinela = registros.OfType<RegistroNaoReconhecido>().Should().ContainSingle().Subject;
        sentinela.Codigo.Should().Be("X999");
        sentinela.LinhaCrua.Should().Be("|X999|conteudo novo|");
    }

    [Fact]
    public async Task Leitura_DeLeiauteConhecidoComCodigoDesconhecido_ContinuaAbortando()
    {
        var act = async () => await ReadAsync(12, "|X999|conteudo novo|");

        await act.Should().ThrowAsync<ErroLayoutSpedException>();
    }

    internal static async Task<List<RegistroSped>> ReadAsync(int versao, string linha)
    {
        string arquivo =
            $"|0000|LECF|{versao:0000}|11111111000191|EMPRESA TESTE|0|0|||01012025|31122025|N||0||\r\n" +
            linha + "\r\n" +
            "|9999|3|\r\n";
        var registros = new List<RegistroSped>();
        await using var stream = new MemoryStream(Encoding.Latin1.GetBytes(arquivo));
        await foreach (var registro in new ParserEcf().ReadStreamingAsync(
            stream, TestContext.Current.CancellationToken))
            registros.Add(registro);
        return registros;
    }
}
