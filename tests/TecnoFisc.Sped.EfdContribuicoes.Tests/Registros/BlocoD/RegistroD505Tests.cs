using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoD;

public sealed class RegistroD505Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD505).Assembly);

    [Fact]
    public void Atributo_DeclaraD505_Nivel4_BlocoD()
    {
        var atributo = typeof(RegistroD505).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D505");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD505ComSeteCamposNaOrdem()
    {
        _catalogo.TentarObter("D505".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D505");
        meta.Campos.Select(c => c.Nome).Should().Equal(
            ["CstCofins", "VlItem", "NatBcCred", "VlBcCofins", "AliqCofins", "VlCofins", "CodCta"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D505".AsSpan(), out var meta);
        var registro = (RegistroD505)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "50".AsSpan());         // CstCofins
        meta.Campos[1].Definidor(registro, "1000,00".AsSpan());    // VlItem
        meta.Campos[2].Definidor(registro, "03".AsSpan());         // NatBcCred
        meta.Campos[3].Definidor(registro, "1000,00".AsSpan());    // VlBcCofins
        meta.Campos[4].Definidor(registro, "7,6000".AsSpan());     // AliqCofins
        meta.Campos[5].Definidor(registro, "76,00".AsSpan());      // VlCofins
        meta.Campos[6].Definidor(registro, "1.1.01.001".AsSpan()); // CodCta

        registro.CstCofins.Should().Be(50);
        registro.VlItem.Should().Be(1000.00m);
        registro.NatBcCred.Should().Be(CodigoBaseCalculoCredito.AquisicaoServicosInsumo);
        registro.VlBcCofins.Should().Be(1000.00m);
        registro.AliqCofins.Should().Be(7.60m);
        registro.VlCofins.Should().Be(76.00m);
        registro.CodCta.Should().Be("1.1.01.001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D505".AsSpan(), out var meta);
        var registro = (RegistroD505)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty);   // NatBcCred
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty);   // VlBcCofins
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty);   // CodCta

        registro.NatBcCred.Should().BeNull();
        registro.VlBcCofins.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|D505|50|1000,00|03|1000,00|7,6000|76,00|1.1.01.001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCreditoOpcional_PreservaTextoCanonico()
    {
        const string sped = "|D505|70|500,00||||||\r\n";

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
