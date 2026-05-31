using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.073 — exercita a forma do <see cref="RegistroC320"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 108): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC320Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC320).Assembly);

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
    public void Atributo_DeclaraC320_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC320).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C320");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC320Com8CamposNaOrdem()
    {
        _catalogo.TentarObter("C320".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C320");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["CstIcms", "Cfop", "AliqIcms", "VlOpr", "VlBcIcms", "VlIcms", "VlRedBc", "CodObs"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C320".AsSpan(), out var meta);
        var registro = (RegistroC320)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "020".AsSpan());        // CstIcms
        meta.Campos[1].Definidor(registro, "5102".AsSpan());       // Cfop
        meta.Campos[2].Definidor(registro, "12,00".AsSpan());      // AliqIcms
        meta.Campos[3].Definidor(registro, "1000,00".AsSpan());    // VlOpr
        meta.Campos[4].Definidor(registro, "800,00".AsSpan());     // VlBcIcms
        meta.Campos[5].Definidor(registro, "96,00".AsSpan());      // VlIcms
        meta.Campos[6].Definidor(registro, "50,00".AsSpan());      // VlRedBc
        meta.Campos[7].Definidor(registro, "OBS01".AsSpan());      // CodObs

        registro.CstIcms.Should().Be(20);
        registro.Cfop.Should().Be(Cfop.Create("5102".AsSpan()));
        registro.AliqIcms.Should().Be(12.00m);
        registro.VlOpr.Should().Be(1000.00m);
        registro.VlBcIcms.Should().Be(800.00m);
        registro.VlIcms.Should().Be(96.00m);
        registro.VlRedBc.Should().Be(50.00m);
        registro.CodObs.Should().Be("OBS01");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C320".AsSpan(), out var meta);
        var registro = (RegistroC320)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CstIcms.Should().BeNull();
        registro.Cfop.Should().BeNull();
        registro.AliqIcms.Should().BeNull();
        registro.VlOpr.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.VlRedBc.Should().BeNull();
        registro.CodObs.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // CST_ICMS int? serializa sem zero-padding — "20" não "020".
        const string sped =
            "|C320|20|5102|12,00|1000,00|800,00|96,00|50,00|OBS01|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemAliquotaECodObs_PreservaTextoCanonico()
    {
        // ALIQ_ICMS (OC) e COD_OBS (OC) ausentes — CST isento (40).
        const string sped =
            "|C320|40|5102||1000,00|0,00|0,00|0,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
