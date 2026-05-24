using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.053 — exercita a forma do <see cref="RegistroC170"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 76-77): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC170Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC170).Assembly);

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
    public void Atributo_DeclaraC170_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC170).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C170");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC170Com37CamposNaOrdem()
    {
        _catalogo.TentarObter("C170".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C170");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "NumItem", "CodItem", "DescrCompl", "Qtd", "Unid",
            "VlItem", "VlDesc", "IndMov", "CstIcms", "Cfop",
            "CodNat", "VlBcIcms", "AliqIcms", "VlIcms", "VlBcIcmsSt",
            "AliqSt", "VlIcmsSt", "IndApur", "CstIpi", "CodEnq",
            "VlBcIpi", "AliqIpi", "VlIpi", "CstPis", "VlBcPis",
            "AliqPis", "QuantBcPis", "AliqPisQuant", "VlPis", "CstCofins",
            "VlBcCofins", "AliqCofins", "QuantBcCofins", "AliqCofinsQuant", "VlCofins",
            "CodCta", "VlAbatNt",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 37));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C170".AsSpan(), out var meta);
        var registro = (RegistroC170)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());              // NumItem
        meta.Campos[1].Definidor(registro, "PROD001".AsSpan());        // CodItem
        meta.Campos[2].Definidor(registro, "Produto Teste".AsSpan());  // DescrCompl
        meta.Campos[3].Definidor(registro, "100,00000".AsSpan());      // Qtd
        meta.Campos[4].Definidor(registro, "UN".AsSpan());             // Unid
        meta.Campos[5].Definidor(registro, "1500,00".AsSpan());        // VlItem
        meta.Campos[6].Definidor(registro, "50,00".AsSpan());          // VlDesc
        meta.Campos[7].Definidor(registro, "0".AsSpan());              // IndMov
        meta.Campos[8].Definidor(registro, "100".AsSpan());            // CstIcms
        meta.Campos[9].Definidor(registro, "1102".AsSpan());           // Cfop
        meta.Campos[10].Definidor(registro, "NAT001".AsSpan());        // CodNat
        meta.Campos[11].Definidor(registro, "1450,00".AsSpan());       // VlBcIcms
        meta.Campos[12].Definidor(registro, "12,00".AsSpan());         // AliqIcms
        meta.Campos[13].Definidor(registro, "174,00".AsSpan());        // VlIcms
        meta.Campos[14].Definidor(registro, "0,00".AsSpan());          // VlBcIcmsSt
        meta.Campos[15].Definidor(registro, "0,00".AsSpan());          // AliqSt
        meta.Campos[16].Definidor(registro, "0,00".AsSpan());          // VlIcmsSt
        meta.Campos[17].Definidor(registro, "0".AsSpan());             // IndApur
        meta.Campos[18].Definidor(registro, "50".AsSpan());            // CstIpi
        meta.Campos[19].Definidor(registro, "999".AsSpan());           // CodEnq
        meta.Campos[20].Definidor(registro, "1450,00".AsSpan());       // VlBcIpi
        meta.Campos[21].Definidor(registro, "10,00".AsSpan());         // AliqIpi
        meta.Campos[22].Definidor(registro, "145,00".AsSpan());        // VlIpi
        meta.Campos[23].Definidor(registro, "50".AsSpan());            // CstPis
        meta.Campos[24].Definidor(registro, "1450,00".AsSpan());       // VlBcPis
        meta.Campos[25].Definidor(registro, "7,6000".AsSpan());        // AliqPis
        meta.Campos[26].Definidor(registro, "0,000".AsSpan());         // QuantBcPis
        meta.Campos[27].Definidor(registro, "0,0000".AsSpan());        // AliqPisQuant
        meta.Campos[28].Definidor(registro, "110,20".AsSpan());        // VlPis
        meta.Campos[29].Definidor(registro, "50".AsSpan());            // CstCofins
        meta.Campos[30].Definidor(registro, "1450,00".AsSpan());       // VlBcCofins
        meta.Campos[31].Definidor(registro, "3,0000".AsSpan());        // AliqCofins
        meta.Campos[32].Definidor(registro, "0,000".AsSpan());         // QuantBcCofins
        meta.Campos[33].Definidor(registro, "0,0000".AsSpan());        // AliqCofinsQuant
        meta.Campos[34].Definidor(registro, "43,50".AsSpan());         // VlCofins
        meta.Campos[35].Definidor(registro, "CTA001".AsSpan());        // CodCta
        meta.Campos[36].Definidor(registro, "0,00".AsSpan());          // VlAbatNt

        registro.NumItem.Should().Be(1);
        registro.CodItem.Should().Be("PROD001");
        registro.DescrCompl.Should().Be("Produto Teste");
        registro.Qtd.Should().Be(100.00000m);
        registro.Unid.Should().Be("UN");
        registro.VlItem.Should().Be(1500.00m);
        registro.VlDesc.Should().Be(50.00m);
        registro.IndMov.Should().Be(IndicadorMovimentacaoFisica.Sim);
        registro.CstIcms.Should().Be(100);
        registro.Cfop.Should().Be(Cfop.Create("1102"));
        registro.CodNat.Should().Be("NAT001");
        registro.VlBcIcms.Should().Be(1450.00m);
        registro.AliqIcms.Should().Be(12.00m);
        registro.VlIcms.Should().Be(174.00m);
        registro.VlBcIcmsSt.Should().Be(0.00m);
        registro.AliqSt.Should().Be(0.00m);
        registro.VlIcmsSt.Should().Be(0.00m);
        registro.IndApur.Should().Be(IndicadorApuracaoIpi.Mensal);
        registro.CstIpi.Should().Be("50");
        registro.CodEnq.Should().Be("999");
        registro.VlBcIpi.Should().Be(1450.00m);
        registro.AliqIpi.Should().Be(10.00m);
        registro.VlIpi.Should().Be(145.00m);
        registro.CstPis.Should().Be(50);
        registro.VlBcPis.Should().Be(1450.00m);
        registro.AliqPis.Should().Be(7.6000m);
        registro.QuantBcPis.Should().Be(0.000m);
        registro.AliqPisQuant.Should().Be(0.0000m);
        registro.VlPis.Should().Be(110.20m);
        registro.CstCofins.Should().Be(50);
        registro.VlBcCofins.Should().Be(1450.00m);
        registro.AliqCofins.Should().Be(3.0000m);
        registro.QuantBcCofins.Should().Be(0.000m);
        registro.AliqCofinsQuant.Should().Be(0.0000m);
        registro.VlCofins.Should().Be(43.50m);
        registro.CodCta.Should().Be("CTA001");
        registro.VlAbatNt.Should().Be(0.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C170".AsSpan(), out var meta);
        var registro = (RegistroC170)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.NumItem.Should().Be(0);
        registro.CodItem.Should().BeNull();
        registro.DescrCompl.Should().BeNull();
        registro.Qtd.Should().BeNull();
        registro.Unid.Should().BeNull();
        registro.VlItem.Should().Be(0m);
        registro.VlDesc.Should().BeNull();
        registro.IndMov.Should().BeNull();
        registro.CstIcms.Should().BeNull();
        registro.CodNat.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.AliqIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.VlBcIcmsSt.Should().BeNull();
        registro.AliqSt.Should().BeNull();
        registro.VlIcmsSt.Should().BeNull();
        registro.IndApur.Should().BeNull();
        registro.CstIpi.Should().BeNull();
        registro.CodEnq.Should().BeNull();
        registro.VlBcIpi.Should().BeNull();
        registro.AliqIpi.Should().BeNull();
        registro.VlIpi.Should().BeNull();
        registro.CstPis.Should().BeNull();
        registro.VlBcPis.Should().BeNull();
        registro.AliqPis.Should().BeNull();
        registro.QuantBcPis.Should().BeNull();
        registro.AliqPisQuant.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.CstCofins.Should().BeNull();
        registro.VlBcCofins.Should().BeNull();
        registro.AliqCofins.Should().BeNull();
        registro.QuantBcCofins.Should().BeNull();
        registro.AliqCofinsQuant.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
        registro.VlAbatNt.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Item de NF com ICMS, IPI, PIS e COFINS todos preenchidos.
        const string sped =
            "|C170|1|PROD001|Produto Teste|100,00000|UN|1500,00|50,00|0|100|1102|NAT001|1450,00|12,00|174,00|0,00|0,00|0,00|0|50|999|1450,00|10,00|145,00|50|1450,00|7,6000|0,000|0,0000|110,20|50|1450,00|3,0000|0,000|0,0000|43,50|CTA001|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposMinimos_PreservaTextoCanonico()
    {
        // Apenas campos obrigatórios; todos os opcionais (OC) vazios.
        // Após CFOP (campo 11), há 27 campos opcionais (12-38) + pipe de fechamento = 28 pipes.
        // 39 pipes = 1 REG + 37 campos; após campo 11 (CFOP), restam 28 pipes (campos 12-38 + fechamento).
        const string sped =
            "|C170|1|PROD001||100,00000|UN|1500,00||0|100|1102" + "||||||||||||||||||||||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
