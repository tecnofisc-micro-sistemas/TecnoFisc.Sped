using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoB;

/// <summary>
/// Sub-stage 8.030 — exercita a forma do <see cref="RegistroB440"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 53): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroB440Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroB440).Assembly);

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
    public void Atributo_DeclaraB440_Nivel2_BlocoB()
    {
        var atributo = typeof(RegistroB440).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("B440");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("B");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroB440Com5CamposNaOrdem()
    {
        _catalogo.TentarObter("B440".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("B440");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndOper", "CodPart", "VlContRt", "VlBcIssRt", "VlIssRt",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 5));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("B440".AsSpan(), out var meta);
        var registro = (RegistroB440)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());          // IndOper
        meta.Campos[1].Definidor(registro, "PART-001".AsSpan());   // CodPart
        meta.Campos[2].Definidor(registro, "50000,00".AsSpan());   // VlContRt
        meta.Campos[3].Definidor(registro, "45000,00".AsSpan());   // VlBcIssRt
        meta.Campos[4].Definidor(registro, "2250,00".AsSpan());    // VlIssRt

        registro.IndOper.Should().Be(IndicadorOperacaoIss.Prestacao);
        registro.CodPart.Should().Be("PART-001");
        registro.VlContRt.Should().Be(50000.00m);
        registro.VlBcIssRt.Should().Be(45000.00m);
        registro.VlIssRt.Should().Be(2250.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("B440".AsSpan(), out var meta);
        var registro = (RegistroB440)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, Span<char>.Empty); // CodPart
        registro.CodPart.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|B440|1|PART-001|50000,00|45000,00|2250,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_OperacaoAquisicao_PreservaTextoCanonico()
    {
        // IND_OPER=0 (aquisição): prestador é o participante.
        const string sped = "|B440|0|FORN-002|10000,00|10000,00|500,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemRetencao_PreservaTextoCanonico()
    {
        // VL_BC_ISS_RT e VL_ISS_RT zerados — sem retenção no período.
        const string sped = "|B440|1|SERV-003|20000,00|0,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
