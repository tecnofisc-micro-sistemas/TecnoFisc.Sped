using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoF;

public sealed class RegistroF150Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroF150).Assembly);

    [Fact]
    public void Atributo_DeclaraF150_Nivel3_BlocoF()
    {
        var atributo = typeof(RegistroF150).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("F150");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("F");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroF150Com13CamposNaOrdem()
    {
        _catalogo.TentarObter("F150".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("F150");
        meta.Campos.Select(c => c.Ordem).Should().Equal(
            [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "NatBcCred", "VlTotEst", "EstImp", "VlBcEst", "VlBcMenEst",
            "CstPis", "AliqPis", "VlCredPis",
            "CstCofins", "AliqCofins", "VlCredCofins",
            "DescEst", "CodCta"
        ]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("F150".AsSpan(), out var meta);
        var registro = (RegistroF150)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "18".AsSpan());              // NatBcCred
        meta.Campos[1].Definidor(registro, "100000,00".AsSpan());       // VlTotEst
        meta.Campos[2].Definidor(registro, "10000,00".AsSpan());        // EstImp
        meta.Campos[3].Definidor(registro, "90000,00".AsSpan());        // VlBcEst
        meta.Campos[4].Definidor(registro, "7500,00".AsSpan());         // VlBcMenEst
        meta.Campos[5].Definidor(registro, "50".AsSpan());              // CstPis
        meta.Campos[6].Definidor(registro, "1,6500".AsSpan());          // AliqPis
        meta.Campos[7].Definidor(registro, "123,75".AsSpan());          // VlCredPis
        meta.Campos[8].Definidor(registro, "50".AsSpan());              // CstCofins
        meta.Campos[9].Definidor(registro, "7,6000".AsSpan());          // AliqCofins
        meta.Campos[10].Definidor(registro, "570,00".AsSpan());         // VlCredCofins
        meta.Campos[11].Definidor(registro, "Matérias-primas".AsSpan()); // DescEst
        meta.Campos[12].Definidor(registro, "101050001".AsSpan());      // CodCta

        registro.NatBcCred.Should().Be("18");
        registro.VlTotEst.Should().Be(100000.00m);
        registro.EstImp.Should().Be(10000.00m);
        registro.VlBcEst.Should().Be(90000.00m);
        registro.VlBcMenEst.Should().Be(7500.00m);
        registro.CstPis.Should().Be("50");
        registro.AliqPis.Should().Be(1.6500m);
        registro.VlCredPis.Should().Be(123.75m);
        registro.CstCofins.Should().Be("50");
        registro.AliqCofins.Should().Be(7.6000m);
        registro.VlCredCofins.Should().Be(570.00m);
        registro.DescEst.Should().Be("Matérias-primas");
        registro.CodCta.Should().Be("101050001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("F150".AsSpan(), out var meta);
        var registro = (RegistroF150)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);   // EstImp
        meta.Campos[11].Definidor(registro, ReadOnlySpan<char>.Empty);  // DescEst
        meta.Campos[12].Definidor(registro, ReadOnlySpan<char>.Empty);  // CodCta

        registro.EstImp.Should().BeNull();
        registro.DescEst.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|F150|18|100000,00|10000,00|90000,00|7500,00|50|1,6500|123,75|50|7,6000|570,00|Matérias-primas|101050001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        const string sped =
            "|F150|18|50000,00||50000,00|4166,67|50|1,6500|68,75|50|7,6000|316,67|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComDescricaoEstoqueCompleta_PreservaTextoCanonico()
    {
        const string sped =
            "|F150|18|200000,00|20000,00|180000,00|15000,00|50|1,6500|247,50|50|7,6000|1140,00|Produtos intermediários e embalagens|202010001|\r\n";

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
