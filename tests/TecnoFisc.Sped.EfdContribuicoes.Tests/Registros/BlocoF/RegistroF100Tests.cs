using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoF;

public sealed class RegistroF100Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroF100).Assembly);

    [Fact]
    public void Atributo_DeclaraF100_Nivel3_BlocoF()
    {
        var atributo = typeof(RegistroF100).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("F100");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("F");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroF100Com18CamposNaOrdem()
    {
        _catalogo.TentarObter("F100".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("F100");
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "IndOper", "CodPart", "CodItem", "DtOper", "VlOper",
            "CstPis", "VlBcPis", "AliqPis", "VlPis",
            "CstCofins", "VlBcCofins", "AliqCofins", "VlCofins",
            "NatBcCred", "IndOrigCred", "CodCta", "CodCcus", "DescDocOper"
        ]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("F100".AsSpan(), out var meta);
        var registro = (RegistroF100)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());                      // IndOper
        meta.Campos[1].Definidor(registro, "FORNEC001".AsSpan());              // CodPart
        meta.Campos[2].Definidor(registro, "ITEM001".AsSpan());                // CodItem
        meta.Campos[3].Definidor(registro, "31012023".AsSpan());               // DtOper
        meta.Campos[4].Definidor(registro, "1000,00".AsSpan());                // VlOper
        meta.Campos[5].Definidor(registro, "50".AsSpan());                     // CstPis (string)
        meta.Campos[6].Definidor(registro, "1000,0000".AsSpan());              // VlBcPis
        meta.Campos[7].Definidor(registro, "1,6500".AsSpan());                 // AliqPis
        meta.Campos[8].Definidor(registro, "16,50".AsSpan());                  // VlPis
        meta.Campos[9].Definidor(registro, "50".AsSpan());                     // CstCofins (string)
        meta.Campos[10].Definidor(registro, "1000,0000".AsSpan());             // VlBcCofins
        meta.Campos[11].Definidor(registro, "7,6000".AsSpan());                // AliqCofins
        meta.Campos[12].Definidor(registro, "76,00".AsSpan());                 // VlCofins
        meta.Campos[13].Definidor(registro, "13".AsSpan());                    // NatBcCred
        meta.Campos[14].Definidor(registro, "0".AsSpan());                     // IndOrigCred
        meta.Campos[15].Definidor(registro, "101050025".AsSpan());             // CodCta
        meta.Campos[16].Definidor(registro, "CUSTO001".AsSpan());              // CodCcus
        meta.Campos[17].Definidor(registro, "Arrendamento mercantil".AsSpan()); // DescDocOper

        registro.IndOper.Should().Be(IndicadorOperacaoF100.AquisicaoCustosEncargosComCredito);
        registro.CodPart.Should().Be("FORNEC001");
        registro.CodItem.Should().Be("ITEM001");
        registro.DtOper.Should().Be(new DateOnly(2023, 1, 31));
        registro.VlOper.Should().Be(1000.00m);
        registro.CstPis.Should().Be("50");
        registro.VlBcPis.Should().Be(1000.0000m);
        registro.AliqPis.Should().Be(1.6500m);
        registro.VlPis.Should().Be(16.50m);
        registro.CstCofins.Should().Be("50");
        registro.VlBcCofins.Should().Be(1000.0000m);
        registro.AliqCofins.Should().Be(7.6000m);
        registro.VlCofins.Should().Be(76.00m);
        registro.NatBcCred.Should().Be("13");
        registro.IndOrigCred.Should().Be(IndicadorOrigemCredito.MercadoInterno);
        registro.CodCta.Should().Be("101050025");
        registro.CodCcus.Should().Be("CUSTO001");
        registro.DescDocOper.Should().Be("Arrendamento mercantil");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("F100".AsSpan(), out var meta);
        var registro = (RegistroF100)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);   // CodPart
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);   // CodItem
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);   // VlBcPis
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);   // AliqPis
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);   // VlPis
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlBcCofins
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqCofins
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlCofins
        meta.Campos[13].Definidor(registro, ReadOnlySpan<char>.Empty);  // NatBcCred
        meta.Campos[14].Definidor(registro, ReadOnlySpan<char>.Empty);  // IndOrigCred
        meta.Campos[15].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCta
        meta.Campos[16].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCcus
        meta.Campos[17].Definidor(registro, ReadOnlySpan<char>.Empty);  // DescDocOper

        registro.CodPart.Should().BeNull();
        registro.CodItem.Should().BeNull();
        registro.VlBcPis.Should().BeNull();
        registro.AliqPis.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlBcCofins.Should().BeNull();
        registro.AliqCofins.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.NatBcCred.Should().BeNull();
        registro.IndOrigCred.Should().BeNull();
        registro.CodCta.Should().BeNull();
        registro.CodCcus.Should().BeNull();
        registro.DescDocOper.Should().BeNull();
    }

    [Theory]
    [InlineData(IndicadorOperacaoF100.AquisicaoCustosEncargosComCredito, "0")]
    [InlineData(IndicadorOperacaoF100.ReceitaAuferidaSujeitaContribuicao, "1")]
    [InlineData(IndicadorOperacaoF100.ReceitaAuferidaNaoSujeitaContribuicao, "2")]
    public void Serializar_IndOper_RetornaCodigoSpedCorreto(
        IndicadorOperacaoF100 indOper, string esperado)
    {
        _catalogo.TentarObter("F100".AsSpan(), out var meta);
        var registro = (RegistroF100)meta!.Fabrica();
        registro.IndOper = indOper;

        meta.Campos[0].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|F100|0|FORNEC001|ITEM001|31012023|1000,00|50|1000,0000|1,6500|16,50|50|1000,0000|7,6000|76,00|13|0|101050025|CUSTO001|Arrendamento mercantil|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // COD_PART, COD_ITEM, VL_BC_PIS, ALIQ_PIS, VL_PIS, VL_BC_COFINS, ALIQ_COFINS, VL_COFINS,
        // NAT_BC_CRED, IND_ORIG_CRED, COD_CTA, COD_CCUS, DESC_DOC_OPER omitidos.
        const string sped =
            "|F100|1|||31012023|50000,00|01||||01|||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ReceitaNaoSujeitaContribuicao_PreservaTextoCanonico()
    {
        const string sped =
            "|F100|2|||31012023|25000,00|04||||07|||||||||\r\n";

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
