using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.025 — exercita a forma do <see cref="RegistroI050"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 117–122): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI050Tests
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
    public void Atributo_DeclaraI050_Nivel3_BlocoI()
    {
        var atributo = typeof(RegistroI050).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I050");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI050Com7CamposNaOrdem()
    {
        _catalogo.TentarObter("I050".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I050");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtAlt", "CodNat", "IndCta", "Nivel", "CodCta", "CodCtaSup", "Cta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I050".AsSpan(), out var meta);
        var registro = (RegistroI050)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01012023".AsSpan());
        meta.Campos[1].Definidor(registro, "01".AsSpan());
        meta.Campos[2].Definidor(registro, "A".AsSpan());
        meta.Campos[3].Definidor(registro, "4".AsSpan());
        meta.Campos[4].Definidor(registro, "1.1.1.1".AsSpan());
        meta.Campos[5].Definidor(registro, "1.1.1".AsSpan());
        meta.Campos[6].Definidor(registro, "Ativo Analítica 1".AsSpan());

        registro.DtAlt.Should().Be(new DateOnly(2023, 1, 1));
        registro.CodNat.Should().Be("01");
        registro.IndCta.Should().Be(IndicadorTipoConta.Analitica);
        registro.Nivel.Should().Be(4);
        registro.CodCta.Should().Be("1.1.1.1");
        registro.CodCtaSup.Should().Be("1.1.1");
        registro.Cta.Should().Be("Ativo Analítica 1");
    }

    [Fact]
    public void Definidor_CampoOpcionalVazio_DevolveNulo()
    {
        _catalogo.TentarObter("I050".AsSpan(), out var meta);
        var registro = (RegistroI050)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCtaSup

        registro.CodCtaSup.Should().BeNull();
    }

    [Theory]
    [InlineData(IndicadorTipoConta.Analitica, "A")]
    [InlineData(IndicadorTipoConta.Sintetica, "S")]
    public void Serializar_IndCta_RetornaCodigo(IndicadorTipoConta tipo, string esperado)
    {
        _catalogo.TentarObter("I050".AsSpan(), out var meta);
        var registro = (RegistroI050)meta!.Fabrica();
        registro.IndCta = tipo;

        meta.Campos[2].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual: conta analítica nível 4
        const string sped = "|I050|01012023|01|A|4|1.1.1.1|1.1.1|Ativo Analítica 1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ContaSinteticaNivel1_SemCodCtaSup_PreservaTextoCanonico()
    {
        // Conta sintética de nível 1 não tem conta superior
        const string sped = "|I050|01012023|01|S|1|1||Ativo Sintética 1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ExemploDoManual_ContaAnaliticaNivel4_PreservaTextoCanonico()
    {
        // Segundo exemplo analítico do manual (p. 121): nível 4, cod 1.1.1.2
        const string sped = "|I050|01012023|01|A|4|1.1.1.2|1.1.1|Ativo Analítica 2|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
