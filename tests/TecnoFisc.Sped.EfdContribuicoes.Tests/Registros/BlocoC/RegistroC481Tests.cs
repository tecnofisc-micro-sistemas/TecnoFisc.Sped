using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC481Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC481).Assembly);

    [Fact]
    public void Atributo_DeclaraC481_Nivel5_BlocoC()
    {
        var atributo = typeof(RegistroC481).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C481");
        atributo.Nivel.Should().Be(5);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC481ComNoveCamposNaOrdem()
    {
        _catalogo.TentarObter("C481".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C481");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["CstPis", "VlItem", "VlBcPis", "AliqPis", "QuantBcPis", "AliqPisQuant", "VlPis", "CodItem", "CodCta"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();    // CstPis
        meta.Campos[1].Obrigatorio.Should().BeTrue();    // VlItem
        meta.Campos[2].Obrigatorio.Should().BeFalse();   // VlBcPis
        meta.Campos[3].Tamanho.Should().Be(8);
        meta.Campos[3].Obrigatorio.Should().BeFalse();   // AliqPis
        meta.Campos[6].Obrigatorio.Should().BeFalse();   // VlPis
        meta.Campos[7].Tamanho.Should().Be(60);
        meta.Campos[7].Obrigatorio.Should().BeFalse();   // CodItem
        meta.Campos[8].Tamanho.Should().Be(255);
        meta.Campos[8].Obrigatorio.Should().BeFalse();   // CodCta
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C481".AsSpan(), out var meta);
        var registro = (RegistroC481)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01".AsSpan());          // CstPis
        meta.Campos[1].Definidor(registro, "1500,00".AsSpan());     // VlItem
        meta.Campos[2].Definidor(registro, "1200,00".AsSpan());     // VlBcPis
        meta.Campos[3].Definidor(registro, "1,6500".AsSpan());      // AliqPis
        meta.Campos[4].Definidor(registro, "0,000".AsSpan());       // QuantBcPis
        meta.Campos[5].Definidor(registro, "0,0000".AsSpan());      // AliqPisQuant
        meta.Campos[6].Definidor(registro, "16,50".AsSpan());       // VlPis
        meta.Campos[7].Definidor(registro, "ITEM001".AsSpan());     // CodItem
        meta.Campos[8].Definidor(registro, "CONTA001".AsSpan());    // CodCta

        registro.CstPis.Should().Be(1);
        registro.VlItem.Should().Be(1500m);
        registro.VlBcPis.Should().Be(1200m);
        registro.AliqPis.Should().Be(1.6500m);
        registro.QuantBcPis.Should().Be(0m);
        registro.AliqPisQuant.Should().Be(0m);
        registro.VlPis.Should().Be(16.50m);
        registro.CodItem.Should().Be("ITEM001");
        registro.CodCta.Should().Be("CONTA001");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("C481".AsSpan(), out var meta);
        var registro = (RegistroC481)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcPis
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqPis
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // QuantBcPis
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqPisQuant
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // VlPis
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty); // CodItem
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta

        registro.VlBcPis.Should().BeNull();
        registro.AliqPis.Should().BeNull();
        registro.QuantBcPis.Should().BeNull();
        registro.AliqPisQuant.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.CodItem.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C481|1|1500,00|1200,00|1,6500|0,000|0,0000|16,50|ITEM001|CONTA001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|C481|7|2000,00||||||||\r\n";

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
