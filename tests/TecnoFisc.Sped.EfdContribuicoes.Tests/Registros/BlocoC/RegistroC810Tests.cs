using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC810Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC810).Assembly);

    [Fact]
    public void Atributo_DeclaraC810_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC810).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C810");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC810ComDozeCamposNaOrdem()
    {
        _catalogo.TentarObter("C810".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C810");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "Cfop", "VlItem", "CodItem", "CstPis",
            "VlBcPis", "AliqPis", "VlPis",
            "CstCofins", "VlBcCofins", "AliqCofins", "VlCofins", "CodCta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13]);
        meta.Campos[0].Tamanho.Should().Be(4);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // Cfop
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // VlItem
        meta.Campos[2].Tamanho.Should().Be(60);
        meta.Campos[2].Obrigatorio.Should().BeFalse();  // CodItem
        meta.Campos[3].Tamanho.Should().Be(2);
        meta.Campos[3].Obrigatorio.Should().BeTrue();   // CstPis
        meta.Campos[4].Obrigatorio.Should().BeFalse();  // VlBcPis
        meta.Campos[6].Obrigatorio.Should().BeFalse();  // VlPis
        meta.Campos[7].Tamanho.Should().Be(2);
        meta.Campos[7].Obrigatorio.Should().BeTrue();   // CstCofins
        meta.Campos[11].Tamanho.Should().Be(255);
        meta.Campos[11].Obrigatorio.Should().BeFalse(); // CodCta
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C810".AsSpan(), out var meta);
        var registro = (RegistroC810)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "5102".AsSpan());          // Cfop
        meta.Campos[1].Definidor(registro, "1000,00".AsSpan());       // VlItem
        meta.Campos[2].Definidor(registro, "PROD001".AsSpan());       // CodItem
        meta.Campos[3].Definidor(registro, "01".AsSpan());            // CstPis
        meta.Campos[4].Definidor(registro, "1000,00".AsSpan());       // VlBcPis
        meta.Campos[5].Definidor(registro, "0,6500".AsSpan());        // AliqPis
        meta.Campos[6].Definidor(registro, "6,50".AsSpan());          // VlPis
        meta.Campos[7].Definidor(registro, "01".AsSpan());            // CstCofins
        meta.Campos[8].Definidor(registro, "1000,00".AsSpan());       // VlBcCofins
        meta.Campos[9].Definidor(registro, "3,0000".AsSpan());        // AliqCofins
        meta.Campos[10].Definidor(registro, "30,00".AsSpan());        // VlCofins
        meta.Campos[11].Definidor(registro, "3.1.01.001".AsSpan());   // CodCta

        registro.Cfop.Should().Be(Cfop.Criar("5102"));
        registro.VlItem.Should().Be(1000.00m);
        registro.CodItem.Should().Be("PROD001");
        registro.CstPis.Should().Be(1);
        registro.VlBcPis.Should().Be(1000.00m);
        registro.AliqPis.Should().Be(0.65m);
        registro.VlPis.Should().Be(6.50m);
        registro.CstCofins.Should().Be(1);
        registro.VlBcCofins.Should().Be(1000.00m);
        registro.AliqCofins.Should().Be(3.00m);
        registro.VlCofins.Should().Be(30.00m);
        registro.CodCta.Should().Be("3.1.01.001");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("C810".AsSpan(), out var meta);
        var registro = (RegistroC810)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);   // CodItem
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);   // VlBcPis
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);   // AliqPis
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);   // VlPis
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);   // VlBcCofins
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);   // AliqCofins
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlCofins
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCta

        registro.CodItem.Should().BeNull();
        registro.VlBcPis.Should().BeNull();
        registro.AliqPis.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlBcCofins.Should().BeNull();
        registro.AliqCofins.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C810|5102|1000,00|PROD001|49|1000,00|0,6500|6,50|49|1000,00|3,0000|30,00|3.1.01.001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposObrigatoriosApenas_PreservaTextoCanonico()
    {
        const string sped = "|C810|5102|1000,00||49||||49|||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CstOutrasOperacoes_PreservaTextoCanonico()
    {
        const string sped = "|C810|5102|500,00||99||||99|||||\r\n";

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
