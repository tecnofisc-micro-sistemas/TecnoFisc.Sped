using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.121 — exercita a forma do <see cref="RegistroD150"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 172): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD150Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD150).Assembly);

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
    public void Atributo_DeclaraD150_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD150).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D150");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD150Com10CamposNaOrdem()
    {
        _catalogo.TentarObter("D150".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D150");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodMunOrig", "CodMunDest", "VeicId", "Viagem", "IndTfa",
            "VlPesoTx", "VlTxTerr", "VlTxRed", "VlOut", "VlTxAdv",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 10));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D150".AsSpan(), out var meta);
        var registro = (RegistroD150)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "3550308".AsSpan());  // CodMunOrig
        meta.Campos[1].Definidor(registro, "3304557".AsSpan());  // CodMunDest
        meta.Campos[2].Definidor(registro, "PR-GIG".AsSpan());   // VeicId
        meta.Campos[3].Definidor(registro, "1234".AsSpan());     // Viagem
        meta.Campos[4].Definidor(registro, "0".AsSpan());        // IndTfa
        meta.Campos[5].Definidor(registro, "500.00".AsSpan());   // VlPesoTx
        meta.Campos[6].Definidor(registro, "50.00".AsSpan());    // VlTxTerr
        meta.Campos[7].Definidor(registro, "30.00".AsSpan());    // VlTxRed
        meta.Campos[8].Definidor(registro, "20.00".AsSpan());    // VlOut
        meta.Campos[9].Definidor(registro, "10.00".AsSpan());    // VlTxAdv

        registro.CodMunOrig.Should().Be(3550308);
        registro.CodMunDest.Should().Be(3304557);
        registro.VeicId.Should().Be("PR-GIG");
        registro.Viagem.Should().Be(1234);
        registro.IndTfa.Should().Be(IndicadorTarifaAerea.Expedito);
        registro.VlPesoTx.Should().Be(500.00m);
        registro.VlTxTerr.Should().Be(50.00m);
        registro.VlTxRed.Should().Be(30.00m);
        registro.VlOut.Should().Be(20.00m);
        registro.VlTxAdv.Should().Be(10.00m);
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("D150".AsSpan(), out var meta);
        var registro = (RegistroD150)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, Span<char>.Empty);  // VeicId
        meta.Campos[3].Definidor(registro, Span<char>.Empty);  // Viagem
        meta.Campos[6].Definidor(registro, Span<char>.Empty);  // VlTxTerr
        meta.Campos[7].Definidor(registro, Span<char>.Empty);  // VlTxRed
        meta.Campos[8].Definidor(registro, Span<char>.Empty);  // VlOut
        meta.Campos[9].Definidor(registro, Span<char>.Empty);  // VlTxAdv

        registro.VeicId.Should().BeNull();
        registro.Viagem.Should().BeNull();
        registro.VlTxTerr.Should().BeNull();
        registro.VlTxRed.Should().BeNull();
        registro.VlOut.Should().BeNull();
        registro.VlTxAdv.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // CT Aéreo: São Paulo → Rio de Janeiro, voo 1234, tarifa Expedito.
        const string sped = "|D150|3550308|3304557|PR-GIG|1234|0|500,00|50,00|30,00|20,00|10,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // CT aéreo sem identificação de aeronave, sem viagem e sem taxas adicionais.
        const string sped = "|D150|3550308|3304557|||1|800,00|||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_MunicipioExterior_PreservaTextoCanonico()
    {
        // CT aéreo com origem/destino no Exterior — código 9999999.
        const string sped = "|D150|9999999|9999999|||9|1200,00|||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", IndicadorTarifaAerea.Expedito)]
    [InlineData("1", IndicadorTarifaAerea.Encomenda)]
    [InlineData("2", IndicadorTarifaAerea.CargaInferior)]
    [InlineData("9", IndicadorTarifaAerea.Outra)]
    public void IndTfa_Definidor_MapeiaTodosOsValores(string texto, IndicadorTarifaAerea esperado)
    {
        _catalogo.TentarObter("D150".AsSpan(), out var meta);
        var registro = (RegistroD150)meta!.Fabrica();

        meta.Campos[4].Definidor(registro, texto.AsSpan());

        registro.IndTfa.Should().Be(esperado);
    }

    [Fact]
    public void IndTfa_Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D150".AsSpan(), out var meta);
        var registro = (RegistroD150)meta!.Fabrica();

        meta.Campos[4].Definidor(registro, Span<char>.Empty);

        registro.IndTfa.Should().BeNull();
    }
}
