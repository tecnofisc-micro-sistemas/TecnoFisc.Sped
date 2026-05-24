using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.083 — exercita a forma do <see cref="RegistroC420"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 120): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC420Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC420).Assembly);

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
    public void Atributo_DeclaraC420_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC420).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C420");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC420Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("C420".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C420");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodTotPar", "VlrAcumTot", "NrTot", "DescrNrTot"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C420".AsSpan(), out var meta);
        var registro = (RegistroC420)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "T1700".AsSpan());      // CodTotPar
        meta.Campos[1].Definidor(registro, "1500,25".AsSpan());    // VlrAcumTot
        meta.Campos[2].Definidor(registro, "1".AsSpan());          // NrTot
        meta.Campos[3].Definidor(registro, "Carga 17%".AsSpan()); // DescrNrTot

        registro.CodTotPar.Should().Be("T1700");
        registro.VlrAcumTot.Should().Be(1500.25m);
        registro.NrTot.Should().Be(1);
        registro.DescrNrTot.Should().Be("Carga 17%");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C420".AsSpan(), out var meta);
        var registro = (RegistroC420)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // NrTot
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // DescrNrTot

        registro.NrTot.Should().BeNull();
        registro.DescrNrTot.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C420|T1700|1500,25|1|Carga 17%|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemNrTotEDescrNrTot_PreservaTextoCanonico()
    {
        const string sped = "|C420|T1700|1500,25|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
