using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC381Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC381).Assembly);

    [Fact]
    public void Atributo_DeclaraC381_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC381).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C381");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC381ComNoveCamposNaOrdem()
    {
        _catalogo.TentarObter("C381".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C381");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CstPis", "CodItem", "VlItem", "VlBcPis",
            "AliqPis", "QuantBcPis", "AliqPisQuant", "VlPis", "CodCta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // CstPis
        meta.Campos[1].Tamanho.Should().Be(60);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // CodItem
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // VlItem
        meta.Campos[4].Tamanho.Should().Be(8);          // AliqPis tamanho fixo
        meta.Campos[7].Obrigatorio.Should().BeTrue();   // VlPis
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C381".AsSpan(), out var meta);
        var registro = (RegistroC381)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());          // CstPis
        meta.Campos[1].Definidor(registro, "PROD001".AsSpan());    // CodItem
        meta.Campos[2].Definidor(registro, "1000,00".AsSpan());    // VlItem
        meta.Campos[3].Definidor(registro, "950,00".AsSpan());     // VlBcPis
        meta.Campos[4].Definidor(registro, "1,6500".AsSpan());     // AliqPis
        meta.Campos[5].Definidor(registro, "10,000".AsSpan());     // QuantBcPis
        meta.Campos[6].Definidor(registro, "0,0100".AsSpan());     // AliqPisQuant
        meta.Campos[7].Definidor(registro, "15,68".AsSpan());      // VlPis
        meta.Campos[8].Definidor(registro, "3.1.01.001".AsSpan()); // CodCta

        registro.CstPis.Should().Be(1);
        registro.CodItem.Should().Be("PROD001");
        registro.VlItem.Should().Be(1000m);
        registro.VlBcPis.Should().Be(950m);
        registro.AliqPis.Should().Be(1.65m);
        registro.QuantBcPis.Should().Be(10m);
        registro.AliqPisQuant.Should().Be(0.01m);
        registro.VlPis.Should().Be(15.68m);
        registro.CodCta.Should().Be("3.1.01.001");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("C381".AsSpan(), out var meta);
        var registro = (RegistroC381)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcPis
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqPis
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // QuantBcPis
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqPisQuant
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta

        registro.VlBcPis.Should().BeNull();
        registro.AliqPis.Should().BeNull();
        registro.QuantBcPis.Should().BeNull();
        registro.AliqPisQuant.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C381|1|PROD001|1000,00|950,00|1,6500|10,000|0,0100|15,68|3.1.01.001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|C381|6|ITEM002|2500,00|||||62,50||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComAliquotaQuantidade_PreservaTextoCanonico()
    {
        const string sped = "|C381|3|COMB001|5000,00|||||75,00||\r\n";

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
