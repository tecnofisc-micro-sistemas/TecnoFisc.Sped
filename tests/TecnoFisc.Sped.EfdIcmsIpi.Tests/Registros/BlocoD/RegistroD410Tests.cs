using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.140 — exercita a forma do <see cref="RegistroD410"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 189): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD410Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD410).Assembly);

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
    public void Atributo_DeclaraD410_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD410).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D410");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD410Com14CamposNaOrdem()
    {
        _catalogo.TentarObter("D410".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D410");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["CodMod", "Ser", "Sub", "NumDocIni", "NumDocFin", "DtDoc",
             "CstIcms", "Cfop", "AliqIcms", "VlOpr", "VlDesc", "VlServ", "VlBcIcms", "VlIcms"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D410".AsSpan(), out var meta);
        var registro = (RegistroD410)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "13".AsSpan());           // CodMod
        meta.Campos[1].Definidor(registro, "SER1".AsSpan());         // Ser
        meta.Campos[2].Definidor(registro, "1".AsSpan());            // Sub
        meta.Campos[3].Definidor(registro, "1".AsSpan());            // NumDocIni
        meta.Campos[4].Definidor(registro, "100".AsSpan());          // NumDocFin
        meta.Campos[5].Definidor(registro, "01012024".AsSpan());     // DtDoc
        meta.Campos[6].Definidor(registro, "060".AsSpan());          // CstIcms
        meta.Campos[7].Definidor(registro, "6101".AsSpan());         // Cfop
        meta.Campos[8].Definidor(registro, "12,00".AsSpan());        // AliqIcms
        meta.Campos[9].Definidor(registro, "5000,00".AsSpan());      // VlOpr
        meta.Campos[10].Definidor(registro, "100,00".AsSpan());      // VlDesc
        meta.Campos[11].Definidor(registro, "4900,00".AsSpan());     // VlServ
        meta.Campos[12].Definidor(registro, "4900,00".AsSpan());     // VlBcIcms
        meta.Campos[13].Definidor(registro, "588,00".AsSpan());      // VlIcms

        registro.CodMod.Should().Be("13");
        registro.Ser.Should().Be("SER1");
        registro.Sub.Should().Be(1);
        registro.NumDocIni.Should().Be(1);
        registro.NumDocFin.Should().Be(100);
        registro.DtDoc.Should().Be(new DateOnly(2024, 1, 1));
        registro.CstIcms.Should().Be(60);
        registro.Cfop.Should().Be(Cfop.Criar("6101"));
        registro.AliqIcms.Should().Be(12.00m);
        registro.VlOpr.Should().Be(5000.00m);
        registro.VlDesc.Should().Be(100.00m);
        registro.VlServ.Should().Be(4900.00m);
        registro.VlBcIcms.Should().Be(4900.00m);
        registro.VlIcms.Should().Be(588.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D410".AsSpan(), out var meta);
        var registro = (RegistroD410)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, Span<char>.Empty);   // Sub
        meta.Campos[8].Definidor(registro, Span<char>.Empty);   // AliqIcms
        meta.Campos[10].Definidor(registro, Span<char>.Empty);  // VlDesc

        registro.Sub.Should().BeNull();
        registro.AliqIcms.Should().BeNull();
        registro.VlDesc.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // CstIcms int? serializa sem zero-padding — forma canônica é "60" não "060".
        const string sped =
            "|D410|13|SER1|1|1|100|01012024|60|6101|12,00|5000,00|100,00|4900,00|4900,00|588,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // Bilhete rodoviário sem subsérie, sem alíquota e sem desconto. CST "60" sem zero-padding.
        const string sped =
            "|D410|13|SER1||1|100|01012024|60|6101||5000,00||4900,00|4900,00|588,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CatraqueSerie9999_PreservaTextoCanonico()
    {
        // Uso de catraca: SER="9999", NUM_DOC_INI=0 (conforme nota do guia p. 189). CST "60" sem zero-padding.
        const string sped =
            "|D410|13|9999||0|250|01012024|60|6101|12,00|2500,00||2500,00|2500,00|300,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
