using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.125 — exercita a forma do <see cref="RegistroD170"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 175-176): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD170Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD170).Assembly);

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
    public void Atributo_DeclaraD170_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD170).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D170");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD170Com13CamposNaOrdem()
    {
        _catalogo.TentarObter("D170".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D170");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodPartConsg", "CodPartRed", "CodMunOrig", "CodMunDest",
            "Otm", "IndNatFrt", "VlLiqFrt", "VlGris",
            "VlPdg", "VlOut", "VlFrt", "VeicId", "UfId",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 13));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D170".AsSpan(), out var meta);
        var registro = (RegistroD170)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "CONSG0001".AsSpan());    // CodPartConsg
        meta.Campos[1].Definidor(registro, "RED0001".AsSpan());      // CodPartRed
        meta.Campos[2].Definidor(registro, "3550308".AsSpan());      // CodMunOrig
        meta.Campos[3].Definidor(registro, "3304557".AsSpan());      // CodMunDest
        meta.Campos[4].Definidor(registro, "12345678".AsSpan());     // Otm
        meta.Campos[5].Definidor(registro, "0".AsSpan());            // IndNatFrt
        meta.Campos[6].Definidor(registro, "1500,00".AsSpan());      // VlLiqFrt
        meta.Campos[7].Definidor(registro, "100,00".AsSpan());       // VlGris
        meta.Campos[8].Definidor(registro, "50,00".AsSpan());        // VlPdg
        meta.Campos[9].Definidor(registro, "200,00".AsSpan());       // VlOut
        meta.Campos[10].Definidor(registro, "1850,00".AsSpan());     // VlFrt
        meta.Campos[11].Definidor(registro, "ABC1234".AsSpan());     // VeicId
        meta.Campos[12].Definidor(registro, "SP".AsSpan());          // UfId

        registro.CodPartConsg.Should().Be("CONSG0001");
        registro.CodPartRed.Should().Be("RED0001");
        registro.CodMunOrig.Should().Be(3550308);
        registro.CodMunDest.Should().Be(3304557);
        registro.Otm.Should().Be("12345678");
        registro.IndNatFrt.Should().Be(IndicadorNaturezaFreteMultimodal.Negociavel);
        registro.VlLiqFrt.Should().Be(1500.00m);
        registro.VlGris.Should().Be(100.00m);
        registro.VlPdg.Should().Be(50.00m);
        registro.VlOut.Should().Be(200.00m);
        registro.VlFrt.Should().Be(1850.00m);
        registro.VeicId.Should().Be("ABC1234");
        registro.UfId.Should().Be("SP");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("D170".AsSpan(), out var meta);
        var registro = (RegistroD170)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.CodPartConsg.Should().BeNull();
        registro.CodPartRed.Should().BeNull();
        registro.CodMunOrig.Should().BeNull();
        registro.CodMunDest.Should().BeNull();
        registro.Otm.Should().BeNull();
        registro.IndNatFrt.Should().BeNull();
        registro.VlLiqFrt.Should().BeNull();
        registro.VlGris.Should().BeNull();
        registro.VlPdg.Should().BeNull();
        registro.VlOut.Should().BeNull();
        registro.VlFrt.Should().BeNull();
        registro.VeicId.Should().BeNull();
        registro.UfId.Should().BeNull();
    }

    [Theory]
    [InlineData("0", IndicadorNaturezaFreteMultimodal.Negociavel)]
    [InlineData("1", IndicadorNaturezaFreteMultimodal.NaoNegociavel)]
    public void Definidor_IndNatFrt_ConverteLiteral(string literal, IndicadorNaturezaFreteMultimodal esperado)
    {
        _catalogo.TentarObter("D170".AsSpan(), out var meta);
        var registro = (RegistroD170)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, literal.AsSpan());

        registro.IndNatFrt.Should().Be(esperado);
    }

    [Fact]
    public void Definidor_IndNatFrt_Vazio_DevolveNulo()
    {
        _catalogo.TentarObter("D170".AsSpan(), out var meta);
        var registro = (RegistroD170)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, Span<char>.Empty);

        registro.IndNatFrt.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // CTMC negociável com consignatário, redespachante, municípios, OTM, valores e veículo.
        const string sped = "|D170|CONSG0001|RED0001|3550308|3304557|12345678|0|1500,00|100,00|50,00|200,00|1850,00|ABC1234|SP|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // Campos opcionais (consignatário, redespachante, gris, pedágio, outros, veículo, UF) vazios.
        const string sped = "|D170|||1234567|7654321|87654321|1|2000,00||||2000,00|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_FreteNaoNegociavel_PreservaTextoCanonico()
    {
        // CTMC não negociável com exterior (município 9999999).
        const string sped = "|D170|CONS001||9999999|9999999|00000001|1|5000,00|||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
