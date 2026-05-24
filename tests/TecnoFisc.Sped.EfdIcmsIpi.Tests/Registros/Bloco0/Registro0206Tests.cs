using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 8.012 — exercita a forma do <see cref="Registro0206"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 36-37): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class Registro0206Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0206).Assembly);

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
    public void Atributo_Declara0206_Nivel3_Bloco0()
    {
        var atributo = typeof(Registro0206).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0206");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0206Com1CampoNaOrdem()
    {
        _catalogo.TentarObter("0206".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0206");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodComb"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0206".AsSpan(), out var meta);
        var registro = (Registro0206)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0001".AsSpan());

        registro.CodComb.Should().Be("0001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0206".AsSpan(), out var meta);
        var registro = (Registro0206)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);

        registro.CodComb.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0206|0001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCodigoCombustivelLongo_PreservaTextoCanonico()
    {
        const string sped = "|0206|GAS-C-GASOLINA-COMUM|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
