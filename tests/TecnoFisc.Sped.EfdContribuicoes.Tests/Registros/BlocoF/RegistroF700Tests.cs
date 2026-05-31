using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoF;

public sealed class RegistroF700Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroF700).Assembly);

    [Fact]
    public void Atributo_DeclaraF700_Nivel3_BlocoF()
    {
        var atributo = typeof(RegistroF700).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("F700");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("F");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroF700Com7CamposNaOrdem()
    {
        _catalogo.TentarObter("F700".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("F700");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "IndOriDed", "IndNatDed", "VlDedPis", "VlDedCofins",
            "VlBcOper", "Cnpj", "InfComp",
        ]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();  // IndOriDed
        meta.Campos[1].Tamanho.Should().Be(1);
        meta.Campos[1].Obrigatorio.Should().BeTrue();  // IndNatDed
        meta.Campos[2].Obrigatorio.Should().BeTrue();  // VlDedPis
        meta.Campos[3].Obrigatorio.Should().BeTrue();  // VlDedCofins
        meta.Campos[5].Tamanho.Should().Be(14);        // Cnpj
        meta.Campos[6].Tamanho.Should().Be(90);        // InfComp
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("F700".AsSpan(), out var meta);
        var registro = (RegistroF700)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01".AsSpan());             // IndOriDed
        meta.Campos[1].Definidor(registro, "0".AsSpan());              // IndNatDed
        meta.Campos[2].Definidor(registro, "500,00".AsSpan());         // VlDedPis
        meta.Campos[3].Definidor(registro, "1500,00".AsSpan());        // VlDedCofins
        meta.Campos[4].Definidor(registro, "100000,00".AsSpan());      // VlBcOper
        meta.Campos[5].Definidor(registro, "12345678000195".AsSpan()); // Cnpj
        meta.Campos[6].Definidor(registro, "Credito presumido medicamentos".AsSpan()); // InfComp

        registro.IndOriDed.Should().Be(IndicadorOrigemDeducoesDiversas.CreditosPresumidosMedicamentos);
        registro.IndNatDed.Should().Be(IndicadorNaturezaCumulativa.NaoCumulativa);
        registro.VlDedPis.Should().Be(500m);
        registro.VlDedCofins.Should().Be(1500m);
        registro.VlBcOper.Should().Be(100000m);
        registro.Cnpj.Should().Be(Cnpj.Create("12345678000195"));
        registro.InfComp.Should().Be("Credito presumido medicamentos");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("F700".AsSpan(), out var meta);
        var registro = (RegistroF700)meta!.Fabrica();

        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcOper
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // Cnpj
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // InfComp

        registro.VlBcOper.Should().BeNull();
        registro.Cnpj.Should().BeNull();
        registro.InfComp.Should().BeNull();
    }

    [Theory]
    [InlineData("01", IndicadorOrigemDeducoesDiversas.CreditosPresumidosMedicamentos)]
    [InlineData("02", IndicadorOrigemDeducoesDiversas.CreditosAdmitidosRegimeCumulativoBebidas)]
    [InlineData("03", IndicadorOrigemDeducoesDiversas.ContribuicaoPagaSubstitutoTributarioZfm)]
    [InlineData("04", IndicadorOrigemDeducoesDiversas.SubstituicaoTributariaNaoOcorrenciaFatoGerador)]
    [InlineData("99", IndicadorOrigemDeducoesDiversas.OutrasDeducoes)]
    public void Definidor_IndOriDed_AtribuiEnumCorreto(string codigo, IndicadorOrigemDeducoesDiversas esperado)
    {
        _catalogo.TentarObter("F700".AsSpan(), out var meta);
        var registro = (RegistroF700)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, codigo.AsSpan());

        registro.IndOriDed.Should().Be(esperado);
    }

    [Theory]
    [InlineData("0", IndicadorNaturezaCumulativa.NaoCumulativa)]
    [InlineData("1", IndicadorNaturezaCumulativa.Cumulativa)]
    public void Definidor_IndNatDed_AtribuiEnumCorreto(string codigo, IndicadorNaturezaCumulativa esperado)
    {
        _catalogo.TentarObter("F700".AsSpan(), out var meta);
        var registro = (RegistroF700)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, codigo.AsSpan());

        registro.IndNatDed.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|F700|01|0|500,00|1500,00|100000,00|12345678000195|Credito presumido medicamentos|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        const string sped =
            "|F700|99|1|250,00|750,00||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_BebidasFrias_PreservaTextoCanonico()
    {
        const string sped =
            "|F700|02|0|1200,00|3600,00|500000,00||Ressarcimento CMB bebidas frias|\r\n";

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
