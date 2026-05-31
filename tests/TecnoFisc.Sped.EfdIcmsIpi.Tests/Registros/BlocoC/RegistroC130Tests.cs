using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.048 — exercita a forma do <see cref="RegistroC130"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 73): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC130Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC130).Assembly);

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
    public void Atributo_DeclaraC130_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC130).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C130");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC130Com7CamposNaOrdem()
    {
        _catalogo.TentarObter("C130".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C130");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "VlServNt", "VlBcIssqn", "VlIssqn", "VlBcIrrf", "VlIrrf", "VlBcPrev", "VlPrev",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 7));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C130".AsSpan(), out var meta);
        var registro = (RegistroC130)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1000.00".AsSpan());   // VlServNt
        meta.Campos[1].Definidor(registro, "500.00".AsSpan());    // VlBcIssqn
        meta.Campos[2].Definidor(registro, "25.00".AsSpan());     // VlIssqn
        meta.Campos[3].Definidor(registro, "800.00".AsSpan());    // VlBcIrrf
        meta.Campos[4].Definidor(registro, "120.00".AsSpan());    // VlIrrf
        meta.Campos[5].Definidor(registro, "600.00".AsSpan());    // VlBcPrev
        meta.Campos[6].Definidor(registro, "66.00".AsSpan());     // VlPrev

        registro.VlServNt.Should().Be(1000.00m);
        registro.VlBcIssqn.Should().Be(500.00m);
        registro.VlIssqn.Should().Be(25.00m);
        registro.VlBcIrrf.Should().Be(800.00m);
        registro.VlIrrf.Should().Be(120.00m);
        registro.VlBcPrev.Should().Be(600.00m);
        registro.VlPrev.Should().Be(66.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C130".AsSpan(), out var meta);
        var registro = (RegistroC130)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);  // VlServNt
        meta.Campos[1].Definidor(registro, Span<char>.Empty);  // VlBcIssqn
        meta.Campos[2].Definidor(registro, Span<char>.Empty);  // VlIssqn
        meta.Campos[3].Definidor(registro, Span<char>.Empty);  // VlBcIrrf
        meta.Campos[4].Definidor(registro, Span<char>.Empty);  // VlIrrf
        meta.Campos[5].Definidor(registro, Span<char>.Empty);  // VlBcPrev
        meta.Campos[6].Definidor(registro, Span<char>.Empty);  // VlPrev

        registro.VlServNt.Should().BeNull();
        registro.VlBcIssqn.Should().BeNull();
        registro.VlIssqn.Should().BeNull();
        registro.VlBcIrrf.Should().BeNull();
        registro.VlIrrf.Should().BeNull();
        registro.VlBcPrev.Should().BeNull();
        registro.VlPrev.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // ISSQN + IRRF + Previdência Social todos informados.
        const string sped = "|C130|1000,00|500,00|25,00|800,00|120,00|600,00|66,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SomenteCamposIssqn_PreservaTextoCanonico()
    {
        // Apenas ISSQN informado; IRRF e Previdência em branco.
        const string sped = "|C130|2500,00|1200,00|60,00|||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_TodosCamposVazios_PreservaTextoCanonico()
    {
        // Registro emitido sem nenhum campo preenchido (todos opcionais).
        const string sped = "|C130||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
