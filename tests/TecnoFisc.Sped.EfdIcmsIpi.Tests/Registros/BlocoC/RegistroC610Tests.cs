using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.100 — exercita a forma do <see cref="RegistroC610"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 146): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC610Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC610).Assembly);

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
    public void Atributo_DeclaraC610_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC610).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C610");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC610Com16CamposNaOrdem()
    {
        _catalogo.TentarObter("C610".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C610");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodClass", "CodItem", "Qtd", "Unid", "VlItem", "VlDesc",
            "CstIcms", "Cfop", "AliqIcms", "VlBcIcms", "VlIcms",
            "VlBcIcmsSt", "VlIcmsSt", "VlPis", "VlCofins", "CodCta"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([
            2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17
        ]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C610".AsSpan(), out var meta);
        var registro = (RegistroC610)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "6".AsSpan());             // CodClass
        meta.Campos[1].Definidor(registro, "ENERGIA001".AsSpan());    // CodItem
        meta.Campos[2].Definidor(registro, "10,000".AsSpan());        // Qtd
        meta.Campos[3].Definidor(registro, "KWH".AsSpan());           // Unid
        meta.Campos[4].Definidor(registro, "1500,00".AsSpan());       // VlItem
        meta.Campos[5].Definidor(registro, "50,00".AsSpan());         // VlDesc
        meta.Campos[6].Definidor(registro, "60".AsSpan());            // CstIcms
        meta.Campos[7].Definidor(registro, "5251".AsSpan());          // Cfop
        meta.Campos[8].Definidor(registro, "12,00".AsSpan());         // AliqIcms
        meta.Campos[9].Definidor(registro, "1250,00".AsSpan());       // VlBcIcms
        meta.Campos[10].Definidor(registro, "150,00".AsSpan());       // VlIcms
        meta.Campos[11].Definidor(registro, "100,00".AsSpan());       // VlBcIcmsSt
        meta.Campos[12].Definidor(registro, "12,00".AsSpan());        // VlIcmsSt
        meta.Campos[13].Definidor(registro, "150,00".AsSpan());       // VlPis
        meta.Campos[14].Definidor(registro, "75,00".AsSpan());        // VlCofins
        meta.Campos[15].Definidor(registro, "CTA001".AsSpan());       // CodCta

        registro.CodClass.Should().Be(6);
        registro.CodItem.Should().Be("ENERGIA001");
        registro.Qtd.Should().Be(10.000m);
        registro.Unid.Should().Be("KWH");
        registro.VlItem.Should().Be(1500.00m);
        registro.VlDesc.Should().Be(50.00m);
        registro.CstIcms.Should().Be(60);
        registro.Cfop.Should().Be(Cfop.Create("5251"));
        registro.AliqIcms.Should().Be(12.00m);
        registro.VlBcIcms.Should().Be(1250.00m);
        registro.VlIcms.Should().Be(150.00m);
        registro.VlBcIcmsSt.Should().Be(100.00m);
        registro.VlIcmsSt.Should().Be(12.00m);
        registro.VlPis.Should().Be(150.00m);
        registro.VlCofins.Should().Be(75.00m);
        registro.CodCta.Should().Be("CTA001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C610".AsSpan(), out var meta);
        var registro = (RegistroC610)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodClass.Should().BeNull();
        registro.CodItem.Should().BeNull();
        registro.Qtd.Should().Be(0m);
        registro.Unid.Should().BeNull();
        registro.VlItem.Should().Be(0m);
        registro.VlDesc.Should().BeNull();
        registro.CstIcms.Should().BeNull();
        registro.AliqIcms.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.VlBcIcmsSt.Should().BeNull();
        registro.VlIcmsSt.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C610|6|ENERGIA001|10,000|KWH|1500,00|50,00|60|5251|12,00|1250,00|150,00|100,00|12,00|150,00|75,00|CTA001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // Apenas obrigatórios: COD_ITEM, QTD, UNID, VL_ITEM, CST_ICMS, CFOP; demais opcionais vazios.
        const string sped =
            "|C610||ENERGIA001|10,000|KWH|1500,00||60|5251|||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
