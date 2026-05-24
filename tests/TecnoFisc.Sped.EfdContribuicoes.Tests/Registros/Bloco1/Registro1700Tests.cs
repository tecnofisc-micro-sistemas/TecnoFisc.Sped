using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco1;

public sealed class Registro1700Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1700).Assembly);

    [Fact]
    public void Atributo_Declara1700_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1700).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1700");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1700Com7CamposNaOrdem()
    {
        _catalogo.TentarObter("1700".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1700");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8]);
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndNatRet", "PrRecRet", "VlRetApu", "VlRetDed", "VlRetPer", "VlRetDcomp", "SldRet",
        ]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // IndNatRet
        meta.Campos[1].Tamanho.Should().Be(6);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // PrRecRet
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // VlRetApu
        meta.Campos[3].Obrigatorio.Should().BeTrue();   // VlRetDed
        meta.Campos[4].Obrigatorio.Should().BeTrue();   // VlRetPer
        meta.Campos[5].Obrigatorio.Should().BeTrue();   // VlRetDcomp
        meta.Campos[6].Obrigatorio.Should().BeTrue();   // SldRet
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1700".AsSpan(), out var meta);
        var registro = (Registro1700)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01".AsSpan());       // IndNatRet
        meta.Campos[1].Definidor(registro, "012021".AsSpan());   // PrRecRet
        meta.Campos[2].Definidor(registro, "5000,00".AsSpan());  // VlRetApu
        meta.Campos[3].Definidor(registro, "1200,00".AsSpan());  // VlRetDed
        meta.Campos[4].Definidor(registro, "500,00".AsSpan());   // VlRetPer
        meta.Campos[5].Definidor(registro, "300,00".AsSpan());   // VlRetDcomp
        meta.Campos[6].Definidor(registro, "3000,00".AsSpan());  // SldRet

        registro.IndNatRet.Should().Be(IndicadorNaturezaRetencao.OrgaosAutarquiasFundacoesFederais);
        registro.PrRecRet.Should().Be("012021");
        registro.VlRetApu.Should().Be(5000.00m);
        registro.VlRetDed.Should().Be(1200.00m);
        registro.VlRetPer.Should().Be(500.00m);
        registro.VlRetDcomp.Should().Be(300.00m);
        registro.SldRet.Should().Be(3000.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("1700".AsSpan(), out var meta);
        var registro = (Registro1700)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);  // PrRecRet

        registro.PrRecRet.Should().BeNull();
    }

    [Theory]
    [InlineData("01", IndicadorNaturezaRetencao.OrgaosAutarquiasFundacoesFederais)]
    [InlineData("02", IndicadorNaturezaRetencao.OutrasEntidadesAdmPublicaFederal)]
    [InlineData("03", IndicadorNaturezaRetencao.PessoasJuridicasDireitoPrivado)]
    [InlineData("04", IndicadorNaturezaRetencao.RecolhimentoSociedadeCooperativa)]
    [InlineData("05", IndicadorNaturezaRetencao.FabricanteMaquinasVeiculos)]
    [InlineData("51", IndicadorNaturezaRetencao.OrgaosAutarquiasFundacoesFederaisCumulativoLucroReal)]
    [InlineData("52", IndicadorNaturezaRetencao.OutrasEntidadesAdmPublicaFederalCumulativoLucroReal)]
    [InlineData("53", IndicadorNaturezaRetencao.PessoasJuridicasDireitoPrivadoCumulativoLucroReal)]
    [InlineData("54", IndicadorNaturezaRetencao.RecolhimentoSociedadeCooperativaCumulativoLucroReal)]
    [InlineData("55", IndicadorNaturezaRetencao.FabricanteMaquinasVeiculosCumulativoLucroReal)]
    [InlineData("59", IndicadorNaturezaRetencao.OutrasRetencoesCumulativoLucroReal)]
    [InlineData("99", IndicadorNaturezaRetencao.OutrasRetencoes)]
    public void Definidor_IndNatRet_MapeiaValoresValidos(string sped, IndicadorNaturezaRetencao esperado)
    {
        _catalogo.TentarObter("1700".AsSpan(), out var meta);
        var registro = (Registro1700)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, sped.AsSpan());

        registro.IndNatRet.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1700|01|012021|5000,00|1200,00|500,00|300,00|3000,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_RetencaoCumulativaLucroReal_PreservaTextoCanonico()
    {
        const string sped = "|1700|51|032024|8500,75|2000,00|1500,00|500,00|4500,75|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

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
}
