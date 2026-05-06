using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoF;

public sealed class RegistroF519Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroF519).Assembly);

    [Fact]
    public void Atributo_DeclaraF519_Nivel4_BlocoF()
    {
        var atributo = typeof(RegistroF519).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("F519");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("F");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroF519ComDoisCamposNaOrdem()
    {
        _catalogo.TentarObter("F519".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("F519");
        meta.Campos.Select(c => c.Nome).Should().Equal(["NumProc", "IndProc"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
        meta.Campos[0].Tamanho.Should().Be(20);
        meta.Campos[0].Obrigatorio.Should().BeTrue();
        meta.Campos[1].Obrigatorio.Should().BeTrue();
    }

    [Fact]
    public void Definidor_AtribuiNumProcEIndProc()
    {
        _catalogo.TentarObter("F519".AsSpan(), out var meta);
        var registro = (RegistroF519)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "10166.720001/2022-10".AsSpan());
        meta.Campos[1].Definidor(registro, "9".AsSpan());

        registro.NumProc.Should().Be("10166.720001/2022-10");
        registro.IndProc.Should().Be(IndicadorOrigemProcesso.Outros);
    }

    [Theory]
    [InlineData("1", IndicadorOrigemProcesso.JusticaFederal)]
    [InlineData("3", IndicadorOrigemProcesso.ReceitaFederal)]
    [InlineData("9", IndicadorOrigemProcesso.Outros)]
    public void Definidor_IndProc_AtribuiEnumCorreto(string codigo, IndicadorOrigemProcesso esperado)
    {
        _catalogo.TentarObter("F519".AsSpan(), out var meta);
        var registro = (RegistroF519)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, codigo.AsSpan());

        registro.IndProc.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|F519|10166.720001/2022-10|9|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ReceitaFederal_PreservaTextoCanonico()
    {
        const string sped = "|F519|16327.720001/2023-44|3|\r\n";

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
