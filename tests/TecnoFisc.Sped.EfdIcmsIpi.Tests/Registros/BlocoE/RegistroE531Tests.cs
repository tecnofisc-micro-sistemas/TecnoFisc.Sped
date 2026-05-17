using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoE;

/// <summary>
/// Sub-stage 8.178 — exercita a forma do <see cref="RegistroE531"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 235-236): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroE531Tests
{
    // Chave NF-e válida (UF SP, Jan/2024, DV=8) reutilizada dos testes de ChaveAcesso.
    private const string ChaveNfeValida = "35240111222333000181550010000000011000000018";

    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroE531).Assembly);

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
    public void Atributo_DeclaraE531_Nivel5_BlocoE()
    {
        var atributo = typeof(RegistroE531).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("E531");
        atributo.Nivel.Should().Be(5);
        atributo.Bloco.Should().Be("E");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroE531Com9CamposNaOrdem()
    {
        _catalogo.TentarObter("E531".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("E531");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodPart", "CodMod", "Ser", "Sub", "NumDoc", "DtDoc", "CodItem", "VlAjItem", "ChvNfe",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 9));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("E531".AsSpan(), out var meta);
        var registro = (RegistroE531)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "FORN001".AsSpan());      // CodPart
        meta.Campos[1].Definidor(registro, "55".AsSpan());           // CodMod
        meta.Campos[2].Definidor(registro, "001".AsSpan());          // Ser
        meta.Campos[3].Definidor(registro, "1".AsSpan());            // Sub
        meta.Campos[4].Definidor(registro, "12345".AsSpan());        // NumDoc
        meta.Campos[5].Definidor(registro, "01012023".AsSpan());     // DtDoc
        meta.Campos[6].Definidor(registro, "PROD001".AsSpan());      // CodItem
        meta.Campos[7].Definidor(registro, "250,75".AsSpan());       // VlAjItem
        meta.Campos[8].Definidor(registro, ChaveNfeValida.AsSpan());  // ChvNfe

        registro.CodPart.Should().Be("FORN001");
        registro.CodMod.Should().Be(ModeloDocumento.Criar("55"));
        registro.Ser.Should().Be("001");
        registro.Sub.Should().Be(1);
        registro.NumDoc.Should().Be(12345);
        registro.DtDoc.Should().Be(new DateOnly(2023, 1, 1));
        registro.CodItem.Should().Be("PROD001");
        registro.VlAjItem.Should().Be(250.75m);
        registro.ChvNfe.Should().Be(ChaveAcesso.Criar(ChaveNfeValida));
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("E531".AsSpan(), out var meta);
        var registro = (RegistroE531)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);  // CodPart
        meta.Campos[1].Definidor(registro, Span<char>.Empty);  // CodMod
        meta.Campos[2].Definidor(registro, Span<char>.Empty);  // Ser
        meta.Campos[3].Definidor(registro, Span<char>.Empty);  // Sub
        meta.Campos[4].Definidor(registro, Span<char>.Empty);  // NumDoc
        meta.Campos[6].Definidor(registro, Span<char>.Empty);  // CodItem
        meta.Campos[8].Definidor(registro, Span<char>.Empty);  // ChvNfe

        registro.CodPart.Should().BeNull();
        registro.CodMod.Should().BeNull();
        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
        registro.NumDoc.Should().BeNull();
        registro.CodItem.Should().BeNull();
        registro.ChvNfe.Should().BeNull();
    }

    [Theory]
    [InlineData("01")]
    [InlineData("55")]
    public void Definidor_CodMod_MapeiaModelosValidosDoRegistro(string valor)
    {
        _catalogo.TentarObter("E531".AsSpan(), out var meta);
        var registro = (RegistroE531)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, valor.AsSpan());

        registro.CodMod.Should().Be(ModeloDocumento.Criar(valor));
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        var sped = $"|E531|FORN001|55|001|1|12345|01012023|PROD001|250,75|{ChaveNfeValida}|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComNotaFiscalModelo01SemChave_PreservaTextoCanonico()
    {
        const string sped = "|E531|FORN001|01|A||98765|15022023||80,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
