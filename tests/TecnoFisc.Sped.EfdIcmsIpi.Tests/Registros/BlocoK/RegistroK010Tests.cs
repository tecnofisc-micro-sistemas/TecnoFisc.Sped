using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 8.016.028 — exercita a forma do <see cref="RegistroK010"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.2.2, p. 266 (Subseção 12): metadados de catálogo, mapeamento de campo
/// e invariante de round-trip parse → gerar → texto idêntico. Registro introduzido em
/// V016 (Guide 3.0.9 item 3) como facultativo; obrigatório a partir de V017.
/// </summary>
public sealed class RegistroK010Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroK010).Assembly);

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
    public void Atributo_DeclaraK010_Nivel2_BlocoK_IntroduzidoEmV016()
    {
        var atributo = typeof(RegistroK010).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K010");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("K");
        atributo.IntroduzidoEm.Should().Be((int)LayoutEfdIcmsIpi.V016);
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK010ComUmCampoNaOrdem()
    {
        _catalogo.TentarObter("K010".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K010");
        meta.Campos.Select(c => c.Nome).Should().Equal(["IndTpLeiaute"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
        meta.IntroduzidoEm.Should().Be((int)LayoutEfdIcmsIpi.V016);
    }

    [Fact]
    public void Definidor_AtribuiCampoIndTpLeiaute()
    {
        _catalogo.TentarObter("K010".AsSpan(), out var meta);
        var registro = (RegistroK010)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());

        registro.IndTpLeiaute.Should().Be(TipoLeiauteBlocoK.Completo);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("K010".AsSpan(), out var meta);
        var registro = (RegistroK010)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);

        registro.IndTpLeiaute.Should().BeNull();
    }

    [Theory]
    [InlineData("0", TipoLeiauteBlocoK.Simplificado)]
    [InlineData("1", TipoLeiauteBlocoK.Completo)]
    [InlineData("2", TipoLeiauteBlocoK.RestritoSaldosEstoque)]
    public void Definidor_TipoLeiauteBlocoK_MapeiaCadaValor(string codigo, TipoLeiauteBlocoK esperado)
    {
        _catalogo.TentarObter("K010".AsSpan(), out var meta);
        var registro = (RegistroK010)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, codigo.AsSpan());

        registro.IndTpLeiaute.Should().Be(esperado);
    }

    [Theory]
    [InlineData(TipoLeiauteBlocoK.Simplificado, "0")]
    [InlineData(TipoLeiauteBlocoK.Completo, "1")]
    [InlineData(TipoLeiauteBlocoK.RestritoSaldosEstoque, "2")]
    public void Serializar_IndTpLeiaute_RetornaCodigo(TipoLeiauteBlocoK valor, string esperado)
    {
        _catalogo.TentarObter("K010".AsSpan(), out var meta);
        var registro = (RegistroK010)meta!.Fabrica();
        registro.IndTpLeiaute = valor;

        meta.Campos[0].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_LeiauteSimplificado_PreservaTextoCanonico()
    {
        const string sped = "|K010|0|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_LeiauteCompleto_PreservaTextoCanonico()
    {
        const string sped = "|K010|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_LeiauteRestritoSaldosEstoque_PreservaTextoCanonico()
    {
        const string sped = "|K010|2|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
