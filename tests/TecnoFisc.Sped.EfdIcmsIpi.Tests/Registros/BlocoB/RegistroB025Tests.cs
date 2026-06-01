using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoB;

/// <summary>
/// Sub-stage 8.025 — exercita a forma do <see cref="RegistroB025"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 47): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroB025Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroB025).Assembly);

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
    public void Atributo_DeclaraB025_Nivel3_BlocoB()
    {
        var atributo = typeof(RegistroB025).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("B025");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("B");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroB025Com6CamposNaOrdem()
    {
        _catalogo.TentarObter("B025".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("B025");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "VlContP", "VlBcIssP", "AliqIss", "VlIssP", "VlIsntIssP", "CodServ",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 6));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("B025".AsSpan(), out var meta);
        var registro = (RegistroB025)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1000,00".AsSpan());  // VlContP
        meta.Campos[1].Definidor(registro, "850,00".AsSpan());   // VlBcIssP
        meta.Campos[2].Definidor(registro, "5,00".AsSpan());     // AliqIss
        meta.Campos[3].Definidor(registro, "42,50".AsSpan());    // VlIssP
        meta.Campos[4].Definidor(registro, "0,00".AsSpan());     // VlIsntIssP
        meta.Campos[5].Definidor(registro, "0101".AsSpan());     // CodServ

        registro.VlContP.Should().Be(1000.00m);
        registro.VlBcIssP.Should().Be(850.00m);
        registro.AliqIss.Should().Be(5.00m);
        registro.VlIssP.Should().Be(42.50m);
        registro.VlIsntIssP.Should().Be(0.00m);
        registro.CodServ.Should().Be("0101");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("B025".AsSpan(), out var meta);
        var registro = (RegistroB025)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, Span<char>.Empty); // CodServ
        registro.CodServ.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|B025|1000,00|850,00|5,00|42,50|0,00|0101|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComValoresIsencaoEAliquotaMenorQueMax_PreservaTextoCanonico()
    {
        // ISS com alíquota 2%, parte isenta > 0.
        const string sped = "|B025|500,00|300,00|2,00|6,00|200,00|1403|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComBaseCalculoZero_PreservaTextoCanonico()
    {
        // Operação totalmente isenta: BC=0, VL_ISS=0, isenta=500.
        const string sped = "|B025|500,00|0,00|5,00|0,00|500,00|0701|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
