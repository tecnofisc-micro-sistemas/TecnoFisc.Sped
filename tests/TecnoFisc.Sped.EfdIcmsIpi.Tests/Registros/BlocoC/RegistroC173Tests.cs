using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.056 — exercita a forma do <see cref="RegistroC173"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 83): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC173Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC173).Assembly);

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
    public void Atributo_DeclaraC173_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC173).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C173");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC173Com7CamposNaOrdem()
    {
        _catalogo.TentarObter("C173".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C173");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "LoteMed", "QtdItem", "DtFab", "DtVal", "IndMed", "TpProd", "VlTabMax",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 7));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C173".AsSpan(), out var meta);
        var registro = (RegistroC173)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "LOTE0001".AsSpan());  // LoteMed
        meta.Campos[1].Definidor(registro, "100,000".AsSpan());   // QtdItem
        meta.Campos[2].Definidor(registro, "01012023".AsSpan());  // DtFab
        meta.Campos[3].Definidor(registro, "31122025".AsSpan());  // DtVal
        meta.Campos[4].Definidor(registro, "2".AsSpan());         // IndMed
        meta.Campos[5].Definidor(registro, "1".AsSpan());         // TpProd
        meta.Campos[6].Definidor(registro, "45,90".AsSpan());     // VlTabMax

        registro.LoteMed.Should().Be("LOTE0001");
        registro.QtdItem.Should().Be(100.000m);
        registro.DtFab.Should().Be(new DateOnly(2023, 1, 1));
        registro.DtVal.Should().Be(new DateOnly(2025, 12, 31));
        registro.IndMed.Should().Be(IndicadorBaseMedicamento.ListaNegativa);
        registro.TpProd.Should().Be(TipoProdutoFarmaceutico.Generico);
        registro.VlTabMax.Should().Be(45.90m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C173".AsSpan(), out var meta);
        var registro = (RegistroC173)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.LoteMed.Should().BeNull();
        registro.QtdItem.Should().BeNull();
        registro.DtFab.Should().BeNull();
        registro.DtVal.Should().BeNull();
        registro.IndMed.Should().BeNull();
        registro.TpProd.Should().BeNull();
        registro.VlTabMax.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Medicamento genérico, lote, quantidade, fabricação 01/01/2023, validade 31/12/2025, Lista Negativa, preço máx 45,90.
        const string sped = "|C173|LOTE0001|100,000|01012023|31122025|2|1|45,90|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposVazios_PreservaTextoCanonico()
    {
        // Todos os campos opcionais ausentes.
        const string sped = "|C173||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", IndicadorBaseMedicamento.PrecoTabeladoOuMaximoSugerido)]
    [InlineData("1", IndicadorBaseMedicamento.MargemValorAgregado)]
    [InlineData("2", IndicadorBaseMedicamento.ListaNegativa)]
    [InlineData("3", IndicadorBaseMedicamento.ListaPositiva)]
    [InlineData("4", IndicadorBaseMedicamento.ListaNeutra)]
    public void Definidor_IndMed_CobertosEnum(string valor, IndicadorBaseMedicamento esperado)
    {
        _catalogo.TentarObter("C173".AsSpan(), out var meta);
        var registro = (RegistroC173)meta!.Fabrica();

        meta.Campos[4].Definidor(registro, valor.AsSpan());

        registro.IndMed.Should().Be(esperado);
    }

    [Theory]
    [InlineData("0", TipoProdutoFarmaceutico.Similar)]
    [InlineData("1", TipoProdutoFarmaceutico.Generico)]
    [InlineData("2", TipoProdutoFarmaceutico.EticoOuDeMarca)]
    public void Definidor_TpProd_CobertosEnum(string valor, TipoProdutoFarmaceutico esperado)
    {
        _catalogo.TentarObter("C173".AsSpan(), out var meta);
        var registro = (RegistroC173)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, valor.AsSpan());

        registro.TpProd.Should().Be(esperado);
    }
}
