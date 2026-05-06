using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoA;

public sealed class RegistroA100Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroA100).Assembly);

    [Fact]
    public void Atributo_DeclaraCodigoA100_Nivel3_BlocoA()
    {
        var atributo = typeof(RegistroA100).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("A100");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("A");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroA100Com20CamposNaOrdem()
    {
        _catalogo.TentarObter("A100".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("A100");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "IndOper", "IndEmit", "CodPart", "CodSit", "Ser", "Sub", "NumDoc",
            "ChvNfse", "DtDoc", "DtExeServ", "VlDoc", "IndPgto",
            "VlDesc", "VlBcPis", "VlPis", "VlBcCofins", "VlCofins",
            "VlPisRet", "VlCofinsRet", "VlIss",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("A100".AsSpan(), out var meta);
        var registro = (RegistroA100)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());   // IndOper
        meta.Campos[1].Definidor(registro, "0".AsSpan());   // IndEmit
        meta.Campos[2].Definidor(registro, "PART01".AsSpan()); // CodPart
        meta.Campos[3].Definidor(registro, "00".AsSpan());  // CodSit
        meta.Campos[4].Definidor(registro, "001".AsSpan()); // Ser
        meta.Campos[5].Definidor(registro, "A".AsSpan());   // Sub
        meta.Campos[6].Definidor(registro, "NF-001".AsSpan()); // NumDoc
        meta.Campos[7].Definidor(registro, "CHAVE123".AsSpan()); // ChvNfse
        meta.Campos[8].Definidor(registro, "01012025".AsSpan()); // DtDoc
        meta.Campos[9].Definidor(registro, "01012025".AsSpan()); // DtExeServ
        meta.Campos[10].Definidor(registro, "1500.00".AsSpan()); // VlDoc
        meta.Campos[11].Definidor(registro, "0".AsSpan());  // IndPgto
        meta.Campos[12].Definidor(registro, "50.00".AsSpan()); // VlDesc
        meta.Campos[13].Definidor(registro, "1450.00".AsSpan()); // VlBcPis
        meta.Campos[14].Definidor(registro, "50.00".AsSpan()); // VlPis
        meta.Campos[15].Definidor(registro, "1450.00".AsSpan()); // VlBcCofins
        meta.Campos[16].Definidor(registro, "100.00".AsSpan()); // VlCofins
        meta.Campos[17].Definidor(registro, "10.00".AsSpan()); // VlPisRet
        meta.Campos[18].Definidor(registro, "46.00".AsSpan()); // VlCofinsRet
        meta.Campos[19].Definidor(registro, "200.00".AsSpan()); // VlIss

        registro.IndOper.Should().Be(IndicadorOperacaoServico.ServicoPrestado);
        registro.IndEmit.Should().Be(IndicadorEmissaoDocumento.EmissaoPropria);
        registro.CodPart.Should().Be("PART01");
        registro.CodSit.Should().Be(CodigoSituacaoDocumentoFiscal.DocumentoRegular);
        registro.Ser.Should().Be("001");
        registro.Sub.Should().Be("A");
        registro.NumDoc.Should().Be("NF-001");
        registro.ChvNfse.Should().Be("CHAVE123");
        registro.DtDoc.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtExeServ.Should().Be(new DateOnly(2025, 1, 1));
        registro.VlDoc.Should().Be(1500.00m);
        registro.IndPgto.Should().Be(IndicadorPagamento.AVista);
        registro.VlDesc.Should().Be(50.00m);
        registro.VlBcPis.Should().Be(1450.00m);
        registro.VlPis.Should().Be(50.00m);
        registro.VlBcCofins.Should().Be(1450.00m);
        registro.VlCofins.Should().Be(100.00m);
        registro.VlPisRet.Should().Be(10.00m);
        registro.VlCofinsRet.Should().Be(46.00m);
        registro.VlIss.Should().Be(200.00m);
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("A100".AsSpan(), out var meta);
        var registro = (RegistroA100)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodPart
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);  // Ser
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);  // Sub
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);  // ChvNfse
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty); // VlDesc
        meta.Campos[13].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcPis
        meta.Campos[17].Definidor(registro, ReadOnlySpan<char>.Empty); // VlPisRet
        meta.Campos[18].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCofinsRet
        meta.Campos[19].Definidor(registro, ReadOnlySpan<char>.Empty); // VlIss

        registro.CodPart.Should().BeNull();
        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
        registro.ChvNfse.Should().BeNull();
        registro.VlDesc.Should().BeNull();
        registro.VlBcPis.Should().BeNull();
        registro.VlPisRet.Should().BeNull();
        registro.VlCofinsRet.Should().BeNull();
        registro.VlIss.Should().BeNull();
    }

    [Theory]
    [InlineData(CodigoSituacaoDocumentoFiscal.DocumentoRegular, "00")]
    [InlineData(CodigoSituacaoDocumentoFiscal.DocumentoCancelado, "02")]
    public void Serializar_CodSit_RetornaCodigoSpedComDoisDigitos(
        CodigoSituacaoDocumentoFiscal situacao, string esperado)
    {
        _catalogo.TentarObter("A100".AsSpan(), out var meta);
        var registro = (RegistroA100)meta!.Fabrica();
        registro.CodSit = situacao;

        meta.Campos[3].Serializar(registro).Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorPagamento.AVista, "0")]
    [InlineData(IndicadorPagamento.APrazo, "1")]
    [InlineData(IndicadorPagamento.SemPagamento, "9")]
    public void Serializar_IndPgto_RetornaCodigoSpedCorreto(
        IndicadorPagamento pagamento, string esperado)
    {
        _catalogo.TentarObter("A100".AsSpan(), out var meta);
        var registro = (RegistroA100)meta!.Fabrica();
        registro.IndPgto = pagamento;

        meta.Campos[11].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|A100|1|0||00|001||NF-001||01012025|01012025|1500,00|0|||50,00|1450,00|100,00||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_DocumentoCancelado_PreservaTextoCanonico()
    {
        const string sped =
            "|A100|0|1|PART01|02|||||01012025|01012025|0,00|9|||0,00|0,00|0,00||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

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
}
