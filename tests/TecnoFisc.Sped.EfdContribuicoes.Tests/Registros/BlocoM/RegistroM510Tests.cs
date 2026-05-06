using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.BlocoM;

public sealed class RegistroM510Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroM510).Assembly);

    [Fact]
    public void Atributo_DeclaraM510_Nivel3_BlocoM()
    {
        var atributo = typeof(RegistroM510).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("M510");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("M");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroM510Com6CamposNaOrdem()
    {
        _catalogo.TentarObter("M510".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("M510");
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
        _catalogo.TentarObter("M510".AsSpan(), out var meta);
        var registro = (RegistroM510)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());          // IndAj
        meta.Campos[1].Definidor(registro, "1500,00".AsSpan());    // VlAj
        meta.Campos[2].Definidor(registro, "02".AsSpan());         // CodAj
        meta.Campos[3].Definidor(registro, "PROC-2024-001".AsSpan()); // NumDoc
        meta.Campos[4].Definidor(registro, "Devolução de compras".AsSpan()); // DescrAj
        meta.Campos[5].Definidor(registro, "31012024".AsSpan());   // DtRef

        registro.IndAj.Should().Be(IndicadorAjuste.Reducao);
        registro.VlAj.Should().Be(1500m);
        registro.CodAj.Should().Be("02");
        registro.NumDoc.Should().Be("PROC-2024-001");
        registro.DescrAj.Should().Be("Devolução de compras");
        registro.DtRef.Should().Be(new DateOnly(2024, 1, 31));
    }

    [Fact]
    public void Definidor_CamposOpcionaisVazios_DevolveNulo()
    {
        _catalogo.TentarObter("M510".AsSpan(), out var meta);
        var registro = (RegistroM510)meta!.Fabrica();

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
            "|M510|0|1500,00|02|PROC-2024-001|Devolução de compras|31012024|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        const string sped =
            "|M510|1|2750,00|01||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", IndicadorAjuste.Reducao)]
    [InlineData("1", IndicadorAjuste.Acrescimo)]
    public void IndicadorAjuste_Roundtrip_MapeiaCodigo(string codigo, IndicadorAjuste esperado)
    {
        _catalogo.TentarObter("M510".AsSpan(), out var meta);
        var registro = (RegistroM510)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, codigo.AsSpan());

        registro.IndAj.Should().Be(esperado);
    }

    [Fact]
    public void IndicadorAjuste_Nulo_DevolveVazio()
    {
        _catalogo.TentarObter("M510".AsSpan(), out var meta);
        var registro = (RegistroM510)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.IndAj.Should().Be(default(IndicadorAjuste));
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
