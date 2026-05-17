using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.120 — exercita a forma do <see cref="RegistroD140"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.2.2 (p. 179): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD140Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD140).Assembly);

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
    public void Atributo_DeclaraD140_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD140).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D140");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD140Com13CamposNaOrdem()
    {
        _catalogo.TentarObter("D140".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D140");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodPartConsg", "CodMunOrig", "CodMunDest", "IndVeic", "VeicId",
            "IndNav", "Viagem", "VlFrtLiq", "VlDespPort", "VlDespCarDesc",
            "VlOut", "VlFrtBrt", "VlFrtMm",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 13));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D140".AsSpan(), out var meta);
        var registro = (RegistroD140)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "CONSIG001".AsSpan());  // CodPartConsg
        meta.Campos[1].Definidor(registro, "3550308".AsSpan());    // CodMunOrig
        meta.Campos[2].Definidor(registro, "3304557".AsSpan());    // CodMunDest
        meta.Campos[3].Definidor(registro, "0".AsSpan());           // IndVeic
        meta.Campos[4].Definidor(registro, "IMO1234567".AsSpan()); // VeicId
        meta.Campos[5].Definidor(registro, "1".AsSpan());           // IndNav
        meta.Campos[6].Definidor(registro, "42".AsSpan());          // Viagem
        meta.Campos[7].Definidor(registro, "5000.00".AsSpan());    // VlFrtLiq
        meta.Campos[8].Definidor(registro, "200.00".AsSpan());     // VlDespPort
        meta.Campos[9].Definidor(registro, "150.00".AsSpan());     // VlDespCarDesc
        meta.Campos[10].Definidor(registro, "50.00".AsSpan());     // VlOut
        meta.Campos[11].Definidor(registro, "5400.00".AsSpan());   // VlFrtBrt
        meta.Campos[12].Definidor(registro, "100.00".AsSpan());    // VlFrtMm

        registro.CodPartConsg.Should().Be("CONSIG001");
        registro.CodMunOrig.Should().Be(3550308);
        registro.CodMunDest.Should().Be(3304557);
        registro.IndVeic.Should().Be(IndicadorTipoVeiculoAquaviario.Embarcacao);
        registro.VeicId.Should().Be("IMO1234567");
        registro.IndNav.Should().Be(IndicadorTipoNavegacao.Cabotagem);
        registro.Viagem.Should().Be(42);
        registro.VlFrtLiq.Should().Be(5000.00m);
        registro.VlDespPort.Should().Be(200.00m);
        registro.VlDespCarDesc.Should().Be(150.00m);
        registro.VlOut.Should().Be(50.00m);
        registro.VlFrtBrt.Should().Be(5400.00m);
        registro.VlFrtMm.Should().Be(100.00m);
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("D140".AsSpan(), out var meta);
        var registro = (RegistroD140)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);  // CodPartConsg
        meta.Campos[4].Definidor(registro, Span<char>.Empty);  // VeicId
        meta.Campos[6].Definidor(registro, Span<char>.Empty);  // Viagem
        meta.Campos[8].Definidor(registro, Span<char>.Empty);  // VlDespPort
        meta.Campos[9].Definidor(registro, Span<char>.Empty);  // VlDespCarDesc
        meta.Campos[10].Definidor(registro, Span<char>.Empty); // VlOut
        meta.Campos[12].Definidor(registro, Span<char>.Empty); // VlFrtMm

        registro.CodPartConsg.Should().BeNull();
        registro.VeicId.Should().BeNull();
        registro.Viagem.Should().BeNull();
        registro.VlDespPort.Should().BeNull();
        registro.VlDespCarDesc.Should().BeNull();
        registro.VlOut.Should().BeNull();
        registro.VlFrtMm.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // CT Aquaviário: Santos → Rio de Janeiro, embarcação de cabotagem.
        const string sped = "|D140|CONSIG001|3550308|3304557|0|IMO1234567|1|42|5000,00|200,00|150,00|50,00|5400,00|100,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // CT sem consignatário, sem embarcação identificada, sem viagem, sem despesas adicionais.
        const string sped = "|D140||3550308|3304557|1||0||5000,00||||5000,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_MunicipioExterior_PreservaTextoCanonico()
    {
        // CT aquaviário com origem/destino no Exterior — código 9999999.
        const string sped = "|D140||9999999|9999999|0||1||8000,00||||8000,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", IndicadorTipoVeiculoAquaviario.Embarcacao)]
    [InlineData("1", IndicadorTipoVeiculoAquaviario.EmpurradorRebocador)]
    public void IndVeic_Definidor_MapeiaTodosOsValores(string texto, IndicadorTipoVeiculoAquaviario esperado)
    {
        _catalogo.TentarObter("D140".AsSpan(), out var meta);
        var registro = (RegistroD140)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, texto.AsSpan());

        registro.IndVeic.Should().Be(esperado);
    }

    [Fact]
    public void IndVeic_Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D140".AsSpan(), out var meta);
        var registro = (RegistroD140)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, Span<char>.Empty);

        registro.IndVeic.Should().BeNull();
    }

    [Theory]
    [InlineData("0", IndicadorTipoNavegacao.Interior)]
    [InlineData("1", IndicadorTipoNavegacao.Cabotagem)]
    public void IndNav_Definidor_MapeiaTodosOsValores(string texto, IndicadorTipoNavegacao esperado)
    {
        _catalogo.TentarObter("D140".AsSpan(), out var meta);
        var registro = (RegistroD140)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, texto.AsSpan());

        registro.IndNav.Should().Be(esperado);
    }

    [Fact]
    public void IndNav_Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D140".AsSpan(), out var meta);
        var registro = (RegistroD140)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, Span<char>.Empty);

        registro.IndNav.Should().BeNull();
    }
}
