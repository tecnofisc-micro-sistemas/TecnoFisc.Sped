using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.047 — exercita a forma do <see cref="RegistroC120"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 72): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC120Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC120).Assembly);

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
    public void Atributo_DeclaraC120_Nivel3_BlocoC()
    {
        var atributo = typeof(RegistroC120).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C120");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC120Com5CamposNaOrdem()
    {
        _catalogo.TentarObter("C120".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C120");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodDocImp", "NumDocImp", "PisImp", "CofinsImp", "NumAcdraw",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 5));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C120".AsSpan(), out var meta);
        var registro = (RegistroC120)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "0".AsSpan());                     // CodDocImp
        meta.Campos[1].Definidor(registro, "21000012345".AsSpan());           // NumDocImp
        meta.Campos[2].Definidor(registro, "1500.75".AsSpan());               // PisImp
        meta.Campos[3].Definidor(registro, "6923.50".AsSpan());               // CofinsImp
        meta.Campos[4].Definidor(registro, "21/3456-2-000001-001".AsSpan());  // NumAcdraw

        registro.CodDocImp.Should().Be(CodigoDocumentoImportacao.DeclaracaoImportacao);
        registro.NumDocImp.Should().Be("21000012345");
        registro.PisImp.Should().Be(1500.75m);
        registro.CofinsImp.Should().Be(6923.50m);
        registro.NumAcdraw.Should().Be("21/3456-2-000001-001");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C120".AsSpan(), out var meta);
        var registro = (RegistroC120)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);  // CodDocImp
        meta.Campos[1].Definidor(registro, Span<char>.Empty);  // NumDocImp
        meta.Campos[2].Definidor(registro, Span<char>.Empty);  // PisImp
        meta.Campos[3].Definidor(registro, Span<char>.Empty);  // CofinsImp
        meta.Campos[4].Definidor(registro, Span<char>.Empty);  // NumAcdraw

        registro.CodDocImp.Should().BeNull();
        registro.NumDocImp.Should().BeNull();
        registro.PisImp.Should().BeNull();
        registro.CofinsImp.Should().BeNull();
        registro.NumAcdraw.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // DI com PIS, COFINS e Drawback informados.
        const string sped = "|C120|0|21000012345|1500,75|6923,50|21/3456-2-000001-001|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemCamposOpcionais_PreservaTextoCanonico()
    {
        // DSI sem PIS, COFINS nem Drawback (contribuinte que entrega EFD-Contribuições).
        const string sped = "|C120|1|21000098765||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", CodigoDocumentoImportacao.DeclaracaoImportacao)]
    [InlineData("1", CodigoDocumentoImportacao.DeclaracaoSimplificadaImportacao)]
    public void Definidor_CodigoDocumentoImportacao_MapeiaCadaValor(
        string codigo, CodigoDocumentoImportacao esperado)
    {
        _catalogo.TentarObter("C120".AsSpan(), out var meta);
        var registro = (RegistroC120)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, codigo.AsSpan());

        registro.CodDocImp.Should().Be(esperado);
    }

    [Fact]
    public void Definidor_CodigoDocumentoImportacao_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C120".AsSpan(), out var meta);
        var registro = (RegistroC120)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);

        registro.CodDocImp.Should().BeNull();
    }
}
