using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 8.212 — exercita a forma do <see cref="RegistroK301"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 266): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroK301Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroK301).Assembly);

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
    public void Atributo_DeclaraK301_Nivel4_BlocoK()
    {
        var atributo = typeof(RegistroK301).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K301");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK301ComDoisCamposNaOrdem()
    {
        _catalogo.TentarObter("K301".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K301");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodItem",
            "Qtd",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K301".AsSpan(), out var meta);
        var registro = (RegistroK301)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "PROD-TERC-001".AsSpan());
        meta.Campos[1].Definidor(registro, "12,345678".AsSpan());

        registro.CodItem.Should().Be("PROD-TERC-001");
        registro.Qtd.Should().Be(12.345678m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("K301".AsSpan(), out var meta);
        var registro = (RegistroK301)meta!.Fabrica();

        meta!.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodItem.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|K301|PROD-TERC-001|12,345678|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComProdutoAcabado_PreservaTextoCanonico()
    {
        const string sped = "|K301|PROD-ACABADO-002|1,000001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
