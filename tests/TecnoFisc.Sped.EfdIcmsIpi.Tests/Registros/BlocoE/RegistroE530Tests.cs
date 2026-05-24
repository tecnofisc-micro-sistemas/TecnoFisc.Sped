using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoE;

/// <summary>
/// Sub-stage 8.177 — exercita a forma do <see cref="RegistroE530"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 233-235): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroE530Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroE530).Assembly);

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
    public void Atributo_DeclaraE530_Nivel4_BlocoE()
    {
        var atributo = typeof(RegistroE530).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("E530");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("E");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroE530Com6CamposNaOrdem()
    {
        _catalogo.TentarObter("E530".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("E530");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["IndAj", "VlAj", "CodAj", "IndDoc", "NumDoc", "DescrAj"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("E530".AsSpan(), out var meta);
        var registro = (RegistroE530)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());                    // IndAj
        meta.Campos[1].Definidor(registro, "250,75".AsSpan());               // VlAj
        meta.Campos[2].Definidor(registro, "199".AsSpan());                  // CodAj
        meta.Campos[3].Definidor(registro, "1".AsSpan());                    // IndDoc
        meta.Campos[4].Definidor(registro, "PA-IPI-2025-001".AsSpan());      // NumDoc
        meta.Campos[5].Definidor(registro, "Ajuste de débito IPI".AsSpan()); // DescrAj

        registro.IndAj.Should().Be(IndicadorTipoAjusteIpi.Debito);
        registro.VlAj.Should().Be(250.75m);
        registro.CodAj.Should().Be("199");
        registro.IndDoc.Should().Be(IndicadorOrigemDocumentoAjusteIpi.ProcessoAdministrativo);
        registro.NumDoc.Should().Be("PA-IPI-2025-001");
        registro.DescrAj.Should().Be("Ajuste de débito IPI");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("E530".AsSpan(), out var meta);
        var registro = (RegistroE530)meta!.Fabrica();

        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // NumDoc

        registro.NumDoc.Should().BeNull();
    }

    [Theory]
    [InlineData("0", IndicadorTipoAjusteIpi.Debito)]
    [InlineData("1", IndicadorTipoAjusteIpi.Credito)]
    public void Definidor_IndAj_MapeiaCodigos(string input, IndicadorTipoAjusteIpi esperado)
    {
        _catalogo.TentarObter("E530".AsSpan(), out var meta);
        var registro = (RegistroE530)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, input.AsSpan());

        registro.IndAj.Should().Be(esperado);
    }

    [Theory]
    [InlineData("0", IndicadorOrigemDocumentoAjusteIpi.ProcessoJudicial)]
    [InlineData("1", IndicadorOrigemDocumentoAjusteIpi.ProcessoAdministrativo)]
    [InlineData("2", IndicadorOrigemDocumentoAjusteIpi.PerDcomp)]
    [InlineData("3", IndicadorOrigemDocumentoAjusteIpi.DocumentoFiscal)]
    [InlineData("9", IndicadorOrigemDocumentoAjusteIpi.Outros)]
    public void Definidor_IndDoc_MapeiaCodigos(string input, IndicadorOrigemDocumentoAjusteIpi esperado)
    {
        _catalogo.TentarObter("E530".AsSpan(), out var meta);
        var registro = (RegistroE530)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, input.AsSpan());

        registro.IndDoc.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|E530|0|250,75|199|1|PA-IPI-2025-001|Ajuste de débito IPI|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCreditoTransferido_PreservaTextoCanonico()
    {
        const string sped = "|E530|1|1200,00|002|9|TRANSF-2025-04|Crédito recebido por transferência|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComDocumentoFiscalSemNumeroDocumento_PreservaTextoCanonico()
    {
        const string sped = "|E530|0|80,00|101|3||Estorno de crédito vinculado a documento fiscal|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
