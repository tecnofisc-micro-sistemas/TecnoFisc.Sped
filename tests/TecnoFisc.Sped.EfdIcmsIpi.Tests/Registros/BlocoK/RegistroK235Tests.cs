using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 8.200 — exercita a forma do <see cref="RegistroK235"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 255-256): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroK235Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroK235).Assembly);

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
    public void Atributo_DeclaraK235_Nivel4_BlocoK()
    {
        var atributo = typeof(RegistroK235).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K235");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK235ComQuatroCamposNaOrdem()
    {
        _catalogo.TentarObter("K235".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K235");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtSaida",
            "CodItem",
            "Qtd",
            "CodInsSubst",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K235".AsSpan(), out var meta);
        var registro = (RegistroK235)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "10012025".AsSpan());
        meta.Campos[1].Definidor(registro, "INSUMO-001".AsSpan());
        meta.Campos[2].Definidor(registro, "7,654321".AsSpan());
        meta.Campos[3].Definidor(registro, "INSUMO-SUBST".AsSpan());

        registro.DtSaida.Should().Be(new DateOnly(2025, 1, 10));
        registro.CodItem.Should().Be("INSUMO-001");
        registro.Qtd.Should().Be(7.654321m);
        registro.CodInsSubst.Should().Be("INSUMO-SUBST");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("K235".AsSpan(), out var meta);
        var registro = (RegistroK235)meta!.Fabrica();

        meta!.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodInsSubst.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|K235|10012025|INSUMO-001|7,654321|INSUMO-SUBST|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemInsumoSubstituido_PreservaTextoCanonico()
    {
        const string sped = "|K235|11012025|INSUMO-002|3,000000||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
