using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.022 — exercita a forma do <see cref="RegistroI015"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 108–109): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI015Tests
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
    public void Atributo_DeclaraI015_Nivel4_BlocoI()
    {
        var atributo = typeof(RegistroI015).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I015");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI015ComUmCampoNaOrdem()
    {
        _catalogo.TentarObter("I015".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I015");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodCtaRes"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I015".AsSpan(), out var meta);
        var registro = (RegistroI015)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "2328.1.0001".AsSpan());

        registro.CodCtaRes.Should().Be("2328.1.0001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("I015".AsSpan(), out var meta);
        var registro = (RegistroI015)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodCtaRes.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|I015|2328.1.0001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CodigoContaAlternativo_PreservaTextoCanonico()
    {
        const string sped = "|I015|1.01.02.03.0001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
