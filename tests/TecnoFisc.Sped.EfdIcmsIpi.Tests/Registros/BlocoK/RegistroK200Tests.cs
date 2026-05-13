using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoK;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoK;

/// <summary>
/// Sub-stage 8.195 — exercita a forma do <see cref="RegistroK200"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 250-251): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroK200Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroK200).Assembly);

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
    public void Atributo_DeclaraK200_Nivel3_BlocoK()
    {
        var atributo = typeof(RegistroK200).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("K200");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("K");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroK200ComCincoCamposNaOrdem()
    {
        _catalogo.TentarObter("K200".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("K200");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtEst",
            "CodItem",
            "Qtd",
            "IndEst",
            "CodPart",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("K200".AsSpan(), out var meta);
        var registro = (RegistroK200)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "31012025".AsSpan());
        meta.Campos[1].Definidor(registro, "PROD001".AsSpan());
        meta.Campos[2].Definidor(registro, "12,345".AsSpan());
        meta.Campos[3].Definidor(registro, "1".AsSpan());
        meta.Campos[4].Definidor(registro, "PART001".AsSpan());

        registro.DtEst.Should().Be(new DateOnly(2025, 1, 31));
        registro.CodItem.Should().Be("PROD001");
        registro.Qtd.Should().Be(12.345m);
        registro.IndEst.Should().Be(IndicadorPropriedadeItem.PropriedadeInformantePosseTerceiros);
        registro.CodPart.Should().Be("PART001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("K200".AsSpan(), out var meta);
        var registro = (RegistroK200)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.DtEst.Should().Be(default(DateOnly));
        registro.CodItem.Should().BeNull();
        registro.Qtd.Should().Be(0m);
        registro.IndEst.Should().Be(default(IndicadorPropriedadeItem));
        registro.CodPart.Should().BeNull();
    }

    [Theory]
    [InlineData("0", IndicadorPropriedadeItem.PropriedadeInformanteEmSeuPoder)]
    [InlineData("1", IndicadorPropriedadeItem.PropriedadeInformantePosseTerceiros)]
    [InlineData("2", IndicadorPropriedadeItem.PropriedadeTerceirosPosseInformante)]
    public void Definidor_IndEst_MapeiaCodigos(string input, IndicadorPropriedadeItem esperado)
    {
        _catalogo.TentarObter("K200".AsSpan(), out var meta);
        var registro = (RegistroK200)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, input.AsSpan());

        registro.IndEst.Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorPropriedadeItem.PropriedadeInformanteEmSeuPoder, "0")]
    [InlineData(IndicadorPropriedadeItem.PropriedadeInformantePosseTerceiros, "1")]
    [InlineData(IndicadorPropriedadeItem.PropriedadeTerceirosPosseInformante, "2")]
    public void Serializar_IndEst_RetornaCodigo(IndicadorPropriedadeItem indicador, string esperado)
    {
        _catalogo.TentarObter("K200".AsSpan(), out var meta);
        var registro = (RegistroK200)meta!.Fabrica();
        registro.IndEst = indicador;

        meta.Campos[3].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|K200|31012025|PROD001|12,345|1|PART001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComEstoqueProprioSemParticipante_PreservaTextoCanonico()
    {
        const string sped = "|K200|31012025|PROD002|0,000|0||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
