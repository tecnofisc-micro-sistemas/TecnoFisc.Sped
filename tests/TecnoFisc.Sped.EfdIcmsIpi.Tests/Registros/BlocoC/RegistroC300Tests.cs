using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.071 — exercita a forma do <see cref="RegistroC300"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 103): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC300Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC300).Assembly);

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
    public void Atributo_DeclaraC300_Nivel2_BlocoC()
    {
        var atributo = typeof(RegistroC300).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C300");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC300Com10CamposNaOrdem()
    {
        _catalogo.TentarObter("C300".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C300");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["CodMod", "Ser", "Sub", "NumDocIni", "NumDocFin", "DtDoc", "VlDoc", "VlPis", "VlCofins", "CodCta"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C300".AsSpan(), out var meta);
        var registro = (RegistroC300)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "02".AsSpan());          // CodMod
        meta.Campos[1].Definidor(registro, "001".AsSpan());          // Ser
        meta.Campos[2].Definidor(registro, "001".AsSpan());          // Sub
        meta.Campos[3].Definidor(registro, "1".AsSpan());            // NumDocIni
        meta.Campos[4].Definidor(registro, "100".AsSpan());          // NumDocFin
        meta.Campos[5].Definidor(registro, "01012023".AsSpan());     // DtDoc
        meta.Campos[6].Definidor(registro, "5000,00".AsSpan());      // VlDoc
        meta.Campos[7].Definidor(registro, "250,00".AsSpan());       // VlPis
        meta.Campos[8].Definidor(registro, "1150,00".AsSpan());      // VlCofins
        meta.Campos[9].Definidor(registro, "3.01.01".AsSpan());      // CodCta

        registro.CodMod.Should().Be("02");
        registro.Ser.Should().Be("001");
        registro.Sub.Should().Be("001");
        registro.NumDocIni.Should().Be(1);
        registro.NumDocFin.Should().Be(100);
        registro.DtDoc.Should().Be(new DateOnly(2023, 1, 1));
        registro.VlDoc.Should().Be(5000m);
        registro.VlPis.Should().Be(250m);
        registro.VlCofins.Should().Be(1150m);
        registro.CodCta.Should().Be("3.01.01");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C300".AsSpan(), out var meta);
        var registro = (RegistroC300)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty); // CodMod
        meta.Campos[1].Definidor(registro, Span<char>.Empty); // Ser
        meta.Campos[2].Definidor(registro, Span<char>.Empty); // Sub
        meta.Campos[3].Definidor(registro, Span<char>.Empty); // NumDocIni
        meta.Campos[4].Definidor(registro, Span<char>.Empty); // NumDocFin
        meta.Campos[7].Definidor(registro, Span<char>.Empty); // VlPis
        meta.Campos[8].Definidor(registro, Span<char>.Empty); // VlCofins
        meta.Campos[9].Definidor(registro, Span<char>.Empty); // CodCta

        registro.CodMod.Should().BeNull();
        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
        registro.NumDocIni.Should().BeNull();
        registro.NumDocFin.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C300|02|001|001|1|100|01012023|5000,00|250,00|1150,00|3.01.01|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SomenteObrigatorios_PreservaTextoCanonico()
    {
        // Sub, VL_PIS, VL_COFINS e COD_CTA são opcionais.
        const string sped =
            "|C300|02|001||1|100|01012023|5000,00||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
