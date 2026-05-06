using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC605Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC605).Assembly);

    [Fact]
    public void Atributo_DeclaraC605_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC605).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C605");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC605ComSeisCamposNaOrdem()
    {
        _catalogo.TentarObter("C605".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C605");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CstCofins", "VlItem", "VlBcCofins", "AliqCofins", "VlCofins", "CodCta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // CstCofins
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // VlItem
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // VlBcCofins
        meta.Campos[3].Tamanho.Should().Be(8);
        meta.Campos[3].Obrigatorio.Should().BeTrue();   // AliqCofins
        meta.Campos[4].Obrigatorio.Should().BeTrue();   // VlCofins
        meta.Campos[5].Tamanho.Should().Be(255);
        meta.Campos[5].Obrigatorio.Should().BeFalse();  // CodCta
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C605".AsSpan(), out var meta);
        var registro = (RegistroC605)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "49".AsSpan());           // CstCofins
        meta.Campos[1].Definidor(registro, "5000,00".AsSpan());      // VlItem
        meta.Campos[2].Definidor(registro, "5000,00".AsSpan());      // VlBcCofins
        meta.Campos[3].Definidor(registro, "7,6000".AsSpan());       // AliqCofins
        meta.Campos[4].Definidor(registro, "380,00".AsSpan());       // VlCofins
        meta.Campos[5].Definidor(registro, "3.1.01.001".AsSpan());   // CodCta

        registro.CstCofins.Should().Be(49);
        registro.VlItem.Should().Be(5000m);
        registro.VlBcCofins.Should().Be(5000m);
        registro.AliqCofins.Should().Be(7.60m);
        registro.VlCofins.Should().Be(380.00m);
        registro.CodCta.Should().Be("3.1.01.001");
    }

    [Fact]
    public void Definidor_CodCtaVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C605".AsSpan(), out var meta);
        var registro = (RegistroC605)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta

        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C605|49|5000,00|5000,00|7,6000|380,00|3.1.01.001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCodCta_PreservaTextoCanonico()
    {
        const string sped = "|C605|49|5000,00|5000,00|7,6000|380,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CstIsento_PreservaTextoCanonico()
    {
        const string sped = "|C605|99|3000,00|3000,00|0,0000|0,00||\r\n";

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
