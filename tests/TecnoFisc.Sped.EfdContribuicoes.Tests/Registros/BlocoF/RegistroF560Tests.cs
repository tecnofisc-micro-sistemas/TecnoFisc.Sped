using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoF;

public sealed class RegistroF560Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroF560).Assembly);

    [Fact]
    public void Atributo_DeclaraF560_Nivel3_BlocoF()
    {
        var atributo = typeof(RegistroF560).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("F560");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("F");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroF560Com15CamposNaOrdem()
    {
        _catalogo.TentarObter("F560".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("F560");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "VlRecComp", "CstPis", "VlDescPis", "QuantBcPis", "AliqPisQuant", "VlPis",
            "CstCofins", "VlDescCofins", "QuantBcCofins", "AliqCofinsQuant", "VlCofins",
            "CodMod", "Cfop", "CodCta", "InfoCompl",
        ]);
        meta.Campos[0].Obrigatorio.Should().BeTrue();  // VlRecComp
        meta.Campos[1].Tamanho.Should().Be(2);
        meta.Campos[1].Obrigatorio.Should().BeTrue();  // CstPis
        meta.Campos[6].Obrigatorio.Should().BeTrue();  // CstCofins
        meta.Campos[12].Tamanho.Should().Be(4);        // Cfop
        meta.Campos[13].Tamanho.Should().Be(255);      // CodCta
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("F560".AsSpan(), out var meta);
        var registro = (RegistroF560)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "300000,00".AsSpan());  // VlRecComp
        meta.Campos[1].Definidor(registro, "03".AsSpan());         // CstPis
        meta.Campos[2].Definidor(registro, "500,00".AsSpan());     // VlDescPis
        meta.Campos[3].Definidor(registro, "10000,000".AsSpan());  // QuantBcPis
        meta.Campos[4].Definidor(registro, "0,0100".AsSpan());     // AliqPisQuant
        meta.Campos[5].Definidor(registro, "100,00".AsSpan());     // VlPis
        meta.Campos[6].Definidor(registro, "03".AsSpan());         // CstCofins
        meta.Campos[7].Definidor(registro, "500,00".AsSpan());     // VlDescCofins
        meta.Campos[8].Definidor(registro, "10000,000".AsSpan());  // QuantBcCofins
        meta.Campos[9].Definidor(registro, "0,0500".AsSpan());     // AliqCofinsQuant
        meta.Campos[10].Definidor(registro, "500,00".AsSpan());    // VlCofins
        meta.Campos[11].Definidor(registro, "02".AsSpan());        // CodMod
        meta.Campos[12].Definidor(registro, "5102".AsSpan());      // Cfop
        meta.Campos[13].Definidor(registro, "3.1.02.001".AsSpan()); // CodCta
        meta.Campos[14].Definidor(registro, "Bebidas frias".AsSpan()); // InfoCompl

        registro.VlRecComp.Should().Be(300000m);
        registro.CstPis.Should().Be("03");
        registro.VlDescPis.Should().Be(500m);
        registro.QuantBcPis.Should().Be(10000m);
        registro.AliqPisQuant.Should().Be(0.01m);
        registro.VlPis.Should().Be(100m);
        registro.CstCofins.Should().Be("03");
        registro.VlDescCofins.Should().Be(500m);
        registro.QuantBcCofins.Should().Be(10000m);
        registro.AliqCofinsQuant.Should().Be(0.05m);
        registro.VlCofins.Should().Be(500m);
        registro.CodMod.Should().Be("02");
        registro.Cfop.Should().Be(Cfop.Criar("5102"));
        registro.CodCta.Should().Be("3.1.02.001");
        registro.InfoCompl.Should().Be("Bebidas frias");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("F560".AsSpan(), out var meta);
        var registro = (RegistroF560)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlDescPis
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);  // QuantBcPis
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqPisQuant
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlPis
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlDescCofins
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);  // QuantBcCofins
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqCofinsQuant
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCofins
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty); // CodMod
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty); // Cfop
        meta.Campos[13].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta
        meta.Campos[14].Definidor(registro, ReadOnlySpan<char>.Empty); // InfoCompl

        registro.VlDescPis.Should().BeNull();
        registro.QuantBcPis.Should().BeNull();
        registro.AliqPisQuant.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlDescCofins.Should().BeNull();
        registro.QuantBcCofins.Should().BeNull();
        registro.AliqCofinsQuant.Should().BeNull();
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
            "|F560|300000,00|03|500,00|10000,000|0,0100|100,00|03|500,00|10000,000|0,0500|500,00|02|5102|3.1.02.001|Bebidas frias|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        const string sped =
            "|F560|150000,00|03|||||03|||||||||\r\n";

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
