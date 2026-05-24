using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.059 — exercita a forma do <see cref="RegistroC176"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 85-87): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC176Tests
{
    // Chave NF-e válida (UF SP, Jan/2024, DV=8) reutilizada dos testes de ChaveAcesso.
    private const string ChaveNfeValida = "35240111222333000181550010000000011000000018";

    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC176).Assembly);

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
    public void Atributo_DeclaraC176_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC176).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C176");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC176Com26CamposNaOrdem()
    {
        _catalogo.TentarObter("C176".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C176");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodModUltE", "NumDocUltE", "SerUltE", "DtUltE", "CodPartUltE",
            "QuantUltE", "VlUnitUltE", "VlUnitBcSt",
            "ChaveNfeUltE", "NumItemUltE",
            "VlUnitBcIcmsUltE", "AliqIcmsUltE", "VlUnitLimiteBcIcmsUltE",
            "VlUnitIcmsUltE", "AliqStUltE", "VlUnitRes",
            "CodRespRet", "CodMotRes",
            "ChaveNfeRet", "CodPartNfeRet", "SerNfeRet", "NumNfeRet", "ItemNfeRet",
            "CodDa", "NumDa", "VlUnitResFcpSt",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 26));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C176".AsSpan(), out var meta);
        var registro = (RegistroC176)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01".AsSpan());                                            // CodModUltE
        meta.Campos[1].Definidor(registro, "12345".AsSpan());                                         // NumDocUltE
        meta.Campos[2].Definidor(registro, "A01".AsSpan());                                           // SerUltE
        meta.Campos[3].Definidor(registro, "01012023".AsSpan());                                      // DtUltE
        meta.Campos[4].Definidor(registro, "FORN001".AsSpan());                                       // CodPartUltE
        meta.Campos[5].Definidor(registro, "10,000".AsSpan());                                        // QuantUltE
        meta.Campos[6].Definidor(registro, "100,000".AsSpan());                                       // VlUnitUltE
        meta.Campos[7].Definidor(registro, "110,000".AsSpan());                                       // VlUnitBcSt
        meta.Campos[8].Definidor(registro, ChaveNfeValida.AsSpan());                                  // ChaveNfeUltE
        meta.Campos[9].Definidor(registro, "001".AsSpan());                                           // NumItemUltE
        meta.Campos[10].Definidor(registro, "120,00".AsSpan());                                       // VlUnitBcIcmsUltE
        meta.Campos[11].Definidor(registro, "12,00".AsSpan());                                        // AliqIcmsUltE
        meta.Campos[12].Definidor(registro, "115,00".AsSpan());                                       // VlUnitLimiteBcIcmsUltE
        meta.Campos[13].Definidor(registro, "13,800".AsSpan());                                       // VlUnitIcmsUltE
        meta.Campos[14].Definidor(registro, "18,00".AsSpan());                                        // AliqStUltE
        meta.Campos[15].Definidor(registro, "5,000".AsSpan());                                        // VlUnitRes
        meta.Campos[16].Definidor(registro, "1".AsSpan());                                            // CodRespRet
        meta.Campos[17].Definidor(registro, "1".AsSpan());                                            // CodMotRes
        meta.Campos[18].Definidor(registro, ChaveNfeValida.AsSpan());                                 // ChaveNfeRet
        meta.Campos[19].Definidor(registro, "FORN001".AsSpan());                                      // CodPartNfeRet
        meta.Campos[20].Definidor(registro, "001".AsSpan());                                          // SerNfeRet
        meta.Campos[21].Definidor(registro, "99999".AsSpan());                                        // NumNfeRet
        meta.Campos[22].Definidor(registro, "001".AsSpan());                                          // ItemNfeRet
        meta.Campos[23].Definidor(registro, "0".AsSpan());                                            // CodDa
        meta.Campos[24].Definidor(registro, "DA12345".AsSpan());                                      // NumDa
        meta.Campos[25].Definidor(registro, "2,000".AsSpan());                                        // VlUnitResFcpSt

        registro.CodModUltE.Should().Be("01");
        registro.NumDocUltE.Should().Be(12345);
        registro.SerUltE.Should().Be("A01");
        registro.DtUltE.Should().Be(new DateOnly(2023, 1, 1));
        registro.CodPartUltE.Should().Be("FORN001");
        registro.QuantUltE.Should().Be(10.000m);
        registro.VlUnitUltE.Should().Be(100.000m);
        registro.VlUnitBcSt.Should().Be(110.000m);
        registro.ChaveNfeUltE.Should().Be(ChaveAcesso.Create(ChaveNfeValida));
        registro.NumItemUltE.Should().Be(1);
        registro.VlUnitBcIcmsUltE.Should().Be(120.00m);
        registro.AliqIcmsUltE.Should().Be(12.00m);
        registro.VlUnitLimiteBcIcmsUltE.Should().Be(115.00m);
        registro.VlUnitIcmsUltE.Should().Be(13.800m);
        registro.AliqStUltE.Should().Be(18.00m);
        registro.VlUnitRes.Should().Be(5.000m);
        registro.CodRespRet.Should().Be(CodigoResponsavelRetencaoSt.RemetenteDireto);
        registro.CodMotRes.Should().Be(CodigoMotivoRessarcimentoSt.SaidaParaOutraUf);
        registro.ChaveNfeRet.Should().Be(ChaveAcesso.Create(ChaveNfeValida));
        registro.CodPartNfeRet.Should().Be("FORN001");
        registro.SerNfeRet.Should().Be("001");
        registro.NumNfeRet.Should().Be(99999);
        registro.ItemNfeRet.Should().Be(1);
        registro.CodDa.Should().Be(CodigoModeloDocumentoArrecadacao.DocumentoEstadual);
        registro.NumDa.Should().Be("DA12345");
        registro.VlUnitResFcpSt.Should().Be(2.000m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C176".AsSpan(), out var meta);
        var registro = (RegistroC176)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodModUltE.Should().BeNull();
        registro.NumDocUltE.Should().BeNull();
        registro.SerUltE.Should().BeNull();
        registro.DtUltE.Should().BeNull();
        registro.CodPartUltE.Should().BeNull();
        registro.QuantUltE.Should().BeNull();
        registro.VlUnitUltE.Should().BeNull();
        registro.VlUnitBcSt.Should().BeNull();
        registro.ChaveNfeUltE.Should().BeNull();
        registro.NumItemUltE.Should().BeNull();
        registro.VlUnitBcIcmsUltE.Should().BeNull();
        registro.AliqIcmsUltE.Should().BeNull();
        registro.VlUnitLimiteBcIcmsUltE.Should().BeNull();
        registro.VlUnitIcmsUltE.Should().BeNull();
        registro.AliqStUltE.Should().BeNull();
        registro.VlUnitRes.Should().BeNull();
        registro.CodRespRet.Should().BeNull();
        registro.CodMotRes.Should().BeNull();
        registro.ChaveNfeRet.Should().BeNull();
        registro.CodPartNfeRet.Should().BeNull();
        registro.SerNfeRet.Should().BeNull();
        registro.NumNfeRet.Should().BeNull();
        registro.ItemNfeRet.Should().BeNull();
        registro.CodDa.Should().BeNull();
        registro.NumDa.Should().BeNull();
        registro.VlUnitResFcpSt.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Ressarcimento com todos os campos preenchidos: ST com remetente direto, saída para outra UF.
        const string sped =
            $"|C176|01|12345|A01|01012023|FORN001|10,000|100,000|110,000|{ChaveNfeValida}|1|120,00|12,00|115,00|13,800|18,00|5,000|1|1|{ChaveNfeValida}|FORN001|001|99999|1|0|DA12345|2,000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // Apenas campos obrigatórios (02-09); campos opcionais 10-27 ausentes.
        const string sped =
            "|C176|01|12345||01012023|FORN001|10,000|100,000|110,000|||||||||||||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("1", CodigoResponsavelRetencaoSt.RemetenteDireto)]
    [InlineData("2", CodigoResponsavelRetencaoSt.RemetenteIndireto)]
    [InlineData("3", CodigoResponsavelRetencaoSt.ProprioDeclarante)]
    public void Definidor_CodRespRet_CobertosEnum(string valor, CodigoResponsavelRetencaoSt esperado)
    {
        _catalogo.TentarObter("C176".AsSpan(), out var meta);
        var registro = (RegistroC176)meta!.Fabrica();

        meta.Campos[16].Definidor(registro, valor.AsSpan());

        registro.CodRespRet.Should().Be(esperado);
    }

    [Theory]
    [InlineData("1", CodigoMotivoRessarcimentoSt.SaidaParaOutraUf)]
    [InlineData("2", CodigoMotivoRessarcimentoSt.SaidaIsentaOuNaoIncidencia)]
    [InlineData("3", CodigoMotivoRessarcimentoSt.PerdaOuDeterioracao)]
    [InlineData("4", CodigoMotivoRessarcimentoSt.FurtoOuRoubo)]
    [InlineData("5", CodigoMotivoRessarcimentoSt.Exportacao)]
    [InlineData("6", CodigoMotivoRessarcimentoSt.VendaInternaParaSimplesNacional)]
    [InlineData("9", CodigoMotivoRessarcimentoSt.Outros)]
    public void Definidor_CodMotRes_CobertosEnum(string valor, CodigoMotivoRessarcimentoSt esperado)
    {
        _catalogo.TentarObter("C176".AsSpan(), out var meta);
        var registro = (RegistroC176)meta!.Fabrica();

        meta.Campos[17].Definidor(registro, valor.AsSpan());

        registro.CodMotRes.Should().Be(esperado);
    }
}
