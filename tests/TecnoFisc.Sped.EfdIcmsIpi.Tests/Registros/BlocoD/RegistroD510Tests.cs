using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.144 — exercita a forma do <see cref="RegistroD510"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (pp. 193-194): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD510Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD510).Assembly);

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
    public void Atributo_DeclaraD510_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD510).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D510");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD510Com19CamposNaOrdem()
    {
        _catalogo.TentarObter("D510".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D510");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "NumItem", "CodItem", "CodClass", "Qtd", "Unid",
            "VlItem", "VlDesc", "CstIcms", "Cfop", "VlBcIcms",
            "AliqIcms", "VlIcms", "VlBcIcmsUf", "VlIcmsUf", "IndRec",
            "CodPart", "VlPis", "VlCofins", "CodCta"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([
            2, 3, 4, 5, 6, 7, 8, 9, 10, 11,
            12, 13, 14, 15, 16, 17, 18, 19, 20
        ]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D510".AsSpan(), out var meta);
        var registro = (RegistroD510)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());          // NumItem
        meta.Campos[1].Definidor(registro, "ITEM0001".AsSpan());   // CodItem
        meta.Campos[2].Definidor(registro, "1001".AsSpan());       // CodClass
        meta.Campos[3].Definidor(registro, "10,000".AsSpan());     // Qtd
        meta.Campos[4].Definidor(registro, "UN".AsSpan());         // Unid
        meta.Campos[5].Definidor(registro, "150,00".AsSpan());     // VlItem
        meta.Campos[6].Definidor(registro, "5,00".AsSpan());       // VlDesc
        meta.Campos[7].Definidor(registro, "090".AsSpan());        // CstIcms
        meta.Campos[8].Definidor(registro, "6404".AsSpan());       // Cfop
        meta.Campos[9].Definidor(registro, "120,00".AsSpan());     // VlBcIcms
        meta.Campos[10].Definidor(registro, "18,00".AsSpan());     // AliqIcms
        meta.Campos[11].Definidor(registro, "21,60".AsSpan());     // VlIcms
        meta.Campos[12].Definidor(registro, "0,00".AsSpan());      // VlBcIcmsUf
        meta.Campos[13].Definidor(registro, "0,00".AsSpan());      // VlIcmsUf
        meta.Campos[14].Definidor(registro, "0".AsSpan());         // IndRec
        meta.Campos[15].Definidor(registro, "PARCEIRO01".AsSpan()); // CodPart
        meta.Campos[16].Definidor(registro, "0,00".AsSpan());      // VlPis
        meta.Campos[17].Definidor(registro, "0,00".AsSpan());      // VlCofins
        meta.Campos[18].Definidor(registro, "CONTA001".AsSpan());  // CodCta

        registro.NumItem.Should().Be(1);
        registro.CodItem.Should().Be("ITEM0001");
        registro.CodClass.Should().Be(1001);
        registro.Qtd.Should().Be(10.000m);
        registro.Unid.Should().Be("UN");
        registro.VlItem.Should().Be(150.00m);
        registro.VlDesc.Should().Be(5.00m);
        registro.CstIcms.Should().Be(90);
        registro.Cfop.Should().Be(Cfop.Create("6404".AsSpan()));
        registro.VlBcIcms.Should().Be(120.00m);
        registro.AliqIcms.Should().Be(18.00m);
        registro.VlIcms.Should().Be(21.60m);
        registro.VlBcIcmsUf.Should().Be(0.00m);
        registro.VlIcmsUf.Should().Be(0.00m);
        registro.IndRec.Should().Be(IndicadorTipoReceitaTelecomunicacao.ReceitaPropriaServicosPrestados);
        registro.CodPart.Should().Be("PARCEIRO01");
        registro.VlPis.Should().Be(0.00m);
        registro.VlCofins.Should().Be(0.00m);
        registro.CodCta.Should().Be("CONTA001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D510".AsSpan(), out var meta);
        var registro = (RegistroD510)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, Span<char>.Empty);   // Qtd
        meta.Campos[6].Definidor(registro, Span<char>.Empty);   // VlDesc
        meta.Campos[9].Definidor(registro, Span<char>.Empty);   // VlBcIcms
        meta.Campos[10].Definidor(registro, Span<char>.Empty);  // AliqIcms
        meta.Campos[11].Definidor(registro, Span<char>.Empty);  // VlIcms
        meta.Campos[12].Definidor(registro, Span<char>.Empty);  // VlBcIcmsUf
        meta.Campos[13].Definidor(registro, Span<char>.Empty);  // VlIcmsUf
        meta.Campos[14].Definidor(registro, Span<char>.Empty);  // IndRec
        meta.Campos[15].Definidor(registro, Span<char>.Empty);  // CodPart
        meta.Campos[16].Definidor(registro, Span<char>.Empty);  // VlPis
        meta.Campos[17].Definidor(registro, Span<char>.Empty);  // VlCofins
        meta.Campos[18].Definidor(registro, Span<char>.Empty);  // CodCta

        registro.Qtd.Should().BeNull();
        registro.VlDesc.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.AliqIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.VlBcIcmsUf.Should().BeNull();
        registro.VlIcmsUf.Should().BeNull();
        registro.IndRec.Should().BeNull();
        registro.CodPart.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Theory]
    [InlineData("0", IndicadorTipoReceitaTelecomunicacao.ReceitaPropriaServicosPrestados)]
    [InlineData("1", IndicadorTipoReceitaTelecomunicacao.ReceitaPropriaCobrancaDebitos)]
    [InlineData("2", IndicadorTipoReceitaTelecomunicacao.ReceitaPropriaVendaMercadorias)]
    [InlineData("3", IndicadorTipoReceitaTelecomunicacao.ReceitaPropriaVendaServicoPrepago)]
    [InlineData("4", IndicadorTipoReceitaTelecomunicacao.OutrasReceitasProprias)]
    [InlineData("5", IndicadorTipoReceitaTelecomunicacao.ReceitasTerceirosCofaturamento)]
    [InlineData("9", IndicadorTipoReceitaTelecomunicacao.OutrasReceitasTerceiros)]
    [InlineData("", null)]
    public void Definidor_IndRec_MapeiaCodigos(string input, IndicadorTipoReceitaTelecomunicacao? esperado)
    {
        _catalogo.TentarObter("D510".AsSpan(), out var meta);
        var registro = (RegistroD510)meta!.Fabrica();

        meta.Campos[14].Definidor(registro, input.AsSpan());

        registro.IndRec.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // CodClass 4 dígitos sem leading zero; CstIcms 3 dígitos sem leading zero (int? não faz zero-pad).
        const string sped = "|D510|1|ITEM0001|1001|10,000|UN|150,00|5,00|400|6404|120,00|18,00|21,60|0,00|0,00|0|PARCEIRO01|0,00|0,00|CONTA001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // Item de NF telecomunicação (cód. 22) sem campos opcionais.
        // 6 pipes após CFOP (5 campos opcionais VlBcIcms..VlIcmsUf); 5 pipes após IndRec (CodPart..CodCta + trailing).
        const string sped = "|D510|1|ITEM0001|2020|5,000|UN|200,00||400|6404||||||0|||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
