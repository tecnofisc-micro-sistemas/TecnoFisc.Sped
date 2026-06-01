using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.031 — exercita a forma do <see cref="RegistroI150"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 131–133): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI150Tests
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
    public void Atributo_DeclaraI150_Nivel3_BlocoI()
    {
        var atributo = typeof(RegistroI150).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I150");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI150Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("I150".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I150");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtIni", "DtFin",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I150".AsSpan(), out var meta);
        var registro = (RegistroI150)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01012023".AsSpan());
        meta.Campos[1].Definidor(registro, "31012023".AsSpan());

        registro.DtIni.Should().Be(new DateOnly(2023, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2023, 1, 31));
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 133): DT_INI=01012023, DT_FIN=31012023
        const string sped = "|I150|01012023|31012023|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComPeriodoFracionado_PreservaTextoCanonico()
    {
        // Fração de mês — cisão, fusão, incorporação ou extinção (p. 131)
        const string sped = "|I150|01032024|15032024|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
