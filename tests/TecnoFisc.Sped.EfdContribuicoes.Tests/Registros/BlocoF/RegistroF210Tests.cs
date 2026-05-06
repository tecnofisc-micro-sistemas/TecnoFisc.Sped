using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoF;

public sealed class RegistroF210Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroF210).Assembly);

    [Fact]
    public void Atributo_DeclaraF210_Nivel4_BlocoF()
    {
        var atributo = typeof(RegistroF210).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("F210");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("F");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroF210Com10CamposNaOrdem()
    {
        _catalogo.TentarObter("F210".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("F210");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9, 10, 11]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "VlCusOrc", "VlExc", "VlCusOrcAju", "VlBcCred",
            "CstPis", "AliqPis", "VlCredPisUtil",
            "CstCofins", "AliqCofins", "VlCredCofinsUtil"
        ]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("F210".AsSpan(), out var meta);
        var registro = (RegistroF210)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "2000000,00".AsSpan());  // VlCusOrc
        meta.Campos[1].Definidor(registro, "300000,00".AsSpan());   // VlExc
        meta.Campos[2].Definidor(registro, "1700000,00".AsSpan());  // VlCusOrcAju
        meta.Campos[3].Definidor(registro, "850000,00".AsSpan());   // VlBcCred
        meta.Campos[4].Definidor(registro, "50".AsSpan());          // CstPis
        meta.Campos[5].Definidor(registro, "1,6500".AsSpan());      // AliqPis
        meta.Campos[6].Definidor(registro, "14025,00".AsSpan());    // VlCredPisUtil
        meta.Campos[7].Definidor(registro, "50".AsSpan());          // CstCofins
        meta.Campos[8].Definidor(registro, "7,6000".AsSpan());      // AliqCofins
        meta.Campos[9].Definidor(registro, "64600,00".AsSpan());    // VlCredCofinsUtil

        registro.VlCusOrc.Should().Be(2000000.00m);
        registro.VlExc.Should().Be(300000.00m);
        registro.VlCusOrcAju.Should().Be(1700000.00m);
        registro.VlBcCred.Should().Be(850000.00m);
        registro.CstPis.Should().Be("50");
        registro.AliqPis.Should().Be(1.6500m);
        registro.VlCredPisUtil.Should().Be(14025.00m);
        registro.CstCofins.Should().Be("50");
        registro.AliqCofins.Should().Be(7.6000m);
        registro.VlCredCofinsUtil.Should().Be(64600.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("F210".AsSpan(), out var meta);
        var registro = (RegistroF210)meta!.Fabrica();

        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqPis
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlCredPisUtil
        meta.Campos[8].Definidor(registro, ReadOnlySpan<char>.Empty);  // AliqCofins
        meta.Campos[9].Definidor(registro, ReadOnlySpan<char>.Empty);  // VlCredCofinsUtil

        registro.AliqPis.Should().BeNull();
        registro.VlCredPisUtil.Should().BeNull();
        registro.AliqCofins.Should().BeNull();
        registro.VlCredCofinsUtil.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|F210|2000000,00|300000,00|1700000,00|850000,00|50|1,6500|14025,00|50|7,6000|64600,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        const string sped =
            "|F210|1500000,00|200000,00|1300000,00|650000,00|50|||50|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}
