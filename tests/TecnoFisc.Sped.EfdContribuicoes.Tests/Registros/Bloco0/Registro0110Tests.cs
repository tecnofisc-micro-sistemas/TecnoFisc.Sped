using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 4.005 — exercita a forma do <see cref="Registro0110"/> contra o Guia Prático
/// v1.35 (p. 71-73): metadados de catálogo, mapeamento de campos, serialização de enums e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class Registro0110Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0110).Assembly);

    [Fact]
    public void Atributo_DeclaraCodigo0110_Nivel2_Bloco0()
    {
        var atributo = typeof(Registro0110).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0110");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0110ComQuatroCamposNaOrdem()
    {
        _catalogo.TentarObter("0110".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0110");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodIncTrib",
            "IndAproCred",
            "CodTipoCont",
            "IndRegCum",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0110".AsSpan(), out var meta);
        var registro = (Registro0110)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());
        meta.Campos[1].Definidor(registro, "2".AsSpan());
        meta.Campos[2].Definidor(registro, "1".AsSpan());
        meta.Campos[3].Definidor(registro, "9".AsSpan());

        registro.CodIncTrib.Should().Be(CodigoIncidenciaTributaria.RegimeNaoCumulativo);
        registro.IndAproCred.Should().Be(IndicadorApropriacaoCredito.RateioProporcionalReceitaBruta);
        registro.CodTipoCont.Should().Be(CodigoTipoContribuicao.AliquotaBasica);
        registro.IndRegCum.Should().Be(IndicadorRegimeCumulativo.RegimeDeCompetenciaDetalhado);
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("0110".AsSpan(), out var meta);
        var registro = (Registro0110)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.IndAproCred.Should().BeNull();
        registro.CodTipoCont.Should().BeNull();
        registro.IndRegCum.Should().BeNull();
    }

    [Theory]
    [InlineData(CodigoIncidenciaTributaria.RegimeNaoCumulativo, "1")]
    [InlineData(CodigoIncidenciaTributaria.RegimeCumulativo, "2")]
    [InlineData(CodigoIncidenciaTributaria.RegimesNaoCumulativoECumulativo, "3")]
    public void Serializar_CodIncTrib_RetornaCodigoSped(CodigoIncidenciaTributaria valor, string esperado)
    {
        _catalogo.TentarObter("0110".AsSpan(), out var meta);
        var registro = (Registro0110)meta!.Fabrica();
        registro.CodIncTrib = valor;

        meta.Campos[0].Serializar(registro).Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorApropriacaoCredito.ApropriacaoDireta, "1")]
    [InlineData(IndicadorApropriacaoCredito.RateioProporcionalReceitaBruta, "2")]
    public void Serializar_IndAproCred_RetornaCodigoSped(IndicadorApropriacaoCredito valor, string esperado)
    {
        _catalogo.TentarObter("0110".AsSpan(), out var meta);
        var registro = (Registro0110)meta!.Fabrica();
        registro.IndAproCred = valor;

        meta.Campos[1].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public void Serializar_IndAproCredNulo_DevolveVazio()
    {
        _catalogo.TentarObter("0110".AsSpan(), out var meta);
        var registro = (Registro0110)meta!.Fabrica();
        registro.IndAproCred = null;

        meta.Campos[1].Serializar(registro).Should().BeEmpty();
    }

    [Theory]
    [InlineData(IndicadorRegimeCumulativo.RegimeDeCaixa, "1")]
    [InlineData(IndicadorRegimeCumulativo.RegimeDeCompetenciaConsolidado, "2")]
    [InlineData(IndicadorRegimeCumulativo.RegimeDeCompetenciaDetalhado, "9")]
    public void Serializar_IndRegCum_RetornaCodigoSped(IndicadorRegimeCumulativo valor, string esperado)
    {
        _catalogo.TentarObter("0110".AsSpan(), out var meta);
        var registro = (Registro0110)meta!.Fabrica();
        registro.IndRegCum = valor;

        meta.Campos[3].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Empresa sujeita a ambos os regimes, rateio proporcional, alíquota básica
        const string sped = "|0110|3|2|1||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_RegimeCumulativoLucroPressumido_PreservaTextoCanonico()
    {
        // Empresa somente cumulativa, lucro presumido, regime de caixa
        const string sped = "|0110|2|||1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_RegimeNaoCumulativo_PreservaTextoCanonico()
    {
        // Empresa somente não-cumulativa, apropriação direta, alíquotas específicas, sem IND_REG_CUM
        const string sped = "|0110|1|1|2||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_AninhadoSob0001_RespeitaHierarquia()
    {
        const string sped =
            "|0000|006|0|||01012025|31012025|EMPRESA SA|11222333000181|SP|3550308||00|2|\r\n" +
            "|0001|0|\r\n" +
            "|0110|1|1|1||\r\n";

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
