using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoJ;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoJ;

/// <summary>
/// Sub-stage 10.046 — exercita a forma do <see cref="RegistroJ005"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 172–174): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroJ005Tests
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
    public void Atributo_DeclaraJ005_Nivel2_BlocoJ()
    {
        var atributo = typeof(RegistroJ005).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("J005");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("J");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroJ005Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("J005".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("J005");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtIni", "DtFin", "IdDem", "CabDem",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("J005".AsSpan(), out var meta);
        var registro = (RegistroJ005)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01012023".AsSpan());
        meta.Campos[1].Definidor(registro, "31122023".AsSpan());
        meta.Campos[2].Definidor(registro, "1".AsSpan());
        meta.Campos[3].Definidor(registro, "Demonstracoes contabeis 2023".AsSpan());

        registro.DtIni.Should().Be(new DateOnly(2023, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2023, 12, 31));
        registro.IdDem.Should().Be(IdentificacaoDemonstracao.Empresario);
        registro.CabDem.Should().Be("Demonstracoes contabeis 2023");
    }

    [Fact]
    public void Definidor_CabecalhoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("J005".AsSpan(), out var meta);
        var registro = (RegistroJ005)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // CabDem

        registro.CabDem.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual p. 174: |J005|01012023|31012023|1||
        const string sped = "|J005|01012023|31012023|1||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_DemonstracoesConsolidadasComCabecalho_PreservaTextoCanonico()
    {
        const string sped = "|J005|01012023|31122023|2|Grupo XYZ Consolidado|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ExercicioCompleto_PreservaTextoCanonico()
    {
        const string sped = "|J005|01012023|31122023|1||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
