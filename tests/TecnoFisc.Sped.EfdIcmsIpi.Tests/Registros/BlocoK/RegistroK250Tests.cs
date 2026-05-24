using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 8.201 — exercita a forma do <see cref="RegistroK250"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 256-257): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroK250Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroK250).Assembly);

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
    public void Atributo_DeclaraK250_Nivel3_BlocoK()
    {
        var atributo = typeof(RegistroK250).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K250");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK250ComTresCamposNaOrdem()
    {
        _catalogo.TentarObter("K250".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K250");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtProd",
            "CodItem",
            "Qtd",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K250".AsSpan(), out var meta);
        var registro = (RegistroK250)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "20012025".AsSpan());
        meta.Campos[1].Definidor(registro, "ITEM-TERCEIRO".AsSpan());
        meta.Campos[2].Definidor(registro, "42,123456".AsSpan());

        registro.DtProd.Should().Be(new DateOnly(2025, 1, 20));
        registro.CodItem.Should().Be("ITEM-TERCEIRO");
        registro.Qtd.Should().Be(42.123456m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("K250".AsSpan(), out var meta);
        var registro = (RegistroK250)meta!.Fabrica();

        meta!.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodItem.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|K250|20012025|ITEM-TERCEIRO|42,123456|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComQuantidadeInteira_PreservaTextoCanonico()
    {
        const string sped = "|K250|21012025|ITEM-ACABADO|10,000000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
