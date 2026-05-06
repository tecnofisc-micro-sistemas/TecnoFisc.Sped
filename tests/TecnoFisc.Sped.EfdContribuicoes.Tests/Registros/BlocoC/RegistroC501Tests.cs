using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoC;

public sealed class RegistroC501Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC501).Assembly);

    [Fact]
    public void Atributo_DeclaraC501_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC501).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C501");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC501ComSeteCamposNaOrdem()
    {
        _catalogo.TentarObter("C501".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C501");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CstPis", "VlItem", "NatBcCred", "VlBcPis", "AliqPis", "VlPis", "CodCta",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // CstPis
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // VlItem
        meta.Campos[2].Tamanho.Should().Be(2);
        meta.Campos[2].Obrigatorio.Should().BeFalse();  // NatBcCred
        meta.Campos[3].Obrigatorio.Should().BeTrue();   // VlBcPis
        meta.Campos[4].Tamanho.Should().Be(8);
        meta.Campos[4].Obrigatorio.Should().BeTrue();   // AliqPis
        meta.Campos[5].Obrigatorio.Should().BeTrue();   // VlPis
        meta.Campos[6].Tamanho.Should().Be(255);
        meta.Campos[6].Obrigatorio.Should().BeFalse();  // CodCta
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C501".AsSpan(), out var meta);
        var registro = (RegistroC501)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "50".AsSpan());           // CstPis
        meta.Campos[1].Definidor(registro, "1000,00".AsSpan());      // VlItem
        meta.Campos[2].Definidor(registro, "04".AsSpan());           // NatBcCred
        meta.Campos[3].Definidor(registro, "950,00".AsSpan());       // VlBcPis
        meta.Campos[4].Definidor(registro, "1,6500".AsSpan());       // AliqPis
        meta.Campos[5].Definidor(registro, "15,68".AsSpan());        // VlPis
        meta.Campos[6].Definidor(registro, "3.1.01.001".AsSpan());   // CodCta

        registro.CstPis.Should().Be(50);
        registro.VlItem.Should().Be(1000m);
        registro.NatBcCred.Should().Be(CodigoBaseCalculoCredito.EnergiaEletricaTermica);
        registro.VlBcPis.Should().Be(950m);
        registro.AliqPis.Should().Be(1.65m);
        registro.VlPis.Should().Be(15.68m);
        registro.CodCta.Should().Be("3.1.01.001");
    }

    [Fact]
    public void Definidor_CamposOpcionais_DevolveNulo()
    {
        _catalogo.TentarObter("C501".AsSpan(), out var meta);
        var registro = (RegistroC501)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // NatBcCred
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta

        registro.NatBcCred.Should().BeNull();
        registro.CodCta.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C501|50|1000,00|04|950,00|1,6500|15,68|3.1.01.001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemNatBcCredECodCta_PreservaTextoCanonico()
    {
        const string sped = "|C501|50|1000,00||950,00|1,6500|15,68||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CstVinculadoExclusivamenteExportacao_PreservaTextoCanonico()
    {
        const string sped = "|C501|52|500,00||500,00|1,6500|8,25||\r\n";

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
