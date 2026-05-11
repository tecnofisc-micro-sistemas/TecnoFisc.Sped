using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 8.013 — exercita a forma do <see cref="Registro0210"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 37): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class Registro0210Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0210).Assembly);

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
    public void Atributo_Declara0210_Nivel3_Bloco0()
    {
        var atributo = typeof(Registro0210).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0210");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0210Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("0210".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0210");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodItemComp",
            "QtdComp",
            "Perda",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0210".AsSpan(), out var meta);
        var registro = (Registro0210)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "MAT-001".AsSpan());
        meta.Campos[1].Definidor(registro, "2.500000".AsSpan());
        meta.Campos[2].Definidor(registro, "1.2500".AsSpan());

        registro.CodItemComp.Should().Be("MAT-001");
        registro.QtdComp.Should().Be(2.5m);
        registro.Perda.Should().Be(1.25m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0210".AsSpan(), out var meta);
        var registro = (Registro0210)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);

        registro.CodItemComp.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0210|MAT-001|2,500000|1,2500|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComQuantidadeInteira_PreservaTextoCanonico()
    {
        const string sped = "|0210|INSUMO-QUIMICO-A|1,000000|0,5000|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
