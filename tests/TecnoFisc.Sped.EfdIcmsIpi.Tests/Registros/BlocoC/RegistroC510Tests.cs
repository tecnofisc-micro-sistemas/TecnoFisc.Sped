using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.093 — exercita a forma do <see cref="RegistroC510"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (pp. 137-138): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC510Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC510).Assembly);

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
    public void Atributo_DeclaraC510_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC510).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C510");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC510Com20CamposNaOrdem()
    {
        _catalogo.TentarObter("C510".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C510");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "NumItem", "CodItem", "CodClass", "Qtd", "Unid",
            "VlItem", "VlDesc", "CstIcms", "Cfop",
            "VlBcIcms", "AliqIcms", "VlIcms",
            "VlBcIcmsSt", "AliqSt", "VlIcmsSt",
            "IndRec", "CodPart", "VlPis", "VlCofins", "CodCta"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([
            2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21
        ]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C510".AsSpan(), out var meta);
        var registro = (RegistroC510)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());             // NumItem
        meta.Campos[1].Definidor(registro, "ITEM001".AsSpan());       // CodItem
        meta.Campos[2].Definidor(registro, "1001".AsSpan());          // CodClass
        meta.Campos[3].Definidor(registro, "250,000".AsSpan());       // Qtd
        meta.Campos[4].Definidor(registro, "kWh".AsSpan());           // Unid
        meta.Campos[5].Definidor(registro, "500,00".AsSpan());        // VlItem
        meta.Campos[6].Definidor(registro, "10,00".AsSpan());         // VlDesc
        meta.Campos[7].Definidor(registro, "60".AsSpan());            // CstIcms
        meta.Campos[8].Definidor(registro, "5102".AsSpan());          // Cfop
        meta.Campos[9].Definidor(registro, "250,00".AsSpan());        // VlBcIcms
        meta.Campos[10].Definidor(registro, "12,00".AsSpan());        // AliqIcms
        meta.Campos[11].Definidor(registro, "30,00".AsSpan());        // VlIcms
        meta.Campos[12].Definidor(registro, "100,00".AsSpan());       // VlBcIcmsSt
        meta.Campos[13].Definidor(registro, "12,00".AsSpan());        // AliqSt
        meta.Campos[14].Definidor(registro, "12,00".AsSpan());        // VlIcmsSt
        meta.Campos[15].Definidor(registro, "0".AsSpan());            // IndRec
        meta.Campos[16].Definidor(registro, "PART001".AsSpan());      // CodPart
        meta.Campos[17].Definidor(registro, "5,00".AsSpan());         // VlPis
        meta.Campos[18].Definidor(registro, "10,00".AsSpan());        // VlCofins
        meta.Campos[19].Definidor(registro, "CONTA001".AsSpan());     // CodCta

        registro.NumItem.Should().Be(1);
        registro.CodItem.Should().Be("ITEM001");
        registro.CodClass.Should().Be(1001);
        registro.Qtd.Should().Be(250.000m);
        registro.Unid.Should().Be("kWh");
        registro.VlItem.Should().Be(500.00m);
        registro.VlDesc.Should().Be(10.00m);
        registro.CstIcms.Should().Be(60);
        registro.Cfop.Should().Be(Cfop.Create("5102"));
        registro.VlBcIcms.Should().Be(250.00m);
        registro.AliqIcms.Should().Be(12.00m);
        registro.VlIcms.Should().Be(30.00m);
        registro.VlBcIcmsSt.Should().Be(100.00m);
        registro.AliqSt.Should().Be(12.00m);
        registro.VlIcmsSt.Should().Be(12.00m);
        registro.IndRec.Should().Be(IndicadorTipoReceita.ReceitaPropria);
        registro.CodPart.Should().Be("PART001");
        registro.VlPis.Should().Be(5.00m);
        registro.VlCofins.Should().Be(10.00m);
        registro.CodCta.Should().Be("CONTA001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C510".AsSpan(), out var meta);
        var registro = (RegistroC510)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.NumItem.Should().Be(0);
        registro.CodItem.Should().BeNull();
        registro.CodClass.Should().BeNull();
        registro.Qtd.Should().BeNull();
        registro.Unid.Should().BeNull();
        registro.VlItem.Should().Be(0m);
        registro.VlDesc.Should().BeNull();
        registro.CstIcms.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.AliqIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.VlBcIcmsSt.Should().BeNull();
        registro.AliqSt.Should().BeNull();
        registro.VlIcmsSt.Should().BeNull();
        registro.IndRec.Should().BeNull();
        registro.CodPart.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C510|1|ITEM001|1001|250,000|kWh|500,00|10,00|60|5102|250,00|12,00|30,00|100,00|12,00|12,00|0|PART001|5,00|10,00|CONTA001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // Apenas campos obrigatórios: NUM_ITEM, COD_ITEM, VL_ITEM, CST_ICMS, CFOP, IND_REC.
        // COD_CLASS, QTD, UNID vazios entre COD_ITEM e VL_ITEM (4 pipes);
        // VL_DESC vazio antes de CST_ICMS; 6 opcionais entre CFOP e IND_REC (7 pipes); 4 após IND_REC.
        const string sped =
            "|C510|1|ITEM001||||500,00||60|5102|||||||0|||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComReceitaTerceiros_PreservaTextoCanonico()
    {
        // Item com IND_REC = 1 (terceiros) e COD_PART preenchido.
        const string sped =
            "|C510|2|AGUA001||||320,00||40|1556|||||||1|DIST001||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", IndicadorTipoReceita.ReceitaPropria)]
    [InlineData("1", IndicadorTipoReceita.ReceitaTerceiros)]
    public void IndRec_Definidor_AtribuiValorCorreto(string valor, IndicadorTipoReceita esperado)
    {
        _catalogo.TentarObter("C510".AsSpan(), out var meta);
        var registro = (RegistroC510)meta!.Fabrica();
        var campo = meta.Campos.First(c => c.Nome == "IndRec");

        campo.Definidor(registro, valor.AsSpan());

        registro.IndRec.Should().Be(esperado);
    }
}
