using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoE;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoE;

/// <summary>
/// Sub-stage 8.165 — exercita a forma do <see cref="RegistroE230"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 217): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroE230Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroE230).Assembly);

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
    public void Atributo_DeclaraE230_Nivel5_BlocoE()
    {
        var atributo = typeof(RegistroE230).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("E230");
        atributo.Nivel.Should().Be(5);
        atributo.Bloco.Should().Be("E");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroE230Com5CamposNaOrdem()
    {
        _catalogo.TentarObter("E230".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("E230");
        meta.Campos.Select(c => c.Nome).Should().Equal(["NumDa", "NumProc", "IndProc", "Proc", "TxtCompl"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4, 5, 6]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("E230".AsSpan(), out var meta);
        var registro = (RegistroE230)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "DAE20230001".AsSpan());          // NumDa
        meta.Campos[1].Definidor(registro, "SP/2023/00001".AsSpan());        // NumProc
        meta.Campos[2].Definidor(registro, "2".AsSpan());                    // IndProc
        meta.Campos[3].Definidor(registro, "Processo administrativo".AsSpan()); // Proc
        meta.Campos[4].Definidor(registro, "Ajuste ICMS-ST".AsSpan());       // TxtCompl

        registro.NumDa.Should().Be("DAE20230001");
        registro.NumProc.Should().Be("SP/2023/00001");
        registro.IndProc.Should().Be(IndicadorOrigemProcesso.JusticaEstadual);
        registro.Proc.Should().Be("Processo administrativo");
        registro.TxtCompl.Should().Be("Ajuste ICMS-ST");
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("E230".AsSpan(), out var meta);
        var registro = (RegistroE230)meta!.Fabrica();

        foreach (var campo in meta!.Campos)
            campo.Definidor(registro, Span<char>.Empty);

        registro.NumDa.Should().BeNull();
        registro.NumProc.Should().BeNull();
        registro.IndProc.Should().BeNull();
        registro.Proc.Should().BeNull();
        registro.TxtCompl.Should().BeNull();
    }

    [Theory]
    [InlineData("0", IndicadorOrigemProcesso.Sefaz)]
    [InlineData("1", IndicadorOrigemProcesso.JusticaFederal)]
    [InlineData("2", IndicadorOrigemProcesso.JusticaEstadual)]
    [InlineData("9", IndicadorOrigemProcesso.Outros)]
    [InlineData("", null)]
    public void Definidor_IndProc_MapeiaCodigos(string input, IndicadorOrigemProcesso? esperado)
    {
        _catalogo.TentarObter("E230".AsSpan(), out var meta);
        var registro = (RegistroE230)meta!.Fabrica();

        meta.Campos[2].Definidor(registro, input.AsSpan());

        registro.IndProc.Should().Be(esperado);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|E230|DAE20230001|SP/2023/00001|2|Processo administrativo|Ajuste ICMS-ST|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_TodosCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // Todos os campos do E230 são OC — registro válido só com REG.
        const string sped = "|E230||||||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
