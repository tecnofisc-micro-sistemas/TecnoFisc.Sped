using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC505Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC505).Assembly);

    [Fact]
    public void Atributo_DeclaraC505_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC505).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C505");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC505ComSeteCamposNaOrdem()
    {
        _catalogo.TentarObter("C505".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C505");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CstCofins", "VlItem", "NatBcCred", "VlBcCofins", "AliqCofins", "VlCofins", "CodCta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // CstCofins
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // VlItem
        meta.Campos[2].Tamanho.Should().Be(2);
        meta.Campos[2].Obrigatorio.Should().BeFalse();  // NatBcCred
        meta.Campos[3].Obrigatorio.Should().BeTrue();   // VlBcCofins
        meta.Campos[4].Tamanho.Should().Be(8);
        meta.Campos[4].Obrigatorio.Should().BeTrue();   // AliqCofins
        meta.Campos[5].Obrigatorio.Should().BeTrue();   // VlCofins
        meta.Campos[6].Tamanho.Should().Be(255);
        meta.Campos[6].Obrigatorio.Should().BeFalse();  // CodCta
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C505".AsSpan(), out var meta);
        var registro = (RegistroC505)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "50".AsSpan());           // CstCofins
        meta.Campos[1].Definidor(registro, "1000,00".AsSpan());      // VlItem
        meta.Campos[2].Definidor(registro, "04".AsSpan());           // NatBcCred
        meta.Campos[3].Definidor(registro, "950,00".AsSpan());       // VlBcCofins
        meta.Campos[4].Definidor(registro, "7,6000".AsSpan());       // AliqCofins
        meta.Campos[5].Definidor(registro, "72,20".AsSpan());        // VlCofins
        meta.Campos[6].Definidor(registro, "3.1.01.002".AsSpan());   // CodCta

        registro.CstCofins.Should().Be(50);
        registro.VlItem.Should().Be(1000m);
        registro.NatBcCred.Should().Be(CodigoBaseCalculoCredito.EnergiaEletricaTermica);
        registro.VlBcCofins.Should().Be(950m);
        registro.AliqCofins.Should().Be(7.60m);
        registro.VlCofins.Should().Be(72.20m);
        registro.CodCta.Should().Be("3.1.01.002");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("C505".AsSpan(), out var meta);
        var registro = (RegistroC505)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // NatBcCred
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta

        registro.NatBcCred.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C505|50|1000,00|04|950,00|7,6000|72,20|3.1.01.002|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemNatBcCredECodCta_PreservaTextoCanonico()
    {
        const string sped = "|C505|50|1000,00||950,00|7,6000|72,20||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CstCreditoPresumido_PreservaTextoCanonico()
    {
        const string sped = "|C505|60|800,00|02|800,00|7,6000|60,80||\r\n";

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
