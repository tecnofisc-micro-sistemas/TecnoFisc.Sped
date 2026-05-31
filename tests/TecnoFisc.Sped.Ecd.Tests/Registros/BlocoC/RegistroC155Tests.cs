using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoC;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 10.015 — exercita a forma do <see cref="RegistroC155"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 97): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroC155Tests
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
    public void Atributo_DeclaraC155_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC155).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C155");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC155Com8CamposNaOrdem()
    {
        _catalogo.TentarObter("C155".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C155");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodCtaRec", "CodCcusRec", "VlSldIniRec", "IndDcIniRec",
            "VlDebRec", "VlCredRec", "VlSldFinRec", "IndDcFinRec",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C155".AsSpan(), out var meta);
        var registro = (RegistroC155)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1.01.001".AsSpan());
        meta.Campos[1].Definidor(registro, "CC001".AsSpan());
        meta.Campos[2].Definidor(registro, "1000,00".AsSpan());
        meta.Campos[3].Definidor(registro, "D".AsSpan());
        meta.Campos[4].Definidor(registro, "500,00".AsSpan());
        meta.Campos[5].Definidor(registro, "300,00".AsSpan());
        meta.Campos[6].Definidor(registro, "1200,00".AsSpan());
        meta.Campos[7].Definidor(registro, "D".AsSpan());

        registro.CodCtaRec.Should().Be("1.01.001");
        registro.CodCcusRec.Should().Be("CC001");
        registro.VlSldIniRec.Should().Be(1000.00m);
        registro.IndDcIniRec.Should().Be(IndicadorDebitoCredito.Devedor);
        registro.VlDebRec.Should().Be(500.00m);
        registro.VlCredRec.Should().Be(300.00m);
        registro.VlSldFinRec.Should().Be(1200.00m);
        registro.IndDcFinRec.Should().Be(IndicadorDebitoCredito.Devedor);
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolvemNulo()
    {
        _catalogo.TentarObter("C155".AsSpan(), out var meta);
        var registro = (RegistroC155)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCcusRec
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // IndDcIniRec
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty); // IndDcFinRec

        registro.CodCcusRec.Should().BeNull();
        registro.IndDcIniRec.Should().BeNull();
        registro.IndDcFinRec.Should().BeNull();
    }

    [Theory]
    [InlineData(IndicadorDebitoCredito.Devedor, "D")]
    [InlineData(IndicadorDebitoCredito.Credor, "C")]
    public void Serializar_IndDcIniRec_RetornaCodigo(IndicadorDebitoCredito indicador, string esperado)
    {
        _catalogo.TentarObter("C155".AsSpan(), out var meta);
        var registro = (RegistroC155)meta!.Fabrica();
        registro.IndDcIniRec = indicador;

        meta.Campos[3].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C155|1.01.001|CC001|1000,00|D|500,00|300,00|1200,00|D|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCentrosDeCustosEIndicadores_PreservaTextoCanonico()
    {
        const string sped =
            "|C155|1.01.001||1000,00||500,00|300,00|1200,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SaldoCredor_PreservaTextoCanonico()
    {
        const string sped =
            "|C155|2.01.001||0,00||0,00|5000,00|5000,00|C|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
