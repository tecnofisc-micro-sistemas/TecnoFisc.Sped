using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoF;

public sealed class RegistroF211Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroF211).Assembly);

    [Fact]
    public void Atributo_DeclaraF211_Nivel4_BlocoF()
    {
        var atributo = typeof(RegistroF211).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("F211");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("F");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroF211Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("F211".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("F211");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
        meta.Campos.Select(c => c.Nome).Should().Equal(["NumProc", "IndProc"]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("F211".AsSpan(), out var meta);
        var registro = (RegistroF211)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "12345/2023".AsSpan());  // NumProc
        meta.Campos[1].Definidor(registro, "1".AsSpan());           // IndProc

        registro.NumProc.Should().Be("12345/2023");
        registro.IndProc.Should().Be(IndicadorOrigemProcesso.JusticaFederal);
    }

    [Theory]
    [InlineData("1", IndicadorOrigemProcesso.JusticaFederal)]
    [InlineData("3", IndicadorOrigemProcesso.ReceitaFederal)]
    [InlineData("9", IndicadorOrigemProcesso.Outros)]
    public void Definidor_IndProc_ConverteTodosOsValores(string texto, IndicadorOrigemProcesso esperado)
    {
        _catalogo.TentarObter("F211".AsSpan(), out var meta);
        var registro = (RegistroF211)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, texto.AsSpan());

        registro.IndProc.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|F211|12345/2023|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ProcessoAdministrativo_PreservaTextoCanonico()
    {
        const string sped = "|F211|COSIT-RFB-2021-001|3|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}
