using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.131 — exercita a forma do <see cref="RegistroD301"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 183): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD301Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD301).Assembly);

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
    public void Atributo_DeclaraD301_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD301).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D301");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD301Com1CampoNaOrdem()
    {
        _catalogo.TentarObter("D301".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D301");
        meta.Campos.Select(c => c.Nome).Should().Equal(["NumDocCanc"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D301".AsSpan(), out var meta);
        var registro = (RegistroD301)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "150".AsSpan()); // NumDocCanc

        registro.NumDocCanc.Should().Be(150);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D301".AsSpan(), out var meta);
        var registro = (RegistroD301)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty); // NumDocCanc

        registro.NumDocCanc.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Bilhete rodoviário (cód. 13) cancelado, número 150.
        const string sped = "|D301|150|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_NumeroDocumentoDiferente_PreservaTextoCanonico()
    {
        // Bilhete ferroviário (cód. 16) cancelado, número 999.
        const string sped = "|D301|999|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
