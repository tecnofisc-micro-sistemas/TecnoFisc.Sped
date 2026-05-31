using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.104 — exercita a forma do <see cref="RegistroC791"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 150): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC791Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC791).Assembly);

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
    public void Atributo_DeclaraC791_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC791).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C791");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC791Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("C791".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C791");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "Uf", "VlBcIcmsSt", "VlIcmsSt"
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C791".AsSpan(), out var meta);
        var registro = (RegistroC791)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "SP".AsSpan());          // Uf
        meta.Campos[1].Definidor(registro, "10000,00".AsSpan());    // VlBcIcmsSt
        meta.Campos[2].Definidor(registro, "1200,00".AsSpan());     // VlIcmsSt

        registro.Uf.Should().Be("SP");
        registro.VlBcIcmsSt.Should().Be(10000.00m);
        registro.VlIcmsSt.Should().Be(1200.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C791".AsSpan(), out var meta);
        var registro = (RegistroC791)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.Uf.Should().BeNull();
        registro.VlBcIcmsSt.Should().Be(0m);
        registro.VlIcmsSt.Should().Be(0m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C791|SP|10000,00|1200,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComDiferentesUf_PreservaTextoCanonico()
    {
        const string sped =
            "|C791|MG|5000,00|600,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
