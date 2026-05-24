using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.087 — exercita a forma do <see cref="RegistroC465"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 126): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC465Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC465).Assembly);

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
    public void Atributo_DeclaraC465_Nivel5_BlocoC()
    {
        var atributo = typeof(RegistroC465).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C465");
        atributo.Nivel.Should().Be(5);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC465Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("C465".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C465");
        meta.Campos.Select(c => c.Nome).Should().Equal(["ChvCfe", "NumCcf"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C465".AsSpan(), out var meta);
        var registro = (RegistroC465)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "12345678901234567890123456789012345678901234".AsSpan()); // ChvCfe
        meta.Campos[1].Definidor(registro, "1234".AsSpan());                                        // NumCcf

        registro.ChvCfe.Should().Be("12345678901234567890123456789012345678901234");
        registro.NumCcf.Should().Be(1234);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C465".AsSpan(), out var meta);
        var registro = (RegistroC465)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, ReadOnlySpan<char>.Empty); // ChvCfe
        meta.Campos[1].Definidor(registro, ReadOnlySpan<char>.Empty); // NumCcf

        registro.ChvCfe.Should().BeNull();
        registro.NumCcf.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped =
            "|C465|12345678901234567890123456789012345678901234|1234|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ContadorMaximo_PreservaTextoCanonico()
    {
        // NUM_CCF pode atingir até 9 dígitos (999999999) conforme tamanho do campo.
        const string sped =
            "|C465|99999999999999999999999999999999999999999999|999999999|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
