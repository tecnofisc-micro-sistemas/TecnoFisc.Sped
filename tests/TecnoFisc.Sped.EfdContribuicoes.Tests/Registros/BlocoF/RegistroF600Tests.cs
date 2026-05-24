using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoF;

public sealed class RegistroF600Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroF600).Assembly);

    [Fact]
    public void Atributo_DeclaraF600_Nivel3_BlocoF()
    {
        var atributo = typeof(RegistroF600).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("F600");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("F");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroF600Com10CamposNaOrdem()
    {
        _catalogo.TentarObter("F600".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("F600");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "IndNatRet", "DtRet", "VlBcRet", "VlRet", "CodRec",
            "IndNatRec", "Cnpj", "VlRetPis", "VlRetCofins", "IndDec",
        ]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // IndNatRet
        meta.Campos[1].Tamanho.Should().Be(8);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // DtRet
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // VlBcRet
        meta.Campos[3].Obrigatorio.Should().BeTrue();   // VlRet
        meta.Campos[6].Tamanho.Should().Be(14);
        meta.Campos[6].Obrigatorio.Should().BeTrue();   // Cnpj
        meta.Campos[7].Obrigatorio.Should().BeTrue();   // VlRetPis
        meta.Campos[8].Obrigatorio.Should().BeTrue();   // VlRetCofins
        meta.Campos[9].Obrigatorio.Should().BeTrue();   // IndDec
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("F600".AsSpan(), out var meta);
        var registro = (RegistroF600)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01".AsSpan());             // IndNatRet
        meta.Campos[1].Definidor(registro, "01012024".AsSpan());       // DtRet
        meta.Campos[2].Definidor(registro, "1000,0000".AsSpan());      // VlBcRet
        meta.Campos[3].Definidor(registro, "650,00".AsSpan());         // VlRet
        meta.Campos[4].Definidor(registro, "5952".AsSpan());           // CodRec
        meta.Campos[5].Definidor(registro, "0".AsSpan());              // IndNatRec
        meta.Campos[6].Definidor(registro, "12345678000195".AsSpan()); // Cnpj
        meta.Campos[7].Definidor(registro, "422,50".AsSpan());         // VlRetPis
        meta.Campos[8].Definidor(registro, "227,50".AsSpan());         // VlRetCofins
        meta.Campos[9].Definidor(registro, "0".AsSpan());              // IndDec

        registro.IndNatRet.Should().Be(IndicadorNaturezaRetencao.OrgaosAutarquiasFundacoesFederais);
        registro.DtRet.Should().Be(new DateOnly(2024, 1, 1));
        registro.VlBcRet.Should().Be(1000m);
        registro.VlRet.Should().Be(650m);
        registro.CodRec.Should().Be("5952");
        registro.IndNatRec.Should().Be(IndicadorNaturezaCumulativa.NaoCumulativa);
        registro.Cnpj.Should().Be(Cnpj.Create("12345678000195"));
        registro.VlRetPis.Should().Be(422.50m);
        registro.VlRetCofins.Should().Be(227.50m);
        registro.IndDec.Should().Be(IndicadorDeclarante.BeneficiariaDaRetencao);
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("F600".AsSpan(), out var meta);
        var registro = (RegistroF600)meta!.Fabrica();

        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // CodRec
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // IndNatRec

        registro.CodRec.Should().BeNull();
        registro.IndNatRec.Should().BeNull();
    }

    [Theory]
    [InlineData("01", IndicadorNaturezaRetencao.OrgaosAutarquiasFundacoesFederais)]
    [InlineData("02", IndicadorNaturezaRetencao.OutrasEntidadesAdmPublicaFederal)]
    [InlineData("03", IndicadorNaturezaRetencao.PessoasJuridicasDireitoPrivado)]
    [InlineData("04", IndicadorNaturezaRetencao.RecolhimentoSociedadeCooperativa)]
    [InlineData("05", IndicadorNaturezaRetencao.FabricanteMaquinasVeiculos)]
    [InlineData("99", IndicadorNaturezaRetencao.OutrasRetencoes)]
    public void Definidor_IndNatRet_AtribuiEnumCorreto(string codigo, IndicadorNaturezaRetencao esperado)
    {
        _catalogo.TentarObter("F600".AsSpan(), out var meta);
        var registro = (RegistroF600)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, codigo.AsSpan());

        registro.IndNatRet.Should().Be(esperado);
    }

    [Theory]
    [InlineData("0", IndicadorNaturezaCumulativa.NaoCumulativa)]
    [InlineData("1", IndicadorNaturezaCumulativa.Cumulativa)]
    public void Definidor_IndNatRec_AtribuiEnumCorreto(string codigo, IndicadorNaturezaCumulativa esperado)
    {
        _catalogo.TentarObter("F600".AsSpan(), out var meta);
        var registro = (RegistroF600)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, codigo.AsSpan());

        registro.IndNatRec.Should().Be(esperado);
    }

    [Theory]
    [InlineData("0", IndicadorDeclarante.BeneficiariaDaRetencao)]
    [InlineData("1", IndicadorDeclarante.ResponsavelPeloRecolhimento)]
    public void Definidor_IndDec_AtribuiEnumCorreto(string codigo, IndicadorDeclarante esperado)
    {
        _catalogo.TentarObter("F600".AsSpan(), out var meta);
        var registro = (RegistroF600)meta!.Fabrica();

        meta.Campos[9].Definidor(registro, codigo.AsSpan());

        registro.IndDec.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|F600|01|01012024|1000,0000|650,00|5952|0|12345678000195|422,50|227,50|0|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCodRecEIndNatRec_PreservaTextoCanonico()
    {
        const string sped =
            "|F600|03|15062023|50000,0000|3250,00|||11444777000161|2112,50|1137,50|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SociedadeCooperativa_PreservaTextoCanonico()
    {
        const string sped =
            "|F600|04|31122023|200000,0000|13000,00|1234|1|11222333000181|8450,00|4550,00|1|\r\n";

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
