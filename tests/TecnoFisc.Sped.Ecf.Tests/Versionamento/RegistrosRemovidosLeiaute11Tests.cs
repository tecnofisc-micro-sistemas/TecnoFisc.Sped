using System.Text;

using TecnoFisc.Sped.Ecf.Generated;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Ecf.Tests.Versionamento;

public sealed class RegistrosRemovidosLeiaute11Tests
{
    [Fact]
    public void Catalogo_ConheceX300ComoDescontinuadoNoLeiaute11()
    {
        new CatalogoSpedGerado().TentarObter("X300", out var metadados).Should().BeTrue();

        metadados!.Bloco.Should().Be("X");
        metadados.DescontinuadoEm.Should().Be((int)LayoutEcf.V011);
        metadados.IntroduzidoEm.Should().Be(0);
        metadados.Campos.Should().BeEmpty();
    }

    [Fact]
    public async Task Leitura_DeLeiaute10ComX300_NaoAborta()
    {
        var registros = await ReadAsync(10, "|X300|000001|EXPORTACAO|1234,56|");

        registros.Should().ContainSingle(registro => registro.Codigo == "X300");
        registros.OfType<RegistroNaoReconhecido>().Should().BeEmpty();
    }

    internal static readonly string[] CodigosRemovidos =
        ["X291", "X300", "X305", "X310", "X320", "X325", "X330"];

    [Fact]
    public void Catalogo_ConheceOsSeteRemovidosENenhumTemCampoModelado()
    {
        var catalogo = new CatalogoSpedGerado();

        foreach (var codigo in CodigosRemovidos)
        {
            catalogo.TentarObter(codigo, out var metadados).Should().BeTrue($"{codigo} precisa ser reconhecido");
            metadados!.DescontinuadoEm.Should().Be((int)LayoutEcf.V011, $"{codigo} saiu no leiaute 11");
            metadados.Bloco.Should().Be("X");
        }
    }

    [Fact]
    public void Catalogo_TemOsCentoEOitentaDoLeiaute12MaisOsSeteRemovidos()
        => new CatalogoSpedGerado().EnumerarRegistros().Should().HaveCount(187);

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
