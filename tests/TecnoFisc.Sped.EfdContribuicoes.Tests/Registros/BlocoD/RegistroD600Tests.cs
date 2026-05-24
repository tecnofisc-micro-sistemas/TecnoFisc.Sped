using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoD;

public sealed class RegistroD600Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD600).Assembly);

    [Fact]
    public void Atributo_DeclaraD600_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD600).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D600");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD600Com18CamposNaOrdem()
    {
        _catalogo.TentarObter("D600".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D600");
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodMod", "CodMun", "Ser", "Sub", "IndRec",
            "QtdCons", "DtDocIni", "DtDocFin", "VlDoc", "VlDesc",
            "VlServ", "VlServNt", "VlTerc", "VlDa", "VlBcIcms",
            "VlIcms", "VlPis", "VlCofins"
        ]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D600".AsSpan(), out var meta);
        var registro = (RegistroD600)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "21".AsSpan());         // CodMod
        meta.Campos[1].Definidor(registro, "3550308".AsSpan());    // CodMun
        meta.Campos[2].Definidor(registro, "B".AsSpan());          // Ser
        meta.Campos[3].Definidor(registro, "2".AsSpan());          // Sub
        meta.Campos[4].Definidor(registro, "0".AsSpan());          // IndRec
        meta.Campos[5].Definidor(registro, "150".AsSpan());        // QtdCons
        meta.Campos[6].Definidor(registro, "01012022".AsSpan());   // DtDocIni
        meta.Campos[7].Definidor(registro, "31012022".AsSpan());   // DtDocFin
        meta.Campos[8].Definidor(registro, "50000,00".AsSpan());   // VlDoc
        meta.Campos[9].Definidor(registro, "500,00".AsSpan());     // VlDesc
        meta.Campos[10].Definidor(registro, "49500,00".AsSpan());  // VlServ
        meta.Campos[11].Definidor(registro, "100,00".AsSpan());    // VlServNt
        meta.Campos[12].Definidor(registro, "50,00".AsSpan());     // VlTerc
        meta.Campos[13].Definidor(registro, "20,00".AsSpan());     // VlDa
        meta.Campos[14].Definidor(registro, "5000,00".AsSpan());   // VlBcIcms
        meta.Campos[15].Definidor(registro, "1250,00".AsSpan());   // VlIcms
        meta.Campos[16].Definidor(registro, "82,50".AsSpan());     // VlPis
        meta.Campos[17].Definidor(registro, "380,00".AsSpan());    // VlCofins

        registro.CodMod.Should().Be("21");
        registro.CodMun.Should().Be("3550308");
        registro.Ser.Should().Be("B");
        registro.Sub.Should().Be("2");
        registro.IndRec.Should().Be(IndicadorTipoReceitaTelecom.ReceitaPropriaServicosPrestados);
        registro.QtdCons.Should().Be(150);
        registro.DtDocIni.Should().Be(new DateOnly(2022, 1, 1));
        registro.DtDocFin.Should().Be(new DateOnly(2022, 1, 31));
        registro.VlDoc.Should().Be(50000.00m);
        registro.VlDesc.Should().Be(500.00m);
        registro.VlServ.Should().Be(49500.00m);
        registro.VlServNt.Should().Be(100.00m);
        registro.VlTerc.Should().Be(50.00m);
        registro.VlDa.Should().Be(20.00m);
        registro.VlBcIcms.Should().Be(5000.00m);
        registro.VlIcms.Should().Be(1250.00m);
        registro.VlPis.Should().Be(82.50m);
        registro.VlCofins.Should().Be(380.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D600".AsSpan(), out var meta);
        var registro = (RegistroD600)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);   // CodMun
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);   // Ser
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);   // Sub

        registro.CodMun.Should().BeNull();
        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
    }

    [Theory]
    [InlineData(IndicadorTipoReceitaTelecom.ReceitaPropriaServicosPrestados, "0")]
    [InlineData(IndicadorTipoReceitaTelecom.ReceitaPropriaCobrancaDebitos, "1")]
    [InlineData(IndicadorTipoReceitaTelecom.ReceitaPropriaPrePagoFaturamentoPeriodosAnteriores, "2")]
    [InlineData(IndicadorTipoReceitaTelecom.ReceitaPropriaPrePagoFaturamentoPeriodo, "3")]
    [InlineData(IndicadorTipoReceitaTelecom.OutrasReceitasPropriasComunicacaoTelecom, "4")]
    [InlineData(IndicadorTipoReceitaTelecom.ReceitaPropriaCoFaturamento, "5")]
    [InlineData(IndicadorTipoReceitaTelecom.ReceitaPropriaServicosAFaturar, "6")]
    [InlineData(IndicadorTipoReceitaTelecom.OutrasReceitasPropriasnNaoCumulativa, "7")]
    [InlineData(IndicadorTipoReceitaTelecom.OutrasReceitasTerceiros, "8")]
    [InlineData(IndicadorTipoReceitaTelecom.OutrasReceitas, "9")]
    public void Serializar_IndRec_RetornaCodigoSpedCorreto(
        IndicadorTipoReceitaTelecom indRec, string esperado)
    {
        _catalogo.TentarObter("D600".AsSpan(), out var meta);
        var registro = (RegistroD600)meta!.Fabrica();
        registro.IndRec = indRec;

        meta.Campos[4].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|D600|21|3550308|B|2|0|150|01012022|31012022|50000,00|500,00|49500,00|100,00|50,00|20,00|5000,00|1250,00|82,50|380,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // CodMun, Ser, Sub, VlDesc, VlServNt, VlTerc, VlDa, VlBcIcms, VlIcms, VlPis, VlCofins omitidos.
        const string sped =
            "|D600|22||||0|100|01012022|31012022|20000,00||20000,00||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

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
}
