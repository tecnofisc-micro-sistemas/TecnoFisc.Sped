using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoM;

public sealed class RegistroM410Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroM410).Assembly);

    [Fact]
    public void Atributo_DeclaraM410_Nivel3_BlocoM()
    {
        var atributo = typeof(RegistroM410).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("M410");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("M");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroM410Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("M410".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("M410");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "NatRec", "VlRec", "CodCta", "DescCompl",
        ]);
        meta.Campos[0].Tamanho.Should().Be(3);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // NatRec
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // VlRec
        meta.Campos[2].Tamanho.Should().Be(255);
        meta.Campos[2].Obrigatorio.Should().BeFalse();  // CodCta
        meta.Campos[3].Obrigatorio.Should().BeFalse();  // DescCompl
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("M410".AsSpan(), out var meta);
        var registro = (RegistroM410)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "001".AsSpan());         // NatRec
        meta.Campos[1].Definidor(registro, "5000,00".AsSpan());     // VlRec
        meta.Campos[2].Definidor(registro, "4.1.1.002".AsSpan());   // CodCta
        meta.Campos[3].Definidor(registro, "Produto isento".AsSpan()); // DescCompl

        registro.NatRec.Should().Be("001");
        registro.VlRec.Should().Be(5000m);
        registro.CodCta.Should().Be("4.1.1.002");
        registro.DescCompl.Should().Be("Produto isento");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("M410".AsSpan(), out var meta);
        var registro = (RegistroM410)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // DescCompl

        registro.CodCta.Should().BeNull();
        registro.DescCompl.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|M410|001|5000,00|4.1.1.002|Produto isento|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|M410|603|3000,00|||\r\n";

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
