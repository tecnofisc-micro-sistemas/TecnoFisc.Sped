using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoJ;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoJ;

/// <summary>
/// Sub-stage 10.050 — exercita a forma do <see cref="RegistroJ215"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 189–190): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroJ215Tests
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
    public void Atributo_DeclaraJ215_Nivel4_BlocoJ()
    {
        var atributo = typeof(RegistroJ215).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("J215");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("J");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroJ215Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("J215".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("J215");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodHistFat", "DescFat", "VlFatCont", "IndDcFat",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("J215".AsSpan(), out var meta);
        var registro = (RegistroJ215)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "10".AsSpan());                               // CodHistFat
        meta.Campos[1].Definidor(registro, "DISTRIBUIÇÃO DO LUCRO DO PERÍODO".AsSpan()); // DescFat
        meta.Campos[2].Definidor(registro, "1000,00".AsSpan());                          // VlFatCont
        meta.Campos[3].Definidor(registro, "D".AsSpan());                                // IndDcFat

        registro.CodHistFat.Should().Be("10");
        registro.DescFat.Should().Be("DISTRIBUIÇÃO DO LUCRO DO PERÍODO");
        registro.VlFatCont.Should().Be(1000m);
        registro.IndDcFat.Should().Be(IndicadorSituacaoFatoContabil.Devedor);
    }

    [Fact]
    public async Task RoundTrip_ExemploManual_PreservaTextoCanonico()
    {
        // Exemplo do manual p. 190 — distribuição de lucro, valor 1000,00, Devedor
        const string sped = "|J215|10|DISTRIBUIÇÃO DO LUCRO DO PERÍODO|1000,00|D|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SaldoCredorPositivo_PreservaTextoCanonico()
    {
        // Subtotal positivo — IND_DC_FAT = P (Subtotal ou total positivo)
        const string sped = "|J215|99|SALDO TOTAL DO EXERCÍCIO|500000,00|P|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SaldoNegativo_PreservaTextoCanonico()
    {
        // Subtotal negativo — IND_DC_FAT = N (Subtotal ou total negativo)
        const string sped = "|J215|98|PREJUÍZO DO EXERCÍCIO|250000,50|N|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
