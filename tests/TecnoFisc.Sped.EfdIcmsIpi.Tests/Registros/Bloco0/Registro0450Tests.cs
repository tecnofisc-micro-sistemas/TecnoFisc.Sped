using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 8.018 — exercita a forma do <see cref="Registro0450"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 41): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class Registro0450Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0450).Assembly);

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
    public void Atributo_Declara0450_Nivel2_Bloco0()
    {
        var atributo = typeof(Registro0450).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0450");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0450Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("0450".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0450");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodInf",
            "Txt",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0450".AsSpan(), out var meta);
        var registro = (Registro0450)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "INF001".AsSpan());
        meta.Campos[1].Definidor(registro, "Devolucao de mercadoria conforme NF referenciada".AsSpan());

        registro.CodInf.Should().Be("INF001");
        registro.Txt.Should().Be("Devolucao de mercadoria conforme NF referenciada");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0450".AsSpan(), out var meta);
        var registro = (Registro0450)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);

        registro.CodInf.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0450|INF001|Devolucao de mercadoria conforme NF referenciada|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComTxtLongo_PreservaTextoCanonico()
    {
        const string sped = "|0450|OBS01|Operacao amparada pelo Convenio ICMS 52/91 - isencao nas saidas internas de produtos agropecuarios|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
