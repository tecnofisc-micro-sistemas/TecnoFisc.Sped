using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoK;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 10.065 — exercita a forma do <see cref="RegistroK300"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 223–225): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroK300Tests
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
    public void Atributo_DeclaraK300_Nivel3_BlocoK()
    {
        var atributo = typeof(RegistroK300).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K300");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK300Com7CamposNaOrdem()
    {
        _catalogo.TentarObter("K300".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K300");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["CodCta", "ValAg", "IndValAg", "ValEl", "IndValEl", "ValCs", "IndValCs"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K300".AsSpan(), out var meta);
        var registro = (RegistroK300)meta!.Fabrica();

        // Exemplo do manual (p. 225): |K300|1.01.01.01.01|1000,00|D|300,00|D|700,00|D|
        meta.Campos[0].Definidor(registro, "1.01.01.01.01".AsSpan());
        meta.Campos[1].Definidor(registro, "1000,00".AsSpan());
        meta.Campos[2].Definidor(registro, "D".AsSpan());
        meta.Campos[3].Definidor(registro, "300,00".AsSpan());
        meta.Campos[4].Definidor(registro, "D".AsSpan());
        meta.Campos[5].Definidor(registro, "700,00".AsSpan());
        meta.Campos[6].Definidor(registro, "D".AsSpan());

        registro.CodCta.Should().Be("1.01.01.01.01");
        registro.ValAg.Should().Be(1000.00m);
        registro.IndValAg.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.ValEl.Should().Be(300.00m);
        registro.IndValEl.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.ValCs.Should().Be(700.00m);
        registro.IndValCs.Should().Be(IndicadorDebitoCredito.Devedor);
    }

    [Fact]
    public void Definidor_CodCtaVazio_DevolveNulo()
    {
        _catalogo.TentarObter("K300".AsSpan(), out var meta);
        var registro = (RegistroK300)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 225): conta 1.01.01.01.01 aglutinado 1000, eliminado 300, consolidado 700 (todos Devedor)
        const string sped = "|K300|1.01.01.01.01|1000,00|D|300,00|D|700,00|D|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComValoresCredor_PreservaTextoCanonico()
    {
        // Conta de passivo — saldos credores; eliminação zero
        const string sped = "|K300|2.01.01|500,00|C|0,00|C|500,00|C|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
