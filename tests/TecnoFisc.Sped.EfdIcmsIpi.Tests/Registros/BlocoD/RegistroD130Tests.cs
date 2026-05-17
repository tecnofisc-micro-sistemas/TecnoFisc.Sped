using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.119 — exercita a forma do <see cref="RegistroD130"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 170): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD130Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD130).Assembly);

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
    public void Atributo_DeclaraD130_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD130).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D130");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD130Com13CamposNaOrdem()
    {
        _catalogo.TentarObter("D130".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D130");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodPartConsg", "CodPartRed", "IndFrtRed", "CodMunOrig", "CodMunDest",
            "VeicId", "VlLiqFrt", "VlSecCat", "VlDesp", "VlPedg", "VlOut", "VlFrt", "UfId",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 13));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D130".AsSpan(), out var meta);
        var registro = (RegistroD130)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "CONSIG001".AsSpan());  // CodPartConsg
        meta.Campos[1].Definidor(registro, "REDESP001".AsSpan());  // CodPartRed
        meta.Campos[2].Definidor(registro, "0".AsSpan());           // IndFrtRed
        meta.Campos[3].Definidor(registro, "3550308".AsSpan());    // CodMunOrig
        meta.Campos[4].Definidor(registro, "3304557".AsSpan());    // CodMunDest
        meta.Campos[5].Definidor(registro, "ABC1234".AsSpan());    // VeicId
        meta.Campos[6].Definidor(registro, "1500.00".AsSpan());    // VlLiqFrt
        meta.Campos[7].Definidor(registro, "50.00".AsSpan());      // VlSecCat
        meta.Campos[8].Definidor(registro, "30.00".AsSpan());      // VlDesp
        meta.Campos[9].Definidor(registro, "20.00".AsSpan());      // VlPedg
        meta.Campos[10].Definidor(registro, "10.00".AsSpan());     // VlOut
        meta.Campos[11].Definidor(registro, "1610.00".AsSpan());   // VlFrt
        meta.Campos[12].Definidor(registro, "SP".AsSpan());        // UfId

        registro.CodPartConsg.Should().Be("CONSIG001");
        registro.CodPartRed.Should().Be("REDESP001");
        registro.IndFrtRed.Should().Be(IndicadorFreteRedespacho.SemRedespacho);
        registro.CodMunOrig.Should().Be(3550308);
        registro.CodMunDest.Should().Be(3304557);
        registro.VeicId.Should().Be("ABC1234");
        registro.VlLiqFrt.Should().Be(1500.00m);
        registro.VlSecCat.Should().Be(50.00m);
        registro.VlDesp.Should().Be(30.00m);
        registro.VlPedg.Should().Be(20.00m);
        registro.VlOut.Should().Be(10.00m);
        registro.VlFrt.Should().Be(1610.00m);
        registro.UfId.Should().Be("SP");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("D130".AsSpan(), out var meta);
        var registro = (RegistroD130)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);  // CodPartConsg
        meta.Campos[1].Definidor(registro, Span<char>.Empty);  // CodPartRed
        meta.Campos[2].Definidor(registro, Span<char>.Empty);  // IndFrtRed
        meta.Campos[5].Definidor(registro, Span<char>.Empty);  // VeicId
        meta.Campos[7].Definidor(registro, Span<char>.Empty);  // VlSecCat
        meta.Campos[8].Definidor(registro, Span<char>.Empty);  // VlDesp
        meta.Campos[9].Definidor(registro, Span<char>.Empty);  // VlPedg
        meta.Campos[10].Definidor(registro, Span<char>.Empty); // VlOut
        meta.Campos[12].Definidor(registro, Span<char>.Empty); // UfId

        registro.CodPartConsg.Should().BeNull();
        registro.CodPartRed.Should().BeNull();
        registro.IndFrtRed.Should().BeNull();
        registro.VeicId.Should().BeNull();
        registro.VlSecCat.Should().BeNull();
        registro.VlDesp.Should().BeNull();
        registro.VlPedg.Should().BeNull();
        registro.VlOut.Should().BeNull();
        registro.UfId.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // CT Rodoviário: São Paulo → Rio de Janeiro, com todos os valores preenchidos.
        const string sped = "|D130|CONSIG001|REDESP001|0|3550308|3304557|ABC1234|1500,00|50,00|30,00|20,00|10,00|1610,00|SP|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // CT sem consignatário, sem redespachado, sem veículo, sem valores adicionais.
        const string sped = "|D130|||1|3550308|3304557||1500,00||||10,00|1510,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_MunicipioExterior_PreservaTextoCanonico()
    {
        // CT com origem/destino no Exterior — código 9999999 conforme orientação do guia.
        const string sped = "|D130|||2|9999999|9999999||2000,00||||100,00|2100,00||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", IndicadorFreteRedespacho.SemRedespacho)]
    [InlineData("1", IndicadorFreteRedespacho.PorContaDoEmitente)]
    [InlineData("2", IndicadorFreteRedespacho.PorContaDoDestinatario)]
    [InlineData("9", IndicadorFreteRedespacho.Outros)]
    public void IndFrtRed_Definidor_MapeiaTodosOsValores(string texto, IndicadorFreteRedespacho esperado)
    {
        _catalogo.TentarObter("D130".AsSpan(), out var meta);
        var registro = (RegistroD130)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, texto.AsSpan());

        registro.IndFrtRed.Should().Be(esperado);
    }

    [Fact]
    public void IndFrtRed_Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D130".AsSpan(), out var meta);
        var registro = (RegistroD130)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, Span<char>.Empty);

        registro.IndFrtRed.Should().BeNull();
    }
}
