using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoE;

/// <summary>
/// Sub-stage 8.169 — exercita a forma do <see cref="RegistroE310"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 223-227): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroE310Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroE310).Assembly);

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
    public void Atributo_DeclaraE310_Nivel3_BlocoE()
    {
        var atributo = typeof(RegistroE310).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("E310");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("E");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroE310Com21CamposNaOrdem()
    {
        _catalogo.TentarObter("E310".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("E310");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndMovFcpDifal", "VlSldCredAntDifal", "VlTotDebitosDifal", "VlOutDebDifal",
            "VlTotCreditosDifal", "VlOutCredDifal", "VlSldDevAntDifal", "VlDeducoesDifal",
            "VlRecolDifal", "VlSldCredTransportarDifal", "DebEspDifal", "VlSldCredAntFcp",
            "VlTotDebFcp", "VlOutDebFcp", "VlTotCredFcp", "VlOutCredFcp", "VlSldDevAntFcp",
            "VlDeducoesFcp", "VlRecolFcp", "VlSldCredTransportarFcp", "DebEspFcp"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("E310".AsSpan(), out var meta);
        var registro = (RegistroE310)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());          // IndMovFcpDifal
        meta.Campos[1].Definidor(registro, "100,00".AsSpan());     // VlSldCredAntDifal
        meta.Campos[2].Definidor(registro, "1000,00".AsSpan());    // VlTotDebitosDifal
        meta.Campos[3].Definidor(registro, "50,00".AsSpan());      // VlOutDebDifal
        meta.Campos[4].Definidor(registro, "200,00".AsSpan());     // VlTotCreditosDifal
        meta.Campos[5].Definidor(registro, "25,00".AsSpan());      // VlOutCredDifal
        meta.Campos[6].Definidor(registro, "725,00".AsSpan());     // VlSldDevAntDifal
        meta.Campos[7].Definidor(registro, "100,00".AsSpan());     // VlDeducoesDifal
        meta.Campos[8].Definidor(registro, "625,00".AsSpan());     // VlRecolDifal
        meta.Campos[9].Definidor(registro, "0,00".AsSpan());       // VlSldCredTransportarDifal
        meta.Campos[10].Definidor(registro, "10,00".AsSpan());     // DebEspDifal
        meta.Campos[11].Definidor(registro, "20,00".AsSpan());     // VlSldCredAntFcp
        meta.Campos[12].Definidor(registro, "300,00".AsSpan());    // VlTotDebFcp
        meta.Campos[13].Definidor(registro, "30,00".AsSpan());     // VlOutDebFcp
        meta.Campos[14].Definidor(registro, "40,00".AsSpan());     // VlTotCredFcp
        meta.Campos[15].Definidor(registro, "15,00".AsSpan());     // VlOutCredFcp
        meta.Campos[16].Definidor(registro, "255,00".AsSpan());    // VlSldDevAntFcp
        meta.Campos[17].Definidor(registro, "55,00".AsSpan());     // VlDeducoesFcp
        meta.Campos[18].Definidor(registro, "200,00".AsSpan());    // VlRecolFcp
        meta.Campos[19].Definidor(registro, "0,00".AsSpan());      // VlSldCredTransportarFcp
        meta.Campos[20].Definidor(registro, "5,00".AsSpan());      // DebEspFcp

        registro.IndMovFcpDifal.Should().Be(IndicadorMovimentoFcpDifal.ComOperacoes);
        registro.VlSldCredAntDifal.Should().Be(100.00m);
        registro.VlTotDebitosDifal.Should().Be(1000.00m);
        registro.VlOutDebDifal.Should().Be(50.00m);
        registro.VlTotCreditosDifal.Should().Be(200.00m);
        registro.VlOutCredDifal.Should().Be(25.00m);
        registro.VlSldDevAntDifal.Should().Be(725.00m);
        registro.VlDeducoesDifal.Should().Be(100.00m);
        registro.VlRecolDifal.Should().Be(625.00m);
        registro.VlSldCredTransportarDifal.Should().Be(0.00m);
        registro.DebEspDifal.Should().Be(10.00m);
        registro.VlSldCredAntFcp.Should().Be(20.00m);
        registro.VlTotDebFcp.Should().Be(300.00m);
        registro.VlOutDebFcp.Should().Be(30.00m);
        registro.VlTotCredFcp.Should().Be(40.00m);
        registro.VlOutCredFcp.Should().Be(15.00m);
        registro.VlSldDevAntFcp.Should().Be(255.00m);
        registro.VlDeducoesFcp.Should().Be(55.00m);
        registro.VlRecolFcp.Should().Be(200.00m);
        registro.VlSldCredTransportarFcp.Should().Be(0.00m);
        registro.DebEspFcp.Should().Be(5.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("E310".AsSpan(), out var meta);
        var registro = (RegistroE310)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.IndMovFcpDifal.Should().Be(default(IndicadorMovimentoFcpDifal));
        registro.VlSldCredAntDifal.Should().Be(0m);
        registro.VlTotDebitosDifal.Should().Be(0m);
        registro.VlOutDebDifal.Should().Be(0m);
        registro.VlTotCreditosDifal.Should().Be(0m);
        registro.VlOutCredDifal.Should().Be(0m);
        registro.VlSldDevAntDifal.Should().Be(0m);
        registro.VlDeducoesDifal.Should().Be(0m);
        registro.VlRecolDifal.Should().Be(0m);
        registro.VlSldCredTransportarDifal.Should().Be(0m);
        registro.DebEspDifal.Should().Be(0m);
        registro.VlSldCredAntFcp.Should().Be(0m);
        registro.VlTotDebFcp.Should().Be(0m);
        registro.VlOutDebFcp.Should().Be(0m);
        registro.VlTotCredFcp.Should().Be(0m);
        registro.VlOutCredFcp.Should().Be(0m);
        registro.VlSldDevAntFcp.Should().Be(0m);
        registro.VlDeducoesFcp.Should().Be(0m);
        registro.VlRecolFcp.Should().Be(0m);
        registro.VlSldCredTransportarFcp.Should().Be(0m);
        registro.DebEspFcp.Should().Be(0m);
    }

    [Theory]
    [InlineData("0", IndicadorMovimentoFcpDifal.SemOperacoes)]
    [InlineData("1", IndicadorMovimentoFcpDifal.ComOperacoes)]
    public void Definidor_IndMovFcpDifal_MapeiaValoresCorretos(string valor, IndicadorMovimentoFcpDifal esperado)
    {
        _catalogo.TentarObter("E310".AsSpan(), out var meta);
        var registro = (RegistroE310)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, valor.AsSpan());

        registro.IndMovFcpDifal.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|E310|1|100,00|1000,00|50,00|200,00|25,00|725,00|100,00|625,00|0,00|10,00|20,00|300,00|30,00|40,00|15,00|255,00|55,00|200,00|0,00|5,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComPeriodoSemMovimentoFcpDifal_PreservaTextoCanonico()
    {
        const string sped =
            "|E310|0|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
