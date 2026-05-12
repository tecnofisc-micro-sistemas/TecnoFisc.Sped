using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.084 — exercita a forma do <see cref="RegistroC425"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 121): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC425Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC425).Assembly);

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
    public void Atributo_DeclaraC425_Nivel5_BlocoC()
    {
        var atributo = typeof(RegistroC425).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C425");
        atributo.Nivel.Should().Be(5);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC425Com6CamposNaOrdem()
    {
        _catalogo.TentarObter("C425".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C425");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodItem", "Qtd", "Unid", "VlItem", "VlPis", "VlCofins"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C425".AsSpan(), out var meta);
        var registro = (RegistroC425)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "PROD001".AsSpan());  // CodItem
        meta.Campos[1].Definidor(registro, "10,500".AsSpan());   // Qtd
        meta.Campos[2].Definidor(registro, "UN".AsSpan());       // Unid
        meta.Campos[3].Definidor(registro, "150,00".AsSpan());   // VlItem
        meta.Campos[4].Definidor(registro, "2,50".AsSpan());     // VlPis
        meta.Campos[5].Definidor(registro, "11,50".AsSpan());    // VlCofins

        registro.CodItem.Should().Be("PROD001");
        registro.Qtd.Should().Be(10.500m);
        registro.Unid.Should().Be("UN");
        registro.VlItem.Should().Be(150.00m);
        registro.VlPis.Should().Be(2.50m);
        registro.VlCofins.Should().Be(11.50m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C425".AsSpan(), out var meta);
        var registro = (RegistroC425)meta!.Fabrica();

        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // VlPis
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCofins

        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C425|PROD001|10,500|UN|150,00|2,50|11,50|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemPisECofins_PreservaTextoCanonico()
    {
        const string sped = "|C425|PROD001|10,500|UN|150,00|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
