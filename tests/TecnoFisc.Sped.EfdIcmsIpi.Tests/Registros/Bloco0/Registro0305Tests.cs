using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 8.016 — exercita a forma do <see cref="Registro0305"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 40): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class Registro0305Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0305).Assembly);

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
    public void Atributo_Declara0305_Nivel3_Bloco0()
    {
        var atributo = typeof(Registro0305).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0305");
        atributo.Nivel.Should().Be(3);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0305Com3CamposNaOrdem()
    {
        _catalogo.TentarObter("0305".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0305");
        meta.Campos.Select(c => c.Nome).Should().Equal(
        [
            "CodCcus",
            "Func",
            "VidaUtil",
        ]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3, 4]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0305".AsSpan(), out var meta);
        var registro = (Registro0305)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "CC-001".AsSpan());
        meta.Campos[1].Definidor(registro, "Producao de embalagens plasticas".AsSpan());
        meta.Campos[2].Definidor(registro, "120".AsSpan());

        registro.CodCcus.Should().Be("CC-001");
        registro.Func.Should().Be("Producao de embalagens plasticas");
        registro.VidaUtil.Should().Be(120);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("0305".AsSpan(), out var meta);
        var registro = (Registro0305)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, Span<char>.Empty);

        registro.CodCcus.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0305|CC-001|Producao de embalagens plasticas|120|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_SemVidaUtil_PreservaTextoCanonico()
    {
        const string sped = "|0305|CC-002|Armazenamento de materias-primas||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
