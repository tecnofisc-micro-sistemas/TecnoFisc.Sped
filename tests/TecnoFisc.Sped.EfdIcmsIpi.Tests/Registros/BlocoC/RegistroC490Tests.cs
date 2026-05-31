using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.090 — exercita a forma do <see cref="RegistroC490"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 131): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC490Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC490).Assembly);

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
    public void Atributo_DeclaraC490_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC490).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C490");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC490Com7CamposNaOrdem()
    {
        _catalogo.TentarObter("C490".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C490");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CstIcms", "Cfop", "AliqIcms",
            "VlOpr", "VlBcIcms", "VlIcms",
            "CodObs"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C490".AsSpan(), out var meta);
        var registro = (RegistroC490)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "060".AsSpan());         // CstIcms
        meta.Campos[1].Definidor(registro, "5102".AsSpan());        // Cfop
        meta.Campos[2].Definidor(registro, "18,00".AsSpan());       // AliqIcms
        meta.Campos[3].Definidor(registro, "5000,00".AsSpan());     // VlOpr
        meta.Campos[4].Definidor(registro, "4237,29".AsSpan());     // VlBcIcms
        meta.Campos[5].Definidor(registro, "762,71".AsSpan());      // VlIcms
        meta.Campos[6].Definidor(registro, "OBS001".AsSpan());      // CodObs

        registro.CstIcms.Should().Be(60);
        registro.Cfop.Should().Be(Cfop.Create("5102".AsSpan()));
        registro.AliqIcms.Should().Be(18.00m);
        registro.VlOpr.Should().Be(5000.00m);
        registro.VlBcIcms.Should().Be(4237.29m);
        registro.VlIcms.Should().Be(762.71m);
        registro.CodObs.Should().Be("OBS001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C490".AsSpan(), out var meta);
        var registro = (RegistroC490)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CstIcms.Should().BeNull();
        registro.Cfop.Should().BeNull();
        registro.AliqIcms.Should().BeNull();
        registro.VlOpr.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.CodObs.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // CST_ICMS int? serializa sem zero-padding — forma canônica é "60" não "060".
        const string sped =
            "|C490|60|5102|18,00|5000,00|4237,29|762,71|OBS001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComAliquotaVazia_PreservaTextoCanonico()
    {
        // CST 041 (isento): ALIQ_ICMS vazio, BC e ICMS zerados. CST "41" sem zero-padding.
        const string sped =
            "|C490|41|5405||1200,00|0,00|0,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCodObs_PreservaTextoCanonico()
    {
        // Campos obrigatórios mais ALIQ_ICMS; COD_OBS vazio.
        const string sped =
            "|C490|0|5949|12,00|3500,00|3125,00|375,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
