using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.035 — exercita a forma do <see cref="RegistroI250"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 148–152): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI250Tests
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
    public void Atributo_DeclaraI250_Nivel4_BlocoI()
    {
        var atributo = typeof(RegistroI250).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I250");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI250Com8CamposNaOrdem()
    {
        _catalogo.TentarObter("I250".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I250");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodCta", "CodCcus", "VlDc", "IndDc", "NumArq", "CodHistPad", "Hist", "CodPart",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I250".AsSpan(), out var meta);
        var registro = (RegistroI250)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1.1".AsSpan());            // CodCta
        meta.Campos[1].Definidor(registro, "CC001".AsSpan());          // CodCcus
        meta.Campos[2].Definidor(registro, "5000,00".AsSpan());        // VlDc
        meta.Campos[3].Definidor(registro, "D".AsSpan());              // IndDc
        meta.Campos[4].Definidor(registro, "123".AsSpan());            // NumArq
        meta.Campos[5].Definidor(registro, "001".AsSpan());            // CodHistPad
        meta.Campos[6].Definidor(registro, "RECEBIMENTO DE CLIENTES".AsSpan());  // Hist
        meta.Campos[7].Definidor(registro, "PART001".AsSpan());        // CodPart

        registro.CodCta.Should().Be("1.1");
        registro.CodCcus.Should().Be("CC001");
        registro.VlDc.Should().Be(5000m);
        registro.IndDc.Should().Be(IndicadorNaturezaPartida.Debito);
        registro.NumArq.Should().Be("123");
        registro.CodHistPad.Should().Be("001");
        registro.Hist.Should().Be("RECEBIMENTO DE CLIENTES");
        registro.CodPart.Should().Be("PART001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("I250".AsSpan(), out var meta);
        var registro = (RegistroI250)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCcus opcional

        registro.CodCcus.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Baseado no exemplo do manual (p. 151): débito de 5000,00 com histórico
        const string sped = "|I250|1.1||5000,00|D|123||RECEBIMENTO DE CLIENTES DUPLICATA 100.2011||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // Partida crédito mínima: apenas CodCta, VlDc, IndDc preenchidos (f6-f9 vazios = 5 pipes)
        const string sped = "|I250|1.5||5000,00|C|||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComHistoricoPadronizado_PreservaTextoCanonico()
    {
        // Partida com CodHistPad preenchido e Hist vazio
        const string sped = "|I250|2.1||1200,00|D||H001||PART002|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposMoedaFuncional_ParseaCamposPadrao()
    {
        // Arquivo com IDENT_MF="S": I250 recebe campos adicionais VL_DC_MF (campo 10) e
        // IND_DC_MF (campo 11) via I020. O parser descarta os campos adicionais.
        const string entrada = "|I250|1.1||5000,00|D|123||RECEBIMENTO||5000,00|D|\r\n";
        const string esperado = "|I250|1.1||5000,00|D|123||RECEBIMENTO||\r\n";

        var leitor = new LeitorSpedTxt(_catalogo);
        using var stream = new MemoryStream(EncodingSped.Latin1.GetBytes(entrada));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.ReadStreamingAsync(stream, TestContext.Current.CancellationToken))
            registros.Add(registro);

        var i250 = registros.OfType<RegistroI250>().Single();
        i250.CodCta.Should().Be("1.1");
        i250.VlDc.Should().Be(5000m);
        i250.IndDc.Should().Be(IndicadorNaturezaPartida.Debito);

        var escritor = new EscritorSpedTxt(_catalogo);
        using var saida = new MemoryStream();
        await escritor.WriteAsync(saida, registros, TestContext.Current.CancellationToken);
        EncodingSped.Latin1.GetString(saida.ToArray()).Should().Be(esperado);
    }

    [Theory]
    [InlineData("D", IndicadorNaturezaPartida.Debito)]
    [InlineData("C", IndicadorNaturezaPartida.Credito)]
    public void Definidor_TodosOsValoresIndDc_MapeiamCorretamente(string valor, IndicadorNaturezaPartida esperado)
    {
        _catalogo.TentarObter("I250".AsSpan(), out var meta);
        var registro = (RegistroI250)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, valor.AsSpan());

        registro.IndDc.Should().Be(esperado);
    }
}
