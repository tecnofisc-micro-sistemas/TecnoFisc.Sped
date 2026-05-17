using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.082 — exercita a forma do <see cref="RegistroC410"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 120): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC410Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC410).Assembly);

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

    [Fact]
    public void Atributo_DeclaraC410_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC410).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C410");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC410Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("C410".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C410");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "VlPis", "VlCofins"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C410".AsSpan(), out var meta);
        var registro = (RegistroC410)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "150,50".AsSpan());  // VlPis
        meta.Campos[1].Definidor(registro, "695,00".AsSpan());  // VlCofins

        registro.VlPis.Should().Be(150.50m);
        registro.VlCofins.Should().Be(695.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C410".AsSpan(), out var meta);
        var registro = (RegistroC410)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty); // VlPis
        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCofins

        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C410|150,50|695,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposVazios_PreservaTextoCanonico()
    {
        // Contribuintes que entregam EFD-Contribuições do mesmo período estão dispensados —
        // ambos os campos podem ser omitidos.
        const string sped = "|C410|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
