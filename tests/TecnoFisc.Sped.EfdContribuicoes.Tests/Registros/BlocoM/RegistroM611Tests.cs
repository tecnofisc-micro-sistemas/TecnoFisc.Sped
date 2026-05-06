using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoM;

public sealed class RegistroM611Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroM611).Assembly);

    [Fact]
    public void Atributo_DeclaraM611_Nivel4_BlocoM()
    {
        var atributo = typeof(RegistroM611).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("M611");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("M");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroM611Com5CamposNaOrdem()
    {
        _catalogo.TentarObter("M611".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("M611");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "IndTipCoop", "VlBcContAntExcCoop", "VlExcCoopGer", "VlExcEspCoop", "VlBcCont",
        ]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // IndTipCoop
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // VlBcContAntExcCoop
        meta.Campos[2].Obrigatorio.Should().BeFalse();  // VlExcCoopGer
        meta.Campos[3].Obrigatorio.Should().BeFalse();  // VlExcEspCoop
        meta.Campos[4].Obrigatorio.Should().BeTrue();   // VlBcCont
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("M611".AsSpan(), out var meta);
        var registro = (RegistroM611)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01".AsSpan());          // IndTipCoop
        meta.Campos[1].Definidor(registro, "100000,00".AsSpan());   // VlBcContAntExcCoop
        meta.Campos[2].Definidor(registro, "5000,00".AsSpan());     // VlExcCoopGer
        meta.Campos[3].Definidor(registro, "3000,00".AsSpan());     // VlExcEspCoop
        meta.Campos[4].Definidor(registro, "92000,00".AsSpan());    // VlBcCont

        registro.IndTipCoop.Should().Be(IndicadorTipoCooperativa.ProducaoAgropecuaria);
        registro.VlBcContAntExcCoop.Should().Be(100000m);
        registro.VlExcCoopGer.Should().Be(5000m);
        registro.VlExcEspCoop.Should().Be(3000m);
        registro.VlBcCont.Should().Be(92000m);
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("M611".AsSpan(), out var meta);
        var registro = (RegistroM611)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // VlExcCoopGer
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // VlExcEspCoop

        registro.VlExcCoopGer.Should().BeNull();
        registro.VlExcEspCoop.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|M611|01|100000,00|5000,00|3000,00|92000,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemExclusoesOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|M611|02|80000,00|||80000,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("01", IndicadorTipoCooperativa.ProducaoAgropecuaria)]
    [InlineData("02", IndicadorTipoCooperativa.Consumo)]
    [InlineData("03", IndicadorTipoCooperativa.Credito)]
    [InlineData("04", IndicadorTipoCooperativa.EletrificacaoRural)]
    [InlineData("05", IndicadorTipoCooperativa.TransporteRodoviarioCargas)]
    [InlineData("06", IndicadorTipoCooperativa.Medicos)]
    [InlineData("99", IndicadorTipoCooperativa.Outras)]
    public void IndicadorTipoCooperativa_Roundtrip_MapeiaCodigo(string codigo, IndicadorTipoCooperativa esperado)
    {
        _catalogo.TentarObter("M611".AsSpan(), out var meta);
        var registro = (RegistroM611)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, codigo.AsSpan());

        registro.IndTipCoop.Should().Be(esperado);
    }

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
}
