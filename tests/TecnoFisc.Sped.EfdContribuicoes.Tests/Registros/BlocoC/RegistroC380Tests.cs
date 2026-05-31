using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC380Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC380).Assembly);

    [Fact]
    public void Atributo_DeclaraC380_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC380).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C380");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC380ComSeteCamposNaOrdem()
    {
        _catalogo.TentarObter("C380".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C380");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["CodMod", "DtDocIni", "DtDocFin", "NumDocIni", "NumDocFin", "VlDoc", "VlDocCanc"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();
        meta.Campos[1].Tamanho.Should().Be(8);
        meta.Campos[1].Obrigatorio.Should().BeTrue();
        meta.Campos[2].Tamanho.Should().Be(8);
        meta.Campos[2].Obrigatorio.Should().BeTrue();
        meta.Campos[5].Obrigatorio.Should().BeTrue();  // VlDoc
        meta.Campos[6].Obrigatorio.Should().BeTrue();  // VlDocCanc
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C380".AsSpan(), out var meta);
        var registro = (RegistroC380)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "02".AsSpan());         // CodMod
        meta.Campos[1].Definidor(registro, "01012024".AsSpan());   // DtDocIni
        meta.Campos[2].Definidor(registro, "31012024".AsSpan());   // DtDocFin
        meta.Campos[3].Definidor(registro, "1".AsSpan());          // NumDocIni
        meta.Campos[4].Definidor(registro, "100".AsSpan());        // NumDocFin
        meta.Campos[5].Definidor(registro, "5000,00".AsSpan());    // VlDoc
        meta.Campos[6].Definidor(registro, "200,00".AsSpan());     // VlDocCanc

        registro.CodMod.Should().Be("02");
        registro.DtDocIni.Should().Be(new DateOnly(2024, 1, 1));
        registro.DtDocFin.Should().Be(new DateOnly(2024, 1, 31));
        registro.NumDocIni.Should().Be(1);
        registro.NumDocFin.Should().Be(100);
        registro.VlDoc.Should().Be(5000m);
        registro.VlDocCanc.Should().Be(200m);
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("C380".AsSpan(), out var meta);
        var registro = (RegistroC380)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // NumDocIni
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // NumDocFin

        registro.NumDocIni.Should().BeNull();
        registro.NumDocFin.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C380|02|01012024|31012024|1|100|5000,00|200,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemNumeroDocumento_PreservaTextoCanonico()
    {
        const string sped = "|C380|02|01022024|28022024|||3200,50|0,00|\r\n";

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
