using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoE;

/// <summary>
/// Sub-stage 8.162 — exercita a forma do <see cref="RegistroE200"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 214): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroE200Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroE200).Assembly);

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
    public void Atributo_DeclaraE200_Nivel2_BlocoE()
    {
        var atributo = typeof(RegistroE200).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("E200");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("E");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroE200Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("E200".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("E200");
        meta.Campos.Select(c => c.Nome).Should().Equal(["Uf", "DtIni", "DtFin"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("E200".AsSpan(), out var meta);
        var registro = (RegistroE200)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "SP".AsSpan());         // Uf
        meta.Campos[1].Definidor(registro, "01012025".AsSpan());   // DtIni
        meta.Campos[2].Definidor(registro, "31012025".AsSpan());   // DtFin

        registro.Uf.Should().Be("SP");
        registro.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2025, 1, 31));
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("E200".AsSpan(), out var meta);
        var registro = (RegistroE200)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);
        meta.Campos[1].Definidor(registro, Span<char>.Empty);
        meta.Campos[2].Definidor(registro, Span<char>.Empty);

        registro.Uf.Should().BeNull();
        registro.DtIni.Should().Be(default(DateOnly));
        registro.DtFin.Should().Be(default(DateOnly));
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|E200|SP|01012025|31012025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComDiferentesUfs_PreservaTextoCanonico()
    {
        // Apuração ST referente ao estado do Rio de Janeiro, período fevereiro/2025.
        const string sped = "|E200|RJ|01022025|28022025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
