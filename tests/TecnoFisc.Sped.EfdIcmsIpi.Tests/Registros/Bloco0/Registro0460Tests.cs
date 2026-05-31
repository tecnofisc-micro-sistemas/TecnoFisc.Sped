using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 8.019 — exercita a forma do <see cref="Registro0460"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 41): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class Registro0460Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0460).Assembly);

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
    public void Atributo_Declara0460_Nivel2_Bloco0()
    {
        var atributo = typeof(Registro0460).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0460");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0460Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("0460".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0460");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodObs",
            "Txt",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0460".AsSpan(), out var meta);
        var registro = (Registro0460)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "OBS001".AsSpan());
        meta.Campos[1].Definidor(registro, "Diferimento parcial do ICMS conforme art. 15 do RICMS".AsSpan());

        registro.CodObs.Should().Be("OBS001");
        registro.Txt.Should().Be("Diferimento parcial do ICMS conforme art. 15 do RICMS");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0460".AsSpan(), out var meta);
        var registro = (Registro0460)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);

        registro.CodObs.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0460|OBS001|Diferimento parcial do ICMS conforme art. 15 do RICMS|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComTxtLongo_PreservaTextoCanonico()
    {
        const string sped = "|0460|AJ001|Antecipacao tributaria de ICMS nas entradas interestaduais - Decreto 12345/2019|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
