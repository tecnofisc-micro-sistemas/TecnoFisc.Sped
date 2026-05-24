using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC198Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC198).Assembly);

    [Fact]
    public void Atributo_DeclaraCodigoC198_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC198).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C198");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC198ComDoisCamposNaOrdem()
    {
        _catalogo.TentarObter("C198".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C198");
        meta.Campos.Select(c => c.Nome).Should().Equal(["NumProc", "IndProc"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
        meta.Campos[0].Tamanho.Should().Be(20);
        meta.Campos[0].Obrigatorio.Should().BeTrue();
        meta.Campos[1].Obrigatorio.Should().BeTrue();
    }

    [Fact]
    public void Definidor_AtribuiNumProcEIndProc()
    {
        _catalogo.TentarObter("C198".AsSpan(), out var meta);
        var registro = (RegistroC198)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "10166.720001/2020-31".AsSpan());
        meta.Campos[1].Definidor(registro, "9".AsSpan());

        registro.NumProc.Should().Be("10166.720001/2020-31");
        registro.IndProc.Should().Be(IndicadorOrigemProcesso.Outros);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C198|10166.720001/2020-31|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ProcessoAdministrativo_PreservaTextoCanonico()
    {
        const string sped = "|C198|10166.720001/2020-31|3|\r\n";

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
