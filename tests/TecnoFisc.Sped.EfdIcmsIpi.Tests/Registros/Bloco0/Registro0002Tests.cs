using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.Bloco0;

/// <summary>
/// Sub-stage 8.003 — exercita a forma do <see cref="Registro0002"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 27-28): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class Registro0002Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro0002).Assembly);

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
    public void Atributo_Declara0002_Nivel2_Bloco0()
    {
        var atributo = typeof(Registro0002).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("0002");
        atributo.Nivel.Should().Be(2);
        atributo.Bloco.Should().Be("0");
    }

    [Fact]
    public void Catalogo_ExpoeRegistro0002ComUmCampoNaOrdem()
    {
        _catalogo.TentarObter("0002".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("0002");
        meta.Campos.Select(c => c.Nome).Should().Equal(["ClasEstabInd"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("0002".AsSpan(), out var meta);
        var registro = (Registro0002)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "01".AsSpan());

        registro.ClasEstabInd.Should().Be(1);
    }

    [Fact]
    public void Serializar_ClasEstabInd_RetornaCodigo()
    {
        _catalogo.TentarObter("0002".AsSpan(), out var meta);
        var registro = (Registro0002)meta!.Fabrica();
        registro.ClasEstabInd = 1;

        meta.Campos[0].Serializar(registro).Should().Be("1");
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|0002|1|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_CodigoAlternativo_PreservaTextoCanonico()
    {
        const string sped = "|0002|5|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
