using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoE;

/// <summary>
/// Sub-stage 8.161 — exercita a forma do <see cref="RegistroE116"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 212): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroE116Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroE116).Assembly);

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
    public void Atributo_DeclaraE116_Nivel4_BlocoE()
    {
        var atributo = typeof(RegistroE116).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("E116");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("E");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroE116Com9CamposNaOrdem()
    {
        _catalogo.TentarObter("E116".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("E116");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodOr", "VlOr", "DtVcto", "CodRec", "NumProc", "IndProc", "Proc", "TxtCompl", "MesRef",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 9));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("E116".AsSpan(), out var meta);
        var registro = (RegistroE116)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "000".AsSpan());               // CodOr
        meta.Campos[1].Definidor(registro, "1500,00".AsSpan());            // VlOr
        meta.Campos[2].Definidor(registro, "01012025".AsSpan());           // DtVcto
        meta.Campos[3].Definidor(registro, "3310".AsSpan());               // CodRec
        meta.Campos[4].Definidor(registro, "SP/2023/00001".AsSpan());      // NumProc
        meta.Campos[5].Definidor(registro, "0".AsSpan());                  // IndProc
        meta.Campos[6].Definidor(registro, "Processo SEFAZ".AsSpan());     // Proc
        meta.Campos[7].Definidor(registro, "Recolhimento jan/2025".AsSpan()); // TxtCompl
        meta.Campos[8].Definidor(registro, "012025".AsSpan());             // MesRef

        registro.CodOr.Should().Be("000");
        registro.VlOr.Should().Be(1500.00m);
        registro.DtVcto.Should().Be(new DateOnly(2025, 1, 1));
        registro.CodRec.Should().Be("3310");
        registro.NumProc.Should().Be("SP/2023/00001");
        registro.IndProc.Should().Be(IndicadorOrigemProcesso.Sefaz);
        registro.Proc.Should().Be("Processo SEFAZ");
        registro.TxtCompl.Should().Be("Recolhimento jan/2025");
        registro.MesRef.Should().Be("012025");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("E116".AsSpan(), out var meta);
        var registro = (RegistroE116)meta!.Fabrica();

        meta.Campos[4].Definidor(registro, Span<char>.Empty);  // NumProc
        meta.Campos[5].Definidor(registro, Span<char>.Empty);  // IndProc
        meta.Campos[6].Definidor(registro, Span<char>.Empty);  // Proc
        meta.Campos[7].Definidor(registro, Span<char>.Empty);  // TxtCompl

        registro.NumProc.Should().BeNull();
        registro.IndProc.Should().BeNull();
        registro.Proc.Should().BeNull();
        registro.TxtCompl.Should().BeNull();
    }

    [Theory]
    [InlineData("0", IndicadorOrigemProcesso.Sefaz)]
    [InlineData("1", IndicadorOrigemProcesso.JusticaFederal)]
    [InlineData("2", IndicadorOrigemProcesso.JusticaEstadual)]
    [InlineData("9", IndicadorOrigemProcesso.Outros)]
    [InlineData("", null)]
    public void Definidor_IndProc_MapeiaCodigos(string input, IndicadorOrigemProcesso? esperado)
    {
        _catalogo.TentarObter("E116".AsSpan(), out var meta);
        var registro = (RegistroE116)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, input.AsSpan());

        registro.IndProc.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|E116|000|1500,00|01012025|3310|SP/2023/00001|0|Processo SEFAZ|Recolhimento jan/2025|012025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|E116|003|2750,50|10022025|1001|||||012025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
