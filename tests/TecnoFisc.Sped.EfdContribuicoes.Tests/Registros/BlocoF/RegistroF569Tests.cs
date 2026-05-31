using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoF;

public sealed class RegistroF569Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroF569).Assembly);

    [Fact]
    public void Atributo_DeclaraF569_Nivel4_BlocoF()
    {
        var atributo = typeof(RegistroF569).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("F569");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("F");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroF569ComDoisCamposNaOrdem()
    {
        _catalogo.TentarObter("F569".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("F569");
        meta.Campos.Select(c => c.Nome).Should().Equal(["NumProc", "IndProc"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
        meta.Campos[0].Tamanho.Should().Be(20);
        meta.Campos[0].Obrigatorio.Should().BeTrue();
        meta.Campos[1].Obrigatorio.Should().BeTrue();
    }

    [Fact]
    public void Definidor_AtribuiNumProcEIndProc()
    {
        _catalogo.TentarObter("F569".AsSpan(), out var meta);
        var registro = (RegistroF569)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "10166.720001/2021-55".AsSpan());
        meta.Campos[1].Definidor(registro, "3".AsSpan());

        registro.NumProc.Should().Be("10166.720001/2021-55");
        registro.IndProc.Should().Be(IndicadorOrigemProcesso.ReceitaFederal);
    }

    [Theory]
    [InlineData("1", IndicadorOrigemProcesso.JusticaFederal)]
    [InlineData("3", IndicadorOrigemProcesso.ReceitaFederal)]
    [InlineData("9", IndicadorOrigemProcesso.Outros)]
    public void Definidor_IndProc_AtribuiEnumCorreto(string codigo, IndicadorOrigemProcesso esperado)
    {
        _catalogo.TentarObter("F569".AsSpan(), out var meta);
        var registro = (RegistroF569)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, codigo.AsSpan());

        registro.IndProc.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|F569|10166.720001/2021-55|3|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_JusticaFederal_PreservaTextoCanonico()
    {
        const string sped = "|F569|5001234-12.2022.4.01.3400|1|\r\n";

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
