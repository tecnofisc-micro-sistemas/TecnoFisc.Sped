using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoB;

/// <summary>
/// Sub-stage 8.023 — exercita a forma do <see cref="RegistroB001"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 44): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroB001Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroB001).Assembly);

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
    public void Atributo_DeclaraB001_Nivel1_BlocoB()
    {
        var atributo = typeof(RegistroB001).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("B001");
        atributo.Nivel.Should().Be(1);
        atributo.Bloco.Should().Be("B");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroB001ComUmCampoNaOrdem()
    {
        _catalogo.TentarObter("B001".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("B001");
        meta.Campos.Select(c => c.Nome).Should().Equal(["IndDad"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("B001".AsSpan(), out var meta);
        var registro = (RegistroB001)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());

        registro.IndDad.Should().Be(IndicadorMovimentoBloco.ComDados);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("B001".AsSpan(), out var meta);
        var registro = (RegistroB001)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);

        registro.IndDad.Should().Be(default(IndicadorMovimentoBloco));
    }

    [Theory]
    [InlineData(IndicadorMovimentoBloco.ComDados, "0")]
    [InlineData(IndicadorMovimentoBloco.SemDados, "1")]
    public void Serializar_IndDad_RetornaCodigo(IndicadorMovimentoBloco movimento, string esperado)
    {
        _catalogo.TentarObter("B001".AsSpan(), out var meta);
        var registro = (RegistroB001)meta!.Fabrica();
        registro.IndDad = movimento;

        meta.Campos[0].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|B001|0|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_BlocoSemDados_PreservaTextoCanonico()
    {
        const string sped = "|B001|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
