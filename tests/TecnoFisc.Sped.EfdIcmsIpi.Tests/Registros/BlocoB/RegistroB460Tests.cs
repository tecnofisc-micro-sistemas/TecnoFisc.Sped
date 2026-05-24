using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoB;

/// <summary>
/// Sub-stage 8.031 — exercita a forma do <see cref="RegistroB460"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 54): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroB460Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroB460).Assembly);

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
    public void Atributo_DeclaraB460_Nivel2_BlocoB()
    {
        var atributo = typeof(RegistroB460).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("B460");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("B");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroB460Com7CamposNaOrdem()
    {
        _catalogo.TentarObter("B460".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("B460");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndDed", "VlDed", "NumProc", "IndProc", "Proc", "CodInfObs", "IndObr",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 7));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("B460".AsSpan(), out var meta);
        var registro = (RegistroB460)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "2".AsSpan());                       // IndDed
        meta.Campos[1].Definidor(registro, "1500,00".AsSpan());                 // VlDed
        meta.Campos[2].Definidor(registro, "PROC-2024/001".AsSpan());           // NumProc
        meta.Campos[3].Definidor(registro, "1".AsSpan());                       // IndProc
        meta.Campos[4].Definidor(registro, "Decisao judicial 2024".AsSpan());   // Proc
        meta.Campos[5].Definidor(registro, "OBS-001".AsSpan());                 // CodInfObs
        meta.Campos[6].Definidor(registro, "0".AsSpan());                       // IndObr

        registro.IndDed.Should().Be(IndicadorDeducaoIss.DecisaoAdministrativaOuJudicial);
        registro.VlDed.Should().Be(1500.00m);
        registro.NumProc.Should().Be("PROC-2024/001");
        registro.IndProc.Should().Be(IndicadorOrigemProcessoIss.JusticaFederal);
        registro.Proc.Should().Be("Decisao judicial 2024");
        registro.CodInfObs.Should().Be("OBS-001");
        registro.IndObr.Should().Be(IndicadorObrigacaoIss.IssProprio);
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("B460".AsSpan(), out var meta);
        var registro = (RegistroB460)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, Span<char>.Empty); // NumProc
        meta.Campos[3].Definidor(registro, Span<char>.Empty); // IndProc
        meta.Campos[4].Definidor(registro, Span<char>.Empty); // Proc

        registro.NumProc.Should().BeNull();
        registro.IndProc.Should().BeNull();
        registro.Proc.Should().BeNull();
    }

    [Theory]
    [InlineData(IndicadorDeducaoIss.Compensacao, "0")]
    [InlineData(IndicadorDeducaoIss.BeneficioFiscalCultura, "1")]
    [InlineData(IndicadorDeducaoIss.DecisaoAdministrativaOuJudicial, "2")]
    [InlineData(IndicadorDeducaoIss.Outros, "9")]
    public void Serializar_IndDed_RetornaCodigo(IndicadorDeducaoIss deducao, string esperado)
    {
        _catalogo.TentarObter("B460".AsSpan(), out var meta);
        var registro = (RegistroB460)meta!.Fabrica();
        registro.IndDed = deducao;

        meta.Campos[0].Serializar(registro).Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorOrigemProcessoIss.Sefin, "0")]
    [InlineData(IndicadorOrigemProcessoIss.JusticaFederal, "1")]
    [InlineData(IndicadorOrigemProcessoIss.JusticaEstadual, "2")]
    [InlineData(IndicadorOrigemProcessoIss.Outros, "9")]
    public void Serializar_IndProc_RetornaCodigo(IndicadorOrigemProcessoIss origem, string esperado)
    {
        _catalogo.TentarObter("B460".AsSpan(), out var meta);
        var registro = (RegistroB460)meta!.Fabrica();
        registro.IndProc = origem;

        meta.Campos[3].Serializar(registro).Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorObrigacaoIss.IssProprio, "0")]
    [InlineData(IndicadorObrigacaoIss.IssSubstituto, "1")]
    [InlineData(IndicadorObrigacaoIss.IssUniprofissional, "2")]
    public void Serializar_IndObr_RetornaCodigo(IndicadorObrigacaoIss obrigacao, string esperado)
    {
        _catalogo.TentarObter("B460".AsSpan(), out var meta);
        var registro = (RegistroB460)meta!.Fabrica();
        registro.IndObr = obrigacao;

        meta.Campos[6].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|B460|2|1500,00|PROC-2024/001|1|Decisao judicial 2024|OBS-001|0|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemDadosProcesso_PreservaTextoCanonico()
    {
        // NumProc/IndProc/Proc vazios — dedução sem processo vinculado.
        const string sped = "|B460|0|2500,00||||OBS-COMPENSACAO|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
