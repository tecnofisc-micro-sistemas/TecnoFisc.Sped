using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoA;

public sealed class RegistroA110Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroA110).Assembly);

    [Fact]
    public void Atributo_DeclaraCodigoA110_Nivel4_BlocoA()
    {
        var atributo = typeof(RegistroA110).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("A110");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("A");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroA110ComDoisCamposNaOrdem()
    {
        _catalogo.TentarObter("A110".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("A110");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodInf", "TxtCompl"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
        meta.Campos[0].Tamanho.Should().Be(6);
        meta.Campos[0].Obrigatorio.Should().BeTrue();
        meta.Campos[1].Obrigatorio.Should().BeFalse();
    }

    [Fact]
    public void Definidor_AtribuiCodInfETxtCompl()
    {
        _catalogo.TentarObter("A110".AsSpan(), out var meta);
        var registro = (RegistroA110)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "INF001".AsSpan());
        meta.Campos[1].Definidor(registro, "Suspensao das contribuicoes".AsSpan());

        registro.CodInf.Should().Be("INF001");
        registro.TxtCompl.Should().Be("Suspensao das contribuicoes");
    }

    [Fact]
    public void Definidor_TxtComplVazio_DevolveNulo()
    {
        _catalogo.TentarObter("A110".AsSpan(), out var meta);
        var registro = (RegistroA110)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.TxtCompl.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|A110|INF001|Suspensao das contribuicoes|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemTxtCompl_PreservaTextoCanonico()
    {
        const string sped = "|A110|INF001||\r\n";

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
