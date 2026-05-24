using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.124 — exercita a forma do <see cref="RegistroD162"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 175): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD162Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD162).Assembly);

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
    public void Atributo_DeclaraD162_Nivel4_BlocoD()
    {
        var atributo = typeof(RegistroD162).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D162");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD162Com9CamposNaOrdem()
    {
        _catalogo.TentarObter("D162".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D162");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodMod", "Ser", "NumDoc", "DtDoc",
            "VlDoc", "VlMerc", "QtdVol", "PesoBrt", "PesoLiq",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 9));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D162".AsSpan(), out var meta);
        var registro = (RegistroD162)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01".AsSpan());          // CodMod
        meta.Campos[1].Definidor(registro, "A".AsSpan());           // Ser
        meta.Campos[2].Definidor(registro, "123456789".AsSpan());   // NumDoc
        meta.Campos[3].Definidor(registro, "01012024".AsSpan());    // DtDoc
        meta.Campos[4].Definidor(registro, "10500,00".AsSpan());    // VlDoc
        meta.Campos[5].Definidor(registro, "9800,50".AsSpan());     // VlMerc
        meta.Campos[6].Definidor(registro, "42".AsSpan());          // QtdVol
        meta.Campos[7].Definidor(registro, "1250,30".AsSpan());     // PesoBrt
        meta.Campos[8].Definidor(registro, "1200,00".AsSpan());     // PesoLiq

        registro.CodMod.Should().Be("01");
        registro.Ser.Should().Be("A");
        registro.NumDoc.Should().Be(123456789);
        registro.DtDoc.Should().Be(new DateOnly(2024, 1, 1));
        registro.VlDoc.Should().Be(10500.00m);
        registro.VlMerc.Should().Be(9800.50m);
        registro.QtdVol.Should().Be(42);
        registro.PesoBrt.Should().Be(1250.30m);
        registro.PesoLiq.Should().Be(1200.00m);
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("D162".AsSpan(), out var meta);
        var registro = (RegistroD162)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);  // CodMod
        meta.Campos[1].Definidor(registro, Span<char>.Empty);  // Ser
        meta.Campos[2].Definidor(registro, Span<char>.Empty);  // NumDoc
        meta.Campos[3].Definidor(registro, Span<char>.Empty);  // DtDoc
        meta.Campos[4].Definidor(registro, Span<char>.Empty);  // VlDoc
        meta.Campos[5].Definidor(registro, Span<char>.Empty);  // VlMerc
        meta.Campos[6].Definidor(registro, Span<char>.Empty);  // QtdVol
        meta.Campos[7].Definidor(registro, Span<char>.Empty);  // PesoBrt
        meta.Campos[8].Definidor(registro, Span<char>.Empty);  // PesoLiq

        registro.CodMod.Should().BeNull();
        registro.Ser.Should().BeNull();
        registro.NumDoc.Should().BeNull();
        registro.DtDoc.Should().BeNull();
        registro.VlDoc.Should().BeNull();
        registro.VlMerc.Should().BeNull();
        registro.QtdVol.Should().BeNull();
        registro.PesoBrt.Should().BeNull();
        registro.PesoLiq.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // NF (01) com série, número, data, valores e pesos.
        const string sped = "|D162|01|A|123456789|01012024|10500,00|9800,50|42|1250,30|1200,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // Apenas QTD_VOL e NUM_DOC preenchidos; demais campos opcionais vazios.
        const string sped = "|D162|||987654321||||15|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ModeloNFe_PreservaTextoCanonico()
    {
        // NF-e (modelo 55) com data e valores monetários.
        const string sped = "|D162|55||100000001|15062023|50000,00|48000,00|200|5000,00|4800,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
