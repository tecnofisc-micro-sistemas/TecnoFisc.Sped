using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoE;

/// <summary>
/// Sub-stage 8.168 — exercita a forma do <see cref="RegistroE300"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.2.2 (p. 236-237): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroE300Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroE300).Assembly);

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
    public void Atributo_DeclaraE300_Nivel2_BlocoE()
    {
        var atributo = typeof(RegistroE300).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("E300");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("E");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroE300Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("E300".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("E300");
        meta.Campos.Select(c => c.Nome).Should().Equal(["Uf", "DtIni", "DtFin"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("E300".AsSpan(), out var meta);
        var registro = (RegistroE300)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "SP".AsSpan());         // Uf
        meta.Campos[1].Definidor(registro, "01012025".AsSpan());   // DtIni
        meta.Campos[2].Definidor(registro, "31012025".AsSpan());   // DtFin

        registro.Uf.Should().Be("SP");
        registro.DtIni.Should().Be(new DateOnly(2025, 1, 1));
        registro.DtFin.Should().Be(new DateOnly(2025, 1, 31));
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("E300".AsSpan(), out var meta);
        var registro = (RegistroE300)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);
        meta.Campos[1].Definidor(registro, Span<char>.Empty);
        meta.Campos[2].Definidor(registro, Span<char>.Empty);

        registro.Uf.Should().BeNull();
        registro.DtIni.Should().Be(default(DateOnly));
        registro.DtFin.Should().Be(default(DateOnly));
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|E300|SP|01012025|31012025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("SP")]
    [InlineData("RJ")]
    [InlineData("MG")]
    [InlineData("RS")]
    public async Task RoundTrip_ComDiferentesUfs_PreservaTextoCanonico(string uf)
    {
        var sped = $"|E300|{uf}|01022025|28022025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_PeriodoMultiMes_PreservaTextoCanonico()
    {
        // Apuração Difal/FCP cobrindo trimestre inteiro.
        const string sped = "|E300|MG|01012025|31032025|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
