using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco1;

public sealed class Registro1600Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1600).Assembly);

    [Fact]
    public void Atributo_Declara1600_Nivel2_Bloco1()
    {
        var atributo = typeof(Registro1600).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1600");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1600Com10CamposNaOrdem()
    {
        _catalogo.TentarObter("1600".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1600");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11]);
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "PerApurAnt", "NatContRec", "VlContApur", "VlCredCofinsDesc", "VlContDev",
            "VlOutDed", "VlContExt", "VlMul", "VlJur", "DtRecol",
        ]);
        meta.Campos[0].Obrigatorio.Should().BeTrue();  // PerApurAnt
        meta.Campos[1].Obrigatorio.Should().BeTrue();  // NatContRec
        meta.Campos[2].Obrigatorio.Should().BeTrue();  // VlContApur
        meta.Campos[3].Obrigatorio.Should().BeTrue();  // VlCredCofinsDesc
        meta.Campos[4].Obrigatorio.Should().BeTrue();  // VlContDev
        meta.Campos[5].Obrigatorio.Should().BeTrue();  // VlOutDed
        meta.Campos[6].Obrigatorio.Should().BeTrue();  // VlContExt
        meta.Campos[7].Obrigatorio.Should().BeFalse(); // VlMul
        meta.Campos[8].Obrigatorio.Should().BeFalse(); // VlJur
        meta.Campos[9].Obrigatorio.Should().BeFalse(); // DtRecol
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1600".AsSpan(), out var meta);
        var registro = (Registro1600)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "012023".AsSpan());     // PerApurAnt
        meta.Campos[1].Definidor(registro, "01".AsSpan());         // NatContRec
        meta.Campos[2].Definidor(registro, "1000,00".AsSpan());    // VlContApur
        meta.Campos[3].Definidor(registro, "500,00".AsSpan());     // VlCredCofinsDesc
        meta.Campos[4].Definidor(registro, "500,00".AsSpan());     // VlContDev
        meta.Campos[5].Definidor(registro, "100,00".AsSpan());     // VlOutDed
        meta.Campos[6].Definidor(registro, "400,00".AsSpan());     // VlContExt
        meta.Campos[7].Definidor(registro, "50,00".AsSpan());      // VlMul
        meta.Campos[8].Definidor(registro, "10,00".AsSpan());      // VlJur
        meta.Campos[9].Definidor(registro, "15012023".AsSpan());   // DtRecol

        registro.PerApurAnt.Should().Be("012023");
        registro.NatContRec.Should().Be(CodigoContribuicaoSocialApurada.NaoCumulativaAliquotaBasica);
        registro.VlContApur.Should().Be(1000.00m);
        registro.VlCredCofinsDesc.Should().Be(500.00m);
        registro.VlContDev.Should().Be(500.00m);
        registro.VlOutDed.Should().Be(100.00m);
        registro.VlContExt.Should().Be(400.00m);
        registro.VlMul.Should().Be(50.00m);
        registro.VlJur.Should().Be(10.00m);
        registro.DtRecol.Should().Be(new DateOnly(2023, 1, 15));
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("1600".AsSpan(), out var meta);
        var registro = (Registro1600)meta!.Fabrica();

        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty); // VlMul
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty); // VlJur
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty); // DtRecol

        registro.VlMul.Should().BeNull();
        registro.VlJur.Should().BeNull();
        registro.DtRecol.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|1600|012023|01|1000,00|500,00|500,00|100,00|400,00|50,00|10,00|15012023|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemMultaJurosEData_PreservaTextoCanonico()
    {
        const string sped = "|1600|062022|51|2500,00|0,00|2500,00|0,00|2500,00||||\r\n";

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
