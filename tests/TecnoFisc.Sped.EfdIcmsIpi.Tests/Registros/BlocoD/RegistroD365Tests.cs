using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.136 — exercita a forma do <see cref="RegistroD365"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 185): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD365Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD365).Assembly);

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
    public void Atributo_DeclaraD365_Nivel4_BlocoD()
    {
        var atributo = typeof(RegistroD365).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D365");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD365Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("D365".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D365");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodTotPar", "VlrAcumTot", "NrTot", "DescrNrTot"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D365".AsSpan(), out var meta);
        var registro = (RegistroD365)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "T0001".AsSpan());           // CodTotPar
        meta.Campos[1].Definidor(registro, "1234,56".AsSpan());         // VlrAcumTot
        meta.Campos[2].Definidor(registro, "1".AsSpan());               // NrTot
        meta.Campos[3].Definidor(registro, "Tributado ICMS".AsSpan()); // DescrNrTot

        registro.CodTotPar.Should().Be("T0001");
        registro.VlrAcumTot.Should().Be(1234.56m);
        registro.NrTot.Should().Be(1);
        registro.DescrNrTot.Should().Be("Tributado ICMS");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D365".AsSpan(), out var meta);
        var registro = (RegistroD365)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty); // CodTotPar
        meta.Campos[1].Definidor(registro, Span<char>.Empty); // VlrAcumTot
        meta.Campos[2].Definidor(registro, Span<char>.Empty); // NrTot
        meta.Campos[3].Definidor(registro, Span<char>.Empty); // DescrNrTot

        registro.CodTotPar.Should().BeNull();
        registro.VlrAcumTot.Should().BeNull();
        registro.NrTot.Should().BeNull();
        registro.DescrNrTot.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Totalizador parcial da Redução Z com número de totalizador e descrição preenchidos.
        // NR_TOT é campo N sem asterisco (variável) — gerador não adiciona zeros à esquerda.
        const string sped = "|D365|T0001|1234,56|1|Tributado ICMS|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // Totalizador sem número de totalizador e sem descrição (campos opcionais condicionais).
        const string sped = "|D365|T0001|1234,56|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
