using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.020 — exercita a forma do <see cref="RegistroI010"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 102–104): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI010Tests
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
    public void Atributo_DeclaraI010_Nivel2_BlocoI()
    {
        var atributo = typeof(RegistroI010).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I010");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI010ComDoisCamposNaOrdem()
    {
        _catalogo.TentarObter("I010".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I010");
        meta.Campos.Select(c => c.Nome).Should().Equal(["IndEsc", "CodVerLc"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I010".AsSpan(), out var meta);
        var registro = (RegistroI010)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "G".AsSpan());
        meta.Campos[1].Definidor(registro, "9.00".AsSpan());

        registro.IndEsc.Should().Be(FormaEscrituracaoContabil.LivroDiarioGeral);
        registro.CodVerLc.Should().Be("9.00");
    }

    [Theory]
    [InlineData(FormaEscrituracaoContabil.LivroDiarioGeral, "G")]
    [InlineData(FormaEscrituracaoContabil.LivroDiarioEscrituracaoResumida, "R")]
    [InlineData(FormaEscrituracaoContabil.LivroDiarioAuxiliar, "A")]
    [InlineData(FormaEscrituracaoContabil.LivroBalancetesDiarios, "B")]
    [InlineData(FormaEscrituracaoContabil.RazaoAuxiliar, "Z")]
    public void Serializar_IndEsc_RetornaCodigo(FormaEscrituracaoContabil forma, string esperado)
    {
        _catalogo.TentarObter("I010".AsSpan(), out var meta);
        var registro = (RegistroI010)meta!.Fabrica();
        registro.IndEsc = forma;

        meta.Campos[0].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_LivroDiarioGeral_PreservaTextoCanonico()
    {
        const string sped = "|I010|G|9.00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_LivroDiarioResumido_PreservaTextoCanonico()
    {
        const string sped = "|I010|R|9.00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_RazaoAuxiliar_PreservaTextoCanonico()
    {
        const string sped = "|I010|Z|9.00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
