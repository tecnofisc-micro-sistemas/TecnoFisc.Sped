using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.092 — exercita a forma do <see cref="RegistroC500"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (pp. 133-137): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico. Sub-stage 8.016.003 — campos 34-40 introduzidos
/// em V016 (Guia Prático v3.2.2, pp. 136-138, 3.0.7 itens 11-12).
/// </summary>
public sealed class RegistroC500Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC500).Assembly);

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
    public void Atributo_DeclaraC500_Nivel2_BlocoC()
    {
        var atributo = typeof(RegistroC500).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C500");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC500Com39CamposNaOrdem()
    {
        _catalogo.TentarObter("C500".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C500");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndOper", "IndEmit", "CodPart", "CodMod", "CodSit",
            "Ser", "Sub", "CodCons", "NumDoc", "DtDoc", "DtES",
            "VlDoc", "VlDesc", "VlForn", "VlServNt", "VlTerc", "VlDa",
            "VlBcIcms", "VlIcms", "VlBcIcmsSt", "VlIcmsSt", "CodInf",
            "VlPis", "VlCofins", "TpLigacao", "CodGrupoTensao",
            "ChvDoce", "FinDoce", "ChvDoceRef", "IndDest", "CodMunDest", "CodCta",
            "CodModDocRef", "HashDocRef", "SerDocRef", "NumDocRef", "MesDocRef",
            "EnerInjet", "OutrasDed"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([
            2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17,
            18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33,
            34, 35, 36, 37, 38, 39, 40
        ]);
    }

    [Fact]
    public void CamposV016_DesdeVersaoV016()
    {
        _catalogo.TentarObter("C500".AsSpan(), out var meta);

        var nomesV016 = new[] { "CodModDocRef", "HashDocRef", "SerDocRef", "NumDocRef", "MesDocRef", "EnerInjet", "OutrasDed" };
        foreach (var nome in nomesV016)
        {
            var campo = meta!.Campos.Single(c => c.Nome == nome);
            campo.DesdeVersao.Should().Be((int)LayoutEfdIcmsIpi.V016, because: $"{nome} deve ser V016");
        }
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C500".AsSpan(), out var meta);
        var registro = (RegistroC500)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());          // IndOper
        meta.Campos[1].Definidor(registro, "0".AsSpan());          // IndEmit
        meta.Campos[2].Definidor(registro, "PART001".AsSpan());    // CodPart
        meta.Campos[3].Definidor(registro, "06".AsSpan());         // CodMod
        meta.Campos[4].Definidor(registro, "00".AsSpan());         // CodSit
        meta.Campos[5].Definidor(registro, "A".AsSpan());          // Ser
        meta.Campos[6].Definidor(registro, "1".AsSpan());          // Sub
        meta.Campos[7].Definidor(registro, "01".AsSpan());         // CodCons
        meta.Campos[8].Definidor(registro, "123456789".AsSpan());  // NumDoc
        meta.Campos[9].Definidor(registro, "01012021".AsSpan());   // DtDoc
        meta.Campos[10].Definidor(registro, "02012021".AsSpan());  // DtES
        meta.Campos[11].Definidor(registro, "1500,00".AsSpan());   // VlDoc
        meta.Campos[12].Definidor(registro, "50,00".AsSpan());     // VlDesc
        meta.Campos[13].Definidor(registro, "1200,00".AsSpan());   // VlForn
        meta.Campos[14].Definidor(registro, "100,00".AsSpan());    // VlServNt
        meta.Campos[15].Definidor(registro, "30,00".AsSpan());     // VlTerc
        meta.Campos[16].Definidor(registro, "20,00".AsSpan());     // VlDa
        meta.Campos[17].Definidor(registro, "800,00".AsSpan());    // VlBcIcms
        meta.Campos[18].Definidor(registro, "96,00".AsSpan());     // VlIcms
        meta.Campos[19].Definidor(registro, "200,00".AsSpan());    // VlBcIcmsSt
        meta.Campos[20].Definidor(registro, "24,00".AsSpan());     // VlIcmsSt
        meta.Campos[21].Definidor(registro, "INF001".AsSpan());    // CodInf
        meta.Campos[22].Definidor(registro, "5,00".AsSpan());      // VlPis
        meta.Campos[23].Definidor(registro, "10,00".AsSpan());     // VlCofins
        meta.Campos[24].Definidor(registro, "1".AsSpan());         // TpLigacao
        meta.Campos[25].Definidor(registro, "07".AsSpan());        // CodGrupoTensao
        meta.Campos[26].Definidor(registro, "12345678901234567890123456789012345678901234".AsSpan()); // ChvDoce
        meta.Campos[27].Definidor(registro, "1".AsSpan());         // FinDoce
        meta.Campos[28].Definidor(registro, "12345678901234567890123456789012345678901234".AsSpan()); // ChvDoceRef
        meta.Campos[29].Definidor(registro, "1".AsSpan());         // IndDest
        meta.Campos[30].Definidor(registro, "3550308".AsSpan());   // CodMunDest
        meta.Campos[31].Definidor(registro, "CONTA001".AsSpan());  // CodCta
        meta.Campos[32].Definidor(registro, "06".AsSpan());        // CodModDocRef
        meta.Campos[33].Definidor(registro, "ABCDEFGHIJ12345678901234567890AB".AsSpan()); // HashDocRef (32 chars)
        meta.Campos[34].Definidor(registro, "A001".AsSpan());      // SerDocRef
        meta.Campos[35].Definidor(registro, "987654321".AsSpan()); // NumDocRef
        meta.Campos[36].Definidor(registro, "012022".AsSpan());    // MesDocRef
        meta.Campos[37].Definidor(registro, "150,50".AsSpan());    // EnerInjet
        meta.Campos[38].Definidor(registro, "25,00".AsSpan());     // OutrasDed

        registro.IndOper.Should().Be(TecnoFisc.Sped.Core.Enums.IndicadorOperacao.Entrada);
        registro.IndEmit.Should().Be(TecnoFisc.Sped.Core.Enums.IndicadorEmissorDocumento.EmissaoPropria);
        registro.CodPart.Should().Be("PART001");
        registro.CodMod.Should().Be("06");
        registro.CodSit.Should().Be(TecnoFisc.Sped.Core.Enums.CodigoSituacaoDocumentoFiscal.DocumentoRegular);
        registro.Ser.Should().Be("A");
        registro.Sub.Should().Be(1);
        registro.CodCons.Should().Be("01");
        registro.NumDoc.Should().Be(123456789L);
        registro.DtDoc.Should().Be(new DateOnly(2021, 1, 1));
        registro.DtES.Should().Be(new DateOnly(2021, 1, 2));
        registro.VlDoc.Should().Be(1500.00m);
        registro.VlDesc.Should().Be(50.00m);
        registro.VlForn.Should().Be(1200.00m);
        registro.VlServNt.Should().Be(100.00m);
        registro.VlTerc.Should().Be(30.00m);
        registro.VlDa.Should().Be(20.00m);
        registro.VlBcIcms.Should().Be(800.00m);
        registro.VlIcms.Should().Be(96.00m);
        registro.VlBcIcmsSt.Should().Be(200.00m);
        registro.VlIcmsSt.Should().Be(24.00m);
        registro.CodInf.Should().Be("INF001");
        registro.VlPis.Should().Be(5.00m);
        registro.VlCofins.Should().Be(10.00m);
        registro.TpLigacao.Should().Be(TipoLigacaoEletrica.Monofasico);
        registro.CodGrupoTensao.Should().Be("07");
        registro.ChvDoce.Should().Be("12345678901234567890123456789012345678901234");
        registro.FinDoce.Should().Be(FinalidadeDocumentoEletronico.Normal);
        registro.ChvDoceRef.Should().Be("12345678901234567890123456789012345678901234");
        registro.IndDest.Should().Be(IndicadorDestinatario.ContribuinteIcms);
        registro.CodMunDest.Should().Be(3550308);
        registro.CodCta.Should().Be("CONTA001");
        registro.CodModDocRef.Should().Be("06");
        registro.HashDocRef.Should().Be("ABCDEFGHIJ12345678901234567890AB");
        registro.SerDocRef.Should().Be("A001");
        registro.NumDocRef.Should().Be(987654321);
        registro.MesDocRef.Should().Be("012022");
        registro.EnerInjet.Should().Be(150.50m);
        registro.OutrasDed.Should().Be(25.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C500".AsSpan(), out var meta);
        var registro = (RegistroC500)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.IndOper.Should().BeNull();
        registro.IndEmit.Should().BeNull();
        registro.CodPart.Should().BeNull();
        registro.CodMod.Should().BeNull();
        registro.CodSit.Should().BeNull();
        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
        registro.CodCons.Should().BeNull();
        registro.NumDoc.Should().BeNull();
        registro.DtDoc.Should().BeNull();
        registro.DtES.Should().BeNull();
        registro.VlDoc.Should().BeNull();
        registro.VlDesc.Should().BeNull();
        registro.VlForn.Should().BeNull();
        registro.VlServNt.Should().BeNull();
        registro.VlTerc.Should().BeNull();
        registro.VlDa.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.VlBcIcmsSt.Should().BeNull();
        registro.VlIcmsSt.Should().BeNull();
        registro.CodInf.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.TpLigacao.Should().BeNull();
        registro.CodGrupoTensao.Should().BeNull();
        registro.ChvDoce.Should().BeNull();
        registro.FinDoce.Should().BeNull();
        registro.ChvDoceRef.Should().BeNull();
        registro.IndDest.Should().BeNull();
        registro.CodMunDest.Should().BeNull();
        registro.CodCta.Should().BeNull();
        registro.CodModDocRef.Should().BeNull();
        registro.HashDocRef.Should().BeNull();
        registro.SerDocRef.Should().BeNull();
        registro.NumDocRef.Should().BeNull();
        registro.MesDocRef.Should().BeNull();
        registro.EnerInjet.Should().BeNull();
        registro.OutrasDed.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C500|0|0|PART001|06|00|A|1|01|123456789|01012021|02012021|1500,00|50,00|1200,00|100,00|30,00|20,00|800,00|96,00|200,00|24,00|INF001|5,00|10,00|1|07|12345678901234567890123456789012345678901234|1|12345678901234567890123456789012345678901234|1|3550308|CONTA001|06|ABCDEFGHIJ12345678901234567890AB|A001|987654321|012022|150,50|25,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // Apenas campos obrigatórios: IND_OPER, IND_EMIT, COD_PART, COD_MOD, COD_SIT, NUM_DOC,
        // DT_DOC, DT_E_S, VL_DOC e VL_FORN. Campos SER, SUB, COD_CONS vazios antes de NUM_DOC
        // (4 pipes entre COD_SIT e NUM_DOC); 25 campos opcionais vazios após VL_FORN (26 pipes,
        // inclui os 7 campos V016 vazios).
        const string sped =
            "|C500|0|0|FORN001|06|00||||999|01012021|01012021|1000,00||1000,00||||||||||||||||||||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_NF3e_ComChaveEFinalidade_PreservaTextoCanonico()
    {
        // NF3e (modelo 66) de saída: CHV_DOCe obrigatório (44 dígitos), FIN_DOCe = 1 (Normal),
        // IND_DEST = 1 (Contribuinte ICMS) e COD_MUN_DEST obrigatórios na saída.
        // 12 campos opcionais vazios entre VL_FORN e CHV_DOCe; 7 campos V016 vazios ao final.
        const string sped =
            "|C500|1|1|DEST001|66|00||||999|01022021|01022021|5000,00||5000,00|||||||||||||12345678901234567890123456789012345678901234|1||1|3304557|||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("1", TipoLigacaoEletrica.Monofasico)]
    [InlineData("2", TipoLigacaoEletrica.Bifasico)]
    [InlineData("3", TipoLigacaoEletrica.Trifasico)]
    public void TpLigacao_Definidor_AtribuiValorCorreto(string valor, TipoLigacaoEletrica esperado)
    {
        _catalogo.TentarObter("C500".AsSpan(), out var meta);
        var registro = (RegistroC500)meta!.Fabrica();
        var campo = meta.Campos.First(c => c.Nome == "TpLigacao");

        campo.Definidor(registro, valor.AsSpan());

        registro.TpLigacao.Should().Be(esperado);
    }

    [Theory]
    [InlineData("1", FinalidadeDocumentoEletronico.Normal)]
    [InlineData("2", FinalidadeDocumentoEletronico.Substituicao)]
    [InlineData("3", FinalidadeDocumentoEletronico.NormalComAjuste)]
    public void FinDoce_Definidor_AtribuiValorCorreto(string valor, FinalidadeDocumentoEletronico esperado)
    {
        _catalogo.TentarObter("C500".AsSpan(), out var meta);
        var registro = (RegistroC500)meta!.Fabrica();
        var campo = meta.Campos.First(c => c.Nome == "FinDoce");

        campo.Definidor(registro, valor.AsSpan());

        registro.FinDoce.Should().Be(esperado);
    }

    [Theory]
    [InlineData("1", IndicadorDestinatario.ContribuinteIcms)]
    [InlineData("2", IndicadorDestinatario.IsentoInscricao)]
    [InlineData("9", IndicadorDestinatario.NaoContribuinte)]
    public void IndDest_Definidor_AtribuiValorCorreto(string valor, IndicadorDestinatario esperado)
    {
        _catalogo.TentarObter("C500".AsSpan(), out var meta);
        var registro = (RegistroC500)meta!.Fabrica();
        var campo = meta.Campos.First(c => c.Nome == "IndDest");

        campo.Definidor(registro, valor.AsSpan());

        registro.IndDest.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_V016_ComDocumentoReferenciado_PreservaTextoCanonico()
    {
        // V016: campos 34-40 populados — referência a documento fiscal anterior via HASH/série/número,
        // mês-ano de emissão, energia injetada e outras deduções.
        const string sped =
            "|C500|0|0|FORN001|06|00||||999|01012022|01012022|1200,00||1000,00|||||||||||||||||||06|ABCDEFGHIJ12345678901234567890AB|A001|987654321|012022|150,50|25,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_V015_SemCamposV016_EmiteFieldsVazios()
    {
        // Arquivos V015 (sem campos 34-40) são lidos sem as 7 propriedades; ao regerar, o escritor
        // emite 7 pipes vazios adicionais para conformidade com o leiaute V016.
        const string entrada =
            "|C500|0|0|FORN001|06|00||||999|01012021|01012021|1000,00||1000,00|||||||||||||||||||\r\n";
        const string esperado =
            "|C500|0|0|FORN001|06|00||||999|01012021|01012021|1000,00||1000,00||||||||||||||||||||||||||\r\n";

        var resultado = await RoundTripAsync(entrada, TestContext.Current.CancellationToken);

        resultado.Should().Be(esperado);
    }
}
