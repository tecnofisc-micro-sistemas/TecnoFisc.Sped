using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco1;

public sealed class Registro1011Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro1011).Assembly);

    [Fact]
    public void Atributo_Declara1011_Nivel3_Bloco1()
    {
        var atributo = typeof(Registro1011).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("1011");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("1");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro1011Com25CamposNaOrdem()
    {
        _catalogo.TentarObter("1011".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("1011");
        meta.Campos.Select(c => c.Ordem).Should().Equal(
        [
            2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26,
        ]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "RegRef", "ChaveDoc", "CodPart", "CodItem", "DtOper", "VlOper",
            "CstPis", "VlBcPis", "AliqPis", "VlPis",
            "CstCofins", "VlBcCofins", "AliqCofins", "VlCofins",
            "CstPisSusp", "VlBcPisSusp", "AliqPisSusp", "VlPisSusp",
            "CstCofinsSusp", "VlBcCofinsSusp", "AliqCofinsSusp", "VlCofinsSusp",
            "CodCta", "CodCcus", "DescDocOper",
        ]);
        meta.Campos[4].Obrigatorio.Should().BeTrue();   // DtOper
        meta.Campos[5].Obrigatorio.Should().BeTrue();   // VlOper
        meta.Campos[6].Tamanho.Should().Be(2);
        meta.Campos[6].Obrigatorio.Should().BeTrue();   // CstPis
        meta.Campos[8].Tamanho.Should().Be(8);          // AliqPis tamanho fixo
        meta.Campos[10].Obrigatorio.Should().BeTrue();  // CstCofins
        meta.Campos[14].Obrigatorio.Should().BeTrue();  // CstPisSusp
        meta.Campos[18].Obrigatorio.Should().BeTrue();  // CstCofinsSusp
        meta.Campos[0].Obrigatorio.Should().BeFalse();  // RegRef
        meta.Campos[7].Obrigatorio.Should().BeFalse();  // VlBcPis
        meta.Campos[22].Obrigatorio.Should().BeFalse(); // CodCta
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("1011".AsSpan(), out var meta);
        var registro = (Registro1011)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "C170".AsSpan());                                       // RegRef
        meta.Campos[1].Definidor(registro, "12345678901234567890".AsSpan());                       // ChaveDoc
        meta.Campos[2].Definidor(registro, "PART01".AsSpan());                                     // CodPart
        meta.Campos[3].Definidor(registro, "ITEM01".AsSpan());                                     // CodItem
        meta.Campos[4].Definidor(registro, "01012020".AsSpan());                                   // DtOper
        meta.Campos[5].Definidor(registro, "100,00".AsSpan());                                     // VlOper
        meta.Campos[6].Definidor(registro, "1".AsSpan());                                          // CstPis
        meta.Campos[7].Definidor(registro, "100,0000".AsSpan());                                   // VlBcPis
        meta.Campos[8].Definidor(registro, "1,6500".AsSpan());                                     // AliqPis
        meta.Campos[9].Definidor(registro, "1,65".AsSpan());                                       // VlPis
        meta.Campos[10].Definidor(registro, "1".AsSpan());                                         // CstCofins
        meta.Campos[11].Definidor(registro, "100,0000".AsSpan());                                  // VlBcCofins
        meta.Campos[12].Definidor(registro, "7,6000".AsSpan());                                    // AliqCofins
        meta.Campos[13].Definidor(registro, "7,60".AsSpan());                                      // VlCofins
        meta.Campos[14].Definidor(registro, "6".AsSpan());                                         // CstPisSusp
        meta.Campos[15].Definidor(registro, "100,0000".AsSpan());                                  // VlBcPisSusp
        meta.Campos[16].Definidor(registro, "0,0000".AsSpan());                                    // AliqPisSusp
        meta.Campos[17].Definidor(registro, "0,00".AsSpan());                                      // VlPisSusp
        meta.Campos[18].Definidor(registro, "6".AsSpan());                                         // CstCofinsSusp
        meta.Campos[19].Definidor(registro, "100,0000".AsSpan());                                  // VlBcCofinsSusp
        meta.Campos[20].Definidor(registro, "0,0000".AsSpan());                                    // AliqCofinsSusp
        meta.Campos[21].Definidor(registro, "0,00".AsSpan());                                      // VlCofinsSusp
        meta.Campos[22].Definidor(registro, "3.1.01.001".AsSpan());                                // CodCta
        meta.Campos[23].Definidor(registro, "CCUSTO-1".AsSpan());                                  // CodCcus
        meta.Campos[24].Definidor(registro, "Tributação alíquota zero cfe. decisão judicial".AsSpan()); // DescDocOper

        registro.RegRef.Should().Be("C170");
        registro.ChaveDoc.Should().Be("12345678901234567890");
        registro.CodPart.Should().Be("PART01");
        registro.CodItem.Should().Be("ITEM01");
        registro.DtOper.Should().Be(new DateOnly(2020, 1, 1));
        registro.VlOper.Should().Be(100m);
        registro.CstPis.Should().Be(1);
        registro.VlBcPis.Should().Be(100m);
        registro.AliqPis.Should().Be(1.65m);
        registro.VlPis.Should().Be(1.65m);
        registro.CstCofins.Should().Be(1);
        registro.VlBcCofins.Should().Be(100m);
        registro.AliqCofins.Should().Be(7.60m);
        registro.VlCofins.Should().Be(7.60m);
        registro.CstPisSusp.Should().Be(6);
        registro.VlBcPisSusp.Should().Be(100m);
        registro.AliqPisSusp.Should().Be(0m);
        registro.VlPisSusp.Should().Be(0m);
        registro.CstCofinsSusp.Should().Be(6);
        registro.VlBcCofinsSusp.Should().Be(100m);
        registro.AliqCofinsSusp.Should().Be(0m);
        registro.VlCofinsSusp.Should().Be(0m);
        registro.CodCta.Should().Be("3.1.01.001");
        registro.CodCcus.Should().Be("CCUSTO-1");
        registro.DescDocOper.Should().Be("Tributação alíquota zero cfe. decisão judicial");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("1011".AsSpan(), out var meta);
        var registro = (Registro1011)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);  // RegRef
        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);  // ChaveDoc
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodPart
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodItem
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlBcPis
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqPis
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlPis
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcCofins
        meta.Campos[15].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcPisSusp
        meta.Campos[19].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcCofinsSusp
        meta.Campos[22].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta
        meta.Campos[23].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCcus
        meta.Campos[24].Definidor(registro, ReadOnlySpan<char>.Empty); // DescDocOper

        registro.RegRef.Should().BeNull();
        registro.ChaveDoc.Should().BeNull();
        registro.CodPart.Should().BeNull();
        registro.CodItem.Should().BeNull();
        registro.VlBcPis.Should().BeNull();
        registro.AliqPis.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlBcCofins.Should().BeNull();
        registro.VlBcPisSusp.Should().BeNull();
        registro.VlBcCofinsSusp.Should().BeNull();
        registro.CodCta.Should().BeNull();
        registro.CodCcus.Should().BeNull();
        registro.DescDocOper.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|1011|C170|12345678901234567890|PART01|ITEM01|01012020|100,00|1|100,0000|1,6500|1,65|1|100,0000|7,6000|7,60|6|100,0000|0,0000|0,00|6|100,0000|0,0000|0,00|3.1.01.001|CCUSTO-1|Tributação alíquota zero cfe. decisão judicial|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|1011|||||01012020|100,00|1||||1||||6||||6|||||||\r\n";

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
