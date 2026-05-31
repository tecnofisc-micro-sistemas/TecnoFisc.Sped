using System.Reflection;

using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Txt.Engine.Catalogo;
using TecnoFisc.Sped.Txt.Engine.Gerador;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.046 — exercita a forma do <see cref="RegistroC116"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 71): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC116Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC116).Assembly);

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
    public void Atributo_DeclaraC116_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC116).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C116");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC116Com5CamposNaOrdem()
    {
        _catalogo.TentarObter("C116".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C116");
        meta.Campos.Select(c => c.Nome).Should().Equal([
            "CodMod", "NrSat", "ChvCfe", "NumCfe", "DtDoc",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal(Enumerable.Range(2, 5));
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C116".AsSpan(), out var meta);
        var registro = (RegistroC116)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "59".AsSpan());                                          // CodMod
        meta.Campos[1].Definidor(registro, "900004081".AsSpan());                                   // NrSat
        meta.Campos[2].Definidor(registro, "35231111222333000181590000000010000010000006".AsSpan()); // ChvCfe
        meta.Campos[3].Definidor(registro, "000001".AsSpan());                                      // NumCfe
        meta.Campos[4].Definidor(registro, "12012024".AsSpan());                                    // DtDoc

        registro.CodMod.Should().Be("59");
        registro.NrSat.Should().Be(900004081);
        registro.ChvCfe.Should().Be("35231111222333000181590000000010000010000006");
        registro.NumCfe.Should().Be(1);
        registro.DtDoc.Should().Be(new DateOnly(2024, 1, 12));
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C116".AsSpan(), out var meta);
        var registro = (RegistroC116)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);  // CodMod
        meta.Campos[1].Definidor(registro, Span<char>.Empty);  // NrSat
        meta.Campos[2].Definidor(registro, Span<char>.Empty);  // ChvCfe
        meta.Campos[3].Definidor(registro, Span<char>.Empty);  // NumCfe

        registro.CodMod.Should().BeNull();
        registro.NrSat.Should().BeNull();
        registro.ChvCfe.Should().BeNull();
        registro.NumCfe.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        // CF-e SAT referenciado: modelo 59, série SAT, chave 44 dígitos, número e data de emissão.
        const string sped = "|C116|59|900004081|35231111222333000181590000000010000010000006|1|12012024|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComChaveEDataDiferentes_PreservaTextoCanonico()
    {
        // Segundo CF-e SAT referenciado com série e chave distintos.
        const string sped = "|C116|59|123456789|35261111222333000181590000000020000020000002|42|25122023|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
