using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoA;

public sealed class RegistroA120Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroA120).Assembly);

    [Fact]
    public void Atributo_DeclaraCodigoA120_Nivel4_BlocoA()
    {
        var atributo = typeof(RegistroA120).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("A120");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("A");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroA120ComOitoCamposNaOrdem()
    {
        _catalogo.TentarObter("A120".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("A120");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "VlTotServ", "VlBcPis", "VlPisImp", "DtPagPis",
            "VlBcCofins", "VlCofinsImp", "DtPagCofins", "LocExeServ",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("A120".AsSpan(), out var meta);
        var registro = (RegistroA120)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "10000.00".AsSpan()); // VlTotServ
        meta.Campos[1].Definidor(registro, "10000.00".AsSpan()); // VlBcPis
        meta.Campos[2].Definidor(registro, "1500.00".AsSpan());  // VlPisImp
        meta.Campos[3].Definidor(registro, "15012025".AsSpan()); // DtPagPis
        meta.Campos[4].Definidor(registro, "10000.00".AsSpan()); // VlBcCofins
        meta.Campos[5].Definidor(registro, "2300.00".AsSpan());  // VlCofinsImp
        meta.Campos[6].Definidor(registro, "15012025".AsSpan()); // DtPagCofins
        meta.Campos[7].Definidor(registro, "0".AsSpan());        // LocExeServ

        registro.VlTotServ.Should().Be(10000.00m);
        registro.VlBcPis.Should().Be(10000.00m);
        registro.VlPisImp.Should().Be(1500.00m);
        registro.DtPagPis.Should().Be(new DateOnly(2025, 1, 15));
        registro.VlBcCofins.Should().Be(10000.00m);
        registro.VlCofinsImp.Should().Be(2300.00m);
        registro.DtPagCofins.Should().Be(new DateOnly(2025, 1, 15));
        registro.LocExeServ.Should().Be(IndicadorLocalExecucaoServico.Pais);
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("A120".AsSpan(), out var meta);
        var registro = (RegistroA120)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // VlPisImp
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // DtPagPis
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // VlCofinsImp
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // DtPagCofins

        registro.VlPisImp.Should().BeNull();
        registro.DtPagPis.Should().BeNull();
        registro.VlCofinsImp.Should().BeNull();
        registro.DtPagCofins.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|A120|10000,00|10000,00|1500,00|15012025|10000,00|2300,00|15012025|0|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|A120|10000,00|10000,00|||10000,00|||0|\r\n";

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
