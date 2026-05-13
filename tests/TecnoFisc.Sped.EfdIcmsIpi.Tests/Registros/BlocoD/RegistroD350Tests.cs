using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.133 — exercita a forma do <see cref="RegistroD350"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 184): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD350Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD350).Assembly);

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
    public void Atributo_DeclaraD350_Nivel2_BlocoD()
    {
        var atributo = typeof(RegistroD350).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D350");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD350Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("D350".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D350");
        meta.Campos.Select(c => c.Nome).Should().Equal(["CodMod", "EcfMod", "EcfFab", "EcfCx"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D350".AsSpan(), out var meta);
        var registro = (RegistroD350)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "2E".AsSpan());                      // CodMod
        meta.Campos[1].Definidor(registro, "BEMATECH-MP2100TH".AsSpan());       // EcfMod
        meta.Campos[2].Definidor(registro, "ECF0123456789012345678".AsSpan());  // EcfFab
        meta.Campos[3].Definidor(registro, "1".AsSpan());                       // EcfCx

        registro.CodMod.Should().Be("2E");
        registro.EcfMod.Should().Be("BEMATECH-MP2100TH");
        registro.EcfFab.Should().Be("ECF0123456789012345678");
        registro.EcfCx.Should().Be(1);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D350".AsSpan(), out var meta);
        var registro = (RegistroD350)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty); // CodMod
        meta.Campos[3].Definidor(registro, Span<char>.Empty); // EcfCx

        registro.CodMod.Should().BeNull();
        registro.EcfCx.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Equipamento ECF para bilhete de passagem rodoviário, caixa 1.
        const string sped = "|D350|13|BEMATECH-MP2100TH|ECF0123456789012345678|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CupomFiscalBilhetePassagem_PreservaTextoCanonico()
    {
        // Equipamento emitindo Cupom Fiscal Bilhete de Passagem (2E), caixa 5.
        const string sped = "|D350|2E|DARUMA-DR800L|DARUMA9876543210|5|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
