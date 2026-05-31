using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 8.020 — exercita a forma do <see cref="Registro0500"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 42): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class Registro0500Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0500).Assembly);

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
    public void Atributo_Declara0500_Nivel2_Bloco0()
    {
        var atributo = typeof(Registro0500).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0500");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0500Com6CamposNaOrdem()
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
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
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

        registro.DtAlt.Should().Be(new DateOnly(2021, 6, 1));
        registro.CodNatCc.Should().Be(CodigoNaturezaContaContabil.ContasDeAtivo);
        registro.IndCta.Should().Be("A");
        registro.Nivel.Should().Be(3);
        registro.CodCta.Should().Be("1.1.1.01");
        registro.NomeCta.Should().Be("Caixa e Equivalentes de Caixa");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0500".AsSpan(), out var meta);
        var registro = (Registro0500)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, Span<char>.Empty);

        registro.IndCta.Should().BeNull();
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
        const string sped = "|0500|01062021|01|A|3|1.1.1.01|Caixa e Equivalentes de Caixa|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ContaSintetica_PreservaTextoCanonico()
    {
        const string sped = "|0500|01012021|02|S|1|2|Passivo|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
