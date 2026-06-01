using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.148 — exercita a forma do <see cref="RegistroD610"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (pp. 198-200): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD610Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD610).Assembly);

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
    public void Atributo_DeclaraD610_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD610).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D610");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD610Com17CamposNaOrdem()
    {
        _catalogo.TentarObter("D610".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D610");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodClass", "CodItem", "Qtd", "Unid", "VlItem",
            "VlDesc", "CstIcms", "Cfop", "AliqIcms",
            "VlBcIcms", "VlIcms", "VlBcIcmsUf", "VlIcmsUf",
            "VlRedBc", "VlPis", "VlCofins", "CodCta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([
            2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18,
        ]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D610".AsSpan(), out var meta);
        var registro = (RegistroD610)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1001".AsSpan());         // CodClass
        meta.Campos[1].Definidor(registro, "SERVICO-TELECOM-01".AsSpan()); // CodItem
        meta.Campos[2].Definidor(registro, "100,000".AsSpan());      // Qtd
        meta.Campos[3].Definidor(registro, "UN".AsSpan());           // Unid
        meta.Campos[4].Definidor(registro, "5000,00".AsSpan());      // VlItem
        meta.Campos[5].Definidor(registro, "50,00".AsSpan());        // VlDesc
        meta.Campos[6].Definidor(registro, "100".AsSpan());          // CstIcms
        meta.Campos[7].Definidor(registro, "6351".AsSpan());         // Cfop
        meta.Campos[8].Definidor(registro, "12,00".AsSpan());        // AliqIcms
        meta.Campos[9].Definidor(registro, "4500,00".AsSpan());      // VlBcIcms
        meta.Campos[10].Definidor(registro, "540,00".AsSpan());      // VlIcms
        meta.Campos[11].Definidor(registro, "0,00".AsSpan());        // VlBcIcmsUf
        meta.Campos[12].Definidor(registro, "0,00".AsSpan());        // VlIcmsUf
        meta.Campos[13].Definidor(registro, "500,00".AsSpan());      // VlRedBc
        meta.Campos[14].Definidor(registro, "25,00".AsSpan());       // VlPis
        meta.Campos[15].Definidor(registro, "50,00".AsSpan());       // VlCofins
        meta.Campos[16].Definidor(registro, "1.2.3.001".AsSpan());   // CodCta

        registro.CodClass.Should().Be(1001);
        registro.CodItem.Should().Be("SERVICO-TELECOM-01");
        registro.Qtd.Should().Be(100.000m);
        registro.Unid.Should().Be("UN");
        registro.VlItem.Should().Be(5000.00m);
        registro.VlDesc.Should().Be(50.00m);
        registro.CstIcms.Should().Be(100);
        registro.Cfop.Should().Be(Cfop.Create("6351".AsSpan()));
        registro.AliqIcms.Should().Be(12.00m);
        registro.VlBcIcms.Should().Be(4500.00m);
        registro.VlIcms.Should().Be(540.00m);
        registro.VlBcIcmsUf.Should().Be(0.00m);
        registro.VlIcmsUf.Should().Be(0.00m);
        registro.VlRedBc.Should().Be(500.00m);
        registro.VlPis.Should().Be(25.00m);
        registro.VlCofins.Should().Be(50.00m);
        registro.CodCta.Should().Be("1.2.3.001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D610".AsSpan(), out var meta);
        var registro = (RegistroD610)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);   // VlDesc
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);   // AliqIcms
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);   // VlBcIcms
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlIcms
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlBcIcmsUf
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlIcmsUf
        meta.Campos[13].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlRedBc
        meta.Campos[14].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlPis
        meta.Campos[15].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlCofins
        meta.Campos[16].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCta

        registro.VlDesc.Should().BeNull();
        registro.AliqIcms.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.VlBcIcmsUf.Should().BeNull();
        registro.VlIcmsUf.Should().BeNull();
        registro.VlRedBc.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Serviço de telecomunicação, CST 100 (tributado integralmente), CFOP 6351, alíq. 12%.
        const string sped =
            "|D610|1001|SERVICO-TELECOM-01|100,000|UN|5000,00|50,00|100|6351|12,00|4500,00|540,00|0,00|0,00|500,00|25,00|50,00|1.2.3.001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // CST 41 (não tributado): ALIQ_ICMS e campos de valor vazio, sem COD_CTA.
        const string sped =
            "|D610|2001|SERVICO-COMM-02|50,000|UN|3000,00||41|5351||||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComReducaoBaseCalculo_PreservaTextoCanonico()
    {
        // CST 20 (com redução de BC): VL_RED_BC reflete parcela não tributada.
        const string sped =
            "|D610|1002|SERVICO-GAS-03|200,000|M3|8000,00||20|6351|12,00|4000,00|480,00|0,00|0,00|4000,00||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
