using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco1;

public sealed class Registro1809Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1809).Assembly);

    [Fact]
    public void Atributo_Declara1809_Nivel3_Bloco1()
    {
        var atributo = typeof(Registro1809).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1809");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1809Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("1809".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1809");
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
        _catalogo.TentarObter("1809".AsSpan(), out var meta);
        var registro = (Registro1809)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "10166.720001/2020-31".AsSpan());  // NumProc
        meta.Campos[1].Definidor(registro, "1".AsSpan());                     // IndProc

        registro.NumProc.Should().Be("10166.720001/2020-31");
        registro.IndProc.Should().Be(IndicadorOrigemProcesso.JusticaFederal);
    }

    [Theory]
    [InlineData("1", IndicadorOrigemProcesso.JusticaFederal)]
    [InlineData("3", IndicadorOrigemProcesso.ReceitaFederal)]
    [InlineData("9", IndicadorOrigemProcesso.Outros)]
    public void Definidor_IndProc_AtribuiEnumCorreto(string codigo, IndicadorOrigemProcesso esperado)
    {
        _catalogo.TentarObter("1809".AsSpan(), out var meta);
        var registro = (Registro1809)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, codigo.AsSpan());

        registro.IndProc.Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorOrigemProcesso.JusticaFederal, "1")]
    [InlineData(IndicadorOrigemProcesso.ReceitaFederal, "3")]
    [InlineData(IndicadorOrigemProcesso.Outros, "9")]
    public void Serializar_IndProc_RetornaCodigoSpedCorreto(
        IndicadorOrigemProcesso origem, string esperado)
    {
        _catalogo.TentarObter("1809".AsSpan(), out var meta);
        var registro = (Registro1809)meta!.Fabrica();
        registro.IndProc = origem;

        meta.Campos[1].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1809|10166.720001/2020-31|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ProcessoAdministrativoRFB_PreservaTextoCanonico()
    {
        const string sped = "|1809|PA/2023/00012345678901|3|\r\n";

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
