using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC180Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC180).Assembly);

    [Fact]
    public void Atributo_DeclaraC180_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC180).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C180");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC180Com7CamposNaOrdem()
    {
        _catalogo.TentarObter("C180".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C180");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodMod", "DtDocIni", "DtDocFin", "CodItem", "CodNcm", "ExIpi", "VlTotItem",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();  // CodMod
        meta.Campos[1].Tamanho.Should().Be(8);
        meta.Campos[1].Obrigatorio.Should().BeTrue();  // DtDocIni
        meta.Campos[2].Tamanho.Should().Be(8);
        meta.Campos[2].Obrigatorio.Should().BeTrue();  // DtDocFin
        meta.Campos[3].Tamanho.Should().Be(60);
        meta.Campos[3].Obrigatorio.Should().BeTrue();  // CodItem
        meta.Campos[6].Obrigatorio.Should().BeTrue();  // VlTotItem
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C180".AsSpan(), out var meta);
        var registro = (RegistroC180)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "55".AsSpan());         // CodMod
        meta.Campos[1].Definidor(registro, "01012024".AsSpan());   // DtDocIni
        meta.Campos[2].Definidor(registro, "31012024".AsSpan());   // DtDocFin
        meta.Campos[3].Definidor(registro, "PROD001".AsSpan());    // CodItem
        meta.Campos[4].Definidor(registro, "12345678".AsSpan());   // CodNcm
        meta.Campos[5].Definidor(registro, "001".AsSpan());        // ExIpi
        meta.Campos[6].Definidor(registro, "50000,00".AsSpan());   // VlTotItem

        registro.CodMod.Should().Be("55");
        registro.DtDocIni.Should().Be(new DateOnly(2024, 1, 1));
        registro.DtDocFin.Should().Be(new DateOnly(2024, 1, 31));
        registro.CodItem.Should().Be("PROD001");
        registro.CodNcm.Should().Be(Ncm.Create("12345678"));
        registro.ExIpi.Should().Be("001");
        registro.VlTotItem.Should().Be(50000m);
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("C180".AsSpan(), out var meta);
        var registro = (RegistroC180)meta!.Fabrica();

        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // CodNcm
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // ExIpi

        registro.CodNcm.Should().BeNull();
        registro.ExIpi.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C180|55|01012024|31012024|PROD001|12345678|001|50000,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped =
            "|C180|65|01022024|29022024|ITEM-ABC|||1234,56|\r\n";

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
