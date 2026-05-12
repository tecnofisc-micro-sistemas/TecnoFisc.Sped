using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoB;

/// <summary>
/// Sub-stage 8.024 — exercita a forma do <see cref="RegistroB020"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 45-46): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroB020Tests
{
    // Chave NF-e válida (UF SP, Jan/2024, DV=8) reutilizada dos testes de ChaveAcesso.
    private const string ChaveNfeValida = "35240111222333000181550010000000011000000018";

    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroB020).Assembly);

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
    public void Atributo_DeclaraB020_Nivel2_BlocoB()
    {
        var atributo = typeof(RegistroB020).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("B020");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("B");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroB020Com20CamposNaOrdem()
    {
        _catalogo.TentarObter("B020".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("B020");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndOper", "IndEmit", "CodPart", "CodMod", "CodSit",
            "Ser", "NumDoc", "ChvNfe", "DtDoc", "CodMunServ",
            "VlCont", "VlMatTerc", "VlSub", "VlIsntIss", "VlDedBc",
            "VlBcIss", "VlBcIssRt", "VlIssRt", "VlIss", "CodInfObs",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 20));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("B020".AsSpan(), out var meta);
        var registro = (RegistroB020)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());               // IndOper
        meta.Campos[1].Definidor(registro, "0".AsSpan());               // IndEmit
        meta.Campos[2].Definidor(registro, "PART001".AsSpan());         // CodPart
        meta.Campos[3].Definidor(registro, "55".AsSpan());              // CodMod
        meta.Campos[4].Definidor(registro, "00".AsSpan());              // CodSit
        meta.Campos[5].Definidor(registro, "001".AsSpan());             // Ser
        meta.Campos[6].Definidor(registro, "1".AsSpan());               // NumDoc
        meta.Campos[7].Definidor(registro, ChaveNfeValida.AsSpan());    // ChvNfe
        meta.Campos[8].Definidor(registro, "15052024".AsSpan());        // DtDoc
        meta.Campos[9].Definidor(registro, "3516900".AsSpan());         // CodMunServ
        meta.Campos[10].Definidor(registro, "1000.00".AsSpan());        // VlCont
        meta.Campos[11].Definidor(registro, "100.00".AsSpan());         // VlMatTerc
        meta.Campos[12].Definidor(registro, "50.00".AsSpan());          // VlSub
        meta.Campos[13].Definidor(registro, "0.00".AsSpan());           // VlIsntIss
        meta.Campos[14].Definidor(registro, "0.00".AsSpan());           // VlDedBc
        meta.Campos[15].Definidor(registro, "850.00".AsSpan());         // VlBcIss
        meta.Campos[16].Definidor(registro, "0.00".AsSpan());           // VlBcIssRt
        meta.Campos[17].Definidor(registro, "0.00".AsSpan());           // VlIssRt
        meta.Campos[18].Definidor(registro, "42.50".AsSpan());          // VlIss
        meta.Campos[19].Definidor(registro, "OBS001".AsSpan());         // CodInfObs

        registro.IndOper.Should().Be(IndicadorOperacaoIss.Aquisicao);
        registro.IndEmit.Should().Be(IndicadorEmissorDocumento.EmissaoPropria);
        registro.CodPart.Should().Be("PART001");
        registro.CodMod.Should().Be("55");
        registro.CodSit.Should().Be(CodigoSituacaoDocumentoFiscal.DocumentoRegular);
        registro.Ser.Should().Be("001");
        registro.NumDoc.Should().Be(1);
        registro.ChvNfe.Should().Be(ChaveAcesso.Criar(ChaveNfeValida));
        registro.DtDoc.Should().Be(new DateOnly(2024, 5, 15));
        registro.CodMunServ.Should().Be("3516900");
        registro.VlCont.Should().Be(1000.00m);
        registro.VlMatTerc.Should().Be(100.00m);
        registro.VlSub.Should().Be(50.00m);
        registro.VlIsntIss.Should().Be(0.00m);
        registro.VlDedBc.Should().Be(0.00m);
        registro.VlBcIss.Should().Be(850.00m);
        registro.VlBcIssRt.Should().Be(0.00m);
        registro.VlIssRt.Should().Be(0.00m);
        registro.VlIss.Should().Be(42.50m);
        registro.CodInfObs.Should().Be("OBS001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("B020".AsSpan(), out var meta);
        var registro = (RegistroB020)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, Span<char>.Empty); // CodPart
        registro.CodPart.Should().BeNull();

        meta.Campos[7].Definidor(registro, Span<char>.Empty); // ChvNfe
        registro.ChvNfe.Should().BeNull();

        meta.Campos[6].Definidor(registro, Span<char>.Empty); // NumDoc
        registro.NumDoc.Should().BeNull();
    }

    [Theory]
    [InlineData(IndicadorOperacaoIss.Aquisicao, "0")]
    [InlineData(IndicadorOperacaoIss.Prestacao, "1")]
    public void Serializar_IndOper_RetornaCodigo(IndicadorOperacaoIss operacao, string esperado)
    {
        _catalogo.TentarObter("B020".AsSpan(), out var meta);
        var registro = (RegistroB020)meta!.Fabrica();
        registro.IndOper = operacao;

        meta.Campos[0].Serializar(registro).Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorEmissorDocumento.EmissaoPropria, "0")]
    [InlineData(IndicadorEmissorDocumento.Terceiros, "1")]
    public void Serializar_IndEmit_RetornaCodigo(IndicadorEmissorDocumento emissor, string esperado)
    {
        _catalogo.TentarObter("B020".AsSpan(), out var meta);
        var registro = (RegistroB020)meta!.Fabrica();
        registro.IndEmit = emissor;

        meta.Campos[1].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        var sped =
            $"|B020|0|0|PART001|55|00|001|1|{ChaveNfeValida}|15052024|3516900|1000,00|100,00|50,00|0,00|0,00|850,00|0,00|0,00|42,50|OBS001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // NFS (03), sem série, sem ChvNfe, sem COD_PART, sem COD_INF_OBS.
        const string sped =
            "|B020|1|0||03|00||456||15052024|3516900|500,00|0,00|0,00|0,00|0,00|500,00|0,00|0,00|25,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_AquisicaoComDocumentoCancelado_PreservaTextoCanonico()
    {
        // NF (01) cancelada, sem ChvNfe.
        const string sped =
            "|B020|0|1|FORN999|01|02|B|999||10102023|3550308|300,00|0,00|0,00|0,00|0,00|300,00|0,00|0,00|15,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
