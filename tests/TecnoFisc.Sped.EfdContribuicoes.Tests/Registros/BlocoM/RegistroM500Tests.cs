using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoM;

public sealed class RegistroM500Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroM500).Assembly);

    [Fact]
    public void Atributo_DeclaraM500_Nivel2_BlocoM()
    {
        var atributo = typeof(RegistroM500).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("M500");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("M");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroM500Com14CamposNaOrdem()
    {
        _catalogo.TentarObter("M500".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("M500");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodCred", "IndCredOri", "VlBcCofins", "AliqCofins", "QuantBcCofins", "AliqCofinsQuant",
            "VlCred", "VlAjusAcres", "VlAjusReduc", "VlCredDifer", "VlCredDisp",
            "IndDescCred", "VlCredDesc", "SldCred",
        ]);
        meta.Campos[0].Tamanho.Should().Be(3);      // CodCred
        meta.Campos[0].Obrigatorio.Should().BeTrue();
        meta.Campos[1].Tamanho.Should().Be(1);      // IndCredOri
        meta.Campos[1].Obrigatorio.Should().BeTrue();
        meta.Campos[3].Tamanho.Should().Be(8);      // AliqCofins
        meta.Campos[11].Tamanho.Should().Be(1);     // IndDescCred
        meta.Campos[11].Obrigatorio.Should().BeTrue();
        meta.Campos[6].Obrigatorio.Should().BeTrue();   // VlCred
        meta.Campos[13].Obrigatorio.Should().BeTrue();  // SldCred
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("M500".AsSpan(), out var meta);
        var registro = (RegistroM500)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "101".AsSpan());         // CodCred
        meta.Campos[1].Definidor(registro, "0".AsSpan());           // IndCredOri
        meta.Campos[2].Definidor(registro, "100000,00".AsSpan());   // VlBcCofins
        meta.Campos[3].Definidor(registro, "7,6000".AsSpan());      // AliqCofins
        meta.Campos[4].Definidor(registro, "".AsSpan());            // QuantBcCofins
        meta.Campos[5].Definidor(registro, "".AsSpan());            // AliqCofinsQuant
        meta.Campos[6].Definidor(registro, "7600,00".AsSpan());     // VlCred
        meta.Campos[7].Definidor(registro, "0,00".AsSpan());        // VlAjusAcres
        meta.Campos[8].Definidor(registro, "0,00".AsSpan());        // VlAjusReduc
        meta.Campos[9].Definidor(registro, "0,00".AsSpan());        // VlCredDifer
        meta.Campos[10].Definidor(registro, "7600,00".AsSpan());    // VlCredDisp
        meta.Campos[11].Definidor(registro, "0".AsSpan());          // IndDescCred
        meta.Campos[12].Definidor(registro, "7600,00".AsSpan());    // VlCredDesc
        meta.Campos[13].Definidor(registro, "0,00".AsSpan());       // SldCred

        registro.CodCred.Should().Be("101");
        registro.IndCredOri.Should().Be(IndicadorOrigemCreditoApurado.OperacoesProprias);
        registro.VlBcCofins.Should().Be(100000m);
        registro.AliqCofins.Should().Be(7.6m);
        registro.QuantBcCofins.Should().BeNull();
        registro.AliqCofinsQuant.Should().BeNull();
        registro.VlCred.Should().Be(7600m);
        registro.VlAjusAcres.Should().Be(0m);
        registro.VlAjusReduc.Should().Be(0m);
        registro.VlCredDifer.Should().Be(0m);
        registro.VlCredDisp.Should().Be(7600m);
        registro.IndDescCred.Should().Be(IndicadorDescontoCredito.UtilizacaoTotal);
        registro.VlCredDesc.Should().Be(7600m);
        registro.SldCred.Should().Be(0m);
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("M500".AsSpan(), out var meta);
        var registro = (RegistroM500)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlBcCofins
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqCofins
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);  // QuantBcCofins
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqCofinsQuant
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCredDesc

        registro.VlBcCofins.Should().BeNull();
        registro.AliqCofins.Should().BeNull();
        registro.QuantBcCofins.Should().BeNull();
        registro.AliqCofinsQuant.Should().BeNull();
        registro.VlCredDesc.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|M500|101|0|100000,00|7,6000|||7600,00|0,00|0,00|0,00|7600,00|0|7600,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CreditoPorQuantidade_PreservaTextoCanonico()
    {
        const string sped =
            "|M500|103|0|||500,000|1,5000|750,00|0,00|0,00|0,00|750,00|1|375,00|375,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_EventoSuccessao_PreservaTextoCanonico()
    {
        const string sped =
            "|M500|201|1|||||5000,00|500,00|0,00|0,00|5500,00|0|5500,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", IndicadorOrigemCreditoApurado.OperacoesProprias)]
    [InlineData("1", IndicadorOrigemCreditoApurado.EventoSuccessao)]
    public void IndicadorOrigemCreditoApurado_Roundtrip(string valor, IndicadorOrigemCreditoApurado esperado)
    {
        _catalogo.TentarObter("M500".AsSpan(), out var meta);
        var registro = (RegistroM500)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, valor.AsSpan());

        registro.IndCredOri.Should().Be(esperado);
    }

    [Theory]
    [InlineData("0", IndicadorDescontoCredito.UtilizacaoTotal)]
    [InlineData("1", IndicadorDescontoCredito.UtilizacaoParcial)]
    public void IndicadorDescontoCredito_Roundtrip(string valor, IndicadorDescontoCredito esperado)
    {
        _catalogo.TentarObter("M500".AsSpan(), out var meta);
        var registro = (RegistroM500)meta!.Fabrica();

        meta.Campos[11].Definidor(registro, valor.AsSpan());

        registro.IndDescCred.Should().Be(esperado);
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
