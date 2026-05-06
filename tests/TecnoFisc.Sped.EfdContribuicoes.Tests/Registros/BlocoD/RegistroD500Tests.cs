using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoD;

public sealed class RegistroD500Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD500).Assembly);

    [Fact]
    public void Atributo_DeclaraD500_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD500).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D500");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD500Com21CamposNaOrdem()
    {
        _catalogo.TentarObter("D500".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D500");
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "IndOper", "IndEmit", "CodPart", "CodMod", "CodSit",
            "Ser", "Sub", "NumDoc", "DtDoc", "DtAP",
            "VlDoc", "VlDesc", "VlServ", "VlServNt", "VlTerc",
            "VlDa", "VlBcIcms", "VlIcms", "CodInf", "VlPis", "VlCofins"
        ]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D500".AsSpan(), out var meta);
        var registro = (RegistroD500)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());           // IndOper
        meta.Campos[1].Definidor(registro, "1".AsSpan());           // IndEmit
        meta.Campos[2].Definidor(registro, "TELE01".AsSpan());      // CodPart
        meta.Campos[3].Definidor(registro, "21".AsSpan());          // CodMod
        meta.Campos[4].Definidor(registro, "00".AsSpan());          // CodSit
        meta.Campos[5].Definidor(registro, "A".AsSpan());           // Ser
        meta.Campos[6].Definidor(registro, "1".AsSpan());           // Sub
        meta.Campos[7].Definidor(registro, "000012345".AsSpan());   // NumDoc
        meta.Campos[8].Definidor(registro, "01012022".AsSpan());    // DtDoc
        meta.Campos[9].Definidor(registro, "01012022".AsSpan());    // DtAP
        meta.Campos[10].Definidor(registro, "1000,00".AsSpan());    // VlDoc
        meta.Campos[11].Definidor(registro, "50,00".AsSpan());      // VlDesc
        meta.Campos[12].Definidor(registro, "950,00".AsSpan());     // VlServ
        meta.Campos[13].Definidor(registro, "10,00".AsSpan());      // VlServNt
        meta.Campos[14].Definidor(registro, "5,00".AsSpan());       // VlTerc
        meta.Campos[15].Definidor(registro, "2,00".AsSpan());       // VlDa
        meta.Campos[16].Definidor(registro, "100,00".AsSpan());     // VlBcIcms
        meta.Campos[17].Definidor(registro, "25,00".AsSpan());      // VlIcms
        meta.Campos[18].Definidor(registro, "INF001".AsSpan());     // CodInf
        meta.Campos[19].Definidor(registro, "1,65".AsSpan());       // VlPis
        meta.Campos[20].Definidor(registro, "7,60".AsSpan());       // VlCofins

        registro.IndOper.Should().Be(IndicadorOperacaoDocumento.Entrada);
        registro.IndEmit.Should().Be(IndicadorEmissaoDocumento.EmissaoPorTerceiros);
        registro.CodPart.Should().Be("TELE01");
        registro.CodMod.Should().Be("21");
        registro.CodSit.Should().Be(CodigoSituacaoDocumentoFiscal.DocumentoRegular);
        registro.Ser.Should().Be("A");
        registro.Sub.Should().Be("1");
        registro.NumDoc.Should().Be("000012345");
        registro.DtDoc.Should().Be(new DateOnly(2022, 1, 1));
        registro.DtAP.Should().Be(new DateOnly(2022, 1, 1));
        registro.VlDoc.Should().Be(1000.00m);
        registro.VlDesc.Should().Be(50.00m);
        registro.VlServ.Should().Be(950.00m);
        registro.VlServNt.Should().Be(10.00m);
        registro.VlTerc.Should().Be(5.00m);
        registro.VlDa.Should().Be(2.00m);
        registro.VlBcIcms.Should().Be(100.00m);
        registro.VlIcms.Should().Be(25.00m);
        registro.CodInf.Should().Be("INF001");
        registro.VlPis.Should().Be(1.65m);
        registro.VlCofins.Should().Be(7.60m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D500".AsSpan(), out var meta);
        var registro = (RegistroD500)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);   // Ser
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);   // Sub
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlDesc

        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
        registro.VlDesc.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|D500|0|1|TELE01|21|00|A|1|000012345|01012022|01012022|1000,00|50,00|950,00|10,00|5,00|2,00|100,00|25,00|INF001|1,65|7,60|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // Ser, Sub, VlDesc, VlServNt, VlTerc, VlDa, VlBcIcms, VlIcms, CodInf, VlPis, VlCofins omitidos.
        const string sped =
            "|D500|0|0|TELECOM002|22|00|||000099999|15062021|15062021|500,00||950,00|||||||||\r\n";

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
