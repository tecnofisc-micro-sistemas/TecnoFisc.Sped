using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoM;

public sealed class RegistroM225Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroM225).Assembly);

    [Fact]
    public void Atributo_DeclaraM225_Nivel5_BlocoM()
    {
        var atributo = typeof(RegistroM225).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("M225");
        atributo.Nivel.Should().Be(5);
        atributo.Bloco.Should().Be("M");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroM225Com8CamposNaOrdem()
    {
        _catalogo.TentarObter("M225".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("M225");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "DetValorAj", "CstPis", "DetBcCred", "DetAliq", "DtOperAj",
            "DescAj", "CodCta", "InfoCompl",
        ]);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // DetValorAj
        meta.Campos[4].Tamanho.Should().Be(8);
        meta.Campos[4].Obrigatorio.Should().BeTrue();   // DtOperAj
        meta.Campos[6].Tamanho.Should().Be(255);
        meta.Campos[6].Obrigatorio.Should().BeFalse();  // CodCta
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("M225".AsSpan(), out var meta);
        var registro = (RegistroM225)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "3200,00".AsSpan());        // DetValorAj
        meta.Campos[1].Definidor(registro, "50".AsSpan());             // CstPis
        meta.Campos[2].Definidor(registro, "50000,000".AsSpan());      // DetBcCred
        meta.Campos[3].Definidor(registro, "65,0000".AsSpan());        // DetAliq
        meta.Campos[4].Definidor(registro, "28022024".AsSpan());       // DtOperAj
        meta.Campos[5].Definidor(registro, "Ajuste de contribuição".AsSpan()); // DescAj
        meta.Campos[6].Definidor(registro, "3.1.1.01.001".AsSpan());   // CodCta
        meta.Campos[7].Definidor(registro, "Informação complementar".AsSpan()); // InfoCompl

        registro.DetValorAj.Should().Be(3200m);
        registro.CstPis.Should().Be(50);
        registro.DetBcCred.Should().Be(50000m);
        registro.DetAliq.Should().Be(65m);
        registro.DtOperAj.Should().Be(new DateOnly(2024, 2, 28));
        registro.DescAj.Should().Be("Ajuste de contribuição");
        registro.CodCta.Should().Be("3.1.1.01.001");
        registro.InfoCompl.Should().Be("Informação complementar");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("M225".AsSpan(), out var meta);
        var registro = (RegistroM225)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty); // CstPis
        meta.Campos[2].Definidor(registro, ReadOnlySpan<char>.Empty); // DetBcCred
        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // DetAliq
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // DescAj
        meta.Campos[6].Definidor(registro, ReadOnlySpan<char>.Empty); // CodCta
        meta.Campos[7].Definidor(registro, ReadOnlySpan<char>.Empty); // InfoCompl

        registro.CstPis.Should().BeNull();
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
            "|M225|3200,00|50|50000,000|65,0000|28022024|Ajuste de contribuição|3.1.1.01.001|Informação complementar|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // Apenas campos obrigatórios: DetValorAj e DtOperAj
        const string sped =
            "|M225|1500,00||||01032024||||\r\n";

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
