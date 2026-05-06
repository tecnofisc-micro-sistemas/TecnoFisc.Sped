using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoM;

public sealed class RegistroM620Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroM620).Assembly);

    [Fact]
    public void Atributo_DeclaraM620_Nivel4_BlocoM()
    {
        var atributo = typeof(RegistroM620).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("M620");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("M");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroM620Com6CamposNaOrdem()
    {
        _catalogo.TentarObter("M620".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("M620");
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "IndAj", "VlAj", "CodAj", "NumDoc", "DescrAj", "DtRef",
        ]);
        meta.Campos[0].Obrigatorio.Should().BeTrue();   // IndAj
        meta.Campos[1].Obrigatorio.Should().BeTrue();   // VlAj
        meta.Campos[2].Obrigatorio.Should().BeTrue();   // CodAj
        meta.Campos[3].Obrigatorio.Should().BeFalse();  // NumDoc
        meta.Campos[4].Obrigatorio.Should().BeFalse();  // DescrAj
        meta.Campos[5].Obrigatorio.Should().BeFalse();  // DtRef
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("M620".AsSpan(), out var meta);
        var registro = (RegistroM620)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "1".AsSpan());              // IndAj
        meta.Campos[1].Definidor(registro, "1500,00".AsSpan());        // VlAj
        meta.Campos[2].Definidor(registro, "02".AsSpan());             // CodAj
        meta.Campos[3].Definidor(registro, "NF-000123".AsSpan());      // NumDoc
        meta.Campos[4].Definidor(registro, "Ajuste Cofins".AsSpan());  // DescrAj
        meta.Campos[5].Definidor(registro, "15012024".AsSpan());       // DtRef

        registro.IndAj.Should().Be(IndicadorAjuste.Acrescimo);
        registro.VlAj.Should().Be(1500m);
        registro.CodAj.Should().Be("02");
        registro.NumDoc.Should().Be("NF-000123");
        registro.DescrAj.Should().Be("Ajuste Cofins");
        registro.DtRef.Should().Be(new DateOnly(2024, 1, 15));
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("M620".AsSpan(), out var meta);
        var registro = (RegistroM620)meta!.Fabrica();

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
        const string sped = "|M620|1|1500,00|02|NF-000123|Ajuste Cofins|15012024|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped = "|M620|0|800,00|01||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", IndicadorAjuste.Reducao)]
    [InlineData("1", IndicadorAjuste.Acrescimo)]
    public void IndicadorAjuste_Roundtrip_MapeiaCodigo(string codigo, IndicadorAjuste esperado)
    {
        _catalogo.TentarObter("M620".AsSpan(), out var meta);
        var registro = (RegistroM620)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, codigo.AsSpan());

        registro.IndAj.Should().Be(esperado);
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
