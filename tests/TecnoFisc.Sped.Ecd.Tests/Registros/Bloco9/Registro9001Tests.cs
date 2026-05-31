using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.Bloco9;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.Bloco9;

/// <summary>
/// Sub-stage 10.069 — exercita a forma do <see cref="Registro9001"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 230): metadados de catálogo, mapeamento de campo e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class Registro9001Tests
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
    public void Atributo_Declara9001_Nivel1_Bloco9()
    {
        var atributo = typeof(Registro9001).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("9001");
        atributo.Nivel.Should().Be(1);
        atributo.Bloco.Should().Be("9");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro9001ComUmCampoNaOrdem()
    {
        _catalogo.TentarObter("9001".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("9001");
        meta.Campos.Select(c => c.Nome).Should().Equal(["IndDad"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
    }

    [Fact]
    public void Definidor_AtribuiIndDad()
    {
        _catalogo.TentarObter("9001".AsSpan(), out var meta);
        var registro = (Registro9001)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());

        registro.IndDad.Should().Be(IndicadorMovimentoBloco.ComDados);
    }

    [Theory]
    [InlineData(IndicadorMovimentoBloco.ComDados, "0")]
    [InlineData(IndicadorMovimentoBloco.SemDados, "1")]
    public void Serializar_IndDad_RetornaCodigo(IndicadorMovimentoBloco indicador, string esperado)
    {
        _catalogo.TentarObter("9001".AsSpan(), out var meta);
        var registro = (Registro9001)meta!.Fabrica();
        registro.IndDad = indicador;

        meta.Campos[0].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComDados_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 230): bloco 9 com dados
        const string sped = "|9001|0|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemDados_PreservaTextoCanonico()
    {
        const string sped = "|9001|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
