using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.033 — exercita a forma do <see cref="RegistroI157"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 140–142): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI157Tests
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
    public void Atributo_DeclaraI157_Nivel5_BlocoI()
    {
        var atributo = typeof(RegistroI157).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I157");
        atributo.Nivel.Should().Be(5);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI157Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("I157".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I157");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodCta", "CodCcus", "VlSldIni", "IndDcIni",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I157".AsSpan(), out var meta);
        var registro = (RegistroI157)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "2328.1.0001".AsSpan());  // CodCta
        meta.Campos[1].Definidor(registro, "CC001".AsSpan());        // CodCcus
        meta.Campos[2].Definidor(registro, "1000,00".AsSpan());      // VlSldIni
        meta.Campos[3].Definidor(registro, "D".AsSpan());            // IndDcIni

        registro.CodCta.Should().Be("2328.1.0001");
        registro.CodCcus.Should().Be("CC001");
        registro.VlSldIni.Should().Be(1000m);
        registro.IndDcIni.Should().Be(IndicadorDebitoCredito.Devedor);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("I157".AsSpan(), out var meta);
        var registro = (RegistroI157)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCcus opcional
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);  // IndDcIni opcional

        registro.CodCcus.Should().BeNull();
        registro.IndDcIni.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 142): conta 2328.1.0001, sem centro de custos, saldo 1000 (D)
        const string sped = "|I157|2328.1.0001||1000,00|D|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCentroCustos_PreservaTextoCanonico()
    {
        // Conta com centro de custos e saldo credor
        const string sped = "|I157|1.1.1.01|CC001|500,00|C|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposMoedaFuncional_ParseaCamposPadrao()
    {
        // Arquivo com IDENT_MF="S": I157 tem 2 campos adicionais (VL_SLD_INI_MF, IND_DC_INI_MF)
        // após o campo 05. O parser descarta os campos adicionais (não fazem parte do leiaute fixo).
        const string entrada = "|I157|2328.1.0001||1000,00|D|1000,00|D|\r\n";
        const string esperado = "|I157|2328.1.0001||1000,00|D|\r\n";

        var leitor = new LeitorSpedTxt(_catalogo);
        using var stream = new MemoryStream(EncodingSped.Latin1.GetBytes(entrada));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.ReadStreamingAsync(stream, TestContext.Current.CancellationToken))
            registros.Add(registro);

        var i157 = registros.OfType<RegistroI157>().Single();
        i157.CodCta.Should().Be("2328.1.0001");
        i157.VlSldIni.Should().Be(1000m);
        i157.IndDcIni.Should().Be(IndicadorDebitoCredito.Devedor);

        var escritor = new EscritorSpedTxt(_catalogo);
        using var saida = new MemoryStream();
        await escritor.WriteAsync(saida, registros, TestContext.Current.CancellationToken);
        EncodingSped.Latin1.GetString(saida.ToArray()).Should().Be(esperado);
    }
}
