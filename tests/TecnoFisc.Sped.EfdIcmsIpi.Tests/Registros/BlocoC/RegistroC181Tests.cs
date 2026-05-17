using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.064 — exercita a forma do <see cref="RegistroC181"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 93): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC181Tests
{
    // Chave NF-e válida (UF SP, Jan/2024, DV=8) reutilizada dos testes de ChaveAcesso.
    private const string ChaveNfeValida = "35240111222333000181550010000000011000000018";

    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC181).Assembly);

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
    public void Atributo_DeclaraC181_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC181).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C181");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC181Com20CamposNaOrdem()
    {
        _catalogo.TentarObter("C181".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C181");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodMotRestCompl", "QuantConv", "Unid", "CodModSaida",
            "SerieSaida", "EcfFabSaida", "NumDocSaida", "ChvDfeSaida",
            "DtDocSaida", "NumItemSaida",
            "VlUnitConvSaida", "VlUnitIcmsOpEstoqueConvSaida", "VlUnitIcmsStEstoqueConvSaida",
            "VlUnitFcpIcmsStEstoqueConvSaida", "VlUnitIcmsNaOperacaoConvSaida", "VlUnitIcmsOpConvSaida",
            "VlUnitIcmsStConvRest", "VlUnitFcpStConvRest",
            "VlUnitIcmsStConvCompl", "VlUnitFcpStConvCompl"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C181".AsSpan(), out var meta);
        var registro = (RegistroC181)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "00507".AsSpan());                                      // CodMotRestCompl
        meta.Campos[1].Definidor(registro, "10,000000".AsSpan());                                  // QuantConv
        meta.Campos[2].Definidor(registro, "UN".AsSpan());                                         // Unid
        meta.Campos[3].Definidor(registro, "55".AsSpan());                                         // CodModSaida
        meta.Campos[4].Definidor(registro, "001".AsSpan());                                        // SerieSaida
        meta.Campos[5].Definidor(registro, "SERIAL12345678901234".AsSpan());                       // EcfFabSaida
        meta.Campos[6].Definidor(registro, "12345678".AsSpan());                                   // NumDocSaida
        meta.Campos[7].Definidor(registro, ChaveNfeValida.AsSpan());                                   // ChvDfeSaida
        meta.Campos[8].Definidor(registro, "01012024".AsSpan());                                   // DtDocSaida
        meta.Campos[9].Definidor(registro, "1".AsSpan());                                          // NumItemSaida
        meta.Campos[10].Definidor(registro, "50,000000".AsSpan());                                 // VlUnitConvSaida
        meta.Campos[11].Definidor(registro, "5,000000".AsSpan());                                  // VlUnitIcmsOpEstoqueConvSaida
        meta.Campos[12].Definidor(registro, "7,200000".AsSpan());                                  // VlUnitIcmsStEstoqueConvSaida
        meta.Campos[13].Definidor(registro, "0,360000".AsSpan());                                  // VlUnitFcpIcmsStEstoqueConvSaida
        meta.Campos[14].Definidor(registro, "4,500000".AsSpan());                                  // VlUnitIcmsNaOperacaoConvSaida
        meta.Campos[15].Definidor(registro, "5,000000".AsSpan());                                  // VlUnitIcmsOpConvSaida
        meta.Campos[16].Definidor(registro, "6,840000".AsSpan());                                  // VlUnitIcmsStConvRest
        meta.Campos[17].Definidor(registro, "0,360000".AsSpan());                                  // VlUnitFcpStConvRest
        meta.Campos[18].Definidor(registro, "0,000000".AsSpan());                                  // VlUnitIcmsStConvCompl
        meta.Campos[19].Definidor(registro, "0,000000".AsSpan());                                  // VlUnitFcpStConvCompl

        registro.CodMotRestCompl.Should().Be("00507");
        registro.QuantConv.Should().Be(10.000000m);
        registro.Unid.Should().Be("UN");
        registro.CodModSaida.Should().Be("55");
        registro.SerieSaida.Should().Be("001");
        registro.EcfFabSaida.Should().Be("SERIAL12345678901234");
        registro.NumDocSaida.Should().Be(12345678);
        registro.ChvDfeSaida.Should().Be(ChaveAcesso.Criar(ChaveNfeValida.AsSpan()));
        registro.DtDocSaida.Should().Be(new DateOnly(2024, 1, 1));
        registro.NumItemSaida.Should().Be(1);
        registro.VlUnitConvSaida.Should().Be(50.000000m);
        registro.VlUnitIcmsOpEstoqueConvSaida.Should().Be(5.000000m);
        registro.VlUnitIcmsStEstoqueConvSaida.Should().Be(7.200000m);
        registro.VlUnitFcpIcmsStEstoqueConvSaida.Should().Be(0.360000m);
        registro.VlUnitIcmsNaOperacaoConvSaida.Should().Be(4.500000m);
        registro.VlUnitIcmsOpConvSaida.Should().Be(5.000000m);
        registro.VlUnitIcmsStConvRest.Should().Be(6.840000m);
        registro.VlUnitFcpStConvRest.Should().Be(0.360000m);
        registro.VlUnitIcmsStConvCompl.Should().Be(0.000000m);
        registro.VlUnitFcpStConvCompl.Should().Be(0.000000m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C181".AsSpan(), out var meta);
        var registro = (RegistroC181)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodMotRestCompl.Should().BeNull();
        registro.QuantConv.Should().BeNull();
        registro.Unid.Should().BeNull();
        registro.CodModSaida.Should().BeNull();
        registro.SerieSaida.Should().BeNull();
        registro.EcfFabSaida.Should().BeNull();
        registro.NumDocSaida.Should().BeNull();
        registro.ChvDfeSaida.Should().BeNull();
        registro.DtDocSaida.Should().BeNull();
        registro.NumItemSaida.Should().BeNull();
        registro.VlUnitConvSaida.Should().BeNull();
        registro.VlUnitIcmsOpEstoqueConvSaida.Should().BeNull();
        registro.VlUnitIcmsStEstoqueConvSaida.Should().BeNull();
        registro.VlUnitFcpIcmsStEstoqueConvSaida.Should().BeNull();
        registro.VlUnitIcmsNaOperacaoConvSaida.Should().BeNull();
        registro.VlUnitIcmsOpConvSaida.Should().BeNull();
        registro.VlUnitIcmsStConvRest.Should().BeNull();
        registro.VlUnitFcpStConvRest.Should().BeNull();
        registro.VlUnitIcmsStConvCompl.Should().BeNull();
        registro.VlUnitFcpStConvCompl.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // NF-e (cod. 55) — chave eletrônica preenchida, série/ECF/num_doc (campos 6-8) vazios.
        const string sped =
            "|C181|00507|10,000000|UN|55||||35240111222333000181550010000000011000000018|01012024|1|50,000000|5,000000|7,200000|0,360000|4,500000|5,000000|6,840000|0,360000|0,000000|0,000000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComDocumentoPapel_PreservaTextoCanonico()
    {
        // NF papel (cod. 01) — série e número preenchidos, chave eletrônica vazia.
        const string sped =
            "|C181|00505|5,000000|PC|01|001||12345678||01012024|2|30,000000|3,000000|4,320000||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposValoresUnitariosVazios_PreservaTextoCanonico()
    {
        // Somente campos obrigatórios preenchidos; todos os VL_UNIT opcionais (campos 11-21) vazios.
        const string sped =
            "|C181|00608|2,500000|UN|55||||35240111222333000181550010000000011000000018|05062023||||||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
