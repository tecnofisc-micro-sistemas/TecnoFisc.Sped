using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoM;

public sealed class RegistroM350Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroM350).Assembly);

    [Fact]
    public void Atributo_DeclaraM350_Nivel2_BlocoM()
    {
        var atributo = typeof(RegistroM350).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("M350");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("M");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroM350Com5CamposNaOrdem()
    {
        _catalogo.TentarObter("M350".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("M350");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "VlTotFol", "VlExcBc", "VlTotBc", "AliqPisFol", "VlTotContFol",
        ]);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // VlTotFol
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // VlExcBc
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // VlTotBc
        meta.Campos[3].Tamanho.Should().Be(6);
        meta.Campos[3].Obrigatorio.Should().BeTrue();   // AliqPisFol
        meta.Campos[4].Obrigatorio.Should().BeTrue();   // VlTotContFol
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("M350".AsSpan(), out var meta);
        var registro = (RegistroM350)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "100000,00".AsSpan()); // VlTotFol
        meta.Campos[1].Definidor(registro, "5000,00".AsSpan());   // VlExcBc
        meta.Campos[2].Definidor(registro, "95000,00".AsSpan());  // VlTotBc
        meta.Campos[3].Definidor(registro, "1,00".AsSpan());      // AliqPisFol
        meta.Campos[4].Definidor(registro, "950,00".AsSpan());    // VlTotContFol

        registro.VlTotFol.Should().Be(100000m);
        registro.VlExcBc.Should().Be(5000m);
        registro.VlTotBc.Should().Be(95000m);
        registro.AliqPisFol.Should().Be(1m);
        registro.VlTotContFol.Should().Be(950m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|M350|100000,00|5000,00|95000,00|1,00|950,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_FolhaMenorComExclusaoZero_PreservaTextoCanonico()
    {
        const string sped = "|M350|50000,00|0,00|50000,00|1,00|500,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}
