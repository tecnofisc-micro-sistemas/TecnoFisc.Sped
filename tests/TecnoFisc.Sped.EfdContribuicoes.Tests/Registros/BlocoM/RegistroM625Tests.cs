using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoM;

public sealed class RegistroM625Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroM625).Assembly);

    [Fact]
    public void Atributo_DeclaraM625_Nivel5_BlocoM()
    {
        var atributo = typeof(RegistroM625).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("M625");
        atributo.Nivel.Should().Be(5);
        atributo.Bloco.Should().Be("M");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroM625Com8CamposNaOrdem()
    {
        _catalogo.TentarObter("M625".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("M625");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "DetValorAj", "CstCofins", "DetBcCred", "DetAliq", "DtOperAj", "DescAj", "CodCta", "InfoCompl",
        ]);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // DetValorAj
        meta.Campos[1].Obrigatorio.Should().BeFalse();  // CstCofins
        meta.Campos[4].Tamanho.Should().Be(8);
        meta.Campos[4].Obrigatorio.Should().BeTrue();   // DtOperAj
        meta.Campos[5].Obrigatorio.Should().BeFalse();  // DescAj
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("M625".AsSpan(), out var meta);
        var registro = (RegistroM625)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "500,00".AsSpan());          // DetValorAj
        meta.Campos[1].Definidor(registro, "01".AsSpan());              // CstCofins
        meta.Campos[2].Definidor(registro, "1000,000".AsSpan());        // DetBcCred
        meta.Campos[3].Definidor(registro, "7,6000".AsSpan());          // DetAliq
        meta.Campos[4].Definidor(registro, "20032024".AsSpan());        // DtOperAj
        meta.Campos[5].Definidor(registro, "Devolucao venda".AsSpan()); // DescAj
        meta.Campos[6].Definidor(registro, "3.3.01.001".AsSpan());      // CodCta
        meta.Campos[7].Definidor(registro, "Complemento".AsSpan());     // InfoCompl

        registro.DetValorAj.Should().Be(500m);
        registro.CstCofins.Should().Be(1);
        registro.DetBcCred.Should().Be(1000m);
        registro.DetAliq.Should().Be(7.6m);
        registro.DtOperAj.Should().Be(new DateOnly(2024, 3, 20));
        registro.DescAj.Should().Be("Devolucao venda");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("M625".AsSpan(), out var meta);
        var registro = (RegistroM625)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty); // CstCofins
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // DetBcCred
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // DetAliq
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // DescAj
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty); // InfoCompl

        registro.CstCofins.Should().BeNull();
        registro.DetBcCred.Should().BeNull();
        registro.DetAliq.Should().BeNull();
        registro.DescAj.Should().BeNull();
        registro.CodCta.Should().BeNull();
        registro.InfoCompl.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|M625|500,00|1|1000,000|7,6000|20032024|Devolucao venda|3.3.01.001|Complemento|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|M625|200,00||||15012024||||\r\n";

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
