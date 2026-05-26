using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoC;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 10.017 — exercita a forma do <see cref="RegistroC650"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 100): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroC650Tests
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
    public void Atributo_DeclaraC650_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC650).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C650");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC650Com5CamposNaOrdem()
    {
        _catalogo.TentarObter("C650".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C650");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodAgl", "NivelAgl", "DescrCodAgl", "VlCtaFin", "IndDcCtaFin",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C650".AsSpan(), out var meta);
        var registro = (RegistroC650)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "3.01".AsSpan());
        meta.Campos[1].Definidor(registro, "2".AsSpan());
        meta.Campos[2].Definidor(registro, "Receita Bruta de Vendas".AsSpan());
        meta.Campos[3].Definidor(registro, "150000,00".AsSpan());
        meta.Campos[4].Definidor(registro, "C".AsSpan());

        registro.CodAgl.Should().Be("3.01");
        registro.NivelAgl.Should().Be(2);
        registro.DescrCodAgl.Should().Be("Receita Bruta de Vendas");
        registro.VlCtaFin.Should().Be(150000.00m);
        registro.IndDcCtaFin.Should().Be(IndicadorDebitoCredito.Credor);
    }

    [Theory]
    [InlineData(IndicadorDebitoCredito.Devedor, "D")]
    [InlineData(IndicadorDebitoCredito.Credor, "C")]
    public void Serializar_IndDcCtaFin_RetornaCodigo(IndicadorDebitoCredito indicador, string esperado)
    {
        _catalogo.TentarObter("C650".AsSpan(), out var meta);
        var registro = (RegistroC650)meta!.Fabrica();
        registro.IndDcCtaFin = indicador;

        meta.Campos[4].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C650|3.01|2|Receita Bruta de Vendas|150000,00|C|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SaldoDevedor_PreservaTextoCanonico()
    {
        const string sped =
            "|C650|4.01|3|Custo das Mercadorias Vendidas|80000,00|D|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ValorZero_PreservaTextoCanonico()
    {
        const string sped =
            "|C650|5.01|1|Lucro Liquido do Exercicio|0,00|C|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
