using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoE;

/// <summary>
/// Sub-stage 8.173 — exercita a forma do <see cref="RegistroE316"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 230-231): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroE316Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroE316).Assembly);

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
    public void Atributo_DeclaraE316_Nivel4_BlocoE()
    {
        var atributo = typeof(RegistroE316).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("E316");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("E");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroE316Com9CamposNaOrdem()
    {
        _catalogo.TentarObter("E316".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("E316");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodOr", "VlOr", "DtVcto", "CodRec", "NumProc", "IndProc", "Proc", "TxtCompl", "MesRef",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 9));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("E316".AsSpan(), out var meta);
        var registro = (RegistroE316)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "003".AsSpan());                  // CodOr
        meta.Campos[1].Definidor(registro, "875,10".AsSpan());               // VlOr
        meta.Campos[2].Definidor(registro, "20032025".AsSpan());             // DtVcto
        meta.Campos[3].Definidor(registro, "1002".AsSpan());                 // CodRec
        meta.Campos[4].Definidor(registro, "DIFAL/2025/001".AsSpan());       // NumProc
        meta.Campos[5].Definidor(registro, "2".AsSpan());                    // IndProc
        meta.Campos[6].Definidor(registro, "Processo estadual".AsSpan());    // Proc
        meta.Campos[7].Definidor(registro, "Recolhimento DIFAL".AsSpan());   // TxtCompl
        meta.Campos[8].Definidor(registro, "032025".AsSpan());               // MesRef

        registro.CodOr.Should().Be("003");
        registro.VlOr.Should().Be(875.10m);
        registro.DtVcto.Should().Be(new DateOnly(2025, 3, 20));
        registro.CodRec.Should().Be("1002");
        registro.NumProc.Should().Be("DIFAL/2025/001");
        registro.IndProc.Should().Be(IndicadorOrigemProcesso.JusticaEstadual);
        registro.Proc.Should().Be("Processo estadual");
        registro.TxtCompl.Should().Be("Recolhimento DIFAL");
        registro.MesRef.Should().Be("032025");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("E316".AsSpan(), out var meta);
        var registro = (RegistroE316)meta!.Fabrica();

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
        _catalogo.TentarObter("E316".AsSpan(), out var meta);
        var registro = (RegistroE316)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, input.AsSpan());

        registro.IndProc.Should().Be(esperado);
    }

    [Theory]
    [InlineData("000")]
    [InlineData("003")]
    [InlineData("006")]
    [InlineData("090")]
    public async Task RoundTrip_CodOrValido_PreservaTextoCanonico(string codOr)
    {
        var sped = $"|E316|{codOr}|500,00|20032025|1002|||||032025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|E316|003|875,10|20032025|1002|DIFAL/2025/001|2|Processo estadual|Recolhimento DIFAL|032025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|E316|090|1250,75|10042025|1003|||||042025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
