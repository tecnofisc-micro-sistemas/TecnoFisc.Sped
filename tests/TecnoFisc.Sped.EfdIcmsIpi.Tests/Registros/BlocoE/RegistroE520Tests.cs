using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoE;

/// <summary>
/// Sub-stage 8.176 — exercita a forma do <see cref="RegistroE520"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 233): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroE520Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroE520).Assembly);

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

    [Fact]
    public void Atributo_DeclaraE520_Nivel3_BlocoE()
    {
        var atributo = typeof(RegistroE520).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("E520");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("E");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroE520Com7CamposNaOrdem()
    {
        _catalogo.TentarObter("E520".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("E520");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["VlSdAntIpi", "VlDebIpi", "VlCredIpi", "VlOdIpi", "VlOcIpi", "VlScIpi", "VlSdIpi"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("E520".AsSpan(), out var meta);
        var registro = (RegistroE520)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "100,00".AsSpan());   // VlSdAntIpi
        meta.Campos[1].Definidor(registro, "800,00".AsSpan());   // VlDebIpi
        meta.Campos[2].Definidor(registro, "250,00".AsSpan());   // VlCredIpi
        meta.Campos[3].Definidor(registro, "40,00".AsSpan());    // VlOdIpi
        meta.Campos[4].Definidor(registro, "10,00".AsSpan());    // VlOcIpi
        meta.Campos[5].Definidor(registro, "0,00".AsSpan());     // VlScIpi
        meta.Campos[6].Definidor(registro, "480,00".AsSpan());   // VlSdIpi

        registro.VlSdAntIpi.Should().Be(100.00m);
        registro.VlDebIpi.Should().Be(800.00m);
        registro.VlCredIpi.Should().Be(250.00m);
        registro.VlOdIpi.Should().Be(40.00m);
        registro.VlOcIpi.Should().Be(10.00m);
        registro.VlScIpi.Should().Be(0.00m);
        registro.VlSdIpi.Should().Be(480.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("E520".AsSpan(), out var meta);
        var registro = (RegistroE520)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.VlSdAntIpi.Should().Be(0m);
        registro.VlDebIpi.Should().Be(0m);
        registro.VlCredIpi.Should().Be(0m);
        registro.VlOdIpi.Should().Be(0m);
        registro.VlOcIpi.Should().Be(0m);
        registro.VlScIpi.Should().Be(0m);
        registro.VlSdIpi.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|E520|100,00|800,00|250,00|40,00|10,00|0,00|480,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComSaldoCredorATransportar_PreservaTextoCanonico()
    {
        const string sped = "|E520|300,00|100,00|250,00|20,00|30,00|460,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComSaldoDevedorARecolher_PreservaTextoCanonico()
    {
        const string sped = "|E520|0,00|900,00|150,00|25,00|5,00|0,00|770,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
