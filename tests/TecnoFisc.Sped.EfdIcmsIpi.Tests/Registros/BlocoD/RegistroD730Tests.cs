using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.017.008 — exercita a forma do <see cref="RegistroD730"/> (analítico NFCom, código 62)
/// contra o Guia Prático EFD-ICMS/IPI V3.2.2 (p. 215-217, Subseção 12): metadados de catálogo,
/// mapeamento de campos e invariante de round-trip parse → texto idêntico.
/// </summary>
public sealed class RegistroD730Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD730).Assembly);

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
    public void Atributo_DeclaraD730_Nivel3_BlocoD_IntroduzidoEmV017()
    {
        var atributo = typeof(RegistroD730).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D730");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
        atributo.IntroduzidoEm.Should().Be((int)LayoutEfdIcmsIpi.V017);
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD730Com8CamposNaOrdem()
    {
        _catalogo.TentarObter("D730".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D730");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CstIcms", "Cfop", "AliqIcms",
            "VlOpr", "VlBcIcms", "VlIcms",
            "VlRedBc", "CodObs",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9]);
    }

    [Fact]
    public void Definidor_AtribuiCamposObrigatorios()
    {
        _catalogo.TentarObter("D730".AsSpan(), out var meta);
        var registro = (RegistroD730)meta!.Fabrica();

        // Campos obrigatórios: 2 (CST_ICMS), 3 (CFOP), 5-8 (VL_OPR, VL_BC_ICMS, VL_ICMS, VL_RED_BC).
        meta.Campos[0].Definidor(registro, "100".AsSpan());          // CstIcms
        meta.Campos[1].Definidor(registro, "5351".AsSpan());         // Cfop
        meta.Campos[3].Definidor(registro, "15000,00".AsSpan());     // VlOpr
        meta.Campos[4].Definidor(registro, "12500,00".AsSpan());     // VlBcIcms
        meta.Campos[5].Definidor(registro, "1500,00".AsSpan());      // VlIcms
        meta.Campos[6].Definidor(registro, "2500,00".AsSpan());      // VlRedBc

        registro.CstIcms.Should().Be(100);
        registro.Cfop.Should().Be(Cfop.Create("5351".AsSpan()));
        registro.VlOpr.Should().Be(15000.00m);
        registro.VlBcIcms.Should().Be(12500.00m);
        registro.VlIcms.Should().Be(1500.00m);
        registro.VlRedBc.Should().Be(2500.00m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // CST 100 (tributado integralmente), CFOP 5351 (serviço de comunicação a contribuinte), alíq. 12%,
        // com COD_OBS referenciando entrada em Registro 0460.
        const string sped =
            "|D730|100|5351|12,00|15000,00|12500,00|1500,00|2500,00|OBS001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // CST 41 (não tributado): ALIQ_ICMS vazio (OC) e COD_OBS vazio (OC); demais campos
        // obrigatórios preenchidos (valores zerados quando aplicável).
        const string sped =
            "|D730|41|5351||10000,00|0,00|0,00|0,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
