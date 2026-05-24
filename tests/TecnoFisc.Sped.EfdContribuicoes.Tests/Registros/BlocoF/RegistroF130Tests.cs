using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoF;

public sealed class RegistroF130Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroF130).Assembly);

    [Fact]
    public void Atributo_DeclaraF130_Nivel3_BlocoF()
    {
        var atributo = typeof(RegistroF130).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("F130");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("F");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroF130Com20CamposNaOrdem()
    {
        _catalogo.TentarObter("F130".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("F130");
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "NatBcCred", "IdentBemImob", "IndOrigCred", "IndUtilBemImob",
            "MesOperAquis", "VlOperAquis", "ParcOperNaoBcCred", "VlBcCred", "IndNrParc",
            "CstPis", "VlBcPis", "AliqPis", "VlPis",
            "CstCofins", "VlBcCofins", "AliqCofins", "VlCofins",
            "CodCta", "CodCcus", "DescBemImob"
        ]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("F130".AsSpan(), out var meta);
        var registro = (RegistroF130)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "10".AsSpan());                     // NatBcCred
        meta.Campos[1].Definidor(registro, "04".AsSpan());                     // IdentBemImob
        meta.Campos[2].Definidor(registro, "0".AsSpan());                      // IndOrigCred
        meta.Campos[3].Definidor(registro, "1".AsSpan());                      // IndUtilBemImob
        meta.Campos[4].Definidor(registro, "012021".AsSpan());                 // MesOperAquis
        meta.Campos[5].Definidor(registro, "50000,00".AsSpan());               // VlOperAquis
        meta.Campos[6].Definidor(registro, "5000,00".AsSpan());                // ParcOperNaoBcCred
        meta.Campos[7].Definidor(registro, "45000,00".AsSpan());               // VlBcCred
        meta.Campos[8].Definidor(registro, "2".AsSpan());                      // IndNrParc
        meta.Campos[9].Definidor(registro, "50".AsSpan());                     // CstPis
        meta.Campos[10].Definidor(registro, "3750,00".AsSpan());               // VlBcPis
        meta.Campos[11].Definidor(registro, "1,6500".AsSpan());                // AliqPis
        meta.Campos[12].Definidor(registro, "61,88".AsSpan());                 // VlPis
        meta.Campos[13].Definidor(registro, "50".AsSpan());                    // CstCofins
        meta.Campos[14].Definidor(registro, "3750,00".AsSpan());               // VlBcCofins
        meta.Campos[15].Definidor(registro, "7,6000".AsSpan());                // AliqCofins
        meta.Campos[16].Definidor(registro, "285,00".AsSpan());                // VlCofins
        meta.Campos[17].Definidor(registro, "101050025".AsSpan());             // CodCta
        meta.Campos[18].Definidor(registro, "CUSTO001".AsSpan());              // CodCcus
        meta.Campos[19].Definidor(registro, "Máquinas industriais".AsSpan());  // DescBemImob

        registro.NatBcCred.Should().Be("10");
        registro.IdentBemImob.Should().Be(IdentificadorBemImobilizado.Maquinas);
        registro.IndOrigCred.Should().Be(IndicadorOrigemCredito.MercadoInterno);
        registro.IndUtilBemImob.Should().Be(IndicadorUtilizacaoBemImobilizado.ProducaoBensParaVenda);
        registro.MesOperAquis.Should().Be("012021");
        registro.VlOperAquis.Should().Be(50000.00m);
        registro.ParcOperNaoBcCred.Should().Be(5000.00m);
        registro.VlBcCred.Should().Be(45000.00m);
        registro.IndNrParc.Should().Be(IndicadorNumeroParcelas.DozeMeses);
        registro.CstPis.Should().Be("50");
        registro.VlBcPis.Should().Be(3750.00m);
        registro.AliqPis.Should().Be(1.6500m);
        registro.VlPis.Should().Be(61.88m);
        registro.CstCofins.Should().Be("50");
        registro.VlBcCofins.Should().Be(3750.00m);
        registro.AliqCofins.Should().Be(7.6000m);
        registro.VlCofins.Should().Be(285.00m);
        registro.CodCta.Should().Be("101050025");
        registro.CodCcus.Should().Be("CUSTO001");
        registro.DescBemImob.Should().Be("Máquinas industriais");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("F130".AsSpan(), out var meta);
        var registro = (RegistroF130)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);   // IndOrigCred
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);   // MesOperAquis
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);   // ParcOperNaoBcCred
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlBcPis
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqPis
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlPis
        meta.Campos[14].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlBcCofins
        meta.Campos[15].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqCofins
        meta.Campos[16].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlCofins
        meta.Campos[17].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCta
        meta.Campos[18].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCcus
        meta.Campos[19].Definidor(registro, ReadOnlySpan<char>.Empty);  // DescBemImob

        registro.IndOrigCred.Should().BeNull();
        registro.MesOperAquis.Should().BeNull();
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
    [InlineData(IndicadorNumeroParcelas.Integral, "1")]
    [InlineData(IndicadorNumeroParcelas.DozeMeses, "2")]
    [InlineData(IndicadorNumeroParcelas.VinteQuatroMeses, "3")]
    [InlineData(IndicadorNumeroParcelas.QuarentaOitoMeses, "4")]
    [InlineData(IndicadorNumeroParcelas.SeisMeses, "5")]
    [InlineData(IndicadorNumeroParcelas.OutraPeriodicidade, "9")]
    public void Serializar_IndNrParc_RetornaCodigoSpedCorreto(
        IndicadorNumeroParcelas indNrParc, string esperado)
    {
        _catalogo.TentarObter("F130".AsSpan(), out var meta);
        var registro = (RegistroF130)meta!.Fabrica();
        registro.IndNrParc = indNrParc;

        meta.Campos[8].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|F130|10|04|0|1|012021|50000,00|5000,00|45000,00|2|50|3750,00|1,6500|61,88|50|3750,00|7,6000|285,00|101050025|CUSTO001|Máquinas industriais|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        const string sped =
            "|F130|10|01||1||100000,00||100000,00|1|50||||50|||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_Importacao_PreservaTextoCanonico()
    {
        const string sped =
            "|F130|10|05|1|2|032020|200000,00|20000,00|180000,00|2|50|15000,00|1,6500|247,50|50|15000,00|7,6000|1140,00||||\r\n";

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
