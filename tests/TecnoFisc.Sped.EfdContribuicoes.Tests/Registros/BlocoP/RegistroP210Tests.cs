using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoP;

public sealed class RegistroP210Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroP210).Assembly);

    [Fact]
    public void Atributo_DeclaraP210_Nivel3_BlocoP()
    {
        var atributo = typeof(RegistroP210).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("P210");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("P");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroP210Com6CamposNaOrdem()
    {
        _catalogo.TentarObter("P210".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("P210");
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
        meta.Campos[3].Obrigatorio.Should().BeFalse();  // NumDoc
        meta.Campos[4].Obrigatorio.Should().BeFalse();  // DescrAj
        meta.Campos[5].Obrigatorio.Should().BeFalse();  // DtRef
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("P210".AsSpan(), out var meta);
        var registro = (RegistroP210)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());                    // IndAj
        meta.Campos[1].Definidor(registro, "1000,00".AsSpan());              // VlAj
        meta.Campos[2].Definidor(registro, "07".AsSpan());                   // CodAj
        meta.Campos[3].Definidor(registro, "PROC/2023".AsSpan());            // NumDoc
        meta.Campos[4].Definidor(registro, "Ajuste redução judicial".AsSpan()); // DescrAj
        meta.Campos[5].Definidor(registro, "01012023".AsSpan());             // DtRef

        registro.IndAj.Should().Be(IndicadorAjuste.Reducao);
        registro.VlAj.Should().Be(1000m);
        registro.CodAj.Should().Be("07");
        registro.NumDoc.Should().Be("PROC/2023");
        registro.DescrAj.Should().Be("Ajuste redução judicial");
        registro.DtRef.Should().Be(new DateOnly(2023, 1, 1));
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("P210".AsSpan(), out var meta);
        var registro = (RegistroP210)meta!.Fabrica();

        meta.Campos[3].Definidor(registro, ReadOnlySpan<char>.Empty); // NumDoc
        meta.Campos[4].Definidor(registro, ReadOnlySpan<char>.Empty); // DescrAj
        meta.Campos[5].Definidor(registro, ReadOnlySpan<char>.Empty); // DtRef

        registro.NumDoc.Should().BeNull();
        registro.DescrAj.Should().BeNull();
        registro.DtRef.Should().BeNull();
    }

    [Theory]
    [InlineData("0", IndicadorAjuste.Reducao)]
    [InlineData("1", IndicadorAjuste.Acrescimo)]
    public void Definidor_IndAj_AtribuiEnumCorreto(string codigo, IndicadorAjuste esperado)
    {
        _catalogo.TentarObter("P210".AsSpan(), out var meta);
        var registro = (RegistroP210)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, codigo.AsSpan());

        registro.IndAj.Should().Be(esperado);
    }

    [Theory]
    [InlineData(IndicadorAjuste.Reducao, "0")]
    [InlineData(IndicadorAjuste.Acrescimo, "1")]
    public void Serializar_IndAj_RetornaCodigoSpedCorreto(IndicadorAjuste ajuste, string esperado)
    {
        _catalogo.TentarObter("P210".AsSpan(), out var meta);
        var registro = (RegistroP210)meta!.Fabrica();
        registro.IndAj = ajuste;

        meta.Campos[0].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|P210|0|1000,00|07|PROC/2023|Ajuste redução judicial|01012023|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|P210|1|500,00|08||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
    {
        var leitor = new LeitorSpedTxt(_catalogo);
        var escritor = new EscritorSpedTxt(_catalogo);

        using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
        var registros = new List<RegistroSped>();
        await foreach (var registro in leitor.LerStreamingAsync(entrada, cancelamento))
            registros.Add(registro);

        using var saida = new MemoryStream();
        await escritor.EscreverAsync(saida, registros, cancelamento);

        return EncodingSped.Latin1.GetString(saida.ToArray());
    }
}
