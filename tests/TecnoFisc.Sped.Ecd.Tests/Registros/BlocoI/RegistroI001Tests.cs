using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.019 — exercita a forma do <see cref="RegistroI001"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 102): metadados de catálogo, mapeamento de campo e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI001Tests
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
    public void Atributo_DeclaraI001_Nivel1_BlocoI()
    {
        var atributo = typeof(RegistroI001).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I001");
        atributo.Nivel.Should().Be(1);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI001ComUmCampoNaOrdem()
    {
        _catalogo.TentarObter("I001".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I001");
        meta.Campos.Select(c => c.Nome).Should().Equal(["IndDad"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
    }

    [Fact]
    public void Definidor_AtribuiIndDad()
    {
        _catalogo.TentarObter("I001".AsSpan(), out var meta);
        var registro = (RegistroI001)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());

        registro.IndDad.Should().Be(IndicadorMovimentoBloco.ComDados);
    }

    [Theory]
    [InlineData(IndicadorMovimentoBloco.ComDados, "0")]
    [InlineData(IndicadorMovimentoBloco.SemDados, "1")]
    public void Serializar_IndDad_RetornaCodigo(IndicadorMovimentoBloco indicador, string esperado)
    {
        _catalogo.TentarObter("I001".AsSpan(), out var meta);
        var registro = (RegistroI001)meta!.Fabrica();
        registro.IndDad = indicador;

        meta.Campos[0].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComDados_PreservaTextoCanonico()
    {
        const string sped = "|I001|0|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemDados_PreservaTextoCanonico()
    {
        const string sped = "|I001|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
