using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.043 — exercita a forma do <see cref="RegistroC113"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 69): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC113Tests
{
    // Chave NF-e válida (UF SP, Jan/2024, DV=8) reutilizada dos testes de ChaveAcesso.
    private const string ChaveNfeValida = "35240111222333000181550010000000011000000018";

    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC113).Assembly);

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
    public void Atributo_DeclaraC113_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC113).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C113");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC113Com9CamposNaOrdem()
    {
        _catalogo.TentarObter("C113".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C113");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndOper", "IndEmit", "CodPart", "CodMod", "Ser", "Sub", "NumDoc", "DtDoc", "ChvDoce",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 9));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C113".AsSpan(), out var meta);
        var registro = (RegistroC113)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());                  // IndOper
        meta.Campos[1].Definidor(registro, "1".AsSpan());                  // IndEmit
        meta.Campos[2].Definidor(registro, "PART001".AsSpan());            // CodPart
        meta.Campos[3].Definidor(registro, "55".AsSpan());                 // CodMod
        meta.Campos[4].Definidor(registro, "001".AsSpan());                // Ser
        meta.Campos[5].Definidor(registro, "001".AsSpan());                // Sub
        meta.Campos[6].Definidor(registro, "000000001".AsSpan());          // NumDoc
        meta.Campos[7].Definidor(registro, "01012024".AsSpan());           // DtDoc
        meta.Campos[8].Definidor(registro, ChaveNfeValida.AsSpan());       // ChvDoce

        registro.IndOper.Should().Be(IndicadorOperacao.Entrada);
        registro.IndEmit.Should().Be(IndicadorEmissorDocumento.Terceiros);
        registro.CodPart.Should().Be("PART001");
        registro.CodMod.Should().Be("55");
        registro.Ser.Should().Be("001");
        registro.Sub.Should().Be(1);
        registro.NumDoc.Should().Be(1);
        registro.DtDoc.Should().Be(new DateOnly(2024, 1, 1));
        registro.ChvDoce.Should().Be(ChaveAcesso.Create(ChaveNfeValida));
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C113".AsSpan(), out var meta);
        var registro = (RegistroC113)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, Span<char>.Empty);  // CodPart
        meta.Campos[3].Definidor(registro, Span<char>.Empty);  // CodMod
        meta.Campos[4].Definidor(registro, Span<char>.Empty);  // Ser
        meta.Campos[5].Definidor(registro, Span<char>.Empty);  // Sub
        meta.Campos[8].Definidor(registro, Span<char>.Empty);  // ChvDoce

        registro.CodPart.Should().BeNull();
        registro.CodMod.Should().BeNull();
        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
        registro.ChvDoce.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // NF-e entrada de terceiros com chave eletrônica.
        var sped = $"|C113|0|1|PART001|55|001|1|1|01012024|{ChaveNfeValida}|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // NF emissão própria saída sem série, subsérie e chave eletrônica.
        const string sped = "|C113|1|0|EMIT001|01|||42|15032024||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", IndicadorOperacao.Entrada)]
    [InlineData("1", IndicadorOperacao.Saida)]
    public void Definidor_IndOperCobertosEnum(string valor, IndicadorOperacao esperado)
    {
        _catalogo.TentarObter("C113".AsSpan(), out var meta);
        var registro = (RegistroC113)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, valor.AsSpan());

        registro.IndOper.Should().Be(esperado);
    }

    [Theory]
    [InlineData("0", IndicadorEmissorDocumento.EmissaoPropria)]
    [InlineData("1", IndicadorEmissorDocumento.Terceiros)]
    public void Definidor_IndEmitCobertosEnum(string valor, IndicadorEmissorDocumento esperado)
    {
        _catalogo.TentarObter("C113".AsSpan(), out var meta);
        var registro = (RegistroC113)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, valor.AsSpan());

        registro.IndEmit.Should().Be(esperado);
    }
}
