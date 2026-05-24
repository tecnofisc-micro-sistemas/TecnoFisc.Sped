using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.065 — exercita a forma do <see cref="RegistroC185"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 96): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC185Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC185).Assembly);

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
    public void Atributo_DeclaraC185_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC185).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C185");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC185Com17CamposNaOrdem()
    {
        _catalogo.TentarObter("C185".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C185");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "NumItem", "CodItem", "CstIcms", "Cfop", "CodMotRestCompl",
            "QuantConv", "Unid", "VlUnitConv",
            "VlUnitIcmsNaOperacaoConv", "VlUnitIcmsOpConv",
            "VlUnitIcmsOpEstoqueConv", "VlUnitIcmsStEstoqueConv", "VlUnitFcpIcmsStEstoqueConv",
            "VlUnitIcmsStConvRest", "VlUnitFcpStConvRest",
            "VlUnitIcmsStConvCompl", "VlUnitFcpStConvCompl"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C185".AsSpan(), out var meta);
        var registro = (RegistroC185)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());           // NumItem
        meta.Campos[1].Definidor(registro, "PROD001".AsSpan());     // CodItem
        meta.Campos[2].Definidor(registro, "10".AsSpan());          // CstIcms
        meta.Campos[3].Definidor(registro, "5102".AsSpan());        // Cfop
        meta.Campos[4].Definidor(registro, "00507".AsSpan());       // CodMotRestCompl
        meta.Campos[5].Definidor(registro, "10,000000".AsSpan());   // QuantConv
        meta.Campos[6].Definidor(registro, "UN".AsSpan());          // Unid
        meta.Campos[7].Definidor(registro, "50,000000".AsSpan());   // VlUnitConv
        meta.Campos[8].Definidor(registro, "4,500000".AsSpan());    // VlUnitIcmsNaOperacaoConv
        meta.Campos[9].Definidor(registro, "5,000000".AsSpan());    // VlUnitIcmsOpConv
        meta.Campos[10].Definidor(registro, "5,000000".AsSpan());   // VlUnitIcmsOpEstoqueConv
        meta.Campos[11].Definidor(registro, "7,200000".AsSpan());   // VlUnitIcmsStEstoqueConv
        meta.Campos[12].Definidor(registro, "0,360000".AsSpan());   // VlUnitFcpIcmsStEstoqueConv
        meta.Campos[13].Definidor(registro, "6,840000".AsSpan());   // VlUnitIcmsStConvRest
        meta.Campos[14].Definidor(registro, "0,360000".AsSpan());   // VlUnitFcpStConvRest
        meta.Campos[15].Definidor(registro, "0,000000".AsSpan());   // VlUnitIcmsStConvCompl
        meta.Campos[16].Definidor(registro, "0,000000".AsSpan());   // VlUnitFcpStConvCompl

        registro.NumItem.Should().Be(1);
        registro.CodItem.Should().Be("PROD001");
        registro.CstIcms.Should().Be(10);
        registro.Cfop.Should().Be(Cfop.Create("5102".AsSpan()));
        registro.CodMotRestCompl.Should().Be("00507");
        registro.QuantConv.Should().Be(10.000000m);
        registro.Unid.Should().Be("UN");
        registro.VlUnitConv.Should().Be(50.000000m);
        registro.VlUnitIcmsNaOperacaoConv.Should().Be(4.500000m);
        registro.VlUnitIcmsOpConv.Should().Be(5.000000m);
        registro.VlUnitIcmsOpEstoqueConv.Should().Be(5.000000m);
        registro.VlUnitIcmsStEstoqueConv.Should().Be(7.200000m);
        registro.VlUnitFcpIcmsStEstoqueConv.Should().Be(0.360000m);
        registro.VlUnitIcmsStConvRest.Should().Be(6.840000m);
        registro.VlUnitFcpStConvRest.Should().Be(0.360000m);
        registro.VlUnitIcmsStConvCompl.Should().Be(0.000000m);
        registro.VlUnitFcpStConvCompl.Should().Be(0.000000m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C185".AsSpan(), out var meta);
        var registro = (RegistroC185)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.NumItem.Should().BeNull();
        registro.CodItem.Should().BeNull();
        registro.CstIcms.Should().BeNull();
        registro.Cfop.Should().BeNull();
        registro.CodMotRestCompl.Should().BeNull();
        registro.QuantConv.Should().BeNull();
        registro.Unid.Should().BeNull();
        registro.VlUnitConv.Should().BeNull();
        registro.VlUnitIcmsNaOperacaoConv.Should().BeNull();
        registro.VlUnitIcmsOpConv.Should().BeNull();
        registro.VlUnitIcmsOpEstoqueConv.Should().BeNull();
        registro.VlUnitIcmsStEstoqueConv.Should().BeNull();
        registro.VlUnitFcpIcmsStEstoqueConv.Should().BeNull();
        registro.VlUnitIcmsStConvRest.Should().BeNull();
        registro.VlUnitFcpStConvRest.Should().BeNull();
        registro.VlUnitIcmsStConvCompl.Should().BeNull();
        registro.VlUnitFcpStConvCompl.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Saída com todos os campos de valores unitários ST preenchidos.
        const string sped =
            "|C185|1|PROD001|10|5102|00507|10,000000|UN|50,000000|4,500000|5,000000|5,000000|7,200000|0,360000|6,840000|0,360000|0,000000|0,000000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComValoresUnitariosVazios_PreservaTextoCanonico()
    {
        // Apenas campos identificadores preenchidos; todos os VL_UNIT opcionais (campos 10-18) vazios.
        const string sped =
            "|C185|1|PROD001|10|5102|00507|10,000000|UN|50,000000||||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposIdentificadoresVazios_PreservaTextoCanonico()
    {
        // Campos opcionais NumItem, CodItem, CstIcms, Cfop não preenchidos (4 vazios = 5 pipes antes de CodMotRestCompl).
        const string sped =
            "|C185|||||00507|5,000000|PC|30,000000|3,000000|3,500000|4,000000|5,200000|0,260000|5,460000|0,270000|0,000000|0,000000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
