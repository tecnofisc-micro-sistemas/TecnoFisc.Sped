using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.061 — exercita a forma do <see cref="RegistroC178"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 90): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC178Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC178).Assembly);

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
    public void Atributo_DeclaraC178_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC178).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C178");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC178Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("C178".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C178");
        meta.Campos.Select(c => c.Nome).Should().Equal(["ClEnq", "VlUnid", "QuantPad"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C178".AsSpan(), out var meta);
        var registro = (RegistroC178)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "00001".AsSpan());   // ClEnq
        meta.Campos[1].Definidor(registro, "1,50".AsSpan());    // VlUnid
        meta.Campos[2].Definidor(registro, "100,000".AsSpan()); // QuantPad

        registro.ClEnq.Should().Be("00001");
        registro.VlUnid.Should().Be(1.50m);
        registro.QuantPad.Should().Be(100.000m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C178".AsSpan(), out var meta);
        var registro = (RegistroC178)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.ClEnq.Should().BeNull();
        registro.VlUnid.Should().BeNull();
        registro.QuantPad.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C178|00001|1,50|100,000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComValoresIpiAlternativo_PreservaTextoCanonico()
    {
        // Tributação IPI por quantidade: classe 22000, valor unitário e qtd padrão distintos.
        const string sped = "|C178|22000|0,75|500,000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
