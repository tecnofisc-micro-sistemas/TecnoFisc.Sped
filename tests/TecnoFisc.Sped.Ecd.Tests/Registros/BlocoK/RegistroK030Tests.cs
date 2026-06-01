using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoK;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 10.059 — exercita a forma do <see cref="RegistroK030"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 210–211): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroK030Tests
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
    public void Atributo_DeclaraK030_Nivel2_BlocoK()
    {
        var atributo = typeof(RegistroK030).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K030");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK030Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("K030".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K030");
        meta.Campos.Select(c => c.Nome).Should().Equal(["DtIni", "DtFin"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K030".AsSpan(), out var meta);
        var registro = (RegistroK030)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01012023".AsSpan());
        meta.Campos[1].Definidor(registro, "31122023".AsSpan());

        registro.DtIni.Should().Be(new DateOnly(2023, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2023, 12, 31));
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 211): DT_INI=01012023, DT_FIN=31122023
        const string sped = "|K030|01012023|31122023|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_PeriodoAnterior_PreservaTextoCanonico()
    {
        // Período consolidado de ano anterior
        const string sped = "|K030|01012022|31122022|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
