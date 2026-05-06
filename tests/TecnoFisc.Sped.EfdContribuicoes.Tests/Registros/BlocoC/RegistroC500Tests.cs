using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC500Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC500).Assembly);

    // Chave NF-e válida reutilizada dos testes de ChaveAcesso.
    private const string ChaveValida = "35240111222333000181550010000000011000000018";

    [Fact]
    public void Atributo_DeclaraC500_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC500).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C500");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC500ComQuinzeCamposNaOrdem()
    {
        _catalogo.TentarObter("C500".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C500");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodPart", "CodMod", "CodSit", "Ser", "Sub",
            "NumDoc", "DtDoc", "DtEnt", "VlDoc", "VlIcms",
            "CodInf", "VlPis", "VlCofins", "ChvDoce",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
        meta.Campos[0].Tamanho.Should().Be(60);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // CodPart
        meta.Campos[1].Tamanho.Should().Be(2);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // CodMod
        meta.Campos[2].Tamanho.Should().Be(2);
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // CodSit
        meta.Campos[3].Obrigatorio.Should().BeFalse();  // Ser
        meta.Campos[4].Obrigatorio.Should().BeFalse();  // Sub
        meta.Campos[5].Tamanho.Should().Be(9);
        meta.Campos[5].Obrigatorio.Should().BeTrue();   // NumDoc
        meta.Campos[6].Tamanho.Should().Be(8);
        meta.Campos[6].Obrigatorio.Should().BeTrue();   // DtDoc
        meta.Campos[8].Obrigatorio.Should().BeTrue();   // VlDoc
        meta.Campos[13].Tamanho.Should().Be(44);        // ChvDoce
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C500".AsSpan(), out var meta);
        var registro = (RegistroC500)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "FORNECEDOR001".AsSpan());    // CodPart
        meta.Campos[1].Definidor(registro, "06".AsSpan());               // CodMod
        meta.Campos[2].Definidor(registro, "00".AsSpan());               // CodSit
        meta.Campos[3].Definidor(registro, "A".AsSpan());                // Ser
        meta.Campos[4].Definidor(registro, "1".AsSpan());                // Sub
        meta.Campos[5].Definidor(registro, "000000001".AsSpan());        // NumDoc
        meta.Campos[6].Definidor(registro, "01012024".AsSpan());         // DtDoc
        meta.Campos[7].Definidor(registro, "02012024".AsSpan());         // DtEnt
        meta.Campos[8].Definidor(registro, "1500,00".AsSpan());          // VlDoc
        meta.Campos[9].Definidor(registro, "250,00".AsSpan());           // VlIcms
        meta.Campos[10].Definidor(registro, "001234".AsSpan());          // CodInf
        meta.Campos[11].Definidor(registro, "20,00".AsSpan());           // VlPis
        meta.Campos[12].Definidor(registro, "90,00".AsSpan());           // VlCofins
        meta.Campos[13].Definidor(registro, ChaveValida.AsSpan());       // ChvDoce

        registro.CodPart.Should().Be("FORNECEDOR001");
        registro.CodMod.Should().Be("06");
        registro.CodSit.Should().Be(CodigoSituacaoDocumentoFiscal.DocumentoRegular);
        registro.Ser.Should().Be("A");
        registro.Sub.Should().Be(1);
        registro.NumDoc.Should().Be("000000001");
        registro.DtDoc.Should().Be(new DateOnly(2024, 1, 1));
        registro.DtEnt.Should().Be(new DateOnly(2024, 1, 2));
        registro.VlDoc.Should().Be(1500.00m);
        registro.VlIcms.Should().Be(250.00m);
        registro.CodInf.Should().Be("001234");
        registro.VlPis.Should().Be(20.00m);
        registro.VlCofins.Should().Be(90.00m);
        registro.ChvDoce.Should().NotBeNull();
        registro.ChvDoce!.Value.ToString().Should().Be(ChaveValida);
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("C500".AsSpan(), out var meta);
        var registro = (RegistroC500)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);   // Ser
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);   // Sub
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);   // DtEnt
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);   // VlIcms
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodInf
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlPis
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlCofins
        meta.Campos[13].Definidor(registro, ReadOnlySpan<char>.Empty);  // ChvDoce

        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
        registro.DtEnt.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.CodInf.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.ChvDoce.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        var sped =
            $"|C500|FORNECEDOR001|06|00|A|1|000000001|01012024|02012024|1500,00|250,00|001234|20,00|90,00|{ChaveValida}|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposObrigatoriosApenas_PreservaTextoCanonico()
    {
        const string sped = "|C500|FORNECEDOR001|06|00|||000000001|01012024||1500,00||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData(CodigoSituacaoDocumentoFiscal.DocumentoRegular, "00")]
    [InlineData(CodigoSituacaoDocumentoFiscal.DocumentoRegularExtemporaneo, "01")]
    [InlineData(CodigoSituacaoDocumentoFiscal.DocumentoCancelado, "02")]
    [InlineData(CodigoSituacaoDocumentoFiscal.DocumentoFiscalComplementar, "06")]
    [InlineData(CodigoSituacaoDocumentoFiscal.RegimeEspecial, "08")]
    public void Serializar_CodSit_RetornaCodigoSpedComDoisDigitos(
        CodigoSituacaoDocumentoFiscal situacao, string esperado)
    {
        _catalogo.TentarObter("C500".AsSpan(), out var meta);
        var registro = (RegistroC500)meta!.Fabrica();
        registro.CodSit = situacao;

        meta.Campos[2].Serializar(registro).Should().Be(esperado);
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
