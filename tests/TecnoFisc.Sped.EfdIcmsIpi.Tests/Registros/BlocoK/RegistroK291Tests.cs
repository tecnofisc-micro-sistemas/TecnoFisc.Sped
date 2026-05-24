using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 8.209 — exercita a forma do <see cref="RegistroK291"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 265): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroK291Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroK291).Assembly);

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
    public void Atributo_DeclaraK291_Nivel4_BlocoK()
    {
        var atributo = typeof(RegistroK291).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K291");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK291ComDoisCamposNaOrdem()
    {
        _catalogo.TentarObter("K291".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K291");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodItem",
            "Qtd",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K291".AsSpan(), out var meta);
        var registro = (RegistroK291)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "PROD-CONJ-001".AsSpan());
        meta.Campos[1].Definidor(registro, "15,250000".AsSpan());

        registro.CodItem.Should().Be("PROD-CONJ-001");
        registro.Qtd.Should().Be(15.250000m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("K291".AsSpan(), out var meta);
        var registro = (RegistroK291)meta!.Fabrica();

        meta!.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodItem.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|K291|PROD-CONJ-001|15,250000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComProdutoEmProcesso_PreservaTextoCanonico()
    {
        const string sped = "|K291|PROD-PROCESSO-002|1,000001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
