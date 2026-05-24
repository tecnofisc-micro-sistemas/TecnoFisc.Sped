using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.037 — exercita a forma do <see cref="RegistroC100"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 59-64): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC100Tests
{
    // Chave NF-e válida (UF SP, Jan/2024, DV=8) reutilizada dos testes de ChaveAcesso.
    private const string ChaveNfeValida = "35240111222333000181550010000000011000000018";

    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC100).Assembly);

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
    public void Atributo_DeclaraC100_Nivel2_BlocoC()
    {
        var atributo = typeof(RegistroC100).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C100");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC100Com28CamposNaOrdem()
    {
        _catalogo.TentarObter("C100".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C100");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndOper", "IndEmit", "CodPart", "CodMod", "CodSit",
            "Ser", "NumDoc", "ChvNfe", "DtDoc", "DtES",
            "VlDoc", "IndPgto", "VlDesc", "VlAbatNt", "VlMerc",
            "IndFrt", "VlFrt", "VlSeg", "VlOutDa", "VlBcIcms",
            "VlIcms", "VlBcIcmsSt", "VlIcmsSt", "VlIpi", "VlPis",
            "VlCofins", "VlPisSt", "VlCofinsSt",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 28));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C100".AsSpan(), out var meta);
        var registro = (RegistroC100)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());                              // IndOper
        meta.Campos[1].Definidor(registro, "0".AsSpan());                              // IndEmit
        meta.Campos[2].Definidor(registro, "PART001".AsSpan());                        // CodPart
        meta.Campos[3].Definidor(registro, "55".AsSpan());                             // CodMod
        meta.Campos[4].Definidor(registro, "00".AsSpan());                             // CodSit
        meta.Campos[5].Definidor(registro, "001".AsSpan());                            // Ser
        meta.Campos[6].Definidor(registro, "1".AsSpan());                              // NumDoc
        meta.Campos[7].Definidor(registro, ChaveNfeValida.AsSpan());                   // ChvNfe
        meta.Campos[8].Definidor(registro, "01012024".AsSpan());                       // DtDoc
        meta.Campos[9].Definidor(registro, "02012024".AsSpan());                       // DtES
        meta.Campos[10].Definidor(registro, "1500.00".AsSpan());                       // VlDoc
        meta.Campos[11].Definidor(registro, "0".AsSpan());                             // IndPgto
        meta.Campos[12].Definidor(registro, "50.00".AsSpan());                         // VlDesc
        meta.Campos[13].Definidor(registro, "0.00".AsSpan());                          // VlAbatNt
        meta.Campos[14].Definidor(registro, "1450.00".AsSpan());                       // VlMerc
        meta.Campos[15].Definidor(registro, "0".AsSpan());                             // IndFrt
        meta.Campos[16].Definidor(registro, "100.00".AsSpan());                        // VlFrt
        meta.Campos[17].Definidor(registro, "50.00".AsSpan());                         // VlSeg
        meta.Campos[18].Definidor(registro, "25.00".AsSpan());                         // VlOutDa
        meta.Campos[19].Definidor(registro, "1200.00".AsSpan());                       // VlBcIcms
        meta.Campos[20].Definidor(registro, "216.00".AsSpan());                        // VlIcms
        meta.Campos[21].Definidor(registro, "0.00".AsSpan());                          // VlBcIcmsSt
        meta.Campos[22].Definidor(registro, "0.00".AsSpan());                          // VlIcmsSt
        meta.Campos[23].Definidor(registro, "0.00".AsSpan());                          // VlIpi
        meta.Campos[24].Definidor(registro, "0.00".AsSpan());                          // VlPis
        meta.Campos[25].Definidor(registro, "0.00".AsSpan());                          // VlCofins
        meta.Campos[26].Definidor(registro, "0.00".AsSpan());                          // VlPisSt
        meta.Campos[27].Definidor(registro, "0.00".AsSpan());                          // VlCofinsSt

        registro.IndOper.Should().Be(IndicadorOperacao.Entrada);
        registro.IndEmit.Should().Be(IndicadorEmissorDocumento.EmissaoPropria);
        registro.CodPart.Should().Be("PART001");
        registro.CodMod.Should().Be("55");
        registro.CodSit.Should().Be(CodigoSituacaoDocumentoFiscal.DocumentoRegular);
        registro.Ser.Should().Be("001");
        registro.NumDoc.Should().Be(1);
        registro.ChvNfe.Should().Be(ChaveAcesso.Create(ChaveNfeValida));
        registro.DtDoc.Should().Be(new DateOnly(2024, 1, 1));
        registro.DtES.Should().Be(new DateOnly(2024, 1, 2));
        registro.VlDoc.Should().Be(1500.00m);
        registro.IndPgto.Should().Be(IndicadorPagamento.AVista);
        registro.VlDesc.Should().Be(50.00m);
        registro.VlAbatNt.Should().Be(0.00m);
        registro.VlMerc.Should().Be(1450.00m);
        registro.IndFrt.Should().Be(IndicadorFrete.ContaRemetenteCif);
        registro.VlFrt.Should().Be(100.00m);
        registro.VlSeg.Should().Be(50.00m);
        registro.VlOutDa.Should().Be(25.00m);
        registro.VlBcIcms.Should().Be(1200.00m);
        registro.VlIcms.Should().Be(216.00m);
        registro.VlBcIcmsSt.Should().Be(0.00m);
        registro.VlIcmsSt.Should().Be(0.00m);
        registro.VlIpi.Should().Be(0.00m);
        registro.VlPis.Should().Be(0.00m);
        registro.VlCofins.Should().Be(0.00m);
        registro.VlPisSt.Should().Be(0.00m);
        registro.VlCofinsSt.Should().Be(0.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C100".AsSpan(), out var meta);
        var registro = (RegistroC100)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, Span<char>.Empty); // CodPart
        registro.CodPart.Should().BeNull();

        meta.Campos[5].Definidor(registro, Span<char>.Empty); // Ser
        registro.Ser.Should().BeNull();

        meta.Campos[7].Definidor(registro, Span<char>.Empty); // ChvNfe
        registro.ChvNfe.Should().BeNull();

        meta.Campos[9].Definidor(registro, Span<char>.Empty); // DtES
        registro.DtES.Should().BeNull();

        meta.Campos[12].Definidor(registro, Span<char>.Empty); // VlDesc
        registro.VlDesc.Should().BeNull();
    }

    [Theory]
    [InlineData(IndicadorOperacao.Entrada, "0")]
    [InlineData(IndicadorOperacao.Saida, "1")]
    public void Serializar_IndOper_RetornaCodigo(IndicadorOperacao operacao, string esperado)
    {
        _catalogo.TentarObter("C100".AsSpan(), out var meta);
        var registro = (RegistroC100)meta!.Fabrica();
        registro.IndOper = operacao;

        meta.Campos[0].Serializar(registro).Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorPagamento.AVista, "0")]
    [InlineData(IndicadorPagamento.APrazo, "1")]
    [InlineData(IndicadorPagamento.Outros, "2")]
    public void Serializar_IndPgto_RetornaCodigo(IndicadorPagamento pagamento, string esperado)
    {
        _catalogo.TentarObter("C100".AsSpan(), out var meta);
        var registro = (RegistroC100)meta!.Fabrica();
        registro.IndPgto = pagamento;

        meta.Campos[11].Serializar(registro).Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorFrete.ContaRemetenteCif, "0")]
    [InlineData(IndicadorFrete.ContaDestinatarioFob, "1")]
    [InlineData(IndicadorFrete.ContaTerceiros, "2")]
    [InlineData(IndicadorFrete.TransporteProprioRemetente, "3")]
    [InlineData(IndicadorFrete.TransporteProprioDestinatario, "4")]
    [InlineData(IndicadorFrete.SemTransporte, "9")]
    public void Serializar_IndFrt_RetornaCodigo(IndicadorFrete frete, string esperado)
    {
        _catalogo.TentarObter("C100".AsSpan(), out var meta);
        var registro = (RegistroC100)meta!.Fabrica();
        registro.IndFrt = frete;

        meta.Campos[15].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        var sped =
            $"|C100|0|0|PART001|55|00|001|1|{ChaveNfeValida}|01012024|02012024|1500,00|0|50,00|0,00|1450,00|0|100,00|50,00|25,00|1200,00|216,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_EntradaNfPapelSemChave_PreservaTextoCanonico()
    {
        // NF papel (01) de terceiro, sem série, sem chave, sem campos opcionais de valor.
        const string sped =
            "|C100|0|1|FORNEC001|01|00||12345||15032024||1200,00|1|||200,00|9|||||||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SaidaNfeComIcmsSt_PreservaTextoCanonico()
    {
        // NF-e (55) de saída própria com ICMS ST e sem PIS/COFINS (contribuinte EFD-Contribuições).
        var sped =
            $"|C100|1|0|CLIENTE01|55|00|001|9999|{ChaveNfeValida}|20062024|20062024|5800,00|0||0,00|5800,00|0|0,00|0,00|0,00|1044,00|104,40|580,00|58,00|0,00|||0,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
