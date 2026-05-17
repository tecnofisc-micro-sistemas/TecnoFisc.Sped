using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.055 — exercita a forma do <see cref="RegistroC172"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 82): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC172Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC172).Assembly);

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerStreamingAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }

    [Fact]
    public void Atributo_DeclaraC172_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC172).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C172");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC172Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("C172".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C172");
        meta.Campos.Select(c => c.Nome).Should().Equal(["VlBcIssqn", "AliqIssqn", "VlIssqn"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C172".AsSpan(), out var meta);
        var registro = (RegistroC172)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1000,00".AsSpan());  // VlBcIssqn
        meta.Campos[1].Definidor(registro, "2,00".AsSpan());     // AliqIssqn
        meta.Campos[2].Definidor(registro, "20,00".AsSpan());    // VlIssqn

        registro.VlBcIssqn.Should().Be(1000.00m);
        registro.AliqIssqn.Should().Be(2.00m);
        registro.VlIssqn.Should().Be(20.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C172".AsSpan(), out var meta);
        var registro = (RegistroC172)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.VlBcIssqn.Should().BeNull();
        registro.AliqIssqn.Should().BeNull();
        registro.VlIssqn.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // ISSQN com base de cálculo, alíquota e valor preenchidos.
        const string sped = "|C172|1000,00|2,00|20,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposVazios_PreservaTextoCanonico()
    {
        // Todos os campos opcionais ausentes.
        const string sped = "|C172||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
