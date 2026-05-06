using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC110Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC110).Assembly);

    [Fact]
    public void Atributo_DeclaraCodigoC110_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC110).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C110");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC110ComDoisCamposNaOrdem()
    {
        _catalogo.TentarObter("C110".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C110");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodInf", "TxtComp"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
        meta.Campos[0].Tamanho.Should().Be(6);
        meta.Campos[0].Obrigatorio.Should().BeTrue();
        meta.Campos[1].Obrigatorio.Should().BeFalse();
    }

    [Fact]
    public void Definidor_AtribuiCodInfETxtComp()
    {
        _catalogo.TentarObter("C110".AsSpan(), out var meta);
        var registro = (RegistroC110)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "INF001".AsSpan());
        meta.Campos[1].Definidor(registro, "Suspensao das contribuicoes".AsSpan());

        registro.CodInf.Should().Be("INF001");
        registro.TxtComp.Should().Be("Suspensao das contribuicoes");
    }

    [Fact]
    public void Definidor_TxtCompVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C110".AsSpan(), out var meta);
        var registro = (RegistroC110)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.TxtComp.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C110|INF001|Forma de pagamento: a prazo|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemTxtComp_PreservaTextoCanonico()
    {
        const string sped = "|C110|INF001||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerStreamingAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}
