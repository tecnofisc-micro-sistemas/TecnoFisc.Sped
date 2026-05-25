using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.036 — exercita a forma do <see cref="RegistroI300"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 152–153): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI300Tests
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
    public void Atributo_DeclaraI300_Nivel3_BlocoI()
    {
        var atributo = typeof(RegistroI300).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I300");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI300Com1CampoNaOrdem()
    {
        _catalogo.TentarObter("I300".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I300");
        meta.Campos.Select(c => c.Nome).Should().Equal(["DtBcte"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I300".AsSpan(), out var meta);
        var registro = (RegistroI300)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "15052023".AsSpan());  // DtBcte

        registro.DtBcte.Should().Be(new DateOnly(2023, 5, 15));
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 153): DT_BCTE=15052023
        const string sped = "|I300|15052023|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_DataDiferente_PreservaTextoCanonico()
    {
        // Data de balancete no início do exercício
        const string sped = "|I300|01012024|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
