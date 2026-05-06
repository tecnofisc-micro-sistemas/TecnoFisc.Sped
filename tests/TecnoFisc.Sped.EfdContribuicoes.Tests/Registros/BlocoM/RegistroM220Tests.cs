using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoM;

public sealed class RegistroM220Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroM220).Assembly);

    [Fact]
    public void Atributo_DeclaraM220_Nivel4_BlocoM()
    {
        var atributo = typeof(RegistroM220).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("M220");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("M");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroM220Com6CamposNaOrdem()
    {
        _catalogo.TentarObter("M220".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("M220");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "IndAj", "VlAj", "CodAj", "NumDoc", "DescrAj", "DtRef",
        ]);
        meta.Campos[0].Tamanho.Should().Be(1);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // IndAj
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // VlAj
        meta.Campos[2].Tamanho.Should().Be(2);
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // CodAj
        meta.Campos[5].Tamanho.Should().Be(8);
        meta.Campos[5].Obrigatorio.Should().BeFalse();  // DtRef
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("M220".AsSpan(), out var meta);
        var registro = (RegistroM220)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());              // IndAj
        meta.Campos[1].Definidor(registro, "3200,00".AsSpan());        // VlAj
        meta.Campos[2].Definidor(registro, "04".AsSpan());             // CodAj
        meta.Campos[3].Definidor(registro, "PROC-2024-005".AsSpan()); // NumDoc
        meta.Campos[4].Definidor(registro, "Acréscimo de contribuição".AsSpan()); // DescrAj
        meta.Campos[5].Definidor(registro, "28022024".AsSpan());       // DtRef

        registro.IndAj.Should().Be(IndicadorAjuste.Acrescimo);
        registro.VlAj.Should().Be(3200m);
        registro.CodAj.Should().Be("04");
        registro.NumDoc.Should().Be("PROC-2024-005");
        registro.DescrAj.Should().Be("Acréscimo de contribuição");
        registro.DtRef.Should().Be(new DateOnly(2024, 2, 28));
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("M220".AsSpan(), out var meta);
        var registro = (RegistroM220)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // NumDoc
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // DescrAj
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // DtRef

        registro.NumDoc.Should().BeNull();
        registro.DescrAj.Should().BeNull();
        registro.DtRef.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|M220|1|3200,00|04|PROC-2024-005|Acréscimo de contribuição|28022024|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // Ajuste de redução sem processo, descrição ou data de referência
        const string sped =
            "|M220|0|1000,00|02||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", IndicadorAjuste.Reducao)]
    [InlineData("1", IndicadorAjuste.Acrescimo)]
    public void IndicadorAjuste_Roundtrip_MapeiaCodigo(string codigo, IndicadorAjuste esperado)
    {
        _catalogo.TentarObter("M220".AsSpan(), out var meta);
        var registro = (RegistroM220)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, codigo.AsSpan());

        registro.IndAj.Should().Be(esperado);
    }

    [Fact]
    public void IndicadorAjuste_Nulo_DevolveVazio()
    {
        _catalogo.TentarObter("M220".AsSpan(), out var meta);
        var registro = (RegistroM220)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.IndAj.Should().Be(default(IndicadorAjuste));
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
