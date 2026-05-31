using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoK;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 10.068 — exercita a forma do <see cref="RegistroK990"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 229): metadados de catálogo, mapeamento de campo e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroK990Tests
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
    public void Atributo_DeclaraK990_Nivel1_BlocoK()
    {
        var atributo = typeof(RegistroK990).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K990");
        atributo.Nivel.Should().Be(1);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK990ComUmCampoNaOrdem()
    {
        _catalogo.TentarObter("K990".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K990");
        meta.Campos.Select(c => c.Nome).Should().Equal(["QtdLinK"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
    }

    [Fact]
    public void Definidor_AtribuiQtdLinK()
    {
        _catalogo.TentarObter("K990".AsSpan(), out var meta);
        var registro = (RegistroK990)meta!.Fabrica();

        // Exemplo do manual (p. 229): |K990|1000|
        meta.Campos[0].Definidor(registro, "1000".AsSpan());

        registro.QtdLinK.Should().Be(1000);
    }

    [Fact]
    public async Task RoundTrip_ComMilLinhas_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 229): bloco K com 1000 linhas
        const string sped = "|K990|1000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComUmaLinha_PreservaTextoCanonico()
    {
        const string sped = "|K990|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
