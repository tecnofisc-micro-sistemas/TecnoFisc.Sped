using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 8.204 — exercita a forma do <see cref="RegistroK265"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 259): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroK265Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroK265).Assembly);

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
    public void Atributo_DeclaraK265_Nivel4_BlocoK()
    {
        var atributo = typeof(RegistroK265).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K265");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK265ComTresCamposNaOrdem()
    {
        _catalogo.TentarObter("K265".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K265");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodItem",
            "QtdCons",
            "QtdRet",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K265".AsSpan(), out var meta);
        var registro = (RegistroK265)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "MERCADORIA-CONSUMIDA".AsSpan());
        meta.Campos[1].Definidor(registro, "1,234567".AsSpan());
        meta.Campos[2].Definidor(registro, "0,765432".AsSpan());

        registro.CodItem.Should().Be("MERCADORIA-CONSUMIDA");
        registro.QtdCons.Should().Be(1.234567m);
        registro.QtdRet.Should().Be(0.765432m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("K265".AsSpan(), out var meta);
        var registro = (RegistroK265)meta!.Fabrica();

        meta!.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.QtdCons.Should().BeNull();
        registro.QtdRet.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|K265|MERCADORIA-CONSUMIDA|1,234567|0,765432|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ApenasConsumo_PreservaTextoCanonico()
    {
        const string sped = "|K265|MERCADORIA-CONSUMIDA|3,000000||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
