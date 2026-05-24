using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoM;

public sealed class RegistroM400Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroM400).Assembly);

    [Fact]
    public void Atributo_DeclaraM400_Nivel2_BlocoM()
    {
        var atributo = typeof(RegistroM400).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("M400");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("M");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroM400Com4CamposNaOrdem()
    {
        _catalogo.TentarObter("M400".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("M400");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CstPis", "VlTotRec", "CodCta", "DescCompl",
        ]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // CstPis
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // VlTotRec
        meta.Campos[2].Tamanho.Should().Be(255);
        meta.Campos[2].Obrigatorio.Should().BeFalse();  // CodCta
        meta.Campos[3].Obrigatorio.Should().BeFalse();  // DescCompl
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("M400".AsSpan(), out var meta);
        var registro = (RegistroM400)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "7".AsSpan());                     // CstPis
        meta.Campos[1].Definidor(registro, "25000,00".AsSpan());              // VlTotRec
        meta.Campos[2].Definidor(registro, "4.1.1.001".AsSpan());             // CodCta
        meta.Campos[3].Definidor(registro, "Operações isentas por lei".AsSpan()); // DescCompl

        registro.CstPis.Should().Be(7);
        registro.VlTotRec.Should().Be(25000m);
        registro.CodCta.Should().Be("4.1.1.001");
        registro.DescCompl.Should().Be("Operações isentas por lei");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("M400".AsSpan(), out var meta);
        var registro = (RegistroM400)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // DescCompl

        registro.CodCta.Should().BeNull();
        registro.DescCompl.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|M400|7|25000,00|4.1.1.001|Operações isentas por lei|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|M400|6|15000,00|||\r\n";

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
