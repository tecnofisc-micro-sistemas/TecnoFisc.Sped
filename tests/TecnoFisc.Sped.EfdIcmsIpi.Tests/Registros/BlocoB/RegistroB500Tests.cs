using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoB;

/// <summary>
/// Sub-stage 8.033 — exercita a forma do <see cref="RegistroB500"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 56): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroB500Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroB500).Assembly);

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
    public void Atributo_DeclaraB500_Nivel2_BlocoB()
    {
        var atributo = typeof(RegistroB500).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("B500");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("B");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroB500Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("B500".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("B500");
        meta.Campos.Select(c => c.Nome).Should().Equal(["VlRec", "QtdProf", "VlOr"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 3));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("B500".AsSpan(), out var meta);
        var registro = (RegistroB500)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "5000,00".AsSpan());  // VlRec
        meta.Campos[1].Definidor(registro, "10".AsSpan());       // QtdProf
        meta.Campos[2].Definidor(registro, "500,00".AsSpan());   // VlOr

        registro.VlRec.Should().Be(5000.00m);
        registro.QtdProf.Should().Be(10);
        registro.VlOr.Should().Be(500.00m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|B500|5000,00|10|500,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComValoresZerados_PreservaTextoCanonico()
    {
        // Sociedade uniprofissional sem movimento no período.
        const string sped = "|B500|0,00|0|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
