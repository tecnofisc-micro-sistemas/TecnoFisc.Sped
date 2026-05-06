using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco0;

public sealed class Registro0200Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0200).Assembly);

    [Fact]
    public void Atributo_DeclaraCodigo0200_Nivel3_Bloco0()
    {
        var atributo = typeof(Registro0200).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0200");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0200ComOnzeCamposNaOrdem()
    {
        _catalogo.TentarObter("0200".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0200");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodItem",
            "DescrItem",
            "CodBarra",
            "CodAntItem",
            "UnidInv",
            "TipoItem",
            "CodNcm",
            "ExIpi",
            "CodGen",
            "CodLst",
            "AliqIcms",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0200".AsSpan(), out var meta);
        var registro = (Registro0200)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "PROD001".AsSpan());
        meta.Campos[1].Definidor(registro, "Produto de Revenda".AsSpan());
        meta.Campos[2].Definidor(registro, "7891234567890".AsSpan());
        meta.Campos[3].Definidor(registro, "PROD-ANT".AsSpan());
        meta.Campos[4].Definidor(registro, "UN".AsSpan());
        meta.Campos[5].Definidor(registro, "00".AsSpan());
        meta.Campos[6].Definidor(registro, "12345678".AsSpan());
        meta.Campos[7].Definidor(registro, "001".AsSpan());
        meta.Campos[8].Definidor(registro, "03".AsSpan());
        meta.Campos[9].Definidor(registro, "1234".AsSpan());
        meta.Campos[10].Definidor(registro, "12,50".AsSpan());

        registro.CodItem.Should().Be("PROD001");
        registro.DescrItem.Should().Be("Produto de Revenda");
        registro.CodBarra.Should().Be("7891234567890");
        registro.CodAntItem.Should().Be("PROD-ANT");
        registro.UnidInv.Should().Be("UN");
        registro.TipoItem.Should().Be(TipoItem.MercadoriaParaRevenda);
        registro.CodNcm.Should().NotBeNull();
        registro.CodNcm!.Value.ToString().Should().Be("12345678");
        registro.ExIpi.Should().Be("001");
        registro.CodGen.Should().Be("03");
        registro.CodLst.Should().Be("1234");
        registro.AliqIcms.Should().Be(12.50m);
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("0200".AsSpan(), out var meta);
        var registro = (Registro0200)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodBarra.Should().BeNull();
        registro.CodAntItem.Should().BeNull();
        registro.UnidInv.Should().BeNull();
        registro.CodNcm.Should().BeNull();
        registro.ExIpi.Should().BeNull();
        registro.CodGen.Should().BeNull();
        registro.CodLst.Should().BeNull();
        registro.AliqIcms.Should().BeNull();
    }

    [Theory]
    [InlineData(TipoItem.MercadoriaParaRevenda, "00")]
    [InlineData(TipoItem.MateriaPrima, "01")]
    [InlineData(TipoItem.Embalagem, "02")]
    [InlineData(TipoItem.ProdutoEmProcesso, "03")]
    [InlineData(TipoItem.ProdutoAcabado, "04")]
    [InlineData(TipoItem.Subproduto, "05")]
    [InlineData(TipoItem.ProdutoIntermediario, "06")]
    [InlineData(TipoItem.MaterialDeUsoEConsumo, "07")]
    [InlineData(TipoItem.AtivoImobilizado, "08")]
    [InlineData(TipoItem.Servicos, "09")]
    [InlineData(TipoItem.OutrosInsumos, "10")]
    [InlineData(TipoItem.Outras, "99")]
    public void Serializar_TipoItem_RetornaCodigoSped(TipoItem valor, string esperado)
    {
        _catalogo.TentarObter("0200".AsSpan(), out var meta);
        var registro = (Registro0200)meta!.Fabrica();
        registro.TipoItem = valor;

        meta.Campos[5].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0200|PROD001|Produto de Revenda||PROD-ANT|UN|00|12345678|001|03|1234|12,50|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SomenteObrigatorios_PreservaTextoCanonico()
    {
        const string sped = "|0200|SERV001|Servico de Consultoria||||09||||||\r\n";

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
