using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 8.017 — exercita a forma do <see cref="Registro0400"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 41): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class Registro0400Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0400).Assembly);

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
    public void Atributo_Declara0400_Nivel2_Bloco0()
    {
        var atributo = typeof(Registro0400).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0400");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0400Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("0400".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0400");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodNat",
            "DescrNat",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0400".AsSpan(), out var meta);
        var registro = (Registro0400)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "VENDA MERC".AsSpan());
        meta.Campos[1].Definidor(registro, "Venda de mercadoria a contribuinte".AsSpan());

        registro.CodNat.Should().Be("VENDA MERC");
        registro.DescrNat.Should().Be("Venda de mercadoria a contribuinte");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0400".AsSpan(), out var meta);
        var registro = (Registro0400)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);

        registro.CodNat.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0400|VENDA MERC|Venda de mercadoria a contribuinte|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCodNatCurto_PreservaTextoCanonico()
    {
        const string sped = "|0400|001|Transferencia de mercadorias entre estabelecimentos|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
