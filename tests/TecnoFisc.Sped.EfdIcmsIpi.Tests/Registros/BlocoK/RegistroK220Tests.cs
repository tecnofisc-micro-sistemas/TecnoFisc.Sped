using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 8.198 — exercita a forma do <see cref="RegistroK220"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 253-254): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroK220Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroK220).Assembly);

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
    public void Atributo_DeclaraK220_Nivel3_BlocoK()
    {
        var atributo = typeof(RegistroK220).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K220");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK220ComCincoCamposNaOrdem()
    {
        _catalogo.TentarObter("K220".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K220");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtMov",
            "CodItemOri",
            "CodItemDest",
            "QtdOri",
            "QtdDest",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K220".AsSpan(), out var meta);
        var registro = (RegistroK220)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "15012025".AsSpan());
        meta.Campos[1].Definidor(registro, "ITEM-ORI".AsSpan());
        meta.Campos[2].Definidor(registro, "ITEM-DEST".AsSpan());
        meta.Campos[3].Definidor(registro, "12,345678".AsSpan());
        meta.Campos[4].Definidor(registro, "6,543210".AsSpan());

        registro.DtMov.Should().Be(new DateOnly(2025, 1, 15));
        registro.CodItemOri.Should().Be("ITEM-ORI");
        registro.CodItemDest.Should().Be("ITEM-DEST");
        registro.QtdOri.Should().Be(12.345678m);
        registro.QtdDest.Should().Be(6.543210m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("K220".AsSpan(), out var meta);
        var registro = (RegistroK220)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.DtMov.Should().Be(default(DateOnly));
        registro.CodItemOri.Should().BeNull();
        registro.CodItemDest.Should().BeNull();
        registro.QtdOri.Should().Be(0m);
        registro.QtdDest.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|K220|15012025|ITEM-ORI|ITEM-DEST|12,345678|6,543210|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComQuantidadeUnitaria_PreservaTextoCanonico()
    {
        const string sped = "|K220|31012025|ITEM-A|ITEM-B|1,000000|1,000000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
