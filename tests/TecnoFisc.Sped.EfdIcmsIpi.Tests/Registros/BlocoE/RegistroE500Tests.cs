using System.Globalization;
using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoE;

/// <summary>
/// Sub-stage 8.174 — exercita a forma do <see cref="RegistroE500"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 231): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroE500Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroE500).Assembly);

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
    public void Atributo_DeclaraE500_Nivel2_BlocoE()
    {
        var atributo = typeof(RegistroE500).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("E500");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("E");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroE500Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("E500".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("E500");
        meta.Campos.Select(c => c.Nome).Should().Equal(["IndApur", "DtIni", "DtFin"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("E500".AsSpan(), out var meta);
        var registro = (RegistroE500)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());          // IndApur
        meta.Campos[1].Definidor(registro, "01012025".AsSpan());   // DtIni
        meta.Campos[2].Definidor(registro, "31012025".AsSpan());   // DtFin

        registro.IndApur.Should().Be(IndicadorApuracaoIpi.Mensal);
        registro.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2025, 1, 31));
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("E500".AsSpan(), out var meta);
        var registro = (RegistroE500)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.IndApur.Should().BeNull();
        registro.DtIni.Should().Be(default(DateOnly));
        registro.DtFin.Should().Be(default(DateOnly));
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|E500|0|01012025|31012025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData(IndicadorApuracaoIpi.Mensal, "0")]
    [InlineData(IndicadorApuracaoIpi.Decendial, "1")]
    public async Task RoundTrip_ComIndicadorApuracao_PreservaTextoCanonico(
        IndicadorApuracaoIpi valor, string esperado)
    {
        var sped = $"|E500|{esperado}|01022025|10022025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
        ((int)valor).ToString(CultureInfo.InvariantCulture).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComPeriodoDecendial_PreservaTextoCanonico()
    {
        const string sped = "|E500|1|11022025|20022025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
