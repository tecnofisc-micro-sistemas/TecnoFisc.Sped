using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoI;

public sealed class RegistroI200Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroI200).Assembly);

    [Fact]
    public void Atributo_DeclaraI200_Nivel4_BlocoI()
    {
        var atributo = typeof(RegistroI200).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("I200");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("I");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroI200Com5CamposNaOrdem()
    {
        _catalogo.TentarObter("I200".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("I200");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
        meta.Campos.Select(c => c.Nome).Should().Equal(["NumCampo", "CodDet", "DetValor", "CodCta", "InfoCompl"]);
        meta.Campos[0].Tamanho.Should().Be(2);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // NumCampo
        meta.Campos[1].Tamanho.Should().Be(5);
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // CodDet
        meta.Campos[3].Tamanho.Should().Be(255);        // CodCta
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("I200".AsSpan(), out var meta);
        var registro = (RegistroI200)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "02".AsSpan());              // NumCampo
        meta.Campos[1].Definidor(registro, "A0101".AsSpan());           // CodDet
        meta.Campos[2].Definidor(registro, "100000,00".AsSpan());       // DetValor
        meta.Campos[3].Definidor(registro, "1.01.01.001".AsSpan());     // CodCta
        meta.Campos[4].Definidor(registro, "Detalhe receita".AsSpan()); // InfoCompl

        registro.NumCampo.Should().Be("02");
        registro.CodDet.Should().Be("A0101");
        registro.DetValor.Should().Be(100000m);
        registro.CodCta.Should().Be("1.01.01.001");
        registro.InfoCompl.Should().Be("Detalhe receita");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("I200".AsSpan(), out var meta);
        var registro = (RegistroI200)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // DetValor
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // InfoCompl

        registro.DetValor.Should().BeNull();
        registro.CodCta.Should().BeNull();
        registro.InfoCompl.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|I200|02|A0101|100000,00|1.01.01.001|Detalhe receita|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCodCtaEInfoCompl_PreservaTextoCanonico()
    {
        const string sped = "|I200|04|B0201|25000,00|||\r\n";

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
