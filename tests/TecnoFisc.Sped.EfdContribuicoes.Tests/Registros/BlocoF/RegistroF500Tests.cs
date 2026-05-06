using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoF;

public sealed class RegistroF500Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroF500).Assembly);

    [Fact]
    public void Atributo_DeclaraF500_Nivel3_BlocoF()
    {
        var atributo = typeof(RegistroF500).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("F500");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("F");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroF500Com15CamposNaOrdem()
    {
        _catalogo.TentarObter("F500".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("F500");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "VlRecCaixa", "CstPis", "VlDescPis", "VlBcPis", "AliqPis", "VlPis",
            "CstCofins", "VlDescCofins", "VlBcCofins", "AliqCofins", "VlCofins",
            "CodMod", "Cfop", "CodCta", "InfoCompl",
        ]);
        meta.Campos[0].Obrigatorio.Should().BeTrue();  // VlRecCaixa
        meta.Campos[1].Tamanho.Should().Be(2);
        meta.Campos[1].Obrigatorio.Should().BeTrue();  // CstPis
        meta.Campos[6].Obrigatorio.Should().BeTrue();  // CstCofins
        meta.Campos[11].Tamanho.Should().Be(2);        // CodMod
        meta.Campos[12].Tamanho.Should().Be(4);        // Cfop
        meta.Campos[13].Tamanho.Should().Be(255);      // CodCta
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("F500".AsSpan(), out var meta);
        var registro = (RegistroF500)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "500000,00".AsSpan());  // VlRecCaixa
        meta.Campos[1].Definidor(registro, "01".AsSpan());         // CstPis
        meta.Campos[2].Definidor(registro, "1000,00".AsSpan());    // VlDescPis
        meta.Campos[3].Definidor(registro, "499000,00".AsSpan());  // VlBcPis
        meta.Campos[4].Definidor(registro, "0,6500".AsSpan());     // AliqPis
        meta.Campos[5].Definidor(registro, "3243,50".AsSpan());    // VlPis
        meta.Campos[6].Definidor(registro, "01".AsSpan());         // CstCofins
        meta.Campos[7].Definidor(registro, "1000,00".AsSpan());    // VlDescCofins
        meta.Campos[8].Definidor(registro, "499000,00".AsSpan());  // VlBcCofins
        meta.Campos[9].Definidor(registro, "3,0000".AsSpan());     // AliqCofins
        meta.Campos[10].Definidor(registro, "14970,00".AsSpan());  // VlCofins
        meta.Campos[11].Definidor(registro, "55".AsSpan());        // CodMod
        meta.Campos[12].Definidor(registro, "5102".AsSpan());      // Cfop
        meta.Campos[13].Definidor(registro, "3.1.01.001".AsSpan()); // CodCta
        meta.Campos[14].Definidor(registro, "Vendas NF-e".AsSpan()); // InfoCompl

        registro.VlRecCaixa.Should().Be(500000m);
        registro.CstPis.Should().Be("01");
        registro.VlDescPis.Should().Be(1000m);
        registro.VlBcPis.Should().Be(499000m);
        registro.AliqPis.Should().Be(0.65m);
        registro.VlPis.Should().Be(3243.50m);
        registro.CstCofins.Should().Be("01");
        registro.VlDescCofins.Should().Be(1000m);
        registro.VlBcCofins.Should().Be(499000m);
        registro.AliqCofins.Should().Be(3.0m);
        registro.VlCofins.Should().Be(14970m);
        registro.CodMod.Should().Be("55");
        registro.Cfop.Should().Be(Cfop.Criar("5102"));
        registro.CodCta.Should().Be("3.1.01.001");
        registro.InfoCompl.Should().Be("Vendas NF-e");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("F500".AsSpan(), out var meta);
        var registro = (RegistroF500)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlDescPis
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlBcPis
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqPis
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlPis
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlDescCofins
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlBcCofins
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqCofins
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCofins
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty); // CodMod
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty); // Cfop
        meta.Campos[13].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta
        meta.Campos[14].Definidor(registro, ReadOnlySpan<char>.Empty); // InfoCompl

        registro.VlDescPis.Should().BeNull();
        registro.VlBcPis.Should().BeNull();
        registro.AliqPis.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlDescCofins.Should().BeNull();
        registro.VlBcCofins.Should().BeNull();
        registro.AliqCofins.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodMod.Should().BeNull();
        registro.Cfop.Should().BeNull();
        registro.CodCta.Should().BeNull();
        registro.InfoCompl.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|F500|500000,00|01|1000,00|499000,00|0,6500|3243,50|01|1000,00|499000,00|3,0000|14970,00|55|5102|3.1.01.001|Vendas NF-e|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        const string sped =
            "|F500|200000,00|06|||||07|||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCodModECfop_PreservaTextoCanonico()
    {
        const string sped =
            "|F500|100000,00|01||80000,00|0,6500|520,00|01||80000,00|3,0000|2400,00||||3.2.01|\r\n";

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
