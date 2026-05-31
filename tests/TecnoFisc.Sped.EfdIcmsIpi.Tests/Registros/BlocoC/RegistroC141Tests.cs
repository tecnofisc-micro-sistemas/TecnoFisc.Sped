using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.050 — exercita a forma do <see cref="RegistroC141"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 74): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC141Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC141).Assembly);

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
    public void Atributo_DeclaraC141_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC141).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C141");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC141Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("C141".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C141");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "NumParc", "DtVcto", "VlParc",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 3));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C141".AsSpan(), out var meta);
        var registro = (RegistroC141)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01".AsSpan());       // NumParc
        meta.Campos[1].Definidor(registro, "15012025".AsSpan()); // DtVcto
        meta.Campos[2].Definidor(registro, "5000.00".AsSpan());  // VlParc

        registro.NumParc.Should().Be(1);
        registro.DtVcto.Should().Be(new DateOnly(2025, 1, 15));
        registro.VlParc.Should().Be(5000.00m);
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // Parcela 1 de 3, vencimento em 15/01/2025.
        const string sped = "|C141|1|15012025|5000,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_Parcela3_PreservaTextoCanonico()
    {
        // Última parcela com valor diferente.
        const string sped = "|C141|3|15032025|5100,50|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
