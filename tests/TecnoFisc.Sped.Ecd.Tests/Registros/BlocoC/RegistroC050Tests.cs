using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoC;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 10.011 — exercita a forma do <see cref="RegistroC050"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 93): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroC050Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000).Assembly);

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
    public void Atributo_DeclaraC050_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC050).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C050");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC050Com7CamposNaOrdem()
    {
        _catalogo.TentarObter("C050".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C050");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtAlt", "CodNat", "IndCta", "Nivel", "CodCta", "CodCtaSup", "Cta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C050".AsSpan(), out var meta);
        var registro = (RegistroC050)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01012020".AsSpan());
        meta.Campos[1].Definidor(registro, "01".AsSpan());
        meta.Campos[2].Definidor(registro, "A".AsSpan());
        meta.Campos[3].Definidor(registro, "1".AsSpan());
        meta.Campos[4].Definidor(registro, "1.01".AsSpan());
        meta.Campos[5].Definidor(registro, "1".AsSpan());
        meta.Campos[6].Definidor(registro, "Caixa e Equivalentes de Caixa".AsSpan());

        registro.DtAlt.Should().Be(new DateOnly(2020, 1, 1));
        registro.CodNat.Should().Be("01");
        registro.IndCta.Should().Be(IndicadorTipoConta.Analitica);
        registro.Nivel.Should().Be(1);
        registro.CodCta.Should().Be("1.01");
        registro.CodCtaSup.Should().Be("1");
        registro.Cta.Should().Be("Caixa e Equivalentes de Caixa");
    }

    [Fact]
    public void Definidor_CampoOpcionalVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C050".AsSpan(), out var meta);
        var registro = (RegistroC050)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCtaSup

        registro.CodCtaSup.Should().BeNull();
    }

    [Theory]
    [InlineData(IndicadorTipoConta.Analitica, "A")]
    [InlineData(IndicadorTipoConta.Sintetica, "S")]
    public void Serializar_IndCta_RetornaCodigo(IndicadorTipoConta tipo, string esperado)
    {
        _catalogo.TentarObter("C050".AsSpan(), out var meta);
        var registro = (RegistroC050)meta!.Fabrica();
        registro.IndCta = tipo;

        meta.Campos[2].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C050|01012020|01|A|1|1.01|1|Caixa e Equivalentes de Caixa|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCodCtaSupVazio_PreservaTextoCanonico()
    {
        const string sped =
            "|C050|31122024|03|S|1|ATIVO||Ativo|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
