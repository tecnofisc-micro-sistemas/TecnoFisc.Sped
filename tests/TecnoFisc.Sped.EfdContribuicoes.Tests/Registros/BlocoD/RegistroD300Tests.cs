using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoD;

public sealed class RegistroD300Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD300).Assembly);

    [Fact]
    public void Atributo_DeclaraD300_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD300).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D300");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD300Com18CamposNaOrdem()
    {
        _catalogo.TentarObter("D300".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D300");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodMod", "Ser", "Sub", "NumDocIni", "NumDocFin", "Cfop", "DtRef",
            "VlDoc", "VlDesc", "CstPis", "VlBcPis", "AliqPis", "VlPis",
            "CstCofins", "VlBcCofins", "AliqCofins", "VlCofins", "CodCta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();    // CodMod
        meta.Campos[1].Tamanho.Should().Be(4);
        meta.Campos[1].Obrigatorio.Should().BeFalse();   // Ser
        meta.Campos[2].Tamanho.Should().Be(3);
        meta.Campos[2].Obrigatorio.Should().BeFalse();   // Sub
        meta.Campos[3].Tamanho.Should().Be(6);
        meta.Campos[3].Obrigatorio.Should().BeFalse();   // NumDocIni
        meta.Campos[4].Tamanho.Should().Be(6);
        meta.Campos[4].Obrigatorio.Should().BeFalse();   // NumDocFin
        meta.Campos[5].Tamanho.Should().Be(4);
        meta.Campos[5].Obrigatorio.Should().BeTrue();    // Cfop
        meta.Campos[6].Tamanho.Should().Be(8);
        meta.Campos[6].Obrigatorio.Should().BeTrue();    // DtRef
        meta.Campos[7].Obrigatorio.Should().BeTrue();    // VlDoc
        meta.Campos[8].Obrigatorio.Should().BeFalse();   // VlDesc
        meta.Campos[9].Tamanho.Should().Be(2);
        meta.Campos[9].Obrigatorio.Should().BeTrue();    // CstPis
        meta.Campos[13].Tamanho.Should().Be(2);
        meta.Campos[13].Obrigatorio.Should().BeTrue();   // CstCofins
        meta.Campos[17].Tamanho.Should().Be(255);
        meta.Campos[17].Obrigatorio.Should().BeFalse();  // CodCta
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D300".AsSpan(), out var meta);
        var registro = (RegistroD300)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "13".AsSpan());          // CodMod
        meta.Campos[1].Definidor(registro, "A".AsSpan());           // Ser
        meta.Campos[2].Definidor(registro, "1".AsSpan());           // Sub
        meta.Campos[3].Definidor(registro, "100".AsSpan());         // NumDocIni
        meta.Campos[4].Definidor(registro, "200".AsSpan());         // NumDocFin
        meta.Campos[5].Definidor(registro, "6931".AsSpan());        // Cfop
        meta.Campos[6].Definidor(registro, "01012021".AsSpan());    // DtRef
        meta.Campos[7].Definidor(registro, "5000,00".AsSpan());     // VlDoc
        meta.Campos[8].Definidor(registro, "50,00".AsSpan());       // VlDesc
        meta.Campos[9].Definidor(registro, "01".AsSpan());          // CstPis
        meta.Campos[10].Definidor(registro, "4000,00".AsSpan());    // VlBcPis
        meta.Campos[11].Definidor(registro, "1,6500".AsSpan());     // AliqPis
        meta.Campos[12].Definidor(registro, "66,00".AsSpan());      // VlPis
        meta.Campos[13].Definidor(registro, "01".AsSpan());         // CstCofins
        meta.Campos[14].Definidor(registro, "4000,00".AsSpan());    // VlBcCofins
        meta.Campos[15].Definidor(registro, "7,6000".AsSpan());     // AliqCofins
        meta.Campos[16].Definidor(registro, "304,00".AsSpan());     // VlCofins
        meta.Campos[17].Definidor(registro, "RECEITAS-TRANSP".AsSpan()); // CodCta

        registro.CodMod.Should().Be("13");
        registro.Ser.Should().Be("A");
        registro.Sub.Should().Be(1);
        registro.NumDocIni.Should().Be(100L);
        registro.NumDocFin.Should().Be(200L);
        registro.Cfop.Should().Be(Cfop.Create("6931"));
        registro.DtRef.Should().Be(new DateOnly(2021, 1, 1));
        registro.VlDoc.Should().Be(5000m);
        registro.VlDesc.Should().Be(50m);
        registro.CstPis.Should().Be(1);
        registro.VlBcPis.Should().Be(4000m);
        registro.AliqPis.Should().Be(1.6500m);
        registro.VlPis.Should().Be(66m);
        registro.CstCofins.Should().Be(1);
        registro.VlBcCofins.Should().Be(4000m);
        registro.AliqCofins.Should().Be(7.6000m);
        registro.VlCofins.Should().Be(304m);
        registro.CodCta.Should().Be("RECEITAS-TRANSP");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("D300".AsSpan(), out var meta);
        var registro = (RegistroD300)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty);  // Ser
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);  // Sub
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);  // NumDocIni
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty);  // NumDocFin
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlDesc
        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcPis
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqPis
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty); // VlPis
        meta.Campos[14].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcCofins
        meta.Campos[15].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqCofins
        meta.Campos[16].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCofins
        meta.Campos[17].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta

        registro.Ser.Should().BeNull();
        registro.Sub.Should().BeNull();
        registro.NumDocIni.Should().BeNull();
        registro.NumDocFin.Should().BeNull();
        registro.VlDesc.Should().BeNull();
        registro.VlBcPis.Should().BeNull();
        registro.AliqPis.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlBcCofins.Should().BeNull();
        registro.AliqCofins.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|D300|13|A|1|100|200|6931|01012021|5000,00|50,00|1|4000,00|1,6500|66,00|1|4000,00|7,6000|304,00|RECEITAS-TRANSP|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // Bilhete de passagem rodoviário sem série, subsérie, documentos ou desconto/crédito.
        const string sped = "|D300|13|||||6931|01012021|10000,00||7||||7|||||\r\n";

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
