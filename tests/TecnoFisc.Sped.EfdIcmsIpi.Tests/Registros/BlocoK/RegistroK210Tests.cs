using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 8.196 — exercita a forma do <see cref="RegistroK210"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 251-252): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroK210Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroK210).Assembly);

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
    public void Atributo_DeclaraK210_Nivel3_BlocoK()
    {
        var atributo = typeof(RegistroK210).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K210");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK210ComCincoCamposNaOrdem()
    {
        _catalogo.TentarObter("K210".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K210");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtIniOs",
            "DtFinOs",
            "CodDocOs",
            "CodItemOri",
            "QtdOri",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K210".AsSpan(), out var meta);
        var registro = (RegistroK210)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01012025".AsSpan());
        meta.Campos[1].Definidor(registro, "15012025".AsSpan());
        meta.Campos[2].Definidor(registro, "OS-2025-001".AsSpan());
        meta.Campos[3].Definidor(registro, "ITEM-ORI".AsSpan());
        meta.Campos[4].Definidor(registro, "12,345678".AsSpan());

        registro.DtIniOs.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtFinOs.Should().Be(new DateOnly(2025, 1, 15));
        registro.CodDocOs.Should().Be("OS-2025-001");
        registro.CodItemOri.Should().Be("ITEM-ORI");
        registro.QtdOri.Should().Be(12.345678m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("K210".AsSpan(), out var meta);
        var registro = (RegistroK210)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.DtIniOs.Should().BeNull();
        registro.DtFinOs.Should().BeNull();
        registro.CodDocOs.Should().BeNull();
        registro.CodItemOri.Should().BeNull();
        registro.QtdOri.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|K210|01012025|15012025|OS-2025-001|ITEM-ORI|12,345678|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemOrdemServico_PreservaTextoCanonico()
    {
        const string sped = "|K210||||ITEM-ORI|1,000000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
