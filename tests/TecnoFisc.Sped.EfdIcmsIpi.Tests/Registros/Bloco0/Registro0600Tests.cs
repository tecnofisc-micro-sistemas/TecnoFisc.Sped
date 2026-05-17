using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 8.021 — exercita a forma do <see cref="Registro0600"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 43): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class Registro0600Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0600).Assembly);

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
    public void Atributo_Declara0600_Nivel2_Bloco0()
    {
        var atributo = typeof(Registro0600).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0600");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0600Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("0600".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0600");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "DtAlt",
            "CodCcus",
            "Ccus",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0600".AsSpan(), out var meta);
        var registro = (Registro0600)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01062021".AsSpan());
        meta.Campos[1].Definidor(registro, "CC001".AsSpan());
        meta.Campos[2].Definidor(registro, "Área Operacional".AsSpan());

        registro.DtAlt.Should().Be(new DateOnly(2021, 6, 1));
        registro.CodCcus.Should().Be("CC001");
        registro.Ccus.Should().Be("Área Operacional");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0600".AsSpan(), out var meta);
        var registro = (Registro0600)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, Span<char>.Empty);

        registro.CodCcus.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0600|01062021|CC001|Área Operacional|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        const string sped = "|0600||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
