using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.034 — exercita a forma do <see cref="RegistroI200"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 142–147): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI200Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000).Assembly);

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
    public void Atributo_DeclaraI200_Nivel3_BlocoI()
    {
        var atributo = typeof(RegistroI200).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I200");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI200Com5CamposNaOrdem()
    {
        _catalogo.TentarObter("I200".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I200");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "NumLcto", "DtLcto", "VlLcto", "IndLcto", "DtLctoExt",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I200".AsSpan(), out var meta);
        var registro = (RegistroI200)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1000".AsSpan());           // NumLcto
        meta.Campos[1].Definidor(registro, "02052023".AsSpan());       // DtLcto
        meta.Campos[2].Definidor(registro, "5000,00".AsSpan());        // VlLcto
        meta.Campos[3].Definidor(registro, "N".AsSpan());              // IndLcto
        meta.Campos[4].Definidor(registro, "15032022".AsSpan());       // DtLctoExt

        registro.NumLcto.Should().Be("1000");
        registro.DtLcto.Should().Be(new DateOnly(2023, 5, 2));
        registro.VlLcto.Should().Be(5000m);
        registro.IndLcto.Should().Be(IndicadorTipoLancamento.Normal);
        registro.DtLctoExt.Should().Be(new DateOnly(2022, 3, 15));
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("I200".AsSpan(), out var meta);
        var registro = (RegistroI200)meta!.Fabrica();

        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);  // DtLctoExt opcional

        registro.DtLctoExt.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 147): NUM_LCTO=1000, DT_LCTO=02052023, VL_LCTO=5000,00, IND_LCTO=N, DT_LCTO_EXT=vazio
        const string sped = "|I200|1000|02052023|5000,00|N||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_LancamentoEncerramento_PreservaTextoCanonico()
    {
        // Lançamento de encerramento de contas de resultado (IND_LCTO = E)
        const string sped = "|I200|2000|31122023|12500,00|E||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_LancamentoExtemporaneo_PreservaTextoCanonico()
    {
        // Lançamento extemporâneo (IND_LCTO = X) com DT_LCTO_EXT preenchido
        const string sped = "|I200|3000|15012024|800,00|X|31122022|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCampoMoedaFuncional_ParseaCamposPadrao()
    {
        // Arquivo com IDENT_MF="S": I200 tem campo adicional VL_LCTO_MF (campo 07).
        // O parser descarta o campo adicional (não faz parte do leiaute fixo).
        const string entrada = "|I200|1000|02052023|5000,00|N||5000,00|\r\n";
        const string esperado = "|I200|1000|02052023|5000,00|N||\r\n";

        var leitor = new LeitorSpedTxt(_catalogo);
        using var stream = new MemoryStream(EncodingSped.Latin1.GetBytes(entrada));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.ReadStreamingAsync(stream, TestContext.Current.CancellationToken))
            registros.Add(registro);

        var i200 = registros.OfType<RegistroI200>().Single();
        i200.NumLcto.Should().Be("1000");
        i200.VlLcto.Should().Be(5000m);
        i200.IndLcto.Should().Be(IndicadorTipoLancamento.Normal);

        var escritor = new EscritorSpedTxt(_catalogo);
        using var saida = new MemoryStream();
        await escritor.WriteAsync(saida, registros, TestContext.Current.CancellationToken);
        EncodingSped.Latin1.GetString(saida.ToArray()).Should().Be(esperado);
    }

    [Theory]
    [InlineData("N", IndicadorTipoLancamento.Normal)]
    [InlineData("E", IndicadorTipoLancamento.Encerramento)]
    [InlineData("X", IndicadorTipoLancamento.Extemporaneo)]
    public void Definidor_TodosOsValoresIndLcto_MapeiamCorretamente(string valor, IndicadorTipoLancamento esperado)
    {
        _catalogo.TentarObter("I200".AsSpan(), out var meta);
        var registro = (RegistroI200)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, valor.AsSpan());

        registro.IndLcto.Should().Be(esperado);
    }
}
