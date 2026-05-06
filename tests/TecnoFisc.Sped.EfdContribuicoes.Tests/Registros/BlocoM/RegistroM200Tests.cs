using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoM;

public sealed class RegistroM200Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroM200).Assembly);

    [Fact]
    public void Atributo_DeclaraM200_Nivel2_BlocoM()
    {
        var atributo = typeof(RegistroM200).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("M200");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("M");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroM200Com12CamposNaOrdem()
    {
        _catalogo.TentarObter("M200".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("M200");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "VlTotContNcPer", "VlTotCredDesc", "VlTotCredDescAnt", "VlTotContNcDev",
            "VlRetNc", "VlOutDedNc", "VlContNcRec", "VlTotContCumPer",
            "VlRetCum", "VlOutDedCum", "VlContCumRec", "VlTotContRec",
        ]);
        meta.Campos.All(c => c.Obrigatorio).Should().BeTrue();
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("M200".AsSpan(), out var meta);
        var registro = (RegistroM200)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "500,00".AsSpan());      // VlTotContNcPer
        meta.Campos[1].Definidor(registro, "100,00".AsSpan());      // VlTotCredDesc
        meta.Campos[2].Definidor(registro, "400,00".AsSpan());      // VlTotCredDescAnt
        meta.Campos[3].Definidor(registro, "0,00".AsSpan());        // VlTotContNcDev
        meta.Campos[4].Definidor(registro, "0,00".AsSpan());        // VlRetNc
        meta.Campos[5].Definidor(registro, "0,00".AsSpan());        // VlOutDedNc
        meta.Campos[6].Definidor(registro, "0,00".AsSpan());        // VlContNcRec
        meta.Campos[7].Definidor(registro, "1200,00".AsSpan());     // VlTotContCumPer
        meta.Campos[8].Definidor(registro, "50,00".AsSpan());       // VlRetCum
        meta.Campos[9].Definidor(registro, "0,00".AsSpan());        // VlOutDedCum
        meta.Campos[10].Definidor(registro, "1150,00".AsSpan());    // VlContCumRec
        meta.Campos[11].Definidor(registro, "1150,00".AsSpan());    // VlTotContRec

        registro.VlTotContNcPer.Should().Be(500m);
        registro.VlTotCredDesc.Should().Be(100m);
        registro.VlTotCredDescAnt.Should().Be(400m);
        registro.VlTotContNcDev.Should().Be(0m);
        registro.VlRetNc.Should().Be(0m);
        registro.VlOutDedNc.Should().Be(0m);
        registro.VlContNcRec.Should().Be(0m);
        registro.VlTotContCumPer.Should().Be(1200m);
        registro.VlRetCum.Should().Be(50m);
        registro.VlOutDedCum.Should().Be(0m);
        registro.VlContCumRec.Should().Be(1150m);
        registro.VlTotContRec.Should().Be(1150m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|M200|500,00|100,00|400,00|0,00|0,00|0,00|0,00|1200,00|50,00|0,00|1150,00|1150,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SomenteRegimeCumulativo_PreservaTextoCanonico()
    {
        // Contribuinte sujeito exclusivamente ao regime cumulativo (campos não cumulativos zerados)
        const string sped =
            "|M200|0,00|0,00|0,00|0,00|0,00|0,00|0,00|3500,75|120,50|30,00|3350,25|3350,25|\r\n";

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
