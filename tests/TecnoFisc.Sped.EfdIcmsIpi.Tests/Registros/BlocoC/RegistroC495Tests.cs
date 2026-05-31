using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.091 — exercita a forma do <see cref="RegistroC495"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 132): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC495Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC495).Assembly);

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
    public void Atributo_DeclaraC495_Nivel2_BlocoC()
    {
        var atributo = typeof(RegistroC495).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C495");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC495Com14CamposNaOrdem()
    {
        _catalogo.TentarObter("C495".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C495");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "AliqIcms", "CodItem", "Qtd", "QtdCanc", "Unid",
            "VlItem", "VlDesc", "VlCanc", "VlAcmo",
            "VlBcIcms", "VlIcms", "VlIsen", "VlNt", "VlIcmsSt"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C495".AsSpan(), out var meta);
        var registro = (RegistroC495)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "12,00".AsSpan());      // AliqIcms
        meta.Campos[1].Definidor(registro, "ITEM001".AsSpan());    // CodItem
        meta.Campos[2].Definidor(registro, "100,000".AsSpan());    // Qtd
        meta.Campos[3].Definidor(registro, "5,000".AsSpan());      // QtdCanc
        meta.Campos[4].Definidor(registro, "UN".AsSpan());         // Unid
        meta.Campos[5].Definidor(registro, "1200,00".AsSpan());    // VlItem
        meta.Campos[6].Definidor(registro, "50,00".AsSpan());      // VlDesc
        meta.Campos[7].Definidor(registro, "60,00".AsSpan());      // VlCanc
        meta.Campos[8].Definidor(registro, "10,00".AsSpan());      // VlAcmo
        meta.Campos[9].Definidor(registro, "1000,00".AsSpan());    // VlBcIcms
        meta.Campos[10].Definidor(registro, "120,00".AsSpan());    // VlIcms
        meta.Campos[11].Definidor(registro, "80,00".AsSpan());     // VlIsen
        meta.Campos[12].Definidor(registro, "40,00".AsSpan());     // VlNt
        meta.Campos[13].Definidor(registro, "30,00".AsSpan());     // VlIcmsSt

        registro.AliqIcms.Should().Be(12.00m);
        registro.CodItem.Should().Be("ITEM001");
        registro.Qtd.Should().Be(100.000m);
        registro.QtdCanc.Should().Be(5.000m);
        registro.Unid.Should().Be("UN");
        registro.VlItem.Should().Be(1200.00m);
        registro.VlDesc.Should().Be(50.00m);
        registro.VlCanc.Should().Be(60.00m);
        registro.VlAcmo.Should().Be(10.00m);
        registro.VlBcIcms.Should().Be(1000.00m);
        registro.VlIcms.Should().Be(120.00m);
        registro.VlIsen.Should().Be(80.00m);
        registro.VlNt.Should().Be(40.00m);
        registro.VlIcmsSt.Should().Be(30.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C495".AsSpan(), out var meta);
        var registro = (RegistroC495)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.AliqIcms.Should().BeNull();
        registro.CodItem.Should().BeNull();
        registro.Qtd.Should().BeNull();
        registro.QtdCanc.Should().BeNull();
        registro.Unid.Should().BeNull();
        registro.VlItem.Should().BeNull();
        registro.VlDesc.Should().BeNull();
        registro.VlCanc.Should().BeNull();
        registro.VlAcmo.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.VlIsen.Should().BeNull();
        registro.VlNt.Should().BeNull();
        registro.VlIcmsSt.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C495|12,00|ITEM001|100,000|5,000|UN|1200,00|50,00|60,00|10,00|1000,00|120,00|80,00|40,00|30,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // Apenas campos obrigatórios: COD_ITEM, QTD, UNID e VL_ITEM. ALIQ_ICMS e demais valores opcionais vazios.
        const string sped =
            "|C495||ITEM002|200,000||KG|2500,00|||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComApenasIsentos_PreservaTextoCanonico()
    {
        // Saídas isentas: VL_DESC..VL_ICMS (campos 8-12) vazios, VL_ISEN=500,00 (campo 13).
        const string sped =
            "|C495||ITEMISENTO|50,000||PC|500,00||||||500,00|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
