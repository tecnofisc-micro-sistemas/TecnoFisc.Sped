using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoE;

/// <summary>
/// Sub-stage 8.164 — exercita a forma do <see cref="RegistroE220"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 217): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroE220Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroE220).Assembly);

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
    public void Atributo_DeclaraE220_Nivel4_BlocoE()
    {
        var atributo = typeof(RegistroE220).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("E220");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("E");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroE220Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("E220".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("E220");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodAjApur", "DescrComplAj", "VlAjApur"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("E220".AsSpan(), out var meta);
        var registro = (RegistroE220)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "RS10000 ".AsSpan());          // CodAjApur
        meta.Campos[1].Definidor(registro, "Estorno crédito ST".AsSpan()); // DescrComplAj
        meta.Campos[2].Definidor(registro, "1500,00".AsSpan());            // VlAjApur

        registro.CodAjApur.Should().Be("RS10000 ");
        registro.DescrComplAj.Should().Be("Estorno crédito ST");
        registro.VlAjApur.Should().Be(1500.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("E220".AsSpan(), out var meta);
        var registro = (RegistroE220)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.CodAjApur.Should().BeNull();
        registro.DescrComplAj.Should().BeNull();
        registro.VlAjApur.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|E220|RS10000 |Estorno crédito ST|1500,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemDescricaoComplementar_PreservaTextoCanonico()
    {
        // Ajuste sem descrição complementar: campo 03 vazio.
        const string sped = "|E220|SP20010 ||250,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
