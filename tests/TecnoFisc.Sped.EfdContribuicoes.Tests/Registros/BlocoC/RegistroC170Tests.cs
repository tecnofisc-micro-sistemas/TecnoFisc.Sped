using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC170Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC170).Assembly);

    [Fact]
    public void Atributo_DeclaraC170_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC170).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C170");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC170Com36CamposNaOrdem()
    {
        _catalogo.TentarObter("C170".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C170");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "NumItem", "CodItem", "DescrCompl", "Qtd", "Unid",
            "VlItem", "VlDesc", "IndMov", "CstIcms", "Cfop",
            "CodNat", "VlBcIcms", "AliqIcms", "VlIcms", "VlBcIcmsSt",
            "AliqSt", "VlIcmsSt", "IndApur", "CstIpi", "CodEnq",
            "VlBcIpi", "AliqIpi", "VlIpi", "CstPis", "VlBcPis",
            "AliqPis", "QuantBcPis", "AliqPisQuant", "VlPis", "CstCofins",
            "VlBcCofins", "AliqCofins", "QuantBcCofins", "AliqCofinsQuant", "VlCofins",
            "CodCta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21,
             22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37]);
        meta.Campos[0].Tamanho.Should().Be(3);
        meta.Campos[0].Obrigatorio.Should().BeTrue();
        meta.Campos[1].Tamanho.Should().Be(60);
        meta.Campos[1].Obrigatorio.Should().BeTrue();
        meta.Campos[5].Obrigatorio.Should().BeTrue();  // VlItem
        meta.Campos[9].Obrigatorio.Should().BeTrue();  // Cfop
        meta.Campos[23].Obrigatorio.Should().BeTrue(); // CstPis
        meta.Campos[29].Obrigatorio.Should().BeTrue(); // CstCofins
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C170".AsSpan(), out var meta);
        var registro = (RegistroC170)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());           // NumItem
        meta.Campos[1].Definidor(registro, "PROD001".AsSpan());     // CodItem
        meta.Campos[2].Definidor(registro, "Produto XPTO".AsSpan()); // DescrCompl
        meta.Campos[3].Definidor(registro, "10,00000".AsSpan());    // Qtd
        meta.Campos[4].Definidor(registro, "UN".AsSpan());          // Unid
        meta.Campos[5].Definidor(registro, "1000,00".AsSpan());     // VlItem
        meta.Campos[6].Definidor(registro, "50,00".AsSpan());       // VlDesc
        meta.Campos[7].Definidor(registro, "0".AsSpan());           // IndMov
        meta.Campos[8].Definidor(registro, "0".AsSpan());           // CstIcms
        meta.Campos[9].Definidor(registro, "5102".AsSpan());        // Cfop
        meta.Campos[10].Definidor(registro, "1234".AsSpan());       // CodNat
        meta.Campos[11].Definidor(registro, "950,00".AsSpan());     // VlBcIcms
        meta.Campos[12].Definidor(registro, "12,00".AsSpan());      // AliqIcms
        meta.Campos[13].Definidor(registro, "114,00".AsSpan());     // VlIcms
        meta.Campos[14].Definidor(registro, "0,00".AsSpan());       // VlBcIcmsSt
        meta.Campos[15].Definidor(registro, "0,00".AsSpan());       // AliqSt
        meta.Campos[16].Definidor(registro, "0,00".AsSpan());       // VlIcmsSt
        meta.Campos[17].Definidor(registro, "0".AsSpan());          // IndApur
        meta.Campos[18].Definidor(registro, "50".AsSpan());         // CstIpi
        meta.Campos[19].Definidor(registro, "999".AsSpan());        // CodEnq
        meta.Campos[20].Definidor(registro, "950,00".AsSpan());     // VlBcIpi
        meta.Campos[21].Definidor(registro, "5,00".AsSpan());       // AliqIpi
        meta.Campos[22].Definidor(registro, "47,50".AsSpan());      // VlIpi
        meta.Campos[23].Definidor(registro, "1".AsSpan());          // CstPis
        meta.Campos[24].Definidor(registro, "950,00".AsSpan());     // VlBcPis
        meta.Campos[25].Definidor(registro, "0,6500".AsSpan());     // AliqPis
        meta.Campos[26].Definidor(registro, "10,000".AsSpan());     // QuantBcPis
        meta.Campos[27].Definidor(registro, "0,0100".AsSpan());     // AliqPisQuant
        meta.Campos[28].Definidor(registro, "6,18".AsSpan());       // VlPis
        meta.Campos[29].Definidor(registro, "1".AsSpan());          // CstCofins
        meta.Campos[30].Definidor(registro, "950,00".AsSpan());     // VlBcCofins
        meta.Campos[31].Definidor(registro, "3,0000".AsSpan());     // AliqCofins
        meta.Campos[32].Definidor(registro, "10,000".AsSpan());     // QuantBcCofins
        meta.Campos[33].Definidor(registro, "0,0300".AsSpan());     // AliqCofinsQuant
        meta.Campos[34].Definidor(registro, "28,50".AsSpan());      // VlCofins
        meta.Campos[35].Definidor(registro, "3.1.01.001".AsSpan()); // CodCta

        registro.NumItem.Should().Be(1);
        registro.CodItem.Should().Be("PROD001");
        registro.DescrCompl.Should().Be("Produto XPTO");
        registro.Qtd.Should().Be(10m);
        registro.Unid.Should().Be("UN");
        registro.VlItem.Should().Be(1000m);
        registro.VlDesc.Should().Be(50m);
        registro.IndMov.Should().Be(IndicadorMovimentacaoFisica.Sim);
        registro.CstIcms.Should().Be(0);
        registro.Cfop.Should().Be(Cfop.Create("5102"));
        registro.CodNat.Should().Be("1234");
        registro.VlBcIcms.Should().Be(950m);
        registro.AliqIcms.Should().Be(12m);
        registro.VlIcms.Should().Be(114m);
        registro.VlBcIcmsSt.Should().Be(0m);
        registro.AliqSt.Should().Be(0m);
        registro.VlIcmsSt.Should().Be(0m);
        registro.IndApur.Should().Be(IndicadorApuracaoIpi.Mensal);
        registro.CstIpi.Should().Be("50");
        registro.CodEnq.Should().Be("999");
        registro.VlBcIpi.Should().Be(950m);
        registro.AliqIpi.Should().Be(5m);
        registro.VlIpi.Should().Be(47.5m);
        registro.CstPis.Should().Be(1);
        registro.VlBcPis.Should().Be(950m);
        registro.AliqPis.Should().Be(0.65m);
        registro.QuantBcPis.Should().Be(10m);
        registro.AliqPisQuant.Should().Be(0.01m);
        registro.VlPis.Should().Be(6.18m);
        registro.CstCofins.Should().Be(1);
        registro.VlBcCofins.Should().Be(950m);
        registro.AliqCofins.Should().Be(3m);
        registro.QuantBcCofins.Should().Be(10m);
        registro.AliqCofinsQuant.Should().Be(0.03m);
        registro.VlCofins.Should().Be(28.5m);
        registro.CodCta.Should().Be("3.1.01.001");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("C170".AsSpan(), out var meta);
        var registro = (RegistroC170)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);  // DescrCompl
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);  // Qtd
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);  // Unid
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlDesc
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);  // IndMov
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);  // CstIcms
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty); // CodNat
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcIcms
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqIcms
        meta.Campos[13].Definidor(registro, ReadOnlySpan<char>.Empty); // VlIcms
        meta.Campos[14].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcIcmsSt
        meta.Campos[15].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqSt
        meta.Campos[16].Definidor(registro, ReadOnlySpan<char>.Empty); // VlIcmsSt
        meta.Campos[17].Definidor(registro, ReadOnlySpan<char>.Empty); // IndApur
        meta.Campos[18].Definidor(registro, ReadOnlySpan<char>.Empty); // CstIpi
        meta.Campos[19].Definidor(registro, ReadOnlySpan<char>.Empty); // CodEnq
        meta.Campos[20].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcIpi
        meta.Campos[21].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqIpi
        meta.Campos[22].Definidor(registro, ReadOnlySpan<char>.Empty); // VlIpi
        meta.Campos[24].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcPis
        meta.Campos[25].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqPis
        meta.Campos[26].Definidor(registro, ReadOnlySpan<char>.Empty); // QuantBcPis
        meta.Campos[27].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqPisQuant
        meta.Campos[28].Definidor(registro, ReadOnlySpan<char>.Empty); // VlPis
        meta.Campos[30].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcCofins
        meta.Campos[31].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqCofins
        meta.Campos[32].Definidor(registro, ReadOnlySpan<char>.Empty); // QuantBcCofins
        meta.Campos[33].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqCofinsQuant
        meta.Campos[34].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCofins
        meta.Campos[35].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta

        registro.DescrCompl.Should().BeNull();
        registro.Qtd.Should().BeNull();
        registro.Unid.Should().BeNull();
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
        registro.VlBcPis.Should().BeNull();
        registro.AliqPis.Should().BeNull();
        registro.QuantBcPis.Should().BeNull();
        registro.AliqPisQuant.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlBcCofins.Should().BeNull();
        registro.AliqCofins.Should().BeNull();
        registro.QuantBcCofins.Should().BeNull();
        registro.AliqCofinsQuant.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Theory]
    [InlineData(IndicadorMovimentacaoFisica.Sim, "0")]
    [InlineData(IndicadorMovimentacaoFisica.Nao, "1")]
    public void Serializar_IndMov_RetornaCodigoSpedCorreto(
        IndicadorMovimentacaoFisica valor, string esperado)
    {
        _catalogo.TentarObter("C170".AsSpan(), out var meta);
        var registro = (RegistroC170)meta!.Fabrica();
        registro.IndMov = valor;

        meta.Campos[7].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public void Serializar_IndMovNulo_DevolveVazio()
    {
        _catalogo.TentarObter("C170".AsSpan(), out var meta);
        var registro = (RegistroC170)meta!.Fabrica();
        registro.IndMov = null;

        meta.Campos[7].Serializar(registro).Should().BeEmpty();
    }

    [Theory]
    [InlineData(IndicadorApuracaoIpi.Mensal, "0")]
    [InlineData(IndicadorApuracaoIpi.Decendial, "1")]
    public void Serializar_IndApur_RetornaCodigoSpedCorreto(
        IndicadorApuracaoIpi valor, string esperado)
    {
        _catalogo.TentarObter("C170".AsSpan(), out var meta);
        var registro = (RegistroC170)meta!.Fabrica();
        registro.IndApur = valor;

        meta.Campos[17].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public void Serializar_IndApurNulo_DevolveVazio()
    {
        _catalogo.TentarObter("C170".AsSpan(), out var meta);
        var registro = (RegistroC170)meta!.Fabrica();
        registro.IndApur = null;

        meta.Campos[17].Serializar(registro).Should().BeEmpty();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C170|1|PROD001|Produto XPTO|10,00000|UN|1000,00|50,00|0|0|5102|1234|950,00|12,00|114,00|0,00|0,00|0,00|0|50|999|950,00|5,00|47,50|1|950,00|0,6500|10,000|0,0100|6,18|1|950,00|3,0000|10,000|0,0300|28,50|3.1.01.001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped =
            "|C170|1|PROD001||||1000,00||||5102||||||||||||||1||||||1|||||||\r\n";

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
