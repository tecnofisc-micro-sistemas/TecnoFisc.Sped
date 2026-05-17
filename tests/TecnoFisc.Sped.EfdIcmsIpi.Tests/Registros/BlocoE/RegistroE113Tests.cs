using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoE;

/// <summary>
/// Sub-stage 8.159 — exercita a forma do <see cref="RegistroE113"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 211): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroE113Tests
{
    // Chave NF-e válida (UF SP, Jan/2024, DV=8) reutilizada dos testes de ChaveAcesso.
    private const string ChaveNfeValida = "35240111222333000181550010000000011000000018";

    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroE113).Assembly);

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
    public void Atributo_DeclaraE113_Nivel5_BlocoE()
    {
        var atributo = typeof(RegistroE113).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("E113");
        atributo.Nivel.Should().Be(5);
        atributo.Bloco.Should().Be("E");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroE113Com9CamposNaOrdem()
    {
        _catalogo.TentarObter("E113".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("E113");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodPart", "CodMod", "Ser", "Sub", "NumDoc", "DtDoc", "CodItem", "VlAjItem", "ChvDoce",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 9));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("E113".AsSpan(), out var meta);
        var registro = (RegistroE113)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "FORN001".AsSpan());              // CodPart
        meta.Campos[1].Definidor(registro, "55".AsSpan());                   // CodMod
        meta.Campos[2].Definidor(registro, "A".AsSpan());                    // Ser
        meta.Campos[3].Definidor(registro, "1".AsSpan());                    // Sub
        meta.Campos[4].Definidor(registro, "12345".AsSpan());                // NumDoc
        meta.Campos[5].Definidor(registro, "01012023".AsSpan());             // DtDoc
        meta.Campos[6].Definidor(registro, "PROD001".AsSpan());              // CodItem
        meta.Campos[7].Definidor(registro, "1500,00".AsSpan());              // VlAjItem
        meta.Campos[8].Definidor(registro, ChaveNfeValida.AsSpan());         // ChvDoce

        registro.CodPart.Should().Be("FORN001");
        registro.CodMod.Should().Be("55");
        registro.Ser.Should().Be("A");
        registro.Sub.Should().Be(1);
        registro.NumDoc.Should().Be(12345);
        registro.DtDoc.Should().Be(new DateOnly(2023, 1, 1));
        registro.CodItem.Should().Be("PROD001");
        registro.VlAjItem.Should().Be(1500.00m);
        registro.ChvDoce.Should().Be(ChaveAcesso.Criar(ChaveNfeValida));
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("E113".AsSpan(), out var meta);
        var registro = (RegistroE113)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);  // CodPart
        meta.Campos[1].Definidor(registro, Span<char>.Empty);  // CodMod
        meta.Campos[2].Definidor(registro, Span<char>.Empty);  // Ser
        meta.Campos[3].Definidor(registro, Span<char>.Empty);  // Sub
        meta.Campos[4].Definidor(registro, Span<char>.Empty);  // NumDoc
        meta.Campos[6].Definidor(registro, Span<char>.Empty);  // CodItem
        meta.Campos[8].Definidor(registro, Span<char>.Empty);  // ChvDoce

        registro.CodPart.Should().BeNull();
        registro.CodMod.Should().BeNull();
        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
        registro.NumDoc.Should().BeNull();
        registro.CodItem.Should().BeNull();
        registro.ChvDoce.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        var sped = $"|E113|FORN001|55|A|1|12345|01012023|PROD001|1500,00|{ChaveNfeValida}|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // Ajuste sem participante, série, subsérie, item e chave eletrônica — somente campos obrigatórios.
        const string sped = "|E113||55|||12345|01012023||1500,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
