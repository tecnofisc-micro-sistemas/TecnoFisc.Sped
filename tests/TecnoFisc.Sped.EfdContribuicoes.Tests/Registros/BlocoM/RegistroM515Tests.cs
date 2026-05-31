using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoM;

public sealed class RegistroM515Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroM515).Assembly);

    [Fact]
    public void Atributo_DeclaraM515_Nivel4_BlocoM()
    {
        var atributo = typeof(RegistroM515).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("M515");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("M");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroM515Com8CamposNaOrdem()
    {
        _catalogo.TentarObter("M515".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("M515");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7, 8, 9]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "DetValorAj", "CstCofins", "DetBcCred", "DetAliq", "DtOperAj",
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
        _catalogo.TentarObter("M515".AsSpan(), out var meta);
        var registro = (RegistroM515)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1500,00".AsSpan());        // DetValorAj
        meta.Campos[1].Definidor(registro, "50".AsSpan());             // CstCofins
        meta.Campos[2].Definidor(registro, "30000,000".AsSpan());      // DetBcCred
        meta.Campos[3].Definidor(registro, "65,0000".AsSpan());        // DetAliq
        meta.Campos[4].Definidor(registro, "15032024".AsSpan());       // DtOperAj
        meta.Campos[5].Definidor(registro, "Devolução de mercadoria".AsSpan()); // DescAj
        meta.Campos[6].Definidor(registro, "1.2.3.4.5".AsSpan());      // CodCta
        meta.Campos[7].Definidor(registro, "Informação adicional".AsSpan()); // InfoCompl

        registro.DetValorAj.Should().Be(1500m);
        registro.CstCofins.Should().Be(50);
        registro.DetBcCred.Should().Be(30000m);
        registro.DetAliq.Should().Be(65m);
        registro.DtOperAj.Should().Be(new DateOnly(2024, 3, 15));
        registro.DescAj.Should().Be("Devolução de mercadoria");
        registro.CodCta.Should().Be("1.2.3.4.5");
        registro.InfoCompl.Should().Be("Informação adicional");
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("M515".AsSpan(), out var meta);
        var registro = (RegistroM515)meta!.Fabrica();

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
            "|M515|1500,00|50|30000,000|65,0000|15032024|Devolução de mercadoria|1.2.3.4.5|Informação adicional|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // Apenas campos obrigatórios: DetValorAj e DtOperAj
        const string sped =
            "|M515|2000,00||||01062024||||\r\n";

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
