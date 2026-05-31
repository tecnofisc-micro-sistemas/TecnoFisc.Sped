using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoF;

public sealed class RegistroF205Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroF205).Assembly);

    [Fact]
    public void Atributo_DeclaraF205_Nivel4_BlocoF()
    {
        var atributo = typeof(RegistroF205).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("F205");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("F");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroF205Com17CamposNaOrdem()
    {
        _catalogo.TentarObter("F205".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("F205");
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "VlCusIncAcumAnt", "VlCusIncPerEsc", "VlCusIncAcum",
            "VlExcBcCusIncAcum", "VlBcCusInc",
            "CstPis", "AliqPis", "VlCredPisAcum", "VlCredPisDescAnt", "VlCredPisDesc", "VlCredPisDescFut",
            "CstCofins", "AliqCofins", "VlCredCofinsAcum", "VlCredCofinsDescAnt", "VlCredCofinsDesc", "VlCredCofinsDescFut"
        ]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("F205".AsSpan(), out var meta);
        var registro = (RegistroF205)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1000000,00".AsSpan());  // VlCusIncAcumAnt
        meta.Campos[1].Definidor(registro, "50000,00".AsSpan());    // VlCusIncPerEsc
        meta.Campos[2].Definidor(registro, "1050000,00".AsSpan());  // VlCusIncAcum
        meta.Campos[3].Definidor(registro, "150000,00".AsSpan());   // VlExcBcCusIncAcum
        meta.Campos[4].Definidor(registro, "900000,00".AsSpan());   // VlBcCusInc
        meta.Campos[5].Definidor(registro, "50".AsSpan());          // CstPis
        meta.Campos[6].Definidor(registro, "1,6500".AsSpan());      // AliqPis
        meta.Campos[7].Definidor(registro, "14850,00".AsSpan());    // VlCredPisAcum
        meta.Campos[8].Definidor(registro, "5445,00".AsSpan());     // VlCredPisDescAnt
        meta.Campos[9].Definidor(registro, "3217,50".AsSpan());     // VlCredPisDesc
        meta.Campos[10].Definidor(registro, "6187,50".AsSpan());    // VlCredPisDescFut
        meta.Campos[11].Definidor(registro, "50".AsSpan());         // CstCofins
        meta.Campos[12].Definidor(registro, "7,6000".AsSpan());     // AliqCofins
        meta.Campos[13].Definidor(registro, "68400,00".AsSpan());   // VlCredCofinsAcum
        meta.Campos[14].Definidor(registro, "25080,00".AsSpan());   // VlCredCofinsDescAnt
        meta.Campos[15].Definidor(registro, "14820,00".AsSpan());   // VlCredCofinsDesc
        meta.Campos[16].Definidor(registro, "28500,00".AsSpan());   // VlCredCofinsDescFut

        registro.VlCusIncAcumAnt.Should().Be(1000000.00m);
        registro.VlCusIncPerEsc.Should().Be(50000.00m);
        registro.VlCusIncAcum.Should().Be(1050000.00m);
        registro.VlExcBcCusIncAcum.Should().Be(150000.00m);
        registro.VlBcCusInc.Should().Be(900000.00m);
        registro.CstPis.Should().Be("50");
        registro.AliqPis.Should().Be(1.6500m);
        registro.VlCredPisAcum.Should().Be(14850.00m);
        registro.VlCredPisDescAnt.Should().Be(5445.00m);
        registro.VlCredPisDesc.Should().Be(3217.50m);
        registro.VlCredPisDescFut.Should().Be(6187.50m);
        registro.CstCofins.Should().Be("50");
        registro.AliqCofins.Should().Be(7.6000m);
        registro.VlCredCofinsAcum.Should().Be(68400.00m);
        registro.VlCredCofinsDescAnt.Should().Be(25080.00m);
        registro.VlCredCofinsDesc.Should().Be(14820.00m);
        registro.VlCredCofinsDescFut.Should().Be(28500.00m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|F205|1000000,00|50000,00|1050000,00|150000,00|900000,00|50|1,6500|14850,00|5445,00|3217,50|6187,50|50|7,6000|68400,00|25080,00|14820,00|28500,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CreditoZerado_PreservaTextoCanonico()
    {
        const string sped =
            "|F205|0,00|500000,00|500000,00|100000,00|400000,00|50|1,6500|6600,00|0,00|6600,00|0,00|50|7,6000|30400,00|0,00|30400,00|0,00|\r\n";

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
