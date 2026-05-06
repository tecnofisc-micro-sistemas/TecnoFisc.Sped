using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC600Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC600).Assembly);

    [Fact]
    public void Atributo_DeclaraC600_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC600).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C600");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC600ComVinteEUmCamposNaOrdem()
    {
        _catalogo.TentarObter("C600".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C600");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodMod", "CodMun", "Ser", "Sub", "CodCons",
            "QtdCons", "QtdCanc", "DtDoc", "VlDoc", "VlDesc",
            "Cons", "VlForn", "VlServNt", "VlTerc", "VlDa",
            "VlBcIcms", "VlIcms", "VlBcIcmsSt", "VlIcmsSt", "VlPis",
            "VlCofins",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // CodMod
        meta.Campos[1].Tamanho.Should().Be(7);
        meta.Campos[1].Obrigatorio.Should().BeFalse();  // CodMun
        meta.Campos[5].Obrigatorio.Should().BeTrue();   // QtdCons
        meta.Campos[6].Obrigatorio.Should().BeFalse();  // QtdCanc
        meta.Campos[7].Tamanho.Should().Be(8);
        meta.Campos[7].Obrigatorio.Should().BeTrue();   // DtDoc
        meta.Campos[8].Obrigatorio.Should().BeTrue();   // VlDoc
        meta.Campos[19].Obrigatorio.Should().BeTrue();  // VlPis
        meta.Campos[20].Obrigatorio.Should().BeTrue();  // VlCofins
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C600".AsSpan(), out var meta);
        var registro = (RegistroC600)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "06".AsSpan());          // CodMod
        meta.Campos[1].Definidor(registro, "3550308".AsSpan());     // CodMun
        meta.Campos[2].Definidor(registro, "A".AsSpan());           // Ser
        meta.Campos[3].Definidor(registro, "1".AsSpan());           // Sub
        meta.Campos[4].Definidor(registro, "3".AsSpan());            // CodCons
        meta.Campos[5].Definidor(registro, "150".AsSpan());         // QtdCons
        meta.Campos[6].Definidor(registro, "5".AsSpan());           // QtdCanc
        meta.Campos[7].Definidor(registro, "15012024".AsSpan());    // DtDoc
        meta.Campos[8].Definidor(registro, "75000,00".AsSpan());    // VlDoc
        meta.Campos[9].Definidor(registro, "1500,00".AsSpan());     // VlDesc
        meta.Campos[10].Definidor(registro, "12500".AsSpan());      // Cons
        meta.Campos[11].Definidor(registro, "65000,00".AsSpan());   // VlForn
        meta.Campos[12].Definidor(registro, "2000,00".AsSpan());    // VlServNt
        meta.Campos[13].Definidor(registro, "500,00".AsSpan());     // VlTerc
        meta.Campos[14].Definidor(registro, "300,00".AsSpan());     // VlDa
        meta.Campos[15].Definidor(registro, "10000,00".AsSpan());   // VlBcIcms
        meta.Campos[16].Definidor(registro, "1200,00".AsSpan());    // VlIcms
        meta.Campos[17].Definidor(registro, "5000,00".AsSpan());    // VlBcIcmsSt
        meta.Campos[18].Definidor(registro, "600,00".AsSpan());     // VlIcmsSt
        meta.Campos[19].Definidor(registro, "487,50".AsSpan());     // VlPis
        meta.Campos[20].Definidor(registro, "2250,00".AsSpan());    // VlCofins

        registro.CodMod.Should().Be("06");
        registro.CodMun.Should().Be(3550308);
        registro.Ser.Should().Be("A");
        registro.Sub.Should().Be(1);
        registro.CodCons.Should().Be(3);
        registro.QtdCons.Should().Be(150L);
        registro.QtdCanc.Should().Be(5L);
        registro.DtDoc.Should().Be(new DateOnly(2024, 1, 15));
        registro.VlDoc.Should().Be(75000.00m);
        registro.VlDesc.Should().Be(1500.00m);
        registro.Cons.Should().Be(12500L);
        registro.VlForn.Should().Be(65000.00m);
        registro.VlServNt.Should().Be(2000.00m);
        registro.VlTerc.Should().Be(500.00m);
        registro.VlDa.Should().Be(300.00m);
        registro.VlBcIcms.Should().Be(10000.00m);
        registro.VlIcms.Should().Be(1200.00m);
        registro.VlBcIcmsSt.Should().Be(5000.00m);
        registro.VlIcmsSt.Should().Be(600.00m);
        registro.VlPis.Should().Be(487.50m);
        registro.VlCofins.Should().Be(2250.00m);
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("C600".AsSpan(), out var meta);
        var registro = (RegistroC600)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);   // CodMun
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);   // Ser
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);   // Sub
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);   // CodCons
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);   // QtdCanc
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);   // VlDesc
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty);  // Cons
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlForn
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlServNt
        meta.Campos[13].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlTerc
        meta.Campos[14].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlDa
        meta.Campos[15].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlBcIcms
        meta.Campos[16].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlIcms
        meta.Campos[17].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlBcIcmsSt
        meta.Campos[18].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlIcmsSt

        registro.CodMun.Should().BeNull();
        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
        registro.CodCons.Should().BeNull();
        registro.QtdCanc.Should().BeNull();
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
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C600|06|3550308|A|1|3|150|5|15012024|75000,00|1500,00|12500|65000,00|2000,00|500,00|300,00|10000,00|1200,00|5000,00|600,00|487,50|2250,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposObrigatoriosApenas_PreservaTextoCanonico()
    {
        const string sped = "|C600|06|||||150||15012024|75000,00|||||||||||487,50|2250,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ModeloNf3e_PreservaTextoCanonico()
    {
        const string sped = "|C600|66|||||50||01022024|15000,00|||||||||||97,50|450,00|\r\n";

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
