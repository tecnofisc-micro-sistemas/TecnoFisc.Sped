using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoK;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 10.066 — exercita a forma do <see cref="RegistroK310"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 225–226): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroK310Tests
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
    public void Atributo_DeclaraK310_Nivel4_BlocoK()
    {
        var atributo = typeof(RegistroK310).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K310");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK310Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("K310".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K310");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["EmpCodParte", "Valor", "IndValor"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K310".AsSpan(), out var meta);
        var registro = (RegistroK310)meta!.Fabrica();

        // Exemplo do manual (p. 226): |K310|1234|100,00|D|
        meta.Campos[0].Definidor(registro, "1234".AsSpan());
        meta.Campos[1].Definidor(registro, "100,00".AsSpan());
        meta.Campos[2].Definidor(registro, "D".AsSpan());

        registro.EmpCodParte.Should().Be(1234);
        registro.Valor.Should().Be(100.00m);
        registro.IndValor.Should().Be(IndicadorDebitoCredito.Devedor);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 226): empresa 1234, parcela eliminada R$ 100,00 Devedor
        const string sped = "|K310|1234|100,00|D|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComIndicadorCredor_PreservaTextoCanonico()
    {
        // Empresa com parcela eliminada credora
        const string sped = "|K310|5678|250,50|C|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
