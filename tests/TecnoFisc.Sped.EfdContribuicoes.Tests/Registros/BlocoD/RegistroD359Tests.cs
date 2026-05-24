using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoD;

public sealed class RegistroD359Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD359).Assembly);

    [Fact]
    public void Atributo_DeclaraD359_Nivel4_BlocoD()
    {
        var atributo = typeof(RegistroD359).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D359");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD359ComDoisCamposNaOrdem()
    {
        _catalogo.TentarObter("D359".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D359");
        meta.Campos.Select(c => c.Nome).Should().Equal(["NumProc", "IndProc"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
        meta.Campos[0].Tamanho.Should().Be(20);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // NumProc
        meta.Campos[1].Tamanho.Should().Be(1);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // IndProc
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D359".AsSpan(), out var meta);
        var registro = (RegistroD359)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "PROC-2021-099".AsSpan());  // NumProc
        meta.Campos[1].Definidor(registro, "3".AsSpan());              // IndProc

        registro.NumProc.Should().Be("PROC-2021-099");
        registro.IndProc.Should().Be(IndicadorOrigemProcesso.ReceitaFederal);
    }

    [Theory]
    [InlineData(IndicadorOrigemProcesso.JusticaFederal, "1")]
    [InlineData(IndicadorOrigemProcesso.ReceitaFederal, "3")]
    [InlineData(IndicadorOrigemProcesso.Outros, "9")]
    public void Serializar_IndProc_RetornaCodigoSpedCorreto(
        IndicadorOrigemProcesso origem, string esperado)
    {
        _catalogo.TentarObter("D359".AsSpan(), out var meta);
        var registro = (RegistroD359)meta!.Fabrica();
        registro.IndProc = origem;

        meta.Campos[1].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|D359|PROC-2021-099|3|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ProcessoJudicial_PreservaTextoCanonico()
    {
        const string sped = "|D359|JUD-TRF3-2019-001|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.ReadStreamingAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.WriteAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}
