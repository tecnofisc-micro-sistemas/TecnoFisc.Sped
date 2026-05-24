using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoH;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoH;

/// <summary>
/// Sub-stage 8.191 — exercita a forma do <see cref="RegistroH030"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 248): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroH030Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroH030).Assembly);

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
    public void Atributo_DeclaraH030_Nivel4_BlocoH()
    {
        var atributo = typeof(RegistroH030).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("H030");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("H");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroH030ComQuatroCamposNaOrdem()
    {
        _catalogo.TentarObter("H030".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("H030");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "VlIcmsOp",
            "VlBcIcmsSt",
            "VlIcmsSt",
            "VlFcp",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("H030".AsSpan(), out var meta);
        var registro = (RegistroH030)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1,123456".AsSpan());
        meta.Campos[1].Definidor(registro, "12,234567".AsSpan());
        meta.Campos[2].Definidor(registro, "2,345678".AsSpan());
        meta.Campos[3].Definidor(registro, "0,456789".AsSpan());

        registro.VlIcmsOp.Should().Be(1.123456m);
        registro.VlBcIcmsSt.Should().Be(12.234567m);
        registro.VlIcmsSt.Should().Be(2.345678m);
        registro.VlFcp.Should().Be(0.456789m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("H030".AsSpan(), out var meta);
        var registro = (RegistroH030)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.VlIcmsOp.Should().BeNull();
        registro.VlBcIcmsSt.Should().BeNull();
        registro.VlIcmsSt.Should().BeNull();
        registro.VlFcp.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|H030|1,123456|12,234567|2,345678|0,456789|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComValoresZerados_PreservaTextoCanonico()
    {
        const string sped = "|H030|0,000000|0,000000|0,000000|0,000000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
