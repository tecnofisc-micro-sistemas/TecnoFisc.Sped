using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoB;

/// <summary>
/// Sub-stage 8.032 — exercita a forma do <see cref="RegistroB470"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 55): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroB470Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroB470).Assembly);

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

    [Fact]
    public void Atributo_DeclaraB470_Nivel2_BlocoB()
    {
        var atributo = typeof(RegistroB470).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("B470");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("B");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroB470Com14CamposNaOrdem()
    {
        _catalogo.TentarObter("B470".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("B470");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "VlCont", "VlMatTerc", "VlMatProp", "VlSub", "VlIsnt",
            "VlDedBc", "VlBcIss", "VlBcIssRt", "VlIss", "VlIssRt",
            "VlDed", "VlIssRec", "VlIssSt", "VlIssRecUni",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 14));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("B470".AsSpan(), out var meta);
        var registro = (RegistroB470)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "10000,00".AsSpan());  // VlCont
        meta.Campos[1].Definidor(registro, "500,00".AsSpan());    // VlMatTerc
        meta.Campos[2].Definidor(registro, "300,00".AsSpan());    // VlMatProp
        meta.Campos[3].Definidor(registro, "200,00".AsSpan());    // VlSub
        meta.Campos[4].Definidor(registro, "100,00".AsSpan());    // VlIsnt
        meta.Campos[5].Definidor(registro, "1100,00".AsSpan());   // VlDedBc
        meta.Campos[6].Definidor(registro, "8900,00".AsSpan());   // VlBcIss
        meta.Campos[7].Definidor(registro, "7000,00".AsSpan());   // VlBcIssRt
        meta.Campos[8].Definidor(registro, "250,00".AsSpan());    // VlIss
        meta.Campos[9].Definidor(registro, "50,00".AsSpan());     // VlIssRt
        meta.Campos[10].Definidor(registro, "100,00".AsSpan());   // VlDed
        meta.Campos[11].Definidor(registro, "100,00".AsSpan());   // VlIssRec
        meta.Campos[12].Definidor(registro, "1500,00".AsSpan());  // VlIssSt
        meta.Campos[13].Definidor(registro, "80,00".AsSpan());    // VlIssRecUni

        registro.VlCont.Should().Be(10000.00m);
        registro.VlMatTerc.Should().Be(500.00m);
        registro.VlMatProp.Should().Be(300.00m);
        registro.VlSub.Should().Be(200.00m);
        registro.VlIsnt.Should().Be(100.00m);
        registro.VlDedBc.Should().Be(1100.00m);
        registro.VlBcIss.Should().Be(8900.00m);
        registro.VlBcIssRt.Should().Be(7000.00m);
        registro.VlIss.Should().Be(250.00m);
        registro.VlIssRt.Should().Be(50.00m);
        registro.VlDed.Should().Be(100.00m);
        registro.VlIssRec.Should().Be(100.00m);
        registro.VlIssSt.Should().Be(1500.00m);
        registro.VlIssRecUni.Should().Be(80.00m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|B470|10000,00|500,00|300,00|200,00|100,00|1100,00|8900,00|7000,00|250,00|50,00|100,00|100,00|1500,00|80,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComValoresZerados_PreservaTextoCanonico()
    {
        // Registro com todos os valores em zero — contribuinte sem prestações no período.
        const string sped =
            "|B470|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
