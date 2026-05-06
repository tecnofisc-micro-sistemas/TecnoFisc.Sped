using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoF;

public sealed class RegistroF525Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroF525).Assembly);

    [Fact]
    public void Atributo_DeclaraF525_Nivel3_BlocoF()
    {
        var atributo = typeof(RegistroF525).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("F525");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("F");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroF525ComDezCamposNaOrdem()
    {
        _catalogo.TentarObter("F525".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("F525");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "VlRec", "IndRec", "CnpjCpf", "NumDoc", "CodItem",
            "VlRecDet", "CstPis", "CstCofins", "InfoCompl", "CodCta",
        ]);
        meta.Campos[0].Obrigatorio.Should().BeTrue();  // VlRec
        meta.Campos[1].Tamanho.Should().Be(2);
        meta.Campos[1].Obrigatorio.Should().BeTrue();  // IndRec
        meta.Campos[2].Tamanho.Should().Be(14);        // CnpjCpf
        meta.Campos[5].Obrigatorio.Should().BeTrue();  // VlRecDet
        meta.Campos[9].Tamanho.Should().Be(255);       // CodCta
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("F525".AsSpan(), out var meta);
        var registro = (RegistroF525)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "50000,00".AsSpan());       // VlRec
        meta.Campos[1].Definidor(registro, "01".AsSpan());             // IndRec
        meta.Campos[2].Definidor(registro, "12345678000195".AsSpan()); // CnpjCpf
        meta.Campos[3].Definidor(registro, "NF-001234".AsSpan());      // NumDoc
        meta.Campos[4].Definidor(registro, "PROD-001".AsSpan());       // CodItem
        meta.Campos[5].Definidor(registro, "50000,00".AsSpan());       // VlRecDet
        meta.Campos[6].Definidor(registro, "01".AsSpan());             // CstPis
        meta.Campos[7].Definidor(registro, "01".AsSpan());             // CstCofins
        meta.Campos[8].Definidor(registro, "Receita de vendas".AsSpan()); // InfoCompl
        meta.Campos[9].Definidor(registro, "3.1.01.001".AsSpan());     // CodCta

        registro.VlRec.Should().Be(50000m);
        registro.IndRec.Should().Be(IndicadorComposicaoReceitaCaixa.Clientes);
        registro.CnpjCpf.Should().Be("12345678000195");
        registro.NumDoc.Should().Be("NF-001234");
        registro.CodItem.Should().Be("PROD-001");
        registro.VlRecDet.Should().Be(50000m);
        registro.CstPis.Should().Be("01");
        registro.CstCofins.Should().Be("01");
        registro.InfoCompl.Should().Be("Receita de vendas");
        registro.CodCta.Should().Be("3.1.01.001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("F525".AsSpan(), out var meta);
        var registro = (RegistroF525)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // CnpjCpf
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // NumDoc
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // CodItem
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // CstPis
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty); // CstCofins
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty); // InfoCompl
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta

        registro.CnpjCpf.Should().BeNull();
        registro.NumDoc.Should().BeNull();
        registro.CodItem.Should().BeNull();
        registro.CstPis.Should().BeNull();
        registro.CstCofins.Should().BeNull();
        registro.InfoCompl.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Theory]
    [InlineData("01", IndicadorComposicaoReceitaCaixa.Clientes)]
    [InlineData("02", IndicadorComposicaoReceitaCaixa.AdminCartao)]
    [InlineData("03", IndicadorComposicaoReceitaCaixa.TituloCredito)]
    [InlineData("04", IndicadorComposicaoReceitaCaixa.DocumentoFiscal)]
    [InlineData("05", IndicadorComposicaoReceitaCaixa.ItemVendido)]
    [InlineData("99", IndicadorComposicaoReceitaCaixa.Outros)]
    public void Definidor_IndRec_ConverteTodosOsValores(string texto, IndicadorComposicaoReceitaCaixa esperado)
    {
        _catalogo.TentarObter("F525".AsSpan(), out var meta);
        var registro = (RegistroF525)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, texto.AsSpan());

        registro.IndRec.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|F525|50000,00|01|12345678000195|NF-001234|PROD-001|50000,00|01|01|Receita de vendas|3.1.01.001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        const string sped =
            "|F525|30000,00|02||||30000,00|||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_IndRecOutros_PreservaTextoCanonico()
    {
        const string sped =
            "|F525|10000,00|99||||10000,00|||Receitas financeiras diversas||\r\n";

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
