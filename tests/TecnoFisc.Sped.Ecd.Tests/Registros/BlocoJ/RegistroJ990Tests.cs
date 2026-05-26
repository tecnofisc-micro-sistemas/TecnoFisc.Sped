using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoJ;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoJ;

/// <summary>
/// Sub-stage 10.057 — exercita a forma do <see cref="RegistroJ990"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 208): metadados de catálogo, mapeamento de campo e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroJ990Tests
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
    public void Atributo_DeclaraJ990_Nivel1_BlocoJ()
    {
        var atributo = typeof(RegistroJ990).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("J990");
        atributo.Nivel.Should().Be(1);
        atributo.Bloco.Should().Be("J");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroJ990ComUmCampoNaOrdem()
    {
        _catalogo.TentarObter("J990".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("J990");
        meta.Campos.Select(c => c.Nome).Should().Equal(["QtdLinJ"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
    }

    [Fact]
    public void Definidor_AtribuiQtdLinJ()
    {
        _catalogo.TentarObter("J990".AsSpan(), out var meta);
        var registro = (RegistroJ990)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "100".AsSpan());

        registro.QtdLinJ.Should().Be(100);
    }

    [Fact]
    public async Task RoundTrip_ComCemLinhas_PreservaTextoCanonico()
    {
        const string sped = "|J990|100|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComUmaLinha_PreservaTextoCanonico()
    {
        const string sped = "|J990|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
