using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.098 — exercita a forma do <see cref="RegistroC600"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (pp. 143-144): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC600Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC600).Assembly);

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
    public void Atributo_DeclaraC600_Nivel2_BlocoC()
    {
        var atributo = typeof(RegistroC600).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C600");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC600Com21CamposNaOrdem()
    {
        _catalogo.TentarObter("C600".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C600");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodMod", "CodMun", "Ser", "Sub", "CodCons", "QtdCons", "QtdCanc",
            "DtDoc", "VlDoc", "VlDesc", "Cons", "VlForn", "VlServNt", "VlTerc",
            "VlDa", "VlBcIcms", "VlIcms", "VlBcIcmsSt", "VlIcmsSt", "VlPis", "VlCofins",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C600".AsSpan(), out var meta);
        var registro = (RegistroC600)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "06".AsSpan());         // CodMod
        meta.Campos[1].Definidor(registro, "3550308".AsSpan());    // CodMun
        meta.Campos[2].Definidor(registro, "A".AsSpan());          // Ser
        meta.Campos[3].Definidor(registro, "1".AsSpan());          // Sub
        meta.Campos[4].Definidor(registro, "06".AsSpan());         // CodCons
        meta.Campos[5].Definidor(registro, "100".AsSpan());        // QtdCons
        meta.Campos[6].Definidor(registro, "2".AsSpan());          // QtdCanc
        meta.Campos[7].Definidor(registro, "01012023".AsSpan());   // DtDoc
        meta.Campos[8].Definidor(registro, "5000,00".AsSpan());    // VlDoc
        meta.Campos[9].Definidor(registro, "100,00".AsSpan());     // VlDesc
        meta.Campos[10].Definidor(registro, "50000".AsSpan());     // Cons
        meta.Campos[11].Definidor(registro, "4200,00".AsSpan());   // VlForn
        meta.Campos[12].Definidor(registro, "300,00".AsSpan());    // VlServNt
        meta.Campos[13].Definidor(registro, "50,00".AsSpan());     // VlTerc
        meta.Campos[14].Definidor(registro, "25,00".AsSpan());     // VlDa
        meta.Campos[15].Definidor(registro, "4000,00".AsSpan());   // VlBcIcms
        meta.Campos[16].Definidor(registro, "480,00".AsSpan());    // VlIcms
        meta.Campos[17].Definidor(registro, "500,00".AsSpan());    // VlBcIcmsSt
        meta.Campos[18].Definidor(registro, "60,00".AsSpan());     // VlIcmsSt
        meta.Campos[19].Definidor(registro, "10,00".AsSpan());     // VlPis
        meta.Campos[20].Definidor(registro, "45,00".AsSpan());     // VlCofins

        registro.CodMod.Should().Be("06");
        registro.CodMun.Should().Be(3550308);
        registro.Ser.Should().Be("A");
        registro.Sub.Should().Be(1);
        registro.CodCons.Should().Be("06");
        registro.QtdCons.Should().Be(100);
        registro.QtdCanc.Should().Be(2);
        registro.DtDoc.Should().Be(new DateOnly(2023, 1, 1));
        registro.VlDoc.Should().Be(5000m);
        registro.VlDesc.Should().Be(100m);
        registro.Cons.Should().Be(50000L);
        registro.VlForn.Should().Be(4200m);
        registro.VlServNt.Should().Be(300m);
        registro.VlTerc.Should().Be(50m);
        registro.VlDa.Should().Be(25m);
        registro.VlBcIcms.Should().Be(4000m);
        registro.VlIcms.Should().Be(480m);
        registro.VlBcIcmsSt.Should().Be(500m);
        registro.VlIcmsSt.Should().Be(60m);
        registro.VlPis.Should().Be(10m);
        registro.VlCofins.Should().Be(45m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C600".AsSpan(), out var meta);
        var registro = (RegistroC600)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.CodMod.Should().BeNull();
        registro.CodMun.Should().BeNull();
        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
        registro.CodCons.Should().BeNull();
        registro.QtdCons.Should().BeNull();
        registro.QtdCanc.Should().BeNull();
        registro.DtDoc.Should().BeNull();
        registro.VlDoc.Should().BeNull();
        registro.VlDesc.Should().BeNull();
        registro.Cons.Should().BeNull();
        registro.VlForn.Should().BeNull();
        registro.VlServNt.Should().BeNull();
        registro.VlTerc.Should().BeNull();
        registro.VlDa.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
        registro.VlBcIcmsSt.Should().BeNull();
        registro.VlIcmsSt.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlCofins.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C600|06|3550308|A|1|06|100|2|01012023|5000,00|100,00|50000|4200,00|300,00|50,00|25,00|4000,00|480,00|500,00|60,00|10,00|45,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SomenteObrigatorios_PreservaTextoCanonico()
    {
        // COD_MOD, COD_MUN, COD_CONS, QTD_CONS, DT_DOC, VL_DOC obrigatórios; demais OC vazios.
        const string sped =
            "|C600|06|3550308|||06|50||01012023|3000,00|||||||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ModeloGas_PreservaTextoCanonico()
    {
        // Gás canalizado (COD_MOD=28), COD_CONS=01 (Comercial), sem consumo kWh.
        const string sped =
            "|C600|28|5300108|||01|30||15032023|1200,00|||||||||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
