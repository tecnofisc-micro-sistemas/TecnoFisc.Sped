using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.066 — exercita a forma do <see cref="RegistroC186"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 100): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC186Tests
{
    // Chave NF-e válida (UF SP, Jan/2024) reutilizada dos testes de ChaveAcesso.
    private const string ChaveNfeValida = "35240111222333000181550010000000011000000018";

    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC186).Assembly);

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
    public void Atributo_DeclaraC186_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC186).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C186");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC186Com18CamposNaOrdem()
    {
        _catalogo.TentarObter("C186".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C186");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "NumItem", "CodItem", "CstIcms", "Cfop", "CodMotRestCompl",
            "QuantConv", "Unid", "CodModEntrada",
            "SerieEntrada", "NumDocEntrada", "ChvDfeEntrada",
            "DtDocEntrada", "NumItemEntrada",
            "VlUnitConvEntrada", "VlUnitIcmsOpConvEntrada",
            "VlUnitBcIcmsStConvEntrada", "VlUnitIcmsStConvEntrada", "VlUnitFcpStConvEntrada"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C186".AsSpan(), out var meta);
        var registro = (RegistroC186)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());                // NumItem
        meta.Campos[1].Definidor(registro, "PROD001".AsSpan());          // CodItem
        meta.Campos[2].Definidor(registro, "60".AsSpan());               // CstIcms
        meta.Campos[3].Definidor(registro, "5201".AsSpan());             // Cfop
        meta.Campos[4].Definidor(registro, "00401".AsSpan());            // CodMotRestCompl
        meta.Campos[5].Definidor(registro, "5,000000".AsSpan());         // QuantConv
        meta.Campos[6].Definidor(registro, "UN".AsSpan());               // Unid
        meta.Campos[7].Definidor(registro, "01".AsSpan());               // CodModEntrada
        meta.Campos[8].Definidor(registro, "001".AsSpan());              // SerieEntrada
        meta.Campos[9].Definidor(registro, "123456789".AsSpan());        // NumDocEntrada
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty);   // ChvDfeEntrada (doc papel)
        meta.Campos[11].Definidor(registro, "01012024".AsSpan());        // DtDocEntrada
        meta.Campos[12].Definidor(registro, "1".AsSpan());               // NumItemEntrada
        meta.Campos[13].Definidor(registro, "30,000000".AsSpan());       // VlUnitConvEntrada
        meta.Campos[14].Definidor(registro, "4,500000".AsSpan());        // VlUnitIcmsOpConvEntrada
        meta.Campos[15].Definidor(registro, "6,000000".AsSpan());        // VlUnitBcIcmsStConvEntrada
        meta.Campos[16].Definidor(registro, "7,200000".AsSpan());        // VlUnitIcmsStConvEntrada
        meta.Campos[17].Definidor(registro, "0,360000".AsSpan());        // VlUnitFcpStConvEntrada

        registro.NumItem.Should().Be(1);
        registro.CodItem.Should().Be("PROD001");
        registro.CstIcms.Should().Be(60);
        registro.Cfop.Should().Be(Cfop.Create("5201".AsSpan()));
        registro.CodMotRestCompl.Should().Be("00401");
        registro.QuantConv.Should().Be(5.000000m);
        registro.Unid.Should().Be("UN");
        registro.CodModEntrada.Should().Be("01");
        registro.SerieEntrada.Should().Be("001");
        registro.NumDocEntrada.Should().Be(123456789);
        registro.ChvDfeEntrada.Should().BeNull();
        registro.DtDocEntrada.Should().Be(new DateOnly(2024, 1, 1));
        registro.NumItemEntrada.Should().Be(1);
        registro.VlUnitConvEntrada.Should().Be(30.000000m);
        registro.VlUnitIcmsOpConvEntrada.Should().Be(4.500000m);
        registro.VlUnitBcIcmsStConvEntrada.Should().Be(6.000000m);
        registro.VlUnitIcmsStConvEntrada.Should().Be(7.200000m);
        registro.VlUnitFcpStConvEntrada.Should().Be(0.360000m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C186".AsSpan(), out var meta);
        var registro = (RegistroC186)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.NumItem.Should().BeNull();
        registro.CodItem.Should().BeNull();
        registro.CstIcms.Should().BeNull();
        registro.Cfop.Should().BeNull();
        registro.CodMotRestCompl.Should().BeNull();
        registro.QuantConv.Should().BeNull();
        registro.Unid.Should().BeNull();
        registro.CodModEntrada.Should().BeNull();
        registro.SerieEntrada.Should().BeNull();
        registro.NumDocEntrada.Should().BeNull();
        registro.ChvDfeEntrada.Should().BeNull();
        registro.DtDocEntrada.Should().BeNull();
        registro.NumItemEntrada.Should().BeNull();
        registro.VlUnitConvEntrada.Should().BeNull();
        registro.VlUnitIcmsOpConvEntrada.Should().BeNull();
        registro.VlUnitBcIcmsStConvEntrada.Should().BeNull();
        registro.VlUnitIcmsStConvEntrada.Should().BeNull();
        registro.VlUnitFcpStConvEntrada.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComDocumentoPapel_PreservaTextoCanonico()
    {
        // Devolução de entrada em documento papel (01); CHV_DFE_ENTRADA vazio, SERIE+NUM preenchidos.
        const string sped =
            "|C186|1|PROD001|60|5201|00401|5,000000|UN|01|001|123456789||01012024|1|30,000000|4,500000|6,000000|7,200000|0,360000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComDocumentoEletronico_PreservaTextoCanonico()
    {
        // Devolução de entrada em NF-e (55); SERIE e NUM_DOC vazios, CHV_DFE preenchida.
        var sped =
            $"|C186|2|PROD002|60|5201|00401|3,000000|PC|55|||{ChaveNfeValida}|15032024|2|25,000000|3,750000|5,000000|6,000000|0,300000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposValoresUnitariosVazios_PreservaTextoCanonico()
    {
        // Campos identificadores preenchidos; todos os VL_UNIT opcionais vazios.
        const string sped =
            "|C186|1|PROD003|60|5201|00401|2,000000|KG|01|001|987654321||20062024|3||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
