using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoH;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoH;

/// <summary>
/// Sub-stage 8.189 — exercita a forma do <see cref="RegistroH010"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 246-247): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroH010Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroH010).Assembly);

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
    public void Atributo_DeclaraH010_Nivel3_BlocoH()
    {
        var atributo = typeof(RegistroH010).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("H010");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("H");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroH010ComDezCamposNaOrdem()
    {
        _catalogo.TentarObter("H010".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("H010");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodItem",
            "Unid",
            "Qtd",
            "VlUnit",
            "VlItem",
            "IndProp",
            "CodPart",
            "TxtCompl",
            "CodCta",
            "VlItemIr",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 10));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("H010".AsSpan(), out var meta);
        var registro = (RegistroH010)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "PROD001".AsSpan());
        meta.Campos[1].Definidor(registro, "UN".AsSpan());
        meta.Campos[2].Definidor(registro, "12,345".AsSpan());
        meta.Campos[3].Definidor(registro, "10,123456".AsSpan());
        meta.Campos[4].Definidor(registro, "125,00".AsSpan());
        meta.Campos[5].Definidor(registro, "1".AsSpan());
        meta.Campos[6].Definidor(registro, "PART001".AsSpan());
        meta.Campos[7].Definidor(registro, "Lote fiscal".AsSpan());
        meta.Campos[8].Definidor(registro, "CTA001".AsSpan());
        meta.Campos[9].Definidor(registro, "120,00".AsSpan());

        registro.CodItem.Should().Be("PROD001");
        registro.Unid.Should().Be("UN");
        registro.Qtd.Should().Be(12.345m);
        registro.VlUnit.Should().Be(10.123456m);
        registro.VlItem.Should().Be(125.00m);
        registro.IndProp.Should().Be(IndicadorPropriedadeItem.PropriedadeInformantePosseTerceiros);
        registro.CodPart.Should().Be("PART001");
        registro.TxtCompl.Should().Be("Lote fiscal");
        registro.CodCta.Should().Be("CTA001");
        registro.VlItemIr.Should().Be(120.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("H010".AsSpan(), out var meta);
        var registro = (RegistroH010)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodItem.Should().BeNull();
        registro.Unid.Should().BeNull();
        registro.Qtd.Should().Be(0m);
        registro.VlUnit.Should().Be(0m);
        registro.VlItem.Should().Be(0m);
        registro.IndProp.Should().Be(default(IndicadorPropriedadeItem));
        registro.CodPart.Should().BeNull();
        registro.TxtCompl.Should().BeNull();
        registro.CodCta.Should().BeNull();
        registro.VlItemIr.Should().BeNull();
    }

    [Theory]
    [InlineData("0", IndicadorPropriedadeItem.PropriedadeInformanteEmSeuPoder)]
    [InlineData("1", IndicadorPropriedadeItem.PropriedadeInformantePosseTerceiros)]
    [InlineData("2", IndicadorPropriedadeItem.PropriedadeTerceirosPosseInformante)]
    public void Definidor_IndProp_MapeiaCodigos(string input, IndicadorPropriedadeItem esperado)
    {
        _catalogo.TentarObter("H010".AsSpan(), out var meta);
        var registro = (RegistroH010)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, input.AsSpan());

        registro.IndProp.Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorPropriedadeItem.PropriedadeInformanteEmSeuPoder, "0")]
    [InlineData(IndicadorPropriedadeItem.PropriedadeInformantePosseTerceiros, "1")]
    [InlineData(IndicadorPropriedadeItem.PropriedadeTerceirosPosseInformante, "2")]
    public void Serializar_IndProp_RetornaCodigo(IndicadorPropriedadeItem indicador, string esperado)
    {
        _catalogo.TentarObter("H010".AsSpan(), out var meta);
        var registro = (RegistroH010)meta!.Fabrica();
        registro.IndProp = indicador;

        meta.Campos[5].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|H010|PROD001|UN|12,345|10,123456|125,00|1|PART001|Lote fiscal|CTA001|120,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComItemProprioSemCamposCondicionais_PreservaTextoCanonico()
    {
        const string sped = "|H010|PROD002|KG|1,000|20,000000|20,00|0|||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
