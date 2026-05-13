using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoE;

/// <summary>
/// Sub-stage 8.160 — exercita a forma do <see cref="RegistroE115"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 212): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroE115Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroE115).Assembly);

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerStreamingAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }

    [Fact]
    public void Atributo_DeclaraE115_Nivel4_BlocoE()
    {
        var atributo = typeof(RegistroE115).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("E115");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("E");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroE115Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("E115".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("E115");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodInfAdic", "VlInfAdic", "DescrComplAj",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 3));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("E115".AsSpan(), out var meta);
        var registro = (RegistroE115)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "ES000001".AsSpan());      // CodInfAdic
        meta.Campos[1].Definidor(registro, "1250,50".AsSpan());        // VlInfAdic
        meta.Campos[2].Definidor(registro, "Ajuste declaratório".AsSpan()); // DescrComplAj

        registro.CodInfAdic.Should().Be("ES000001");
        registro.VlInfAdic.Should().Be(1250.50m);
        registro.DescrComplAj.Should().Be("Ajuste declaratório");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("E115".AsSpan(), out var meta);
        var registro = (RegistroE115)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);  // CodInfAdic
        meta.Campos[2].Definidor(registro, Span<char>.Empty);  // DescrComplAj

        registro.CodInfAdic.Should().BeNull();
        registro.DescrComplAj.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|E115|ES000001|1250,50|Ajuste declaratório|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemDescricaoComplementar_PreservaTextoCanonico()
    {
        const string sped = "|E115|MG000099|500,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
