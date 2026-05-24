using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoP;

public sealed class RegistroP200Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroP200).Assembly);

    [Fact]
    public void Atributo_DeclaraP200_Nivel2_BlocoP()
    {
        var atributo = typeof(RegistroP200).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("P200");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("P");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroP200Com6CamposNaOrdem()
    {
        _catalogo.TentarObter("P200".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("P200");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "PerRef", "VlTotContApu", "VlTotAjReduc", "VlTotAjAcres", "VlTotContDev", "CodRec",
        ]);
        meta.Campos[0].Tamanho.Should().Be(6);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // PerRef
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // VlTotContApu
        meta.Campos[2].Obrigatorio.Should().BeFalse();  // VlTotAjReduc
        meta.Campos[3].Obrigatorio.Should().BeFalse();  // VlTotAjAcres
        meta.Campos[4].Obrigatorio.Should().BeTrue();   // VlTotContDev
        meta.Campos[5].Tamanho.Should().Be(6);
        meta.Campos[5].Obrigatorio.Should().BeTrue();   // CodRec
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("P200".AsSpan(), out var meta);
        var registro = (RegistroP200)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "012023".AsSpan());   // PerRef
        meta.Campos[1].Definidor(registro, "22500,00".AsSpan()); // VlTotContApu
        meta.Campos[2].Definidor(registro, "1000,00".AsSpan());  // VlTotAjReduc
        meta.Campos[3].Definidor(registro, "500,00".AsSpan());   // VlTotAjAcres
        meta.Campos[4].Definidor(registro, "22000,00".AsSpan()); // VlTotContDev
        meta.Campos[5].Definidor(registro, "298501".AsSpan());   // CodRec

        registro.PerRef.Should().Be("012023");
        registro.VlTotContApu.Should().Be(22500m);
        registro.VlTotAjReduc.Should().Be(1000m);
        registro.VlTotAjAcres.Should().Be(500m);
        registro.VlTotContDev.Should().Be(22000m);
        registro.CodRec.Should().Be("298501");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("P200".AsSpan(), out var meta);
        var registro = (RegistroP200)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // VlTotAjReduc
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // VlTotAjAcres

        registro.VlTotAjReduc.Should().BeNull();
        registro.VlTotAjAcres.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|P200|012023|22500,00|1000,00|500,00|22000,00|298501|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemAjustes_PreservaTextoCanonico()
    {
        const string sped = "|P200|012023|22500,00|||22500,00|299101|\r\n";

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
