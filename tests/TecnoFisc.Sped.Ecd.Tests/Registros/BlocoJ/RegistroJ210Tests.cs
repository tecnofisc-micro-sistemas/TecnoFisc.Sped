using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoJ;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoJ;

/// <summary>
/// Sub-stage 10.049 — exercita a forma do <see cref="RegistroJ210"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 186–189): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroJ210Tests
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
    public void Atributo_DeclaraJ210_Nivel3_BlocoJ()
    {
        var atributo = typeof(RegistroJ210).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("J210");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("J");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroJ210Com8CamposNaOrdem()
    {
        _catalogo.TentarObter("J210".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("J210");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndTip", "CodAgl", "DescrCodAgl",
            "VlCtaIni", "IndDcCtaIni",
            "VlCtaFin", "IndDcCtaFin",
            "NotasExpRef",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("J210".AsSpan(), out var meta);
        var registro = (RegistroJ210)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());                       // IndTip
        meta.Campos[1].Definidor(registro, "1.1".AsSpan());                     // CodAgl
        meta.Campos[2].Definidor(registro, "LUCROS ACUMULADOS".AsSpan());        // DescrCodAgl
        meta.Campos[3].Definidor(registro, "0,00".AsSpan());                    // VlCtaIni
        meta.Campos[4].Definidor(registro, "C".AsSpan());                       // IndDcCtaIni
        meta.Campos[5].Definidor(registro, "0,00".AsSpan());                    // VlCtaFin
        meta.Campos[6].Definidor(registro, "C".AsSpan());                       // IndDcCtaFin
        meta.Campos[7].Definidor(registro, "240".AsSpan());                     // NotasExpRef

        registro.IndTip.Should().Be(IndicadorTipoDlpaDmpl.Dlpa);
        registro.CodAgl.Should().Be("1.1");
        registro.DescrCodAgl.Should().Be("LUCROS ACUMULADOS");
        registro.VlCtaIni.Should().Be(0m);
        registro.IndDcCtaIni.Should().Be(IndicadorDebitoCredito.Credor);
        registro.VlCtaFin.Should().Be(0m);
        registro.IndDcCtaFin.Should().Be(IndicadorDebitoCredito.Credor);
        registro.NotasExpRef.Should().Be("240");
    }

    [Fact]
    public void Definidor_CampoOpcionalVazio_DevolveNulo()
    {
        _catalogo.TentarObter("J210".AsSpan(), out var meta);
        var registro = (RegistroJ210)meta!.Fabrica();

        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);   // NotasExpRef

        registro.NotasExpRef.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_DlpaLucrosAcumulados_PreservaTextoCanonico()
    {
        // Exemplo do manual p. 189 — DLPA, código 1.1, saldo zero, nota 240
        const string sped = "|J210|0|1.1|LUCROS ACUMULADOS|0,00|C|0,00|C|240|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_DmplComSaldosENotaExplicativa_PreservaTextoCanonico()
    {
        // DMPL — conta de capital social com saldo inicial e final preenchidos, com nota
        const string sped = "|J210|1|2.1|CAPITAL SOCIAL|100000,00|C|120000,00|C|241|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemNotaExplicativa_PreservaTextoCanonico()
    {
        // Sem NOTAS_EXP_REF — campo opcional ausente
        const string sped = "|J210|1|3.2|PREJUIZOS ACUMULADOS|5000,00|D|3000,00|D||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
