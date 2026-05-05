using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC601Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC601).Assembly);

    [Fact]
    public void Atributo_DeclaraC601_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC601).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C601");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC601ComSeisCamposNaOrdem()
    {
        _catalogo.TentarObter("C601".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C601");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CstPis", "VlItem", "VlBcPis", "AliqPis", "VlPis", "CodCta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // CstPis
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // VlItem
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // VlBcPis
        meta.Campos[3].Tamanho.Should().Be(8);
        meta.Campos[3].Obrigatorio.Should().BeTrue();   // AliqPis
        meta.Campos[4].Obrigatorio.Should().BeTrue();   // VlPis
        meta.Campos[5].Tamanho.Should().Be(255);
        meta.Campos[5].Obrigatorio.Should().BeFalse();  // CodCta
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C601".AsSpan(), out var meta);
        var registro = (RegistroC601)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "49".AsSpan());           // CstPis
        meta.Campos[1].Definidor(registro, "5000,00".AsSpan());      // VlItem
        meta.Campos[2].Definidor(registro, "5000,00".AsSpan());      // VlBcPis
        meta.Campos[3].Definidor(registro, "1,6500".AsSpan());       // AliqPis
        meta.Campos[4].Definidor(registro, "82,50".AsSpan());        // VlPis
        meta.Campos[5].Definidor(registro, "3.1.01.001".AsSpan());   // CodCta

        registro.CstPis.Should().Be(49);
        registro.VlItem.Should().Be(5000m);
        registro.VlBcPis.Should().Be(5000m);
        registro.AliqPis.Should().Be(1.65m);
        registro.VlPis.Should().Be(82.50m);
        registro.CodCta.Should().Be("3.1.01.001");
    }

    [Fact]
    public void Definidor_CodCtaVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C601".AsSpan(), out var meta);
        var registro = (RegistroC601)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta

        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C601|49|5000,00|5000,00|1,6500|82,50|3.1.01.001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCodCta_PreservaTextoCanonico()
    {
        const string sped = "|C601|49|5000,00|5000,00|1,6500|82,50||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CstIsento_PreservaTextoCanonico()
    {
        const string sped = "|C601|99|3000,00|3000,00|0,0000|0,00||\r\n";

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
