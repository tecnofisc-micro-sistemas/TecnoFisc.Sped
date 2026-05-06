using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC100Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC100).Assembly);

    // Chave NF-e válida reutilizada dos testes de ChaveAcesso.
    private const string ChaveNfeValida = "35240111222333000181550010000000011000000018";

    [Fact]
    public void Atributo_DeclaraCodigoC100_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC100).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C100");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC100Com28CamposNaOrdem()
    {
        _catalogo.TentarObter("C100".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C100");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "IndOper", "IndEmit", "CodPart", "CodMod", "CodSit", "Ser", "NumDoc", "ChvNfe",
            "DtDoc", "DtES", "VlDoc", "IndPgto", "VlDesc", "VlAbatNt", "VlMerc", "IndFrt",
            "VlFrt", "VlSeg", "VlOutDa", "VlBcIcms", "VlIcms", "VlBcIcmsSt", "VlIcmsSt",
            "VlIpi", "VlPis", "VlCofins", "VlPisSt", "VlCofinsSt",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            Enumerable.Range(2, 28));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C100".AsSpan(), out var meta);
        var registro = (RegistroC100)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());                    // IndOper
        meta.Campos[1].Definidor(registro, "1".AsSpan());                    // IndEmit
        meta.Campos[2].Definidor(registro, "FORNEC001".AsSpan());            // CodPart
        meta.Campos[3].Definidor(registro, "01".AsSpan());                   // CodMod
        meta.Campos[4].Definidor(registro, "00".AsSpan());                   // CodSit
        meta.Campos[5].Definidor(registro, "001".AsSpan());                  // Ser
        meta.Campos[6].Definidor(registro, "000000001".AsSpan());            // NumDoc
        meta.Campos[7].Definidor(registro, ChaveNfeValida.AsSpan());         // ChvNfe
        meta.Campos[8].Definidor(registro, "01012025".AsSpan());             // DtDoc
        meta.Campos[9].Definidor(registro, "02012025".AsSpan());             // DtES
        meta.Campos[10].Definidor(registro, "1500,00".AsSpan());             // VlDoc
        meta.Campos[11].Definidor(registro, "0".AsSpan());                   // IndPgto
        meta.Campos[12].Definidor(registro, "50,00".AsSpan());               // VlDesc
        meta.Campos[13].Definidor(registro, "0,00".AsSpan());                // VlAbatNt
        meta.Campos[14].Definidor(registro, "1450,00".AsSpan());             // VlMerc
        meta.Campos[15].Definidor(registro, "9".AsSpan());                   // IndFrt
        meta.Campos[16].Definidor(registro, "0,00".AsSpan());                // VlFrt
        meta.Campos[17].Definidor(registro, "0,00".AsSpan());                // VlSeg
        meta.Campos[18].Definidor(registro, "0,00".AsSpan());                // VlOutDa
        meta.Campos[19].Definidor(registro, "1000,00".AsSpan());             // VlBcIcms
        meta.Campos[20].Definidor(registro, "150,00".AsSpan());              // VlIcms
        meta.Campos[21].Definidor(registro, "0,00".AsSpan());                // VlBcIcmsSt
        meta.Campos[22].Definidor(registro, "0,00".AsSpan());                // VlIcmsSt
        meta.Campos[23].Definidor(registro, "0,00".AsSpan());                // VlIpi
        meta.Campos[24].Definidor(registro, "10,50".AsSpan());               // VlPis
        meta.Campos[25].Definidor(registro, "70,00".AsSpan());               // VlCofins
        meta.Campos[26].Definidor(registro, "0,00".AsSpan());                // VlPisSt
        meta.Campos[27].Definidor(registro, "0,00".AsSpan());                // VlCofinsSt

        registro.IndOper.Should().Be(IndicadorOperacaoDocumento.Entrada);
        registro.IndEmit.Should().Be(IndicadorEmissaoDocumento.EmissaoPorTerceiros);
        registro.CodPart.Should().Be("FORNEC001");
        registro.CodMod.Should().Be("01");
        registro.CodSit.Should().Be(CodigoSituacaoDocumentoFiscal.DocumentoRegular);
        registro.Ser.Should().Be("001");
        registro.NumDoc.Should().Be("000000001");
        registro.ChvNfe.Should().NotBeNull();
        registro.ChvNfe!.Value.ToString().Should().Be(ChaveNfeValida);
        registro.DtDoc.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtES.Should().Be(new DateOnly(2025, 1, 2));
        registro.VlDoc.Should().Be(1500.00m);
        registro.IndPgto.Should().Be(IndicadorPagamento.AVista);
        registro.VlDesc.Should().Be(50.00m);
        registro.VlAbatNt.Should().Be(0.00m);
        registro.VlMerc.Should().Be(1450.00m);
        registro.IndFrt.Should().Be(IndicadorFrete.SemTransporte);
        registro.VlFrt.Should().Be(0.00m);
        registro.VlSeg.Should().Be(0.00m);
        registro.VlOutDa.Should().Be(0.00m);
        registro.VlBcIcms.Should().Be(1000.00m);
        registro.VlIcms.Should().Be(150.00m);
        registro.VlBcIcmsSt.Should().Be(0.00m);
        registro.VlIcmsSt.Should().Be(0.00m);
        registro.VlIpi.Should().Be(0.00m);
        registro.VlPis.Should().Be(10.50m);
        registro.VlCofins.Should().Be(70.00m);
        registro.VlPisSt.Should().Be(0.00m);
        registro.VlCofinsSt.Should().Be(0.00m);
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("C100".AsSpan(), out var meta);
        var registro = (RegistroC100)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodPart
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);  // Ser
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);  // ChvNfe
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);  // DtES
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty); // VlDesc
        meta.Campos[13].Definidor(registro, ReadOnlySpan<char>.Empty); // VlAbatNt
        meta.Campos[14].Definidor(registro, ReadOnlySpan<char>.Empty); // VlMerc
        meta.Campos[16].Definidor(registro, ReadOnlySpan<char>.Empty); // VlFrt
        meta.Campos[17].Definidor(registro, ReadOnlySpan<char>.Empty); // VlSeg
        meta.Campos[18].Definidor(registro, ReadOnlySpan<char>.Empty); // VlOutDa
        meta.Campos[19].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcIcms
        meta.Campos[20].Definidor(registro, ReadOnlySpan<char>.Empty); // VlIcms
        meta.Campos[21].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcIcmsSt
        meta.Campos[22].Definidor(registro, ReadOnlySpan<char>.Empty); // VlIcmsSt
        meta.Campos[23].Definidor(registro, ReadOnlySpan<char>.Empty); // VlIpi
        meta.Campos[24].Definidor(registro, ReadOnlySpan<char>.Empty); // VlPis
        meta.Campos[25].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCofins
        meta.Campos[26].Definidor(registro, ReadOnlySpan<char>.Empty); // VlPisSt
        meta.Campos[27].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCofinsSt

        registro.CodPart.Should().BeNull();
        registro.Ser.Should().BeNull();
        registro.ChvNfe.Should().BeNull();
        registro.DtES.Should().BeNull();
        registro.VlDesc.Should().BeNull();
        registro.VlAbatNt.Should().BeNull();
        registro.VlMerc.Should().BeNull();
        registro.VlFrt.Should().BeNull();
        registro.VlSeg.Should().BeNull();
        registro.VlOutDa.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.VlBcIcmsSt.Should().BeNull();
        registro.VlIcmsSt.Should().BeNull();
        registro.VlIpi.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.VlPisSt.Should().BeNull();
        registro.VlCofinsSt.Should().BeNull();
    }

    [Theory]
    [InlineData(IndicadorOperacaoDocumento.Entrada, "0")]
    [InlineData(IndicadorOperacaoDocumento.Saida, "1")]
    public void Serializar_IndOper_RetornaCodigoSpedCorreto(
        IndicadorOperacaoDocumento operacao, string esperado)
    {
        _catalogo.TentarObter("C100".AsSpan(), out var meta);
        var registro = (RegistroC100)meta!.Fabrica();
        registro.IndOper = operacao;

        meta.Campos[0].Serializar(registro).Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorFrete.ContaRemetenteCif, "0")]
    [InlineData(IndicadorFrete.ContaDestinatarioFob, "1")]
    [InlineData(IndicadorFrete.ContaTerceiros, "2")]
    [InlineData(IndicadorFrete.TransporteProprioRemetente, "3")]
    [InlineData(IndicadorFrete.TransporteProprioDestinatario, "4")]
    [InlineData(IndicadorFrete.SemTransporte, "9")]
    public void Serializar_IndFrt_RetornaCodigoSpedCorreto(
        IndicadorFrete frete, string esperado)
    {
        _catalogo.TentarObter("C100".AsSpan(), out var meta);
        var registro = (RegistroC100)meta!.Fabrica();
        registro.IndFrt = frete;

        meta.Campos[15].Serializar(registro).Should().Be(esperado);
    }

    [Theory]
    [InlineData(CodigoSituacaoDocumentoFiscal.DocumentoRegular, "00")]
    [InlineData(CodigoSituacaoDocumentoFiscal.DocumentoRegularExtemporaneo, "01")]
    [InlineData(CodigoSituacaoDocumentoFiscal.DocumentoCancelado, "02")]
    [InlineData(CodigoSituacaoDocumentoFiscal.NfeDenegada, "04")]
    [InlineData(CodigoSituacaoDocumentoFiscal.NumeracaoInutilizada, "05")]
    [InlineData(CodigoSituacaoDocumentoFiscal.RegimeEspecial, "08")]
    public void Serializar_CodSit_RetornaCodigoSpedComDoisDigitos(
        CodigoSituacaoDocumentoFiscal situacao, string esperado)
    {
        _catalogo.TentarObter("C100".AsSpan(), out var meta);
        var registro = (RegistroC100)meta!.Fabrica();
        registro.CodSit = situacao;

        meta.Campos[4].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Nota Fiscal modelo 01 com todos os campos preenchidos (inclusive ChaveAcesso para NF-e).
        var sped =
            $"|C100|0|1|FORNEC001|01|00|001|000000001|{ChaveNfeValida}|01012025|02012025" +
            "|1500,00|0|50,00|0,00|1450,00|9|0,00|0,00|0,00|1000,00|150,00|0,00|0,00|0,00|10,50|70,00|0,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposObrigatoriosApenas_PreservaTextoCanonico()
    {
        // Nota Fiscal modelo 01, sem ChaveAcesso e sem campos opcionais.
        const string sped =
            "|C100|0|1|FORNEC001|01|00||000000001||01012025||1500,00|0||||9|||||||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_DocumentoCancelado_PreservaTextoCanonico()
    {
        // NFC-e cancelada (CodSit=02), saída de emissão própria, sem participante.
        const string sped =
            "|C100|1|0||65|02||000000001||01012025||0,00|9||||9|||||||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}
