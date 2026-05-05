using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoD;

public sealed class RegistroD101Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD101).Assembly);

    [Fact]
    public void Atributo_DeclaraD101_Nivel4_BlocoD()
    {
        var atributo = typeof(RegistroD101).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D101");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD101ComOitoCamposNaOrdem()
    {
        _catalogo.TentarObter("D101".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D101");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "IndNatFrt", "VlItem", "CstPis", "NatBcCred", "VlBcPis", "AliqPis", "VlPis", "CodCta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9]);
        meta.Campos[0].Tamanho.Should().Be(1);
        meta.Campos[0].Obrigatorio.Should().BeTrue();    // IndNatFrt
        meta.Campos[1].Obrigatorio.Should().BeTrue();    // VlItem
        meta.Campos[2].Tamanho.Should().Be(2);
        meta.Campos[2].Obrigatorio.Should().BeTrue();    // CstPis
        meta.Campos[3].Tamanho.Should().Be(2);
        meta.Campos[3].Obrigatorio.Should().BeFalse();   // NatBcCred
        meta.Campos[4].Obrigatorio.Should().BeFalse();   // VlBcPis
        meta.Campos[5].Tamanho.Should().Be(8);
        meta.Campos[5].Obrigatorio.Should().BeFalse();   // AliqPis
        meta.Campos[6].Obrigatorio.Should().BeFalse();   // VlPis
        meta.Campos[7].Tamanho.Should().Be(255);
        meta.Campos[7].Obrigatorio.Should().BeFalse();   // CodCta
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D101".AsSpan(), out var meta);
        var registro = (RegistroD101)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "2".AsSpan());              // IndNatFrt
        meta.Campos[1].Definidor(registro, "1000,00".AsSpan());        // VlItem
        meta.Campos[2].Definidor(registro, "50".AsSpan());             // CstPis
        meta.Campos[3].Definidor(registro, "14".AsSpan());             // NatBcCred
        meta.Campos[4].Definidor(registro, "950,00".AsSpan());         // VlBcPis
        meta.Campos[5].Definidor(registro, "1,6500".AsSpan());         // AliqPis
        meta.Campos[6].Definidor(registro, "15,68".AsSpan());          // VlPis
        meta.Campos[7].Definidor(registro, "3.1.01.001".AsSpan());     // CodCta

        registro.IndNatFrt.Should().Be(IndicadorNaturezaFrete.ComprasGeradoresCredito);
        registro.VlItem.Should().Be(1000m);
        registro.CstPis.Should().Be(50);
        registro.NatBcCred.Should().Be(CodigoBaseCalculoCredito.TransporteCargasSubcontratacao);
        registro.VlBcPis.Should().Be(950m);
        registro.AliqPis.Should().Be(1.65m);
        registro.VlPis.Should().Be(15.68m);
        registro.CodCta.Should().Be("3.1.01.001");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("D101".AsSpan(), out var meta);
        var registro = (RegistroD101)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);  // NatBcCred
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlBcPis
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqPis
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlPis
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCta

        registro.NatBcCred.Should().BeNull();
        registro.VlBcPis.Should().BeNull();
        registro.AliqPis.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Theory]
    [InlineData(IndicadorNaturezaFrete.VendasOnusVendedor, "0")]
    [InlineData(IndicadorNaturezaFrete.VendasOnusAdquirente, "1")]
    [InlineData(IndicadorNaturezaFrete.ComprasGeradoresCredito, "2")]
    [InlineData(IndicadorNaturezaFrete.ComprasNaoGeradoresCredito, "3")]
    [InlineData(IndicadorNaturezaFrete.TransferenciaProdutosAcabados, "4")]
    [InlineData(IndicadorNaturezaFrete.TransferenciaProdutosEmElaboracao, "5")]
    [InlineData(IndicadorNaturezaFrete.Outras, "9")]
    public void Serializar_IndNatFrt_RetornaCodigoSpedCorreto(
        IndicadorNaturezaFrete natureza, string esperado)
    {
        _catalogo.TentarObter("D101".AsSpan(), out var meta);
        var registro = (RegistroD101)meta!.Fabrica();
        registro.IndNatFrt = natureza;

        meta.Campos[0].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|D101|2|1000,00|50|14|950,00|1,6500|15,68|3.1.01.001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // Operação de compras sem direito a crédito (CST 70), sem base/alíquota/valor PIS.
        const string sped = "|D101|3|500,00|70||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_Subcontratacao_PreservaTextoCanonico()
    {
        // Subcontratação de transporte de cargas, CST de crédito presumido (60).
        const string sped = "|D101|9|2000,00|60|14|2000,00|1,2375|24,75||\r\n";

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
