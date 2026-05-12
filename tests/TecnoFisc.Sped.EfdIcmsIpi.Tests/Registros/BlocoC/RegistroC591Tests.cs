using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;

namespace TecnoFisc.Sped.EfdIcmsIpi.Tests.Registros.BlocoC;

/// <summary>
/// Sub-stage 8.095 — exercita a forma do <see cref="RegistroC591"/> contra o Guia Prático
/// EFD-ICMS/IPI V3.0.6 (p. 141): metadados de catálogo, mapeamento de campos e invariante
/// de round-trip parse → gerar → texto idêntico.
/// </summary>
public sealed class RegistroC591Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(RegistroC591).Assembly);

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
    public void Atributo_DeclaraC591_Nivel4_BlocoC()
    {
        var atributo = typeof(RegistroC591).GetCustomAttribute<RegistroSpedAttribute>();

        atributo.Should().NotBeNull();
        atributo!.Codigo.Should().Be("C591");
        atributo.Nivel.Should().Be(4);
        atributo.Bloco.Should().Be("C");
    }

    [Fact]
    public void Catalogo_ExpoeRegistroC591Com2CamposNaOrdem()
    {
        _catalogo.TentarObter("C591".AsSpan(), out var meta).Should().BeTrue();

        meta!.Codigo.Should().Be("C591");
        meta.Campos.Select(c => c.Nome).Should().Equal(["VlFcpOp", "VlFcpSt"]);
        meta.Campos.Select(c => c.Ordem).Should().Equal([2, 3]);
    }

    [Fact]
    public void Definidor_AtribuiTodosOsCampos()
    {
        _catalogo.TentarObter("C591".AsSpan(), out var meta);
        var registro = (RegistroC591)meta!.Fabrica();

        meta.Campos[0].Definidor(registro, "150,00".AsSpan()); // VlFcpOp
        meta.Campos[1].Definidor(registro, "75,00".AsSpan());  // VlFcpSt

        registro.VlFcpOp.Should().Be(150.00m);
        registro.VlFcpSt.Should().Be(75.00m);
    }

    [Fact]
    public void Definidor_CampoVazio_DevolveNulo()
    {
        _catalogo.TentarObter("C591".AsSpan(), out var meta);
        var registro = (RegistroC591)meta!.Fabrica();

        foreach (var campo in meta.Campos)
            campo.Definidor(registro, ReadOnlySpan<char>.Empty);

        registro.VlFcpOp.Should().BeNull();
        registro.VlFcpSt.Should().BeNull();
    }

    [Fact]
    public async Task RoundTrip_ComTodosOsCampos_PreservaTextoCanonico()
    {
        const string sped = "|C591|150,00|75,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComCamposOpcionaisVazios_PreservaTextoCanonico()
    {
        // Ambos os campos são opcionais; linha com apenas o REG é válida.
        const string sped = "|C591|||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComApenasVlFcpOp_PreservaTextoCanonico()
    {
        // Operação própria com FCP; ST vazio (CST_ICMS do C590 pai é x00 ou x20).
        const string sped = "|C591|200,50||\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }

    [Fact]
    public async Task RoundTrip_ComApenasVlFcpSt_PreservaTextoCanonico()
    {
        // Substituição tributária com FCP; OP vazio (CST_ICMS do C590 pai é x10).
        const string sped = "|C591||30,00|\r\n";

        var resultado = await RoundTripAsync(sped, TestContext.Current.CancellationToken);

        resultado.Should().Be(sped);
    }
}
