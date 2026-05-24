using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.081 — exercita a forma do <see cref="RegistroC405"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 119): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC405Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC405).Assembly);

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
    public void Atributo_DeclaraC405_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC405).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C405");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC405Com6CamposNaOrdem()
    {
        _catalogo.TentarObter("C405".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C405");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtDoc", "Cro", "Crz", "NumCooFin", "GtFin", "VlBrt"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C405".AsSpan(), out var meta);
        var registro = (RegistroC405)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "31012023".AsSpan());   // DtDoc
        meta.Campos[1].Definidor(registro, "1".AsSpan());           // Cro
        meta.Campos[2].Definidor(registro, "250".AsSpan());         // Crz
        meta.Campos[3].Definidor(registro, "123456789".AsSpan());   // NumCooFin
        meta.Campos[4].Definidor(registro, "15000,50".AsSpan());    // GtFin
        meta.Campos[5].Definidor(registro, "12500,00".AsSpan());    // VlBrt

        registro.DtDoc.Should().Be(new DateOnly(2023, 1, 31));
        registro.Cro.Should().Be(1);
        registro.Crz.Should().Be(250);
        registro.NumCooFin.Should().Be(123456789);
        registro.GtFin.Should().Be(15000.5m);
        registro.VlBrt.Should().Be(12500m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C405".AsSpan(), out var meta);
        var registro = (RegistroC405)meta!.Fabrica();

        // DtDoc (index 0), GtFin (index 4) e VlBrt (index 5) são não-nullable — não testados aqui.
        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty); // Cro
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // Crz
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // NumCooFin

        registro.Cro.Should().BeNull();
        registro.Crz.Should().BeNull();
        registro.NumCooFin.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C405|31012023|1|250|123456789|15000,50|12500,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComIntervencaoTecnica_PreservaTextoCanonico()
    {
        // Em caso de intervenção técnica no ECF, deve ser informado um registro para cada
        // redução Z emitida — CRO incrementado, refletindo o reinício de operação.
        const string sped =
            "|C405|01022023|2|1|1|5000,00|4800,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
