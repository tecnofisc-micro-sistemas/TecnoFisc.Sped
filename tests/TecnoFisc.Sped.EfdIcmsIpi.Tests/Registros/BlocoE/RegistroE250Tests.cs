using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoE;

/// <summary>
/// Sub-stage 8.167 — exercita a forma do <see cref="RegistroE250"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 219-220): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroE250Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroE250).Assembly);

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
    public void Atributo_DeclaraE250_Nivel4_BlocoE()
    {
        var atributo = typeof(RegistroE250).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("E250");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("E");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroE250Com9CamposNaOrdem()
    {
        _catalogo.TentarObter("E250".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("E250");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodOr", "VlOr", "DtVcto", "CodRec", "NumProc", "IndProc", "Proc", "TxtCompl", "MesRef",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 9));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("E250".AsSpan(), out var meta);
        var registro = (RegistroE250)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "001".AsSpan());                  // CodOr
        meta.Campos[1].Definidor(registro, "1750,25".AsSpan());              // VlOr
        meta.Campos[2].Definidor(registro, "15022025".AsSpan());             // DtVcto
        meta.Campos[3].Definidor(registro, "1001".AsSpan());                 // CodRec
        meta.Campos[4].Definidor(registro, "ST/2025/00001".AsSpan());        // NumProc
        meta.Campos[5].Definidor(registro, "1".AsSpan());                    // IndProc
        meta.Campos[6].Definidor(registro, "Processo judicial".AsSpan());    // Proc
        meta.Campos[7].Definidor(registro, "Recolhimento ST".AsSpan());      // TxtCompl
        meta.Campos[8].Definidor(registro, "022025".AsSpan());               // MesRef

        registro.CodOr.Should().Be("001");
        registro.VlOr.Should().Be(1750.25m);
        registro.DtVcto.Should().Be(new DateOnly(2025, 2, 15));
        registro.CodRec.Should().Be("1001");
        registro.NumProc.Should().Be("ST/2025/00001");
        registro.IndProc.Should().Be(IndicadorOrigemProcesso.JusticaFederal);
        registro.Proc.Should().Be("Processo judicial");
        registro.TxtCompl.Should().Be("Recolhimento ST");
        registro.MesRef.Should().Be("022025");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("E250".AsSpan(), out var meta);
        var registro = (RegistroE250)meta!.Fabrica();

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
        _catalogo.TentarObter("E250".AsSpan(), out var meta);
        var registro = (RegistroE250)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, input.AsSpan());

        registro.IndProc.Should().Be(esperado);
    }

    [Theory]
    [InlineData("001")]
    [InlineData("002")]
    [InlineData("006")]
    [InlineData("999")]
    public async Task RoundTrip_CodOrValido_PreservaTextoCanonico(string codOr)
    {
        var sped = $"|E250|{codOr}|500,00|15022025|1001|||||022025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|E250|001|1750,25|15022025|1001|ST/2025/00001|1|Processo judicial|Recolhimento ST|022025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|E250|999|2750,50|10032025|1002|||||032025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
