using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 8.202 — exercita a forma do <see cref="RegistroK255"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 257-258): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroK255Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroK255).Assembly);

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
    public void Atributo_DeclaraK255_Nivel4_BlocoK()
    {
        var atributo = typeof(RegistroK255).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K255");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK255ComQuatroCamposNaOrdem()
    {
        _catalogo.TentarObter("K255".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K255");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtCons",
            "CodItem",
            "Qtd",
            "CodInsSubst",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K255".AsSpan(), out var meta);
        var registro = (RegistroK255)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "15012025".AsSpan());
        meta.Campos[1].Definidor(registro, "INSUMO-TERCEIRO".AsSpan());
        meta.Campos[2].Definidor(registro, "5,123456".AsSpan());
        meta.Campos[3].Definidor(registro, "INSUMO-PREVISTO".AsSpan());

        registro.DtCons.Should().Be(new DateOnly(2025, 1, 15));
        registro.CodItem.Should().Be("INSUMO-TERCEIRO");
        registro.Qtd.Should().Be(5.123456m);
        registro.CodInsSubst.Should().Be("INSUMO-PREVISTO");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("K255".AsSpan(), out var meta);
        var registro = (RegistroK255)meta!.Fabrica();

        meta!.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodInsSubst.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|K255|15012025|INSUMO-TERCEIRO|5,123456|INSUMO-PREVISTO|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemInsumoSubstituido_PreservaTextoCanonico()
    {
        const string sped = "|K255|16012025|INSUMO-TERCEIRO|2,000000||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
