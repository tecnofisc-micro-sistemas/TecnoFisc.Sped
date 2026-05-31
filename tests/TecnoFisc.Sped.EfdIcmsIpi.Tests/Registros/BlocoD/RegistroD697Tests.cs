using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;
using TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoD;

/// <summary>
/// Sub-stage 8.152 — exercita a forma do <see cref="RegistroD697"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 205): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroD697Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroD697).Assembly);

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
    public void Atributo_DeclaraD697_Nivel4_BlocoD()
    {
        var atributo = typeof(RegistroD697).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("D697");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("D");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroD697Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("D697".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("D697");
        meta.Campos.Select(c => c.Nome).Should().Equal(["Uf", "VlBcIcms", "VlIcms"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("D697".AsSpan(), out var meta);
        var registro = (RegistroD697)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "SP".AsSpan());         // Uf
        meta.Campos[1].Definidor(registro, "125000,00".AsSpan()); // VlBcIcms
        meta.Campos[2].Definidor(registro, "15000,00".AsSpan());  // VlIcms

        registro.Uf.Should().Be("SP");
        registro.VlBcIcms.Should().Be(125000.00m);
        registro.VlIcms.Should().Be(15000.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("D697".AsSpan(), out var meta);
        var registro = (RegistroD697)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.Uf.Should().BeNull();
        registro.VlBcIcms.Should().BeNull();
        registro.VlIcms.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Prestadora de TV por assinatura via satélite — ICMS de assinantes em SP.
        const string sped = "|D697|SP|125000,00|15000,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComOutraUf_PreservaTextoCanonico()
    {
        // Totalização dos documentos emitidos para assinantes no RJ.
        const string sped = "|D697|RJ|80000,00|9600,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComValoresZerados_PreservaTextoCanonico()
    {
        // UF do próprio prestador (preencher com zero conforme instrução do guia).
        const string sped = "|D697|MG|0,00|0,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
