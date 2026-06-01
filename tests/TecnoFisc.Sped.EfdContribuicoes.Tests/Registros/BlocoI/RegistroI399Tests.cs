using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoI;

public sealed class RegistroI399Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroI399).Assembly);

    [Fact]
    public void Atributo_DeclaraI399_Nivel6_BlocoI()
    {
        var atributo = typeof(RegistroI399).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I399");
        atributo.Nivel.Should().Be(6);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI399Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("I399".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I399");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
        meta.Campos.Select(c => c.Nome).Should().Equal(["NumProc", "IndProc"]);
        meta.Campos[0].Tamanho.Should().Be(20);
        meta.Campos[0].Obrigatorio.Should().BeTrue();
        meta.Campos[1].Tamanho.Should().Be(1);
        meta.Campos[1].Obrigatorio.Should().BeTrue();
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I399".AsSpan(), out var meta);
        var registro = (RegistroI399)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "12345-67.2020.8.01.0000".AsSpan()); // NumProc
        meta.Campos[1].Definidor(registro, "9".AsSpan());                        // IndProc

        registro.NumProc.Should().Be("12345-67.2020.8.01.0000");
        registro.IndProc.Should().Be(IndicadorOrigemProcesso.Outros);
    }

    [Theory]
    [InlineData("1", IndicadorOrigemProcesso.JusticaFederal)]
    [InlineData("3", IndicadorOrigemProcesso.ReceitaFederal)]
    [InlineData("9", IndicadorOrigemProcesso.Outros)]
    public void Definidor_IndProc_AtribuiEnumCorreto(string codigo, IndicadorOrigemProcesso esperado)
    {
        _catalogo.TentarObter("I399".AsSpan(), out var meta);
        var registro = (RegistroI399)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, codigo.AsSpan());

        registro.IndProc.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|I399|12345-67.2020.8.01.0000|9|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_JusticaFederal_PreservaTextoCanonico()
    {
        const string sped = "|I399|99887-66.2022.4.03.0000|1|\r\n";

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
