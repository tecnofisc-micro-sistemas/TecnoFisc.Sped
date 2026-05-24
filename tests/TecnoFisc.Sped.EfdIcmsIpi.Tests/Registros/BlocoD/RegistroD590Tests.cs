using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.146 — exercita a forma do <see cref="RegistroD590"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (pp. 196-197): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD590Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD590).Assembly);

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
    public void Atributo_DeclaraD590_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD590).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D590");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD590Com10CamposNaOrdem()
    {
        _catalogo.TentarObter("D590".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D590");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CstIcms", "Cfop", "AliqIcms",
            "VlOpr", "VlBcIcms", "VlIcms",
            "VlBcIcmsUf", "VlIcmsUf", "VlRedBc", "CodObs",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D590".AsSpan(), out var meta);
        var registro = (RegistroD590)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "100".AsSpan());          // CstIcms
        meta.Campos[1].Definidor(registro, "5351".AsSpan());         // Cfop
        meta.Campos[2].Definidor(registro, "12,00".AsSpan());        // AliqIcms
        meta.Campos[3].Definidor(registro, "15000,00".AsSpan());     // VlOpr
        meta.Campos[4].Definidor(registro, "12500,00".AsSpan());     // VlBcIcms
        meta.Campos[5].Definidor(registro, "1500,00".AsSpan());      // VlIcms
        meta.Campos[6].Definidor(registro, "0,00".AsSpan());         // VlBcIcmsUf
        meta.Campos[7].Definidor(registro, "0,00".AsSpan());         // VlIcmsUf
        meta.Campos[8].Definidor(registro, "2500,00".AsSpan());      // VlRedBc
        meta.Campos[9].Definidor(registro, "OBS001".AsSpan());       // CodObs

        registro.CstIcms.Should().Be(100);
        registro.Cfop.Should().Be(Cfop.Create("5351".AsSpan()));
        registro.AliqIcms.Should().Be(12.00m);
        registro.VlOpr.Should().Be(15000.00m);
        registro.VlBcIcms.Should().Be(12500.00m);
        registro.VlIcms.Should().Be(1500.00m);
        registro.VlBcIcmsUf.Should().Be(0.00m);
        registro.VlIcmsUf.Should().Be(0.00m);
        registro.VlRedBc.Should().Be(2500.00m);
        registro.CodObs.Should().Be("OBS001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D590".AsSpan(), out var meta);
        var registro = (RegistroD590)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CstIcms.Should().BeNull();
        registro.Cfop.Should().BeNull();
        registro.AliqIcms.Should().BeNull();
        registro.VlOpr.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.VlBcIcmsUf.Should().BeNull();
        registro.VlIcmsUf.Should().BeNull();
        registro.VlRedBc.Should().BeNull();
        registro.CodObs.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // CST 100 (tributado integralmente), CFOP 5351 (serviço de comunicação), alíq. 12%.
        const string sped =
            "|D590|100|5351|12,00|15000,00|12500,00|1500,00|0,00|0,00|2500,00|OBS001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComAliquotaVazia_PreservaTextoCanonico()
    {
        // CST 41 (não tributado): ALIQ_ICMS vazio, campos UF zerados, sem COD_OBS.
        const string sped =
            "|D590|41|5351||10000,00|0,00|0,00|0,00|0,00|0,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComReducaoBaseCalculo_PreservaTextoCanonico()
    {
        // CST 20 (com redução de BC): VL_RED_BC reflete a parcela não tributada.
        const string sped =
            "|D590|20|6351|12,00|8000,00|4000,00|480,00|0,00|0,00|4000,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
