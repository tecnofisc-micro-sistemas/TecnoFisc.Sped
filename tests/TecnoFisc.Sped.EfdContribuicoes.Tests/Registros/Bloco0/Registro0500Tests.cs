using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco0;

public sealed class Registro0500Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0500).Assembly);

    [Fact]
    public void Atributo_Declara0500_Nivel2_Bloco0()
    {
        var atributo = typeof(Registro0500).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0500");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0500Com8CamposNaOrdem()
    {
        _catalogo.TentarObter("0500".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0500");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "DtAlt",
            "CodNatCc",
            "IndCta",
            "Nivel",
            "CodCta",
            "NomeCta",
            "CodCtaRef",
            "CnpjEst",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0500".AsSpan(), out var meta);
        var registro = (Registro0500)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01062021".AsSpan());
        meta.Campos[1].Definidor(registro, "01".AsSpan());
        meta.Campos[2].Definidor(registro, "A".AsSpan());
        meta.Campos[3].Definidor(registro, "3".AsSpan());
        meta.Campos[4].Definidor(registro, "1.1.1.01".AsSpan());
        meta.Campos[5].Definidor(registro, "Caixa e Equivalentes de Caixa".AsSpan());
        meta.Campos[6].Definidor(registro, "1.1.1".AsSpan());
        meta.Campos[7].Definidor(registro, "11222333000181".AsSpan());

        registro.DtAlt.Should().Be(new DateOnly(2021, 6, 1));
        registro.CodNatCc.Should().Be(CodigoNaturezaContaContabil.ContasDeAtivo);
        registro.IndCta.Should().Be("A");
        registro.Nivel.Should().Be(3);
        registro.CodCta.Should().Be("1.1.1.01");
        registro.NomeCta.Should().Be("Caixa e Equivalentes de Caixa");
        registro.CodCtaRef.Should().Be("1.1.1");
        registro.CnpjEst!.Value.ToString().Should().Be("11222333000181");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("0500".AsSpan(), out var meta);
        var registro = (Registro0500)meta!.Fabrica();

        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodCtaRef.Should().BeNull();
        registro.CnpjEst.Should().BeNull();
    }

    [Theory]
    [InlineData(CodigoNaturezaContaContabil.ContasDeAtivo, "01")]
    [InlineData(CodigoNaturezaContaContabil.ContasDePassivo, "02")]
    [InlineData(CodigoNaturezaContaContabil.PatrimonioLiquido, "03")]
    [InlineData(CodigoNaturezaContaContabil.ContasDeResultado, "04")]
    [InlineData(CodigoNaturezaContaContabil.ContasDeCompensacao, "05")]
    [InlineData(CodigoNaturezaContaContabil.Outras, "09")]
    public void Serializar_CodNatCc_RetornaCodigoSpedComDoisDigitos(
        CodigoNaturezaContaContabil natureza, string esperado)
    {
        _catalogo.TentarObter("0500".AsSpan(), out var meta);
        var registro = (Registro0500)meta!.Fabrica();
        registro.CodNatCc = natureza;

        meta.Campos[1].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0500|01062021|01|A|3|1.1.1.01|Caixa e Equivalentes de Caixa|1.1.1|11222333000181|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|0500|01012021|03|S|1|PL|Patrimônio Líquido|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerStreamingAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}
