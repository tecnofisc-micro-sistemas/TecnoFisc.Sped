using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.145 — exercita a forma do <see cref="RegistroD530"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (pp. 202-203): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD530Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD530).Assembly);

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

    [Fact]
    public void Atributo_DeclaraD530_Nivel3_BlocoD()
    {
        var atributo = typeof(RegistroD530).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D530");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD530Com6CamposNaOrdem()
    {
        _catalogo.TentarObter("D530".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D530");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "IndServ", "DtIniServ", "DtFinServ", "PerFiscal", "CodArea", "Terminal"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6, 7]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D530".AsSpan(), out var meta);
        var registro = (RegistroD530)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());          // IndServ
        meta.Campos[1].Definidor(registro, "01012024".AsSpan());   // DtIniServ
        meta.Campos[2].Definidor(registro, "31012024".AsSpan());   // DtFinServ
        meta.Campos[3].Definidor(registro, "012024".AsSpan());     // PerFiscal
        meta.Campos[4].Definidor(registro, "011".AsSpan());        // CodArea
        meta.Campos[5].Definidor(registro, "99887766".AsSpan());   // Terminal

        registro.IndServ.Should().Be(IndicadorTipoServico.Telefonia);
        registro.DtIniServ.Should().Be(new DateOnly(2024, 1, 1));
        registro.DtFinServ.Should().Be(new DateOnly(2024, 1, 31));
        registro.PerFiscal.Should().Be("012024");
        registro.CodArea.Should().Be("011");
        registro.Terminal.Should().Be(99887766L);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D530".AsSpan(), out var meta);
        var registro = (RegistroD530)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);  // IndServ
        meta.Campos[1].Definidor(registro, Span<char>.Empty);  // DtIniServ
        meta.Campos[2].Definidor(registro, Span<char>.Empty);  // DtFinServ
        meta.Campos[3].Definidor(registro, Span<char>.Empty);  // PerFiscal
        meta.Campos[4].Definidor(registro, Span<char>.Empty);  // CodArea
        meta.Campos[5].Definidor(registro, Span<char>.Empty);  // Terminal

        registro.IndServ.Should().BeNull();
        registro.DtIniServ.Should().BeNull();
        registro.DtFinServ.Should().BeNull();
        registro.PerFiscal.Should().BeNull();
        registro.CodArea.Should().BeNull();
        registro.Terminal.Should().BeNull();
    }

    [Theory]
    [InlineData("0", IndicadorTipoServico.Telefonia)]
    [InlineData("1", IndicadorTipoServico.ComunicacaoDeDados)]
    [InlineData("2", IndicadorTipoServico.TvPorAssinatura)]
    [InlineData("3", IndicadorTipoServico.ProvimentoAcessoInternet)]
    [InlineData("4", IndicadorTipoServico.Multimidia)]
    [InlineData("9", IndicadorTipoServico.Outros)]
    [InlineData("", null)]
    public void Definidor_IndServ_MapeiaCodigos(string input, IndicadorTipoServico? esperado)
    {
        _catalogo.TentarObter("D530".AsSpan(), out var meta);
        var registro = (RegistroD530)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, input.AsSpan());

        registro.IndServ.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|D530|0|01012024|31012024|012024|011|99887766|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // Terminal sem datas de início/fim nem código de área, apenas PerFiscal e IndServ.
        const string sped = "|D530|0|||012024|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
