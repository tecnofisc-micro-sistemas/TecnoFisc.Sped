using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco0;

public sealed class Registro0450Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0450).Assembly);

    [Fact]
    public void Atributo_Declara0450_Nivel3_Bloco0()
    {
        var atributo = typeof(Registro0450).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0450");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0450Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("0450".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0450");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodInf", "Txt"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0450".AsSpan(), out var meta);
        var registro = (Registro0450)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "INF001".AsSpan());
        meta.Campos[1].Definidor(registro, "ADE 123456 de 01/01/2021 - Suspensão de IPI".AsSpan());

        registro.CodInf.Should().Be("INF001");
        registro.Txt.Should().Be("ADE 123456 de 01/01/2021 - Suspensão de IPI");
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0450|INF001|ADE 123456 de 01/01/2021 - Suspensão de IPI|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_TextoSimples_PreservaTextoCanonico()
    {
        const string sped = "|0450|001|Nota fiscal referenciada conforme legislação|\r\n";

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
