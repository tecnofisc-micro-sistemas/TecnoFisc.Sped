using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoE;

/// <summary>
/// Sub-stage 8.175 — exercita a forma do <see cref="RegistroE510"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 232): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroE510Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroE510).Assembly);

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
    public void Atributo_DeclaraE510_Nivel3_BlocoE()
    {
        var atributo = typeof(RegistroE510).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("E510");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("E");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroE510Com5CamposNaOrdem()
    {
        _catalogo.TentarObter("E510".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("E510");
        meta.Campos.Select(c => c.Nome).Should().Equal(["Cfop", "CstIpi", "VlContIpi", "VlBcIpi", "VlIpi"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("E510".AsSpan(), out var meta);
        var registro = (RegistroE510)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "5102".AsSpan());     // Cfop
        meta.Campos[1].Definidor(registro, "50".AsSpan());       // CstIpi
        meta.Campos[2].Definidor(registro, "1500,00".AsSpan());  // VlContIpi
        meta.Campos[3].Definidor(registro, "1450,00".AsSpan());  // VlBcIpi
        meta.Campos[4].Definidor(registro, "145,00".AsSpan());   // VlIpi

        registro.Cfop.Should().Be(Cfop.Create("5102"));
        registro.CstIpi.Should().Be("50");
        registro.VlContIpi.Should().Be(1500.00m);
        registro.VlBcIpi.Should().Be(1450.00m);
        registro.VlIpi.Should().Be(145.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("E510".AsSpan(), out var meta);
        var registro = (RegistroE510)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.Cfop.Should().Be(default(Cfop));
        registro.CstIpi.Should().BeNull();
        registro.VlContIpi.Should().Be(0m);
        registro.VlBcIpi.Should().Be(0m);
        registro.VlIpi.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|E510|5102|50|1500,00|1450,00|145,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComEntradaComRecuperacaoDeCredito_PreservaTextoCanonico()
    {
        const string sped = "|E510|1102|00|2500,00|2500,00|250,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
