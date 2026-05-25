using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.Ecd.Registros.BlocoI;

namespace TecnoFisc.Sped.Ecd.Tests.Registros.BlocoI;

/// <summary>
/// Sub-stage 10.028 — exercita a forma do <see cref="RegistroI053"/> contra o Manual de
/// Orientação do Leiaute 9 da ECD (p. 127–129): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico. Pacote read-only — o round-trip
/// usa o <see cref="EscritorSpedTxt"/> genérico do Core.
/// </summary>
public sealed class RegistroI053Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0000).Assembly);

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
    public void Atributo_DeclaraI053_Nivel4_BlocoI()
    {
        var atributo = typeof(RegistroI053).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I053");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI053Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("I053".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I053");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodIdt", "CodCntCorr", "NatSubCnt",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I053".AsSpan(), out var meta);
        var registro = (RegistroI053)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "FT1234".AsSpan());
        meta.Campos[1].Definidor(registro, "1.05.01.10".AsSpan());
        meta.Campos[2].Definidor(registro, "02".AsSpan());

        registro.CodIdt.Should().Be("FT1234");
        registro.CodCntCorr.Should().Be("1.05.01.10");
        registro.NatSubCnt.Should().Be(NaturezaSubcontaCorrelata.TbuControladaDiretaExterior);
    }

    [Fact]
    public void Definidor_NatSubCntVazio_DevolveNulo()
    {
        _catalogo.TentarObter("I053".AsSpan(), out var meta);
        var registro = (RegistroI053)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.NatSubCnt.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Exemplo do manual (p. 129): COD_IDT=FT1234, COD_CNT_CORR=1.05.01.10, NAT_SUB_CNT=02
        const string sped = "|I053|FT1234|1.05.01.10|02|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemNatSubCnt_PreservaTextoCanonico()
    {
        // NAT_SUB_CNT ausente — campo opcional
        const string sped = "|I053|GRP001|2.01.01.01||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("02", NaturezaSubcontaCorrelata.TbuControladaDiretaExterior)]
    [InlineData("03", NaturezaSubcontaCorrelata.TbuControladaIndiretaExterior)]
    [InlineData("10", NaturezaSubcontaCorrelata.Goodwill)]
    [InlineData("11", NaturezaSubcontaCorrelata.MaisValia)]
    [InlineData("12", NaturezaSubcontaCorrelata.MenosValia)]
    [InlineData("60", NaturezaSubcontaCorrelata.AvjReflexo)]
    [InlineData("65", NaturezaSubcontaCorrelata.AvjSubscricaoCapital)]
    [InlineData("70", NaturezaSubcontaCorrelata.AvjVinculadaAtivoPassivo)]
    [InlineData("71", NaturezaSubcontaCorrelata.AvjDepreciacaoAcumulada)]
    [InlineData("72", NaturezaSubcontaCorrelata.AvjAmortizacaoAcumulada)]
    [InlineData("73", NaturezaSubcontaCorrelata.AvjExaustaoAcumulada)]
    [InlineData("75", NaturezaSubcontaCorrelata.AvpVinculadaAoAtivo)]
    [InlineData("76", NaturezaSubcontaCorrelata.AvpDepreciacaoAcumulada)]
    [InlineData("77", NaturezaSubcontaCorrelata.AvpAmortizacaoAcumulada)]
    [InlineData("78", NaturezaSubcontaCorrelata.AvpExaustaoAcumulada)]
    [InlineData("80", NaturezaSubcontaCorrelata.MaisValiaAnteriorEstagios)]
    [InlineData("81", NaturezaSubcontaCorrelata.MenosValiaAnteriorEstagios)]
    [InlineData("82", NaturezaSubcontaCorrelata.GoodwillAnteriorEstagios)]
    [InlineData("84", NaturezaSubcontaCorrelata.VariacaoMaisValiaAnteriorEstagios)]
    [InlineData("85", NaturezaSubcontaCorrelata.VariacaoMenosValiaAnteriorEstagios)]
    [InlineData("86", NaturezaSubcontaCorrelata.VariacaoGoodwillAnteriorEstagios)]
    [InlineData("90", NaturezaSubcontaCorrelata.AdocaoInicialAtivoPassivo)]
    [InlineData("91", NaturezaSubcontaCorrelata.AdocaoInicialDepreciacaoAcumulada)]
    [InlineData("92", NaturezaSubcontaCorrelata.AdocaoInicialAmortizacaoAcumulada)]
    [InlineData("93", NaturezaSubcontaCorrelata.AdocaoInicialExaustaoAcumulada)]
    public void NaturezaSubcontaCorrelata_ValorSped_MapeiaMembro(string valorSped, NaturezaSubcontaCorrelata esperado)
    {
        _catalogo.TentarObter("I053".AsSpan(), out var meta);
        var registro = (RegistroI053)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, valorSped.AsSpan());

        registro.NatSubCnt.Should().Be(esperado);
    }
}
