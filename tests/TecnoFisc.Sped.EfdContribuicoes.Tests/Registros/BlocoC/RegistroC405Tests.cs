using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC405Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC405).Assembly);

    [Fact]
    public void Atributo_DeclaraC405_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC405).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C405");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC405ComSeisCamposNaOrdem()
    {
        _catalogo.TentarObter("C405".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C405");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["DtDoc", "Cro", "Crz", "NumCooFin", "GtFin", "VlBrt"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
        meta.Campos[0].Tamanho.Should().Be(8);
        meta.Campos[0].Obrigatorio.Should().BeTrue();    // DtDoc
        meta.Campos[1].Tamanho.Should().Be(3);
        meta.Campos[1].Obrigatorio.Should().BeTrue();    // Cro
        meta.Campos[2].Tamanho.Should().Be(6);
        meta.Campos[2].Obrigatorio.Should().BeTrue();    // Crz
        meta.Campos[3].Tamanho.Should().Be(6);
        meta.Campos[3].Obrigatorio.Should().BeTrue();    // NumCooFin
        meta.Campos[4].Obrigatorio.Should().BeTrue();    // GtFin
        meta.Campos[5].Obrigatorio.Should().BeTrue();    // VlBrt
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C405".AsSpan(), out var meta);
        var registro = (RegistroC405)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01012024".AsSpan());  // DtDoc
        meta.Campos[1].Definidor(registro, "1".AsSpan());         // Cro
        meta.Campos[2].Definidor(registro, "150".AsSpan());       // Crz
        meta.Campos[3].Definidor(registro, "1000".AsSpan());      // NumCooFin
        meta.Campos[4].Definidor(registro, "10000,00".AsSpan());  // GtFin
        meta.Campos[5].Definidor(registro, "8500,00".AsSpan());   // VlBrt

        registro.DtDoc.Should().Be(new DateOnly(2024, 1, 1));
        registro.Cro.Should().Be(1);
        registro.Crz.Should().Be(150);
        registro.NumCooFin.Should().Be(1000);
        registro.GtFin.Should().Be(10000m);
        registro.VlBrt.Should().Be(8500m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C405|01012024|1|150|1000|10000,00|8500,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComValoresAltos_PreservaTextoCanonico()
    {
        const string sped = "|C405|31122023|5|999|9999|99999,99|88888,88|\r\n";

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
