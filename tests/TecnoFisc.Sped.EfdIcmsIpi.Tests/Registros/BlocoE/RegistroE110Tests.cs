using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoE;

/// <summary>
/// Sub-stage 8.156 — exercita a forma do <see cref="RegistroE110"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.2.2 (p. 223): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroE110Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroE110).Assembly);

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
    public void Atributo_DeclaraE110_Nivel3_BlocoE()
    {
        var atributo = typeof(RegistroE110).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("E110");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("E");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroE110Com14CamposNaOrdem()
    {
        _catalogo.TentarObter("E110".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("E110");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "VlTotDebitos", "VlAjDebitos", "VlTotAjDebitos", "VlEstornosCred",
            "VlTotCreditos", "VlAjCreditos", "VlTotAjCreditos", "VlEstornosDeb",
            "VlSldCredorAnt", "VlSldApurado", "VlTotDed", "VlIcmsRecolher",
            "VlSldCredorTransportar", "DebEsp"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("E110".AsSpan(), out var meta);
        var registro = (RegistroE110)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1000,00".AsSpan());   // VlTotDebitos
        meta.Campos[1].Definidor(registro, "100,00".AsSpan());    // VlAjDebitos
        meta.Campos[2].Definidor(registro, "50,00".AsSpan());     // VlTotAjDebitos
        meta.Campos[3].Definidor(registro, "25,00".AsSpan());     // VlEstornosCred
        meta.Campos[4].Definidor(registro, "800,00".AsSpan());    // VlTotCreditos
        meta.Campos[5].Definidor(registro, "80,00".AsSpan());     // VlAjCreditos
        meta.Campos[6].Definidor(registro, "40,00".AsSpan());     // VlTotAjCreditos
        meta.Campos[7].Definidor(registro, "15,00".AsSpan());     // VlEstornosDeb
        meta.Campos[8].Definidor(registro, "200,00".AsSpan());    // VlSldCredorAnt
        meta.Campos[9].Definidor(registro, "100,00".AsSpan());    // VlSldApurado
        meta.Campos[10].Definidor(registro, "10,00".AsSpan());    // VlTotDed
        meta.Campos[11].Definidor(registro, "90,00".AsSpan());    // VlIcmsRecolher
        meta.Campos[12].Definidor(registro, "0,00".AsSpan());     // VlSldCredorTransportar
        meta.Campos[13].Definidor(registro, "5,00".AsSpan());     // DebEsp

        registro.VlTotDebitos.Should().Be(1000.00m);
        registro.VlAjDebitos.Should().Be(100.00m);
        registro.VlTotAjDebitos.Should().Be(50.00m);
        registro.VlEstornosCred.Should().Be(25.00m);
        registro.VlTotCreditos.Should().Be(800.00m);
        registro.VlAjCreditos.Should().Be(80.00m);
        registro.VlTotAjCreditos.Should().Be(40.00m);
        registro.VlEstornosDeb.Should().Be(15.00m);
        registro.VlSldCredorAnt.Should().Be(200.00m);
        registro.VlSldApurado.Should().Be(100.00m);
        registro.VlTotDed.Should().Be(10.00m);
        registro.VlIcmsRecolher.Should().Be(90.00m);
        registro.VlSldCredorTransportar.Should().Be(0.00m);
        registro.DebEsp.Should().Be(5.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("E110".AsSpan(), out var meta);
        var registro = (RegistroE110)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.VlTotDebitos.Should().Be(0m);
        registro.VlAjDebitos.Should().Be(0m);
        registro.VlTotAjDebitos.Should().Be(0m);
        registro.VlEstornosCred.Should().Be(0m);
        registro.VlTotCreditos.Should().Be(0m);
        registro.VlAjCreditos.Should().Be(0m);
        registro.VlTotAjCreditos.Should().Be(0m);
        registro.VlEstornosDeb.Should().Be(0m);
        registro.VlSldCredorAnt.Should().Be(0m);
        registro.VlSldApurado.Should().Be(0m);
        registro.VlTotDed.Should().Be(0m);
        registro.VlIcmsRecolher.Should().Be(0m);
        registro.VlSldCredorTransportar.Should().Be(0m);
        registro.DebEsp.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|E110|1000,00|100,00|50,00|25,00|800,00|80,00|40,00|15,00|200,00|100,00|10,00|90,00|0,00|5,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComPeriodoSemMovimento_PreservaTextoCanonico()
    {
        // Período sem movimento: todos os valores zerados.
        const string sped =
            "|E110|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
