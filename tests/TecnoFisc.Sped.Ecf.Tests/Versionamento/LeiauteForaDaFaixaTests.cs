using System.Text;

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
        zero.ErrosDeFormato.Should().ContainSingle()
            .Which.Mensagem.Should().Contain("fora da faixa");
        registros.Should().Contain(registro => registro.Codigo == "0001");
    }

    [Fact]
    public async Task Leitura_DeLeiauteConhecido_NaoAvisa()
    {
        var registros = await ReadAsync(12, "|0001|0|");

        registros.OfType<Registro0000>().Single().ErrosDeFormato.Should().BeEmpty();
    }

    internal static async Task<List<RegistroSped>> ReadAsync(int versao, string linha)
    {
        string arquivo =
            $"|0000|LECF|{versao:0000}|11111111000191|EMPRESA TESTE|0|0|||01012025|31122025|N||0||\r\n" +
            linha + "\r\n" +
            "|9999|3|\r\n";
        var registros = new List<RegistroSped>();
        await using var stream = new MemoryStream(Encoding.Latin1.GetBytes(arquivo));
        await foreach (var registro in new ParserEcf().ReadStreamingAsync(stream))
            registros.Add(registro);
        return registros;
    }
}
