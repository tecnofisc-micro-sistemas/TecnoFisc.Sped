using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoE;

/// <summary>
/// Sub-stage 8.172 — exercita a forma do <see cref="RegistroE313"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 229-230): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroE313Tests
{
    private const string ChaveNfeValida = "35240111222333000181550010000000011000000018";

    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroE313).Assembly);

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
    public void Atributo_DeclaraE313_Nivel5_BlocoE()
    {
        var atributo = typeof(RegistroE313).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("E313");
        atributo.Nivel.Should().Be(5);
        atributo.Bloco.Should().Be("E");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroE313Com9CamposNaOrdem()
    {
        _catalogo.TentarObter("E313".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("E313");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodPart", "CodMod", "Ser", "Sub", "NumDoc", "ChvDoce", "DtDoc", "CodItem", "VlAjItem",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 9));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("E313".AsSpan(), out var meta);
        var registro = (RegistroE313)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "FORN001".AsSpan());      // CodPart
        meta.Campos[1].Definidor(registro, "55".AsSpan());           // CodMod
        meta.Campos[2].Definidor(registro, "A".AsSpan());            // Ser
        meta.Campos[3].Definidor(registro, "1".AsSpan());            // Sub
        meta.Campos[4].Definidor(registro, "12345".AsSpan());        // NumDoc
        meta.Campos[5].Definidor(registro, ChaveNfeValida.AsSpan()); // ChvDoce
        meta.Campos[6].Definidor(registro, "01012023".AsSpan());     // DtDoc
        meta.Campos[7].Definidor(registro, "PROD001".AsSpan());      // CodItem
        meta.Campos[8].Definidor(registro, "1500,00".AsSpan());      // VlAjItem

        registro.CodPart.Should().Be("FORN001");
        registro.CodMod.Should().Be("55");
        registro.Ser.Should().Be("A");
        registro.Sub.Should().Be(1);
        registro.NumDoc.Should().Be(12345);
        registro.ChvDoce.Should().Be(ChaveAcesso.Create(ChaveNfeValida));
        registro.DtDoc.Should().Be(new DateOnly(2023, 1, 1));
        registro.CodItem.Should().Be("PROD001");
        registro.VlAjItem.Should().Be(1500.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("E313".AsSpan(), out var meta);
        var registro = (RegistroE313)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty); // CodPart
        meta.Campos[1].Definidor(registro, Span<char>.Empty); // CodMod
        meta.Campos[2].Definidor(registro, Span<char>.Empty); // Ser
        meta.Campos[3].Definidor(registro, Span<char>.Empty); // Sub
        meta.Campos[4].Definidor(registro, Span<char>.Empty); // NumDoc
        meta.Campos[5].Definidor(registro, Span<char>.Empty); // ChvDoce
        meta.Campos[7].Definidor(registro, Span<char>.Empty); // CodItem

        registro.CodPart.Should().BeNull();
        registro.CodMod.Should().BeNull();
        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
        registro.NumDoc.Should().BeNull();
        registro.ChvDoce.Should().BeNull();
        registro.CodItem.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        var sped = $"|E313|FORN001|55|A|1|12345|{ChaveNfeValida}|01012023|PROD001|1500,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        const string sped = "|E313||55|||12345||01012023||1500,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
