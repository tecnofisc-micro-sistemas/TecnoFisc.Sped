using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.134 — exercita a forma do <see cref="RegistroD355"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 184): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD355Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD355).Assembly);

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
    public void Atributo_DeclaraD355_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD355).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D355");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD355Com6CamposNaOrdem()
    {
        _catalogo.TentarObter("D355".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D355");
        meta.Campos.Select(c => c.Nome).Should().Equal(["DtDoc", "Cro", "Crz", "NumCooFin", "GtFin", "VlBrt"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D355".AsSpan(), out var meta);
        var registro = (RegistroD355)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "13052025".AsSpan());   // DtDoc
        meta.Campos[1].Definidor(registro, "1".AsSpan());          // Cro
        meta.Campos[2].Definidor(registro, "42".AsSpan());         // Crz
        meta.Campos[3].Definidor(registro, "100".AsSpan());        // NumCooFin
        meta.Campos[4].Definidor(registro, "15000,00".AsSpan());   // GtFin
        meta.Campos[5].Definidor(registro, "14500,00".AsSpan());   // VlBrt

        registro.DtDoc.Should().Be(new DateOnly(2025, 5, 13));
        registro.Cro.Should().Be(1);
        registro.Crz.Should().Be(42);
        registro.NumCooFin.Should().Be(100);
        registro.GtFin.Should().Be(15000m);
        registro.VlBrt.Should().Be(14500m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D355".AsSpan(), out var meta);
        var registro = (RegistroD355)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, Span<char>.Empty); // Cro
        meta.Campos[2].Definidor(registro, Span<char>.Empty); // Crz
        meta.Campos[3].Definidor(registro, Span<char>.Empty); // NumCooFin

        registro.Cro.Should().BeNull();
        registro.Crz.Should().BeNull();
        registro.NumCooFin.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Redução Z de ECF para bilhete de passagem rodoviário, CRO=1, CRZ=42.
        const string sped = "|D355|13052025|1|42|100|15000,00|14500,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ContadoresAltos_PreservaTextoCanonico()
    {
        // Redução Z com contador de redução Z alto (999) e Grande Total elevado.
        const string sped = "|D355|31012025|3|999|5000|9999999,99|9500000,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
