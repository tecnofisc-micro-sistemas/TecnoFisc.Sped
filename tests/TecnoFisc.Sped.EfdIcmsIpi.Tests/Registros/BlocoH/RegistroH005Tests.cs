using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoH;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoH;

/// <summary>
/// Sub-stage 8.188 — exercita a forma do <see cref="RegistroH005"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 245-246): metadados de catálogo, mapeamento de campos e
/// invariante de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroH005Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroH005).Assembly);

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

    [Fact]
    public void Atributo_DeclaraH005_Nivel2_BlocoH()
    {
        var atributo = typeof(RegistroH005).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("H005");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("H");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroH005ComTresCamposNaOrdem()
    {
        _catalogo.TentarObter("H005".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("H005");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "DtInv",
            "VlInv",
            "MotInv",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("H005".AsSpan(), out var meta);
        var registro = (RegistroH005)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "31122025".AsSpan());
        meta.Campos[1].Definidor(registro, "12345,67".AsSpan());
        meta.Campos[2].Definidor(registro, "01".AsSpan());

        registro.DtInv.Should().Be(new DateOnly(2025, 12, 31));
        registro.VlInv.Should().Be(12345.67m);
        registro.MotInv.Should().Be(MotivoInventario.FinalPeriodo);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("H005".AsSpan(), out var meta);
        var registro = (RegistroH005)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.DtInv.Should().Be(default(DateOnly));
        registro.VlInv.Should().Be(0m);
        registro.MotInv.Should().Be(default(MotivoInventario));
    }

    [Theory]
    [InlineData("01", MotivoInventario.FinalPeriodo)]
    [InlineData("02", MotivoInventario.MudancaFormaTributacao)]
    [InlineData("03", MotivoInventario.BaixaCadastralParalisacao)]
    [InlineData("04", MotivoInventario.AlteracaoRegimePagamento)]
    [InlineData("05", MotivoInventario.DeterminacaoFisco)]
    [InlineData("06", MotivoInventario.ControleSubstituicaoTributaria)]
    public void Definidor_MotInv_MapeiaCodigos(string input, MotivoInventario esperado)
    {
        _catalogo.TentarObter("H005".AsSpan(), out var meta);
        var registro = (RegistroH005)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, input.AsSpan());

        registro.MotInv.Should().Be(esperado);
    }

    [Theory]
    [InlineData(MotivoInventario.FinalPeriodo, "01")]
    [InlineData(MotivoInventario.MudancaFormaTributacao, "02")]
    [InlineData(MotivoInventario.BaixaCadastralParalisacao, "03")]
    [InlineData(MotivoInventario.AlteracaoRegimePagamento, "04")]
    [InlineData(MotivoInventario.DeterminacaoFisco, "05")]
    [InlineData(MotivoInventario.ControleSubstituicaoTributaria, "06")]
    public void Serializar_MotInv_RetornaCodigo(MotivoInventario motivo, string esperado)
    {
        _catalogo.TentarObter("H005".AsSpan(), out var meta);
        var registro = (RegistroH005)meta!.Fabrica();
        registro.MotInv = motivo;

        meta.Campos[2].Serializar(registro).Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|H005|31122025|12345,67|01|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComInventarioZerado_PreservaTextoCanonico()
    {
        const string sped = "|H005|31012025|0,00|06|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
