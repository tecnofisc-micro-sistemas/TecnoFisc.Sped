using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoD;

public sealed class RegistroD100Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD100).Assembly);

    // Chave de acesso válida reutilizada dos testes de ChaveAcesso (modelo 55, UF SP).
    private const string ChaveCteValida = "35240111222333000181550010000000011000000018";

    [Fact]
    public void Atributo_DeclaraCodigoD100_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD100).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D100");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD100Com22CamposNaOrdem()
    {
        _catalogo.TentarObter("D100".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D100");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "IndOper", "IndEmit", "CodPart", "CodMod", "CodSit", "Ser", "Sub", "NumDoc",
            "ChvCte", "DtDoc", "DtAP", "TpCTe", "ChvCteRef", "VlDoc", "VlDesc",
            "IndFrt", "VlServ", "VlBcIcms", "VlIcms", "VlNt", "CodInf", "CodCta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            Enumerable.Range(2, 22));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D100".AsSpan(), out var meta);
        var registro = (RegistroD100)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());                   // IndOper
        meta.Campos[1].Definidor(registro, "1".AsSpan());                   // IndEmit
        meta.Campos[2].Definidor(registro, "TRANSP001".AsSpan());           // CodPart
        meta.Campos[3].Definidor(registro, "57".AsSpan());                  // CodMod
        meta.Campos[4].Definidor(registro, "00".AsSpan());                  // CodSit
        meta.Campos[5].Definidor(registro, "001".AsSpan());                 // Ser
        meta.Campos[6].Definidor(registro, "001".AsSpan());                 // Sub
        meta.Campos[7].Definidor(registro, "000000001".AsSpan());           // NumDoc
        meta.Campos[8].Definidor(registro, ChaveCteValida.AsSpan());        // ChvCte
        meta.Campos[9].Definidor(registro, "01012021".AsSpan());            // DtDoc
        meta.Campos[10].Definidor(registro, "15012021".AsSpan());           // DtAP
        meta.Campos[11].Definidor(registro, "1".AsSpan());                  // TpCTe
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty);      // ChvCteRef
        meta.Campos[13].Definidor(registro, "1500,00".AsSpan());            // VlDoc
        meta.Campos[14].Definidor(registro, "50,00".AsSpan());              // VlDesc
        meta.Campos[15].Definidor(registro, "2".AsSpan());                  // IndFrt
        meta.Campos[16].Definidor(registro, "800,00".AsSpan());             // VlServ
        meta.Campos[17].Definidor(registro, "100,00".AsSpan());             // VlBcIcms
        meta.Campos[18].Definidor(registro, "50,00".AsSpan());              // VlIcms
        meta.Campos[19].Definidor(registro, "350,50".AsSpan());             // VlNt
        meta.Campos[20].Definidor(registro, "INF001".AsSpan());             // CodInf
        meta.Campos[21].Definidor(registro, "CONTA001".AsSpan());           // CodCta

        registro.IndOper.Should().Be(IndicadorOperacaoDocumento.Entrada);
        registro.IndEmit.Should().Be(IndicadorEmissaoDocumento.EmissaoPorTerceiros);
        registro.CodPart.Should().Be("TRANSP001");
        registro.CodMod.Should().Be("57");
        registro.CodSit.Should().Be(CodigoSituacaoDocumentoFiscal.DocumentoRegular);
        registro.Ser.Should().Be("001");
        registro.Sub.Should().Be("001");
        registro.NumDoc.Should().Be("000000001");
        registro.ChvCte.Should().NotBeNull();
        registro.ChvCte!.Value.ToString().Should().Be(ChaveCteValida);
        registro.DtDoc.Should().Be(new DateOnly(2021, 1, 1));
        registro.DtAP.Should().Be(new DateOnly(2021, 1, 15));
        registro.TpCTe.Should().Be(1);
        registro.ChvCteRef.Should().BeNull();
        registro.VlDoc.Should().Be(1500.00m);
        registro.VlDesc.Should().Be(50.00m);
        registro.IndFrt.Should().Be(IndicadorFrete.ContaTerceiros);
        registro.VlServ.Should().Be(800.00m);
        registro.VlBcIcms.Should().Be(100.00m);
        registro.VlIcms.Should().Be(50.00m);
        registro.VlNt.Should().Be(350.50m);
        registro.CodInf.Should().Be("INF001");
        registro.CodCta.Should().Be("CONTA001");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("D100".AsSpan(), out var meta);
        var registro = (RegistroD100)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);   // Ser
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);   // Sub
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);   // ChvCte
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty);  // DtAP
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty);  // TpCTe
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty);  // ChvCteRef
        meta.Campos[14].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlDesc
        meta.Campos[17].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlBcIcms
        meta.Campos[18].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlIcms
        meta.Campos[19].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlNt
        meta.Campos[20].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodInf
        meta.Campos[21].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCta

        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
        registro.ChvCte.Should().BeNull();
        registro.DtAP.Should().BeNull();
        registro.TpCTe.Should().BeNull();
        registro.ChvCteRef.Should().BeNull();
        registro.VlDesc.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.VlNt.Should().BeNull();
        registro.CodInf.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Theory]
    [InlineData(IndicadorOperacaoDocumento.Entrada, "0")]
    public void Serializar_IndOper_RetornaCodigoSpedCorreto(
        IndicadorOperacaoDocumento operacao, string esperado)
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
    public void Serializar_IndFrt_RetornaCodigoSpedCorreto(
        IndicadorFrete frete, string esperado)
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
            $"|D100|0|1|TRANSP001|57|00|001|001|000000001|{ChaveCteValida}" +
            "|01012021|15012021|1||1200,50|50,00|2|800,00|100,00|50,00|350,50|INF001|CONTA001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposObrigatoriosApenas_PreservaTextoCanonico()
    {
        // Conhecimento de Transporte Rodoviário (08), emissão própria, sem campos opcionais.
        const string sped =
            "|D100|0|0|TRANSP001|08|00|||000000001||01012021||||1500,00||0|1000,00||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_DocumentoCancelado_PreservaTextoCanonico()
    {
        // Conhecimento cancelado, emissão de terceiros, sem campos opcionais de valor.
        const string sped =
            "|D100|0|1|FORN001|57|02|||000000005||20062021||||0,00||9|0,00||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

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
}
