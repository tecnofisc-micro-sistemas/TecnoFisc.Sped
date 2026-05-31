using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.039 — exercita a forma do <see cref="RegistroI355"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 158–159): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI355Tests
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
    public void Atributo_DeclaraI355_Nivel4_BlocoI()
    {
        var atributo = typeof(RegistroI355).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I355");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI355Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("I355".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I355");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodCta", "CodCcus", "VlCta", "IndDc"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I355".AsSpan(), out var meta);
        var registro = (RegistroI355)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "3.4.1.01".AsSpan());    // CodCta
        meta.Campos[1].Definidor(registro, "CC001".AsSpan());       // CodCcus
        meta.Campos[2].Definidor(registro, "15000,00".AsSpan());    // VlCta
        meta.Campos[3].Definidor(registro, "D".AsSpan());           // IndDc

        registro.CodCta.Should().Be("3.4.1.01");
        registro.CodCcus.Should().Be("CC001");
        registro.VlCta.Should().Be(15000m);
        registro.IndDc.Should().Be(IndicadorDebitoCredito.Devedor);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("I355".AsSpan(), out var meta);
        var registro = (RegistroI355)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCcus opcional

        registro.CodCcus.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Conta de resultado com centro de custos, saldo devedor
        const string sped = "|I355|3.4.1.01|CC001|15000,00|D|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCentroCustos_PreservaTextoCanonico()
    {
        // Conta de resultado sem centro de custos, saldo credor
        const string sped = "|I355|4.1.1.01||8500,00|C|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposMoedaFuncional_ParseaCamposPadrao()
    {
        // Arquivo com IDENT_MF="S": I355 recebe dois campos adicionais (VL_CTA_MF, IND_DC_MF) via I020.
        // O parser descarta os campos adicionais (não fazem parte do leiaute fixo).
        // O round-trip produz apenas os 5 campos padrão.
        const string entrada = "|I355|3.4.1.01||15000,00|D|15000,00|D|\r\n";
        const string esperado = "|I355|3.4.1.01||15000,00|D|\r\n";

        var leitor = new LeitorSpedTxt(_catalogo);
        using var stream = new MemoryStream(EncodingSped.Latin1.GetBytes(entrada));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.ReadStreamingAsync(stream, TestContext.Current.CancellationToken))
            registros.Add(registro);

        var i355 = registros.OfType<RegistroI355>().Single();
        i355.CodCta.Should().Be("3.4.1.01");
        i355.CodCcus.Should().BeNull();
        i355.VlCta.Should().Be(15000m);
        i355.IndDc.Should().Be(IndicadorDebitoCredito.Devedor);

        var escritor = new EscritorSpedTxt(_catalogo);
        using var saida = new MemoryStream();
        await escritor.WriteAsync(saida, registros, TestContext.Current.CancellationToken);
        EncodingSped.Latin1.GetString(saida.ToArray()).Should().Be(esperado);
    }
}
