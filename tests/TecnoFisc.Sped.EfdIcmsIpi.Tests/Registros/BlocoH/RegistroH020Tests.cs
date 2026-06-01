using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoH;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoH;

/// <summary>
/// Sub-stage 8.190 — exercita a forma do <see cref="RegistroH020"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 248): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroH020Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroH020).Assembly);

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
    public void Atributo_DeclaraH020_Nivel4_BlocoH()
    {
        var atributo = typeof(RegistroH020).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("H020");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("H");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroH020ComTresCamposNaOrdem()
    {
        _catalogo.TentarObter("H020".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("H020");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CstIcms",
            "BcIcms",
            "VlIcms",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("H020".AsSpan(), out var meta);
        var registro = (RegistroH020)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "060".AsSpan());
        meta.Campos[1].Definidor(registro, "125,45".AsSpan());
        meta.Campos[2].Definidor(registro, "22,58".AsSpan());

        registro.CstIcms.Should().Be("060");
        registro.BcIcms.Should().Be(125.45m);
        registro.VlIcms.Should().Be(22.58m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("H020".AsSpan(), out var meta);
        var registro = (RegistroH020)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CstIcms.Should().BeNull();
        registro.BcIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|H020|060|125,45|22,58|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComValoresZerados_PreservaTextoCanonico()
    {
        const string sped = "|H020|000|0,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
