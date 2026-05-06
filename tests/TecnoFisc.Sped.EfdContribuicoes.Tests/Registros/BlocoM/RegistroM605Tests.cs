using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoM;

public sealed class RegistroM605Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroM605).Assembly);

    [Fact]
    public void Atributo_DeclaraM605_Nivel3_BlocoM()
    {
        var atributo = typeof(RegistroM605).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("M605");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("M");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroM605Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("M605".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("M605");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
        meta.Campos.Select(c => c.Nome).Should().Equal(["NumCampo", "CodRec", "VlDebito"]);
        meta.Campos.All(c => c.Obrigatorio).Should().BeTrue();
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[1].Tamanho.Should().Be(6);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("M605".AsSpan(), out var meta);
        var registro = (RegistroM605)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "08".AsSpan());         // NumCampo
        meta.Campos[1].Definidor(registro, "560099".AsSpan());     // CodRec
        meta.Campos[2].Definidor(registro, "1500,00".AsSpan());    // VlDebito

        registro.NumCampo.Should().Be("08");
        registro.CodRec.Should().Be("560099");
        registro.VlDebito.Should().Be(1500m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|M605|08|560099|1500,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CampoContribuicaoCumulativa_PreservaTextoCanonico()
    {
        // Campo 12 = contribuição cumulativa a recolher
        const string sped = "|M605|12|560052|800,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
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
