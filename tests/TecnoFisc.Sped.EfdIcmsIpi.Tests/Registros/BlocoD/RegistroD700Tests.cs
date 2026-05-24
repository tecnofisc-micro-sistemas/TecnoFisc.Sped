using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.017.007 — exercita a forma do <see cref="RegistroD700"/> (NFCom, código 62) contra
/// o Guia Prático EFD-ICMS/IPI V3.2.2 (p. 213-215, Subseção 12): metadados de catálogo, mapeamento
/// de campos, decisões de tipo (SER lazy, DED V019) e invariante de round-trip parse → texto idêntico.
/// </summary>
public sealed class RegistroD700Tests
{
    // Chave NFCom de teste (44 dígitos, mod-44, somente para parsing — não há validação de DV).
    private const string ChaveNFComValida = "35240111222333000181620010000000011000000018";
    private const string ChaveNFComReferenciada = "35240111222333000181620010000000021000000026";

    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD700).Assembly);

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
    public void Atributo_DeclaraD700_Nivel2_BlocoD_IntroduzidoEmV017()
    {
        var atributo = typeof(RegistroD700).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D700");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("D");
        atributo.IntroduzidoEm.Should().Be((int)LayoutEfdIcmsIpi.V017);
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD700Com31CamposNaOrdem()
    {
        _catalogo.TentarObter("D700".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D700");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndOper", "IndEmit", "CodPart", "CodMod", "CodSit",
            "Ser", "NumDoc", "DtDoc", "DtES", "VlDoc",
            "VlDesc", "VlServ", "VlServNt", "VlTerc", "VlDa",
            "VlBcIcms", "VlIcms", "CodInf", "VlPis", "VlCofins",
            "ChvDOCe", "FinDOCe", "TipFat", "CodModDocRef", "ChvDOCeRef",
            "HashDocRef", "SerDocRef", "NumDocRef", "MesDocRef", "CodMunDest",
            "Ded",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 31));
    }

    [Fact]
    public void Definidor_AtribuiCamposObrigatorios()
    {
        _catalogo.TentarObter("D700".AsSpan(), out var meta);
        var registro = (RegistroD700)meta!.Fabrica();

        // Campos obrigatórios (saídas: 2,3,5,6,7,8,9,11,22,23,24,31; entradas trocam 4 por 31).
        meta.Campos[0].Definidor(registro, "1".AsSpan());                                 // IndOper (Saída)
        meta.Campos[1].Definidor(registro, "0".AsSpan());                                 // IndEmit
        meta.Campos[3].Definidor(registro, "62".AsSpan());                                // CodMod
        meta.Campos[4].Definidor(registro, "00".AsSpan());                                // CodSit
        meta.Campos[5].Definidor(registro, "001".AsSpan());                               // Ser
        meta.Campos[6].Definidor(registro, "12345".AsSpan());                             // NumDoc
        meta.Campos[7].Definidor(registro, "15012025".AsSpan());                          // DtDoc
        meta.Campos[9].Definidor(registro, "1000,00".AsSpan());                           // VlDoc
        meta.Campos[20].Definidor(registro, ChaveNFComValida.AsSpan());                   // ChvDOCe
        meta.Campos[21].Definidor(registro, "0".AsSpan());                                // FinDOCe (Normal)
        meta.Campos[22].Definidor(registro, "0".AsSpan());                                // TipFat (Normal)
        meta.Campos[29].Definidor(registro, "3550308".AsSpan());                          // CodMunDest

        registro.IndOper.Should().Be(IndicadorOperacao.Saida);
        registro.IndEmit.Should().Be(IndicadorEmissorDocumento.EmissaoPropria);
        registro.CodMod.Should().Be("62");
        registro.CodSit.Should().Be(CodigoSituacaoDocumentoFiscal.DocumentoRegular);
        registro.Ser.Should().Be("001");
        registro.NumDoc.Should().Be(12345);
        registro.DtDoc.Should().Be(new DateOnly(2025, 1, 15));
        registro.VlDoc.Should().Be(1000.00m);
        registro.ChvDOCe.Should().Be(ChaveNFComValida);
        registro.FinDOCe.Should().Be(FinalidadeDocumentoEletronicoNFCom.Normal);
        registro.TipFat.Should().Be(TipoFaturamentoNFCom.Normal);
        registro.CodMunDest.Should().Be(3550308);
    }

    [Fact]
    public void Definidor_CampoSerAceitaTextoLazy()
    {
        // Justificativa: campo 07 SER do D700 é declarado como string? (lazy) no modelo único
        // para tolerar mudanças de tipo entre versões do leiaute (V020 muda N→C; o PDF v3.2.2 já
        // lista C). Texto não-numérico deve ser preservado sem conversão.
        _catalogo.TentarObter("D700".AsSpan(), out var meta);
        var registro = (RegistroD700)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, "ABC".AsSpan());

        registro.Ser.Should().Be("ABC");
    }

    [Theory]
    [InlineData(IndicadorOperacao.Entrada, "0")]
    [InlineData(IndicadorOperacao.Saida, "1")]
    public void Definidor_IndOper_MapeiaEntradaESaida(IndicadorOperacao esperado, string entrada)
    {
        _catalogo.TentarObter("D700".AsSpan(), out var meta);
        var registro = (RegistroD700)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, entrada.AsSpan());

        registro.IndOper.Should().Be(esperado);
    }

    [Theory]
    [InlineData(FinalidadeDocumentoEletronicoNFCom.Normal, "0")]
    [InlineData(FinalidadeDocumentoEletronicoNFCom.Substituicao, "3")]
    [InlineData(FinalidadeDocumentoEletronicoNFCom.Ajuste, "4")]
    public void Definidor_FinDocE_MapeiaTresValores(FinalidadeDocumentoEletronicoNFCom esperado, string entrada)
    {
        _catalogo.TentarObter("D700".AsSpan(), out var meta);
        var registro = (RegistroD700)meta!.Fabrica();

        meta.Campos[21].Definidor(registro, entrada.AsSpan());

        registro.FinDOCe.Should().Be(esperado);
    }

    [Theory]
    [InlineData(TipoFaturamentoNFCom.Normal, "0")]
    [InlineData(TipoFaturamentoNFCom.Centralizado, "1")]
    [InlineData(TipoFaturamentoNFCom.Cofaturamento, "2")]
    public void Definidor_TipFat_MapeiaTresValores(TipoFaturamentoNFCom esperado, string entrada)
    {
        _catalogo.TentarObter("D700".AsSpan(), out var meta);
        var registro = (RegistroD700)meta!.Fabrica();

        meta.Campos[22].Definidor(registro, entrada.AsSpan());

        registro.TipFat.Should().Be(esperado);
    }

    [Fact]
    public void Definidor_Ded_DeclaradoComoDesdeVersaoV019()
    {
        _catalogo.TentarObter("D700".AsSpan(), out var meta);

        var dedCampo = meta!.Campos.Single(c => c.Nome == "Ded");
        dedCampo.DesdeVersao.Should().Be((int)LayoutEfdIcmsIpi.V019);
        dedCampo.Ordem.Should().Be(32);
    }

    [Fact]
    public async Task RoundTrip_NFComCompleta_PreservaTextoCanonico()
    {
        // NFCom de saída, emissão própria, todos os campos populados incluindo DED (V019)
        // e documento referenciado (campos 25-30). Município destino = São Paulo (3550308).
        var sped =
            $"|D700|1|0|TOMADOR01|62|00|001|12345|15012025|15012025|1500,00|50,00|1450,00|0,00|0,00|0,00|1450,00|261,00|OBS001|14,50|66,70|{ChaveNFComValida}|0|0|62|{ChaveNFComReferenciada}|D41D8CD98F00B204E9800998ECF8427E|002|54321|122024|3550308|10,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
