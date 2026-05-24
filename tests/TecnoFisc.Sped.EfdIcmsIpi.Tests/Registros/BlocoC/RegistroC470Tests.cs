using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.088 — exercita a forma do <see cref="RegistroC470"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.2.2 (p. 129): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC470Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC470).Assembly);

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
    public void Atributo_DeclaraC470_Nivel5_BlocoC()
    {
        var atributo = typeof(RegistroC470).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C470");
        atributo.Nivel.Should().Be(5);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC470Com10CamposNaOrdem()
    {
        _catalogo.TentarObter("C470".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C470");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodItem", "Qtd", "QtdCanc", "Unid",
            "VlItem", "CstIcms", "Cfop", "AliqIcms", "VlPis", "VlCofins"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C470".AsSpan(), out var meta);
        var registro = (RegistroC470)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "PROD001".AsSpan());  // CodItem
        meta.Campos[1].Definidor(registro, "10,000".AsSpan());   // Qtd
        meta.Campos[2].Definidor(registro, "1,000".AsSpan());    // QtdCanc
        meta.Campos[3].Definidor(registro, "UN".AsSpan());       // Unid
        meta.Campos[4].Definidor(registro, "100,50".AsSpan());   // VlItem
        meta.Campos[5].Definidor(registro, "60".AsSpan());       // CstIcms
        meta.Campos[6].Definidor(registro, "5102".AsSpan());     // Cfop
        meta.Campos[7].Definidor(registro, "12,00".AsSpan());    // AliqIcms
        meta.Campos[8].Definidor(registro, "1,50".AsSpan());     // VlPis
        meta.Campos[9].Definidor(registro, "7,00".AsSpan());     // VlCofins

        registro.CodItem.Should().Be("PROD001");
        registro.Qtd.Should().Be(10.000m);
        registro.QtdCanc.Should().Be(1.000m);
        registro.Unid.Should().Be("UN");
        registro.VlItem.Should().Be(100.50m);
        registro.CstIcms.Should().Be(60);
        registro.Cfop.Should().Be(Cfop.Create("5102".AsSpan()));
        registro.AliqIcms.Should().Be(12.00m);
        registro.VlPis.Should().Be(1.50m);
        registro.VlCofins.Should().Be(7.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C470".AsSpan(), out var meta);
        var registro = (RegistroC470)meta!.Fabrica();

        // Qtd (índice 1) e VlItem (4) são não-nullable — não testados aqui.
        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty); // CodItem
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // QtdCanc
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // Unid
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // CstIcms
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // Cfop
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqIcms
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty); // VlPis
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCofins

        registro.CodItem.Should().BeNull();
        registro.QtdCanc.Should().BeNull();
        registro.Unid.Should().BeNull();
        registro.CstIcms.Should().BeNull();
        registro.Cfop.Should().BeNull();
        registro.AliqIcms.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // CST_ICMS int? serializa sem zero-padding — forma canônica é "60" não "060".
        const string sped =
            "|C470|PROD001|10,000|1,000|UN|100,50|60|5102|12,00|1,50|7,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // QTD_CANC, ALIQ_ICMS, VL_PIS e VL_COFINS são OC — dispensados para quem entrega EFD-Contribuições.
        const string sped =
            "|C470|PROD002|5,500||CX|75,00|0|5405||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComServicoIssqn_PreservaTextoCanonico()
    {
        // ISSQN: item de serviço (TIPO_ITEM = "09" no 0200) com CST ICMS e CFOP de saída interna.
        const string sped =
            "|C470|SERV001|1,000||SV|200,00|40|5932||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
