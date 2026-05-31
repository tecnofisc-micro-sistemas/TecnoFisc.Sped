using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.135 — exercita a forma do <see cref="RegistroD360"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 185): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD360Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD360).Assembly);

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
    public void Atributo_DeclaraD360_Nivel4_BlocoD()
    {
        var atributo = typeof(RegistroD360).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D360");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD360Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("D360".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D360");
        meta.Campos.Select(c => c.Nome).Should().Equal(["VlPis", "VlCofins"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D360".AsSpan(), out var meta);
        var registro = (RegistroD360)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "123,45".AsSpan());   // VlPis
        meta.Campos[1].Definidor(registro, "567,89".AsSpan());   // VlCofins

        registro.VlPis.Should().Be(123.45m);
        registro.VlCofins.Should().Be(567.89m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D360".AsSpan(), out var meta);
        var registro = (RegistroD360)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty); // VlPis
        meta.Campos[1].Definidor(registro, Span<char>.Empty); // VlCofins

        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // PIS e COFINS totalizados do dia para bilhetes de passagem via ECF.
        const string sped = "|D360|123,45|567,89|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposVazios_PreservaTextoCanonico()
    {
        // Contribuinte que entrega EFD-Contribuições: ambos os campos dispensados (vazios).
        const string sped = "|D360|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
