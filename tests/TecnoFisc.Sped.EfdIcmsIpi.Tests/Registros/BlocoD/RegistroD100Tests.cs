using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.115 — exercita a forma do <see cref="RegistroD100"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.2.2 (p. 171-175): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD100Tests
{
    // Chave CT-e válida (UF SP, Jan/2024, mod 57) reutilizada nos testes de round-trip.
    private const string ChaveCteValida = "35240111222333000181550010000000011000000018";

    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD100).Assembly);

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
    public void Atributo_DeclaraD100_Nivel2_BlocoD()
    {
        var atributo = typeof(RegistroD100).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D100");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD100Com24CamposNaOrdem()
    {
        _catalogo.TentarObter("D100".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D100");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndOper", "IndEmit", "CodPart", "CodMod", "CodSit",
            "Ser", "Sub", "NumDoc", "ChvCte", "DtDoc",
            "DtAP", "TpCTe", "ChvCteRef", "VlDoc", "VlDesc",
            "IndFrt", "VlServ", "VlBcIcms", "VlIcms", "VlNt",
            "CodInf", "CodCta", "CodMunOrig", "CodMunDest",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 24));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D100".AsSpan(), out var meta);
        var registro = (RegistroD100)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());                              // IndOper
        meta.Campos[1].Definidor(registro, "0".AsSpan());                              // IndEmit
        meta.Campos[2].Definidor(registro, "TRANS001".AsSpan());                       // CodPart
        meta.Campos[3].Definidor(registro, "57".AsSpan());                             // CodMod
        meta.Campos[4].Definidor(registro, "00".AsSpan());                             // CodSit
        meta.Campos[5].Definidor(registro, "001".AsSpan());                            // Ser
        meta.Campos[6].Definidor(registro, "A".AsSpan());                              // Sub
        meta.Campos[7].Definidor(registro, "123".AsSpan());                            // NumDoc
        meta.Campos[8].Definidor(registro, ChaveCteValida.AsSpan());                   // ChvCte
        meta.Campos[9].Definidor(registro, "01012024".AsSpan());                       // DtDoc
        meta.Campos[10].Definidor(registro, "01012024".AsSpan());                      // DtAP
        meta.Campos[11].Definidor(registro, "0".AsSpan());                             // TpCTe
        meta.Campos[12].Definidor(registro, ChaveCteValida.AsSpan());                  // ChvCteRef
        meta.Campos[13].Definidor(registro, "5000.00".AsSpan());                       // VlDoc
        meta.Campos[14].Definidor(registro, "50.00".AsSpan());                         // VlDesc
        meta.Campos[15].Definidor(registro, "0".AsSpan());                             // IndFrt
        meta.Campos[16].Definidor(registro, "4500.00".AsSpan());                       // VlServ
        meta.Campos[17].Definidor(registro, "900.00".AsSpan());                        // VlBcIcms
        meta.Campos[18].Definidor(registro, "90.00".AsSpan());                         // VlIcms
        meta.Campos[19].Definidor(registro, "500.00".AsSpan());                        // VlNt
        meta.Campos[20].Definidor(registro, "OBS001".AsSpan());                        // CodInf
        meta.Campos[21].Definidor(registro, "1001-00".AsSpan());                       // CodCta
        meta.Campos[22].Definidor(registro, "1234567".AsSpan());                       // CodMunOrig
        meta.Campos[23].Definidor(registro, "7654321".AsSpan());                       // CodMunDest

        registro.IndOper.Should().Be(IndicadorOperacao.Entrada);
        registro.IndEmit.Should().Be(IndicadorEmissorDocumento.EmissaoPropria);
        registro.CodPart.Should().Be("TRANS001");
        registro.CodMod.Should().Be("57");
        registro.CodSit.Should().Be(CodigoSituacaoDocumentoFiscal.DocumentoRegular);
        registro.Ser.Should().Be("001");
        registro.Sub.Should().Be("A");
        registro.NumDoc.Should().Be(123);
        registro.ChvCte.Should().Be(ChaveAcesso.Create(ChaveCteValida));
        registro.DtDoc.Should().Be(new DateOnly(2024, 1, 1));
        registro.DtAP.Should().Be(new DateOnly(2024, 1, 1));
        registro.TpCTe.Should().Be(0);
        registro.ChvCteRef.Should().Be(ChaveAcesso.Create(ChaveCteValida));
        registro.VlDoc.Should().Be(5000.00m);
        registro.VlDesc.Should().Be(50.00m);
        registro.IndFrt.Should().Be(IndicadorFrete.ContaRemetenteCif);
        registro.VlServ.Should().Be(4500.00m);
        registro.VlBcIcms.Should().Be(900.00m);
        registro.VlIcms.Should().Be(90.00m);
        registro.VlNt.Should().Be(500.00m);
        registro.CodInf.Should().Be("OBS001");
        registro.CodCta.Should().Be("1001-00");
        registro.CodMunOrig.Should().Be(1234567);
        registro.CodMunDest.Should().Be(7654321);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D100".AsSpan(), out var meta);
        var registro = (RegistroD100)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, Span<char>.Empty);   // CodPart
        registro.CodPart.Should().BeNull();

        meta.Campos[8].Definidor(registro, Span<char>.Empty);   // ChvCte
        registro.ChvCte.Should().BeNull();

        meta.Campos[10].Definidor(registro, Span<char>.Empty);  // DtAP
        registro.DtAP.Should().BeNull();

        meta.Campos[14].Definidor(registro, Span<char>.Empty);  // VlDesc
        registro.VlDesc.Should().BeNull();

        meta.Campos[15].Definidor(registro, Span<char>.Empty);  // IndFrt
        registro.IndFrt.Should().BeNull();
    }

    [Theory]
    [InlineData(IndicadorOperacao.Entrada, "0")]
    [InlineData(IndicadorOperacao.Saida, "1")]
    public void Serializar_IndOper_RetornaCodigo(IndicadorOperacao operacao, string esperado)
    {
        _catalogo.TentarObter("D100".AsSpan(), out var meta);
        var registro = (RegistroD100)meta!.Fabrica();
        registro.IndOper = operacao;

        meta.Campos[0].Serializar(registro).Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorFrete.ContaRemetenteCif, "0")]
    [InlineData(IndicadorFrete.ContaDestinatarioFob, "1")]
    [InlineData(IndicadorFrete.ContaTerceiros, "2")]
    [InlineData(IndicadorFrete.SemTransporte, "9")]
    public void Serializar_IndFrt_RetornaCodigo(IndicadorFrete frete, string esperado)
    {
        _catalogo.TentarObter("D100".AsSpan(), out var meta);
        var registro = (RegistroD100)meta!.Fabrica();
        registro.IndFrt = frete;

        meta.Campos[15].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        var sped =
            $"|D100|0|0|TRANS001|57|00|001|A|123|{ChaveCteValida}|01012024|01012024|0|{ChaveCteValida}|5000,00|50,00|0|4500,00|900,00|90,00|500,00|OBS001|1001-00|1234567|7654321|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_NfServicoTransporteSemCte_PreservaTextoCanonico()
    {
        // NF Serviço de Transporte (07), prestação própria, sem chave eletrônica, campos opcionais vazios.
        const string sped =
            "|D100|1|0|TRANS001|07|00|001||654321||01032024|01032024|||5000,00||9|4500,00||||||1234567|7654321|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_AquisicaoCteTerceiros_PreservaTextoCanonico()
    {
        // CT-e (57) de terceiros, aquisição, com ICMS, sem chave substituta.
        var sped =
            $"|D100|0|1|TRANS002|57|00|001||987654|{ChaveCteValida}|15062024|15062024|||8000,00||0|8000,00|1440,00|144,00||||3550308|3550308|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
