using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.040 — exercita a forma do <see cref="RegistroC110"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 67): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC110Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC110).Assembly);

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
    public void Atributo_DeclaraC110_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC110).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C110");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC110Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("C110".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C110");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodInf", "TxtCompl",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 2));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C110".AsSpan(), out var meta);
        var registro = (RegistroC110)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "INF001".AsSpan());               // CodInf
        meta.Campos[1].Definidor(registro, "Nota complementar".AsSpan());    // TxtCompl

        registro.CodInf.Should().Be("INF001");
        registro.TxtCompl.Should().Be("Nota complementar");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C110".AsSpan(), out var meta);
        var registro = (RegistroC110)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, Span<char>.Empty);  // TxtCompl (string nullable)

        registro.TxtCompl.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // CodInf referenciando código de informação complementar com descrição adicional.
        const string sped = "|C110|INF001|Nota complementar da operação|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemTxtCompl_PreservaTextoCanonico()
    {
        // TxtCompl é OC; quando ausente o campo permanece vazio.
        const string sped = "|C110|INF002||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
