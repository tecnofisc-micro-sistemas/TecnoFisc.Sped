using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoF;

public sealed class RegistroF120Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroF120).Assembly);

    [Fact]
    public void Atributo_DeclaraF120_Nivel3_BlocoF()
    {
        var atributo = typeof(RegistroF120).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("F120");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("F");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroF120Com17CamposNaOrdem()
    {
        _catalogo.TentarObter("F120".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("F120");
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "NatBcCred", "IdentBemImob", "IndOrigCred", "IndUtilBemImob",
            "VlOperDep", "ParcOperNaoBcCred",
            "CstPis", "VlBcPis", "AliqPis", "VlPis",
            "CstCofins", "VlBcCofins", "AliqCofins", "VlCofins",
            "CodCta", "CodCcus", "DescBemImob"
        ]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("F120".AsSpan(), out var meta);
        var registro = (RegistroF120)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "09".AsSpan());                     // NatBcCred
        meta.Campos[1].Definidor(registro, "04".AsSpan());                     // IdentBemImob
        meta.Campos[2].Definidor(registro, "0".AsSpan());                      // IndOrigCred
        meta.Campos[3].Definidor(registro, "1".AsSpan());                      // IndUtilBemImob
        meta.Campos[4].Definidor(registro, "5000,00".AsSpan());                // VlOperDep
        meta.Campos[5].Definidor(registro, "500,00".AsSpan());                 // ParcOperNaoBcCred
        meta.Campos[6].Definidor(registro, "50".AsSpan());                     // CstPis
        meta.Campos[7].Definidor(registro, "4500,00".AsSpan());                // VlBcPis
        meta.Campos[8].Definidor(registro, "1,6500".AsSpan());                 // AliqPis
        meta.Campos[9].Definidor(registro, "74,25".AsSpan());                  // VlPis
        meta.Campos[10].Definidor(registro, "50".AsSpan());                    // CstCofins
        meta.Campos[11].Definidor(registro, "4500,00".AsSpan());               // VlBcCofins
        meta.Campos[12].Definidor(registro, "7,6000".AsSpan());                // AliqCofins
        meta.Campos[13].Definidor(registro, "342,00".AsSpan());                // VlCofins
        meta.Campos[14].Definidor(registro, "101050025".AsSpan());             // CodCta
        meta.Campos[15].Definidor(registro, "CUSTO001".AsSpan());              // CodCcus
        meta.Campos[16].Definidor(registro, "Máquinas linha produção".AsSpan()); // DescBemImob

        registro.NatBcCred.Should().Be("09");
        registro.IdentBemImob.Should().Be(IdentificadorBemImobilizado.Maquinas);
        registro.IndOrigCred.Should().Be(IndicadorOrigemCredito.MercadoInterno);
        registro.IndUtilBemImob.Should().Be(IndicadorUtilizacaoBemImobilizado.ProducaoBensParaVenda);
        registro.VlOperDep.Should().Be(5000.00m);
        registro.ParcOperNaoBcCred.Should().Be(500.00m);
        registro.CstPis.Should().Be("50");
        registro.VlBcPis.Should().Be(4500.00m);
        registro.AliqPis.Should().Be(1.6500m);
        registro.VlPis.Should().Be(74.25m);
        registro.CstCofins.Should().Be("50");
        registro.VlBcCofins.Should().Be(4500.00m);
        registro.AliqCofins.Should().Be(7.6000m);
        registro.VlCofins.Should().Be(342.00m);
        registro.CodCta.Should().Be("101050025");
        registro.CodCcus.Should().Be("CUSTO001");
        registro.DescBemImob.Should().Be("Máquinas linha produção");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("F120".AsSpan(), out var meta);
        var registro = (RegistroF120)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);   // IndOrigCred
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);   // ParcOperNaoBcCred
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);   // VlBcPis
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);   // AliqPis
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);   // VlPis
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlBcCofins
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqCofins
        meta.Campos[13].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlCofins
        meta.Campos[14].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCta
        meta.Campos[15].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCcus
        meta.Campos[16].Definidor(registro, ReadOnlySpan<char>.Empty);  // DescBemImob

        registro.IndOrigCred.Should().BeNull();
        registro.ParcOperNaoBcCred.Should().BeNull();
        registro.VlBcPis.Should().BeNull();
        registro.AliqPis.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlBcCofins.Should().BeNull();
        registro.AliqCofins.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
        registro.CodCcus.Should().BeNull();
        registro.DescBemImob.Should().BeNull();
    }

    [Theory]
    [InlineData(IdentificadorBemImobilizado.EdificacoesBenfeitoriasImovelProprio, "01")]
    [InlineData(IdentificadorBemImobilizado.EdificacoesBenfeitoriasImovelTerceiros, "02")]
    [InlineData(IdentificadorBemImobilizado.Instalacoes, "03")]
    [InlineData(IdentificadorBemImobilizado.Maquinas, "04")]
    [InlineData(IdentificadorBemImobilizado.Equipamentos, "05")]
    [InlineData(IdentificadorBemImobilizado.Veiculos, "06")]
    [InlineData(IdentificadorBemImobilizado.Outros, "99")]
    public void Serializar_IdentBemImob_RetornaCodigoSpedCorreto(
        IdentificadorBemImobilizado ident, string esperado)
    {
        _catalogo.TentarObter("F120".AsSpan(), out var meta);
        var registro = (RegistroF120)meta!.Fabrica();
        registro.IdentBemImob = ident;

        meta.Campos[1].Serializar(registro).Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorUtilizacaoBemImobilizado.ProducaoBensParaVenda, "1")]
    [InlineData(IndicadorUtilizacaoBemImobilizado.PrestacaoServicos, "2")]
    [InlineData(IndicadorUtilizacaoBemImobilizado.LocacaoTerceiros, "3")]
    [InlineData(IndicadorUtilizacaoBemImobilizado.Outros, "9")]
    public void Serializar_IndUtilBemImob_RetornaCodigoSpedCorreto(
        IndicadorUtilizacaoBemImobilizado indUtil, string esperado)
    {
        _catalogo.TentarObter("F120".AsSpan(), out var meta);
        var registro = (RegistroF120)meta!.Fabrica();
        registro.IndUtilBemImob = indUtil;

        meta.Campos[3].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|F120|09|04|0|1|5000,00|500,00|50|4500,00|1,6500|74,25|50|4500,00|7,6000|342,00|101050025|CUSTO001|Máquinas linha produção|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_Amortizacao_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        const string sped =
            "|F120|11|01||2|3000,00||50||||50|||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_Importacao_PreservaTextoCanonico()
    {
        const string sped =
            "|F120|09|05|1|1|8000,00||50|8000,00|1,6500|132,00|50|8000,00|7,6000|608,00||||\r\n";

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
