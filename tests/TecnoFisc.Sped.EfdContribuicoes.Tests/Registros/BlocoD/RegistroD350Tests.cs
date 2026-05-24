using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoD;

public sealed class RegistroD350Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD350).Assembly);

    [Fact]
    public void Atributo_DeclaraD350_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD350).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D350");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD350Com22CamposNaOrdem()
    {
        _catalogo.TentarObter("D350".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D350");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodMod", "EcfMod", "EcfFab", "DtDoc", "Cro", "Crz", "NumCooFin",
            "GtFin", "VlBrt", "CstPis", "VlBcPis", "AliqPis", "QuantBcPis", "AliqPisQuant", "VlPis",
            "CstCofins", "VlBcCofins", "AliqCofins", "QuantBcCofins", "AliqCofinsQuant", "VlCofins",
            "CodCta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();    // CodMod
        meta.Campos[1].Tamanho.Should().Be(20);
        meta.Campos[1].Obrigatorio.Should().BeTrue();    // EcfMod
        meta.Campos[2].Tamanho.Should().Be(21);
        meta.Campos[2].Obrigatorio.Should().BeTrue();    // EcfFab
        meta.Campos[3].Tamanho.Should().Be(8);
        meta.Campos[3].Obrigatorio.Should().BeTrue();    // DtDoc
        meta.Campos[4].Tamanho.Should().Be(3);
        meta.Campos[4].Obrigatorio.Should().BeTrue();    // Cro
        meta.Campos[5].Tamanho.Should().Be(6);
        meta.Campos[5].Obrigatorio.Should().BeTrue();    // Crz
        meta.Campos[6].Tamanho.Should().Be(6);
        meta.Campos[6].Obrigatorio.Should().BeTrue();    // NumCooFin
        meta.Campos[7].Obrigatorio.Should().BeTrue();    // GtFin
        meta.Campos[8].Obrigatorio.Should().BeTrue();    // VlBrt
        meta.Campos[9].Tamanho.Should().Be(2);
        meta.Campos[9].Obrigatorio.Should().BeTrue();    // CstPis
        meta.Campos[15].Tamanho.Should().Be(2);
        meta.Campos[15].Obrigatorio.Should().BeTrue();   // CstCofins
        meta.Campos[21].Tamanho.Should().Be(255);
        meta.Campos[21].Obrigatorio.Should().BeFalse();  // CodCta
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D350".AsSpan(), out var meta);
        var registro = (RegistroD350)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "2E".AsSpan());             // CodMod
        meta.Campos[1].Definidor(registro, "ECF-MODEL-ABC".AsSpan());  // EcfMod
        meta.Campos[2].Definidor(registro, "FAB123456789".AsSpan());   // EcfFab
        meta.Campos[3].Definidor(registro, "15032021".AsSpan());       // DtDoc
        meta.Campos[4].Definidor(registro, "5".AsSpan());              // Cro
        meta.Campos[5].Definidor(registro, "123".AsSpan());            // Crz
        meta.Campos[6].Definidor(registro, "456".AsSpan());            // NumCooFin
        meta.Campos[7].Definidor(registro, "50000,00".AsSpan());       // GtFin
        meta.Campos[8].Definidor(registro, "30000,00".AsSpan());       // VlBrt
        meta.Campos[9].Definidor(registro, "01".AsSpan());             // CstPis
        meta.Campos[10].Definidor(registro, "25000,00".AsSpan());      // VlBcPis
        meta.Campos[11].Definidor(registro, "1,6500".AsSpan());        // AliqPis
        meta.Campos[12].Definidor(registro, "100,000".AsSpan());       // QuantBcPis
        meta.Campos[13].Definidor(registro, "0,5000".AsSpan());        // AliqPisQuant
        meta.Campos[14].Definidor(registro, "412,50".AsSpan());        // VlPis
        meta.Campos[15].Definidor(registro, "01".AsSpan());            // CstCofins
        meta.Campos[16].Definidor(registro, "25000,00".AsSpan());      // VlBcCofins
        meta.Campos[17].Definidor(registro, "7,6000".AsSpan());        // AliqCofins
        meta.Campos[18].Definidor(registro, "100,000".AsSpan());       // QuantBcCofins
        meta.Campos[19].Definidor(registro, "2,3000".AsSpan());        // AliqCofinsQuant
        meta.Campos[20].Definidor(registro, "1900,00".AsSpan());       // VlCofins
        meta.Campos[21].Definidor(registro, "RECEITAS-ECF".AsSpan());  // CodCta

        registro.CodMod.Should().Be("2E");
        registro.EcfMod.Should().Be("ECF-MODEL-ABC");
        registro.EcfFab.Should().Be("FAB123456789");
        registro.DtDoc.Should().Be(new DateOnly(2021, 3, 15));
        registro.Cro.Should().Be(5);
        registro.Crz.Should().Be(123);
        registro.NumCooFin.Should().Be(456);
        registro.GtFin.Should().Be(50000m);
        registro.VlBrt.Should().Be(30000m);
        registro.CstPis.Should().Be(1);
        registro.VlBcPis.Should().Be(25000m);
        registro.AliqPis.Should().Be(1.6500m);
        registro.QuantBcPis.Should().Be(100m);
        registro.AliqPisQuant.Should().Be(0.5000m);
        registro.VlPis.Should().Be(412.50m);
        registro.CstCofins.Should().Be(1);
        registro.VlBcCofins.Should().Be(25000m);
        registro.AliqCofins.Should().Be(7.6000m);
        registro.QuantBcCofins.Should().Be(100m);
        registro.AliqCofinsQuant.Should().Be(2.3000m);
        registro.VlCofins.Should().Be(1900m);
        registro.CodCta.Should().Be("RECEITAS-ECF");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("D350".AsSpan(), out var meta);
        var registro = (RegistroD350)meta!.Fabrica();

        meta.Campos[10].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcPis
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqPis
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty); // QuantBcPis
        meta.Campos[13].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqPisQuant
        meta.Campos[14].Definidor(registro, ReadOnlySpan<char>.Empty); // VlPis
        meta.Campos[16].Definidor(registro, ReadOnlySpan<char>.Empty); // VlBcCofins
        meta.Campos[17].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqCofins
        meta.Campos[18].Definidor(registro, ReadOnlySpan<char>.Empty); // QuantBcCofins
        meta.Campos[19].Definidor(registro, ReadOnlySpan<char>.Empty); // AliqCofinsQuant
        meta.Campos[20].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCofins
        meta.Campos[21].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta

        registro.VlBcPis.Should().BeNull();
        registro.AliqPis.Should().BeNull();
        registro.QuantBcPis.Should().BeNull();
        registro.AliqPisQuant.Should().BeNull();
        registro.VlPis.Should().BeNull();
        registro.VlBcCofins.Should().BeNull();
        registro.AliqCofins.Should().BeNull();
        registro.QuantBcCofins.Should().BeNull();
        registro.AliqCofinsQuant.Should().BeNull();
        registro.VlCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|D350|2E|ECF-MODEL-ABC|FAB123456789|15032021|5|123|456|50000,00|30000,00|1|25000,00|1,6500|100,000|0,5000|412,50|1|25000,00|7,6000|100,000|2,3000|1900,00|RECEITAS-ECF|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // ECF de passagem sem bases de cálculo, alíquotas ou conta contábil.
        const string sped = "|D350|13|MODELO-ECF|FAB-SERIE-001|01012021|1|50|300|12000,00|10000,00|7||||||7|||||||\r\n";

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
