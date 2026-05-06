using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC181Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC181).Assembly);

    [Fact]
    public void Atributo_DeclaraC181_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC181).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C181");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC181Com10CamposNaOrdem()
    {
        _catalogo.TentarObter("C181".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C181");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CstPis", "Cfop", "VlItem", "VlDesc", "VlBcPis",
            "AliqPis", "QuantBcPis", "AliqPisQuant", "VlPis", "CodCta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();  // CstPis
        meta.Campos[1].Tamanho.Should().Be(4);
        meta.Campos[1].Obrigatorio.Should().BeTrue();  // Cfop
        meta.Campos[2].Obrigatorio.Should().BeTrue();  // VlItem
        meta.Campos[5].Tamanho.Should().Be(8);         // AliqPis tamanho fixo
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C181".AsSpan(), out var meta);
        var registro = (RegistroC181)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());          // CstPis
        meta.Campos[1].Definidor(registro, "5101".AsSpan());       // Cfop
        meta.Campos[2].Definidor(registro, "1000,00".AsSpan());    // VlItem
        meta.Campos[3].Definidor(registro, "50,00".AsSpan());      // VlDesc
        meta.Campos[4].Definidor(registro, "950,00".AsSpan());     // VlBcPis
        meta.Campos[5].Definidor(registro, "1,6500".AsSpan());     // AliqPis
        meta.Campos[6].Definidor(registro, "10,000".AsSpan());     // QuantBcPis
        meta.Campos[7].Definidor(registro, "0,0100".AsSpan());     // AliqPisQuant
        meta.Campos[8].Definidor(registro, "15,68".AsSpan());      // VlPis
        meta.Campos[9].Definidor(registro, "3.1.01.001".AsSpan()); // CodCta

        registro.CstPis.Should().Be(1);
        registro.Cfop.Should().Be(Cfop.Criar("5101"));
        registro.VlItem.Should().Be(1000m);
        registro.VlDesc.Should().Be(50m);
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
        _catalogo.TentarObter("C181".AsSpan(), out var meta);
        var registro = (RegistroC181)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // VlDesc
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcPis
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqPis
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // QuantBcPis
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqPisQuant
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty); // VlPis
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta

        registro.VlDesc.Should().BeNull();
        registro.VlBcPis.Should().BeNull();
        registro.AliqPis.Should().BeNull();
        registro.QuantBcPis.Should().BeNull();
        registro.AliqPisQuant.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C181|1|5101|1000,00|50,00|950,00|1,6500|10,000|0,0100|15,68|3.1.01.001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped =
            "|C181|6|5405|2500,00||||||||\r\n";

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
