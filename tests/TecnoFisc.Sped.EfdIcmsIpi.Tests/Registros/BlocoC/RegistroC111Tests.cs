using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.041 — exercita a forma do <see cref="RegistroC111"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 67): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC111Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC111).Assembly);

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
    public void Atributo_DeclaraC111_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC111).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C111");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC111Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("C111".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C111");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "NumProc", "IndProc",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 2));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C111".AsSpan(), out var meta);
        var registro = (RegistroC111)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "12345678901234".AsSpan());  // NumProc
        meta.Campos[1].Definidor(registro, "1".AsSpan());               // IndProc

        registro.NumProc.Should().Be("12345678901234");
        registro.IndProc.Should().Be(IndicadorOrigemProcesso.JusticaFederal);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C111".AsSpan(), out var meta);
        var registro = (RegistroC111)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);  // NumProc (string nullable)

        registro.NumProc.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Processo referenciado na SEFAZ.
        const string sped = "|C111|12345678901234|0|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComJusticaFederal_PreservaTextoCanonico()
    {
        // Processo na Justiça Federal com número de ato concessório.
        const string sped = "|C111|000.123456/2023|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Theory]
    [InlineData("0", IndicadorOrigemProcesso.Sefaz)]
    [InlineData("1", IndicadorOrigemProcesso.JusticaFederal)]
    [InlineData("2", IndicadorOrigemProcesso.JusticaEstadual)]
    [InlineData("3", IndicadorOrigemProcesso.SecexSrf)]
    [InlineData("9", IndicadorOrigemProcesso.Outros)]
    public void Definidor_IndProcCobertosEnum(string valor, IndicadorOrigemProcesso esperado)
    {
        _catalogo.TentarObter("C111".AsSpan(), out var meta);
        var registro = (RegistroC111)meta!.Fabrica();

        meta.Campos[1].Definidor(registro, valor.AsSpan());

        registro.IndProc.Should().Be(esperado);
    }
}
