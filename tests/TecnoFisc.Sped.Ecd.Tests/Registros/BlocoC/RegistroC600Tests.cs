using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoC;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 10.016 — exercita a forma do <see cref="RegistroC600"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 99): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroC600Tests
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
    public void Atributo_DeclaraC600_Nivel2_BlocoC()
    {
        var atributo = typeof(RegistroC600).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C600");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC600Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("C600".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C600");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtIni", "DtFin", "IdDem", "CabDem",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C600".AsSpan(), out var meta);
        var registro = (RegistroC600)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01012020".AsSpan());
        meta.Campos[1].Definidor(registro, "31122020".AsSpan());
        meta.Campos[2].Definidor(registro, "1".AsSpan());
        meta.Campos[3].Definidor(registro, "Demonstracoes contabeis do empresario".AsSpan());

        registro.DtIni.Should().Be(new DateOnly(2020, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2020, 12, 31));
        registro.IdDem.Should().Be(IdentificacaoDemonstracao.Empresario);
        registro.CabDem.Should().Be("Demonstracoes contabeis do empresario");
    }

    [Fact]
    public void Definidor_CabecalhoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C600".AsSpan(), out var meta);
        var registro = (RegistroC600)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // CabDem

        registro.CabDem.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C600|01012020|31122020|1|Demonstracoes contabeis do empresario|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_DemonstracoesConsolidadas_PreservaTextoCanonico()
    {
        const string sped =
            "|C600|01012020|31122020|2|Demonstracoes consolidadas|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCabecalho_PreservaTextoCanonico()
    {
        const string sped =
            "|C600|01012020|31122020|1||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
