using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC385Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC385).Assembly);

    [Fact]
    public void Atributo_DeclaraC385_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC385).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C385");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC385ComNoveCamposNaOrdem()
    {
        _catalogo.TentarObter("C385".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C385");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CstCofins", "CodItem", "VlItem", "VlBcCofins",
            "AliqCofins", "QuantBcCofins", "AliqCofinsQuant", "VlCofins", "CodCta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // CstCofins
        meta.Campos[1].Tamanho.Should().Be(60);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // CodItem
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // VlItem
        meta.Campos[4].Tamanho.Should().Be(8);          // AliqCofins tamanho fixo
        meta.Campos[7].Obrigatorio.Should().BeTrue();   // VlCofins
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C385".AsSpan(), out var meta);
        var registro = (RegistroC385)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());          // CstCofins
        meta.Campos[1].Definidor(registro, "PROD001".AsSpan());    // CodItem
        meta.Campos[2].Definidor(registro, "1000,00".AsSpan());    // VlItem
        meta.Campos[3].Definidor(registro, "950,00".AsSpan());     // VlBcCofins
        meta.Campos[4].Definidor(registro, "7,6000".AsSpan());     // AliqCofins
        meta.Campos[5].Definidor(registro, "10,000".AsSpan());     // QuantBcCofins
        meta.Campos[6].Definidor(registro, "0,0200".AsSpan());     // AliqCofinsQuant
        meta.Campos[7].Definidor(registro, "72,20".AsSpan());      // VlCofins
        meta.Campos[8].Definidor(registro, "3.1.01.001".AsSpan()); // CodCta

        registro.CstCofins.Should().Be(1);
        registro.CodItem.Should().Be("PROD001");
        registro.VlItem.Should().Be(1000m);
        registro.VlBcCofins.Should().Be(950m);
        registro.AliqCofins.Should().Be(7.6m);
        registro.QuantBcCofins.Should().Be(10m);
        registro.AliqCofinsQuant.Should().Be(0.02m);
        registro.VlCofins.Should().Be(72.20m);
        registro.CodCta.Should().Be("3.1.01.001");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("C385".AsSpan(), out var meta);
        var registro = (RegistroC385)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcCofins
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqCofins
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // QuantBcCofins
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqCofinsQuant
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta

        registro.VlBcCofins.Should().BeNull();
        registro.AliqCofins.Should().BeNull();
        registro.QuantBcCofins.Should().BeNull();
        registro.AliqCofinsQuant.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C385|1|PROD001|1000,00|950,00|7,6000|10,000|0,0200|72,20|3.1.01.001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|C385|6|ITEM002|2500,00|||||288,75||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComAliquotaQuantidade_PreservaTextoCanonico()
    {
        const string sped = "|C385|3|COMB001|5000,00|||||345,00||\r\n";

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
