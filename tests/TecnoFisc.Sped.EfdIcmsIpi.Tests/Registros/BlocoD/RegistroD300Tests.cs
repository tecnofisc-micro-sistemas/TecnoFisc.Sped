using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.130 — exercita a forma do <see cref="RegistroD300"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 181): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD300Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD300).Assembly);

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
    public void Atributo_DeclaraD300_Nivel2_BlocoD()
    {
        var atributo = typeof(RegistroD300).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D300");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD300Com19CamposNaOrdem()
    {
        _catalogo.TentarObter("D300".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D300");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodMod", "Ser", "Sub", "NumDocIni", "NumDocFin",
            "CstIcms", "Cfop", "AliqIcms", "DtDoc",
            "VlOpr", "VlDesc", "VlServ", "VlSeg", "VlOutDesp",
            "VlBcIcms", "VlIcms", "VlRedBc",
            "CodObs", "CodCta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D300".AsSpan(), out var meta);
        var registro = (RegistroD300)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "13".AsSpan());          // CodMod
        meta.Campos[1].Definidor(registro, "001".AsSpan());          // Ser
        meta.Campos[2].Definidor(registro, "2".AsSpan());            // Sub
        meta.Campos[3].Definidor(registro, "100".AsSpan());          // NumDocIni
        meta.Campos[4].Definidor(registro, "199".AsSpan());          // NumDocFin
        meta.Campos[5].Definidor(registro, "020".AsSpan());          // CstIcms
        meta.Campos[6].Definidor(registro, "5353".AsSpan());         // Cfop
        meta.Campos[7].Definidor(registro, "12,00".AsSpan());        // AliqIcms
        meta.Campos[8].Definidor(registro, "01012023".AsSpan());     // DtDoc
        meta.Campos[9].Definidor(registro, "1000,00".AsSpan());      // VlOpr
        meta.Campos[10].Definidor(registro, "50,00".AsSpan());       // VlDesc
        meta.Campos[11].Definidor(registro, "900,00".AsSpan());      // VlServ
        meta.Campos[12].Definidor(registro, "10,00".AsSpan());       // VlSeg
        meta.Campos[13].Definidor(registro, "40,00".AsSpan());       // VlOutDesp
        meta.Campos[14].Definidor(registro, "810,00".AsSpan());      // VlBcIcms
        meta.Campos[15].Definidor(registro, "108,00".AsSpan());      // VlIcms
        meta.Campos[16].Definidor(registro, "90,00".AsSpan());       // VlRedBc
        meta.Campos[17].Definidor(registro, "OBS01".AsSpan());       // CodObs
        meta.Campos[18].Definidor(registro, "CT001".AsSpan());       // CodCta

        registro.CodMod.Should().Be("13");
        registro.Ser.Should().Be("001");
        registro.Sub.Should().Be(2);
        registro.NumDocIni.Should().Be(100);
        registro.NumDocFin.Should().Be(199);
        registro.CstIcms.Should().Be(20);
        registro.Cfop.Should().Be(Cfop.Criar("5353".AsSpan()));
        registro.AliqIcms.Should().Be(12m);
        registro.DtDoc.Should().Be(new DateOnly(2023, 1, 1));
        registro.VlOpr.Should().Be(1000m);
        registro.VlDesc.Should().Be(50m);
        registro.VlServ.Should().Be(900m);
        registro.VlSeg.Should().Be(10m);
        registro.VlOutDesp.Should().Be(40m);
        registro.VlBcIcms.Should().Be(810m);
        registro.VlIcms.Should().Be(108m);
        registro.VlRedBc.Should().Be(90m);
        registro.CodObs.Should().Be("OBS01");
        registro.CodCta.Should().Be("CT001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D300".AsSpan(), out var meta);
        var registro = (RegistroD300)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);  // CodMod
        meta.Campos[1].Definidor(registro, Span<char>.Empty);  // Ser
        meta.Campos[2].Definidor(registro, Span<char>.Empty);  // Sub
        meta.Campos[3].Definidor(registro, Span<char>.Empty);  // NumDocIni
        meta.Campos[4].Definidor(registro, Span<char>.Empty);  // NumDocFin
        meta.Campos[5].Definidor(registro, Span<char>.Empty);  // CstIcms
        meta.Campos[6].Definidor(registro, Span<char>.Empty);  // Cfop
        meta.Campos[7].Definidor(registro, Span<char>.Empty);  // AliqIcms
        meta.Campos[10].Definidor(registro, Span<char>.Empty); // VlDesc
        meta.Campos[12].Definidor(registro, Span<char>.Empty); // VlSeg
        meta.Campos[13].Definidor(registro, Span<char>.Empty); // VlOutDesp
        meta.Campos[17].Definidor(registro, Span<char>.Empty); // CodObs
        meta.Campos[18].Definidor(registro, Span<char>.Empty); // CodCta

        registro.CodMod.Should().BeNull();
        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
        registro.NumDocIni.Should().BeNull();
        registro.NumDocFin.Should().BeNull();
        registro.CstIcms.Should().BeNull();
        registro.Cfop.Should().BeNull();
        registro.AliqIcms.Should().BeNull();
        registro.VlDesc.Should().BeNull();
        registro.VlSeg.Should().BeNull();
        registro.VlOutDesp.Should().BeNull();
        registro.CodObs.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // CST 20 (tributado com redução de BC), CFOP 5353, alíq. 12%, bilhetes rodoviários.
        const string sped =
            "|D300|13|001|2|100|199|20|5353|12,00|01012023|1000,00|50,00|900,00|10,00|40,00|810,00|108,00|90,00|OBS01|CT001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SomenteObrigatorios_PreservaTextoCanonico()
    {
        // Sub, AliqIcms, VlDesc, VlSeg, VlOutDesp, CodObs e CodCta são opcionais (OC).
        const string sped =
            "|D300|13|001||100|199|20|5353||01012023|1000,00||900,00|||810,00|108,00|90,00|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
