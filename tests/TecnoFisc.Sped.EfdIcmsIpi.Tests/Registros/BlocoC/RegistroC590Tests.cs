using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.094 — exercita a forma do <see cref="RegistroC590"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (pp. 139-140): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC590Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC590).Assembly);

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
    public void Atributo_DeclaraC590_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC590).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C590");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC590Com10CamposNaOrdem()
    {
        _catalogo.TentarObter("C590".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C590");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CstIcms", "Cfop", "AliqIcms",
            "VlOpr", "VlBcIcms", "VlIcms",
            "VlBcIcmsSt", "VlIcmsSt", "VlRedBc",
            "CodObs"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([
            2, 3, 4, 5, 6, 7, 8, 9, 10, 11
        ]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C590".AsSpan(), out var meta);
        var registro = (RegistroC590)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "60".AsSpan());          // CstIcms
        meta.Campos[1].Definidor(registro, "5102".AsSpan());        // Cfop
        meta.Campos[2].Definidor(registro, "12,00".AsSpan());       // AliqIcms
        meta.Campos[3].Definidor(registro, "1000,00".AsSpan());     // VlOpr
        meta.Campos[4].Definidor(registro, "800,00".AsSpan());      // VlBcIcms
        meta.Campos[5].Definidor(registro, "96,00".AsSpan());       // VlIcms
        meta.Campos[6].Definidor(registro, "200,00".AsSpan());      // VlBcIcmsSt
        meta.Campos[7].Definidor(registro, "24,00".AsSpan());       // VlIcmsSt
        meta.Campos[8].Definidor(registro, "50,00".AsSpan());       // VlRedBc
        meta.Campos[9].Definidor(registro, "OBS001".AsSpan());      // CodObs

        registro.CstIcms.Should().Be(60);
        registro.Cfop.Should().Be(Cfop.Criar("5102"));
        registro.AliqIcms.Should().Be(12.00m);
        registro.VlOpr.Should().Be(1000.00m);
        registro.VlBcIcms.Should().Be(800.00m);
        registro.VlIcms.Should().Be(96.00m);
        registro.VlBcIcmsSt.Should().Be(200.00m);
        registro.VlIcmsSt.Should().Be(24.00m);
        registro.VlRedBc.Should().Be(50.00m);
        registro.CodObs.Should().Be("OBS001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C590".AsSpan(), out var meta);
        var registro = (RegistroC590)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CstIcms.Should().BeNull();
        registro.AliqIcms.Should().BeNull();
        registro.VlOpr.Should().Be(0m);
        registro.VlBcIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.VlBcIcmsSt.Should().BeNull();
        registro.VlIcmsSt.Should().BeNull();
        registro.VlRedBc.Should().BeNull();
        registro.CodObs.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C590|60|5102|12,00|1000,00|800,00|96,00|200,00|24,00|50,00|OBS001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // Apenas obrigatórios: CST_ICMS, CFOP, VL_OPR; demais opcionais vazios.
        const string sped =
            "|C590|60|5102||1000,00|||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComAliqIcmsZero_PreservaTextoCanonico()
    {
        // Alíquota zero (ICMS isento ou não tributado).
        const string sped =
            "|C590|40|1102|0,00|500,00|0,00|0,00|||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
