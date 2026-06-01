using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoE;

/// <summary>
/// Sub-stage 8.163 — exercita a forma do <see cref="RegistroE210"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 214): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroE210Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroE210).Assembly);

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
    public void Atributo_DeclaraE210_Nivel3_BlocoE()
    {
        var atributo = typeof(RegistroE210).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("E210");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("E");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroE210Com14CamposNaOrdem()
    {
        _catalogo.TentarObter("E210".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("E210");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndMovSt", "VlSldCredAntSt", "VlDevolSt", "VlRessarcSt",
            "VlOutCredSt", "VlAjCreditosSt", "VlRetencaoSt", "VlOutDebSt",
            "VlAjDebitosSt", "VlSldDevAntSt", "VlDeducoesSt", "VlIcmsRecolSt",
            "VlSldCredStTransportar", "DebEspSt"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("E210".AsSpan(), out var meta);
        var registro = (RegistroE210)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());          // IndMovSt
        meta.Campos[1].Definidor(registro, "500,00".AsSpan());     // VlSldCredAntSt
        meta.Campos[2].Definidor(registro, "100,00".AsSpan());     // VlDevolSt
        meta.Campos[3].Definidor(registro, "50,00".AsSpan());      // VlRessarcSt
        meta.Campos[4].Definidor(registro, "30,00".AsSpan());      // VlOutCredSt
        meta.Campos[5].Definidor(registro, "20,00".AsSpan());      // VlAjCreditosSt
        meta.Campos[6].Definidor(registro, "1000,00".AsSpan());    // VlRetencaoSt
        meta.Campos[7].Definidor(registro, "40,00".AsSpan());      // VlOutDebSt
        meta.Campos[8].Definidor(registro, "10,00".AsSpan());      // VlAjDebitosSt
        meta.Campos[9].Definidor(registro, "0,00".AsSpan());       // VlSldDevAntSt
        meta.Campos[10].Definidor(registro, "0,00".AsSpan());      // VlDeducoesSt
        meta.Campos[11].Definidor(registro, "0,00".AsSpan());      // VlIcmsRecolSt
        meta.Campos[12].Definidor(registro, "300,00".AsSpan());    // VlSldCredStTransportar
        meta.Campos[13].Definidor(registro, "5,00".AsSpan());      // DebEspSt

        registro.IndMovSt.Should().Be(IndicadorMovimentoSt.ComOperacoes);
        registro.VlSldCredAntSt.Should().Be(500.00m);
        registro.VlDevolSt.Should().Be(100.00m);
        registro.VlRessarcSt.Should().Be(50.00m);
        registro.VlOutCredSt.Should().Be(30.00m);
        registro.VlAjCreditosSt.Should().Be(20.00m);
        registro.VlRetencaoSt.Should().Be(1000.00m);
        registro.VlOutDebSt.Should().Be(40.00m);
        registro.VlAjDebitosSt.Should().Be(10.00m);
        registro.VlSldDevAntSt.Should().Be(0.00m);
        registro.VlDeducoesSt.Should().Be(0.00m);
        registro.VlIcmsRecolSt.Should().Be(0.00m);
        registro.VlSldCredStTransportar.Should().Be(300.00m);
        registro.DebEspSt.Should().Be(5.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("E210".AsSpan(), out var meta);
        var registro = (RegistroE210)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.IndMovSt.Should().Be(default(IndicadorMovimentoSt));
        registro.VlSldCredAntSt.Should().Be(0m);
        registro.VlDevolSt.Should().Be(0m);
        registro.VlRessarcSt.Should().Be(0m);
        registro.VlOutCredSt.Should().Be(0m);
        registro.VlAjCreditosSt.Should().Be(0m);
        registro.VlRetencaoSt.Should().Be(0m);
        registro.VlOutDebSt.Should().Be(0m);
        registro.VlAjDebitosSt.Should().Be(0m);
        registro.VlSldDevAntSt.Should().Be(0m);
        registro.VlDeducoesSt.Should().Be(0m);
        registro.VlIcmsRecolSt.Should().Be(0m);
        registro.VlSldCredStTransportar.Should().Be(0m);
        registro.DebEspSt.Should().Be(0m);
    }

    [Theory]
    [InlineData("0", IndicadorMovimentoSt.SemOperacoes)]
    [InlineData("1", IndicadorMovimentoSt.ComOperacoes)]
    public void Definidor_IndMovSt_MapeiaValoresCorretos(string valor, IndicadorMovimentoSt esperado)
    {
        _catalogo.TentarObter("E210".AsSpan(), out var meta);
        var registro = (RegistroE210)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, valor.AsSpan());

        registro.IndMovSt.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|E210|1|500,00|100,00|50,00|30,00|20,00|1000,00|40,00|10,00|0,00|0,00|0,00|300,00|5,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComPeriodoSemMovimentoSt_PreservaTextoCanonico()
    {
        // Período sem operações de ST: IND_MOV_ST = 0 e todos os valores zerados.
        const string sped =
            "|E210|0|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
